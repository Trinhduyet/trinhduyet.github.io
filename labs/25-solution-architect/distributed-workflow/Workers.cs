using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using RabbitMQ.Client;

namespace WorkflowLab;

public abstract class RabbitWorker(
    LabStore store, IConfiguration config, ILogger logger) : BackgroundService
{
    protected LabStore Store { get; } = store;
    protected ILogger Log { get; } = logger;
    private readonly string _rabbitUri = config["RabbitUri"] ?? "amqp://guest:guest@localhost:5672/";
    protected const string Queue = "architecture.workflow.v1";
    protected const string DeadLetterQueue = "architecture.workflow.dlq";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory { Uri = new Uri(_rabbitUri), AutomaticRecoveryEnabled = true };
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await OpenChannelAsync(connection, stoppingToken);
                await channel.QueueDeclareAsync(DeadLetterQueue, durable: true, exclusive: false,
                    autoDelete: false, arguments: null, cancellationToken: stoppingToken);
                var deadLetterArgs = new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = DeadLetterQueue
                };
                await channel.QueueDeclareAsync(Queue, durable: true, exclusive: false,
                    autoDelete: false, arguments: deadLetterArgs, cancellationToken: stoppingToken);
                await RunAsync(channel, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "Broker unavailable; retrying the lab worker");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    protected virtual Task<IChannel> OpenChannelAsync(IConnection connection, CancellationToken ct)
        => connection.CreateChannelAsync(cancellationToken: ct);

    protected abstract Task RunAsync(IChannel channel, CancellationToken ct);
}

public sealed class OutboxPublisher(LabStore store, IConfiguration config, ILogger<OutboxPublisher> logger)
    : RabbitWorker(store, config, logger)
{
    // Demonstrates the ambiguous outcome: Rabbit receives message, app fails before marking outbox.
    private int _failOnce = config.GetValue<bool>("FailAfterPublishOnce") ? 1 : 0;

    protected override Task<IChannel> OpenChannelAsync(IConnection connection, CancellationToken ct)
        => connection.CreateChannelAsync(new CreateChannelOptions(true, true), ct);

    protected override async Task RunAsync(IChannel channel, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var item = await Store.NextOutboxAsync(ct);
            if (item is null)
            {
                await Task.Delay(200, ct);
                continue;
            }
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item));
            var props = new BasicProperties { ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent,
                MessageId = item.EventId.ToString() };
            await channel.BasicPublishAsync("", Queue, mandatory: true,
                basicProperties: props, body: body, cancellationToken: ct);
            if (Interlocked.Exchange(ref _failOnce, 0) == 1)
                throw new InvalidOperationException("LAB FAULT: crash after broker confirm, before outbox mark");
            await Store.MarkPublishedAsync(item.EventId, ct);
        }
    }
}

public sealed class InboxConsumer(LabStore store, IConfiguration config, ILogger<InboxConsumer> logger)
    : RabbitWorker(store, config, logger)
{
    protected override async Task RunAsync(IChannel channel, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var received = await channel.BasicGetAsync(Queue, autoAck: false, cancellationToken: ct);
            if (received is null)
            {
                await Task.Delay(200, ct);
                continue;
            }
            try
            {
                var message = JsonSerializer.Deserialize<WorkflowEvent>(received.Body.Span)
                    ?? throw new InvalidOperationException("Missing workflow event");
                await Store.ProcessEventAsync(message, ct);
                await channel.BasicAckAsync(received.DeliveryTag, multiple: false, cancellationToken: ct);
            }
            catch (SqlException ex)
            {
                Log.LogWarning(ex, "SQL unavailable; requeueing delivery after a bounded delay");
                await Task.Delay(1000, ct);
                await channel.BasicNackAsync(received.DeliveryTag, multiple: false,
                    requeue: true, cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.LogError(ex, "Consumer error; quarantining this delivery for inspection");
                await channel.BasicRejectAsync(received.DeliveryTag, requeue: false, cancellationToken: ct);
            }
        }
    }
}
