using System.Diagnostics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHttpClient("upstream", client =>
    {
        client.Timeout = Timeout.InfiniteTimeSpan;
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        AutomaticDecompression = DecompressionMethods.All,
        ConnectTimeout = TimeSpan.FromSeconds(2),
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    });

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
    app.Logger.LogInformation(
        "Incident service started. ProcessId={ProcessId} Host={Host}",
        Environment.ProcessId,
        Dns.GetHostName()));

app.Lifetime.ApplicationStopping.Register(() =>
    app.Logger.LogInformation(
        "Incident service is stopping gracefully. ProcessId={ProcessId}",
        Environment.ProcessId));

app.Lifetime.ApplicationStopped.Register(() =>
    app.Logger.LogInformation(
        "Incident service stopped. ProcessId={ProcessId}",
        Environment.ProcessId));

app.MapGet("/", () => Results.Ok(new
{
    service = "incident-service",
    purpose = "bounded Linux and networking diagnostics lab",
    endpoints = new[]
    {
        "/health/live",
        "/health/ready",
        "/diagnostics/request",
        "/work?delayMs=1000",
        "/dependency"
    }
}));

app.MapGet("/health/live", () => Results.Ok(new
{
    status = "live",
    processId = Environment.ProcessId,
    host = Dns.GetHostName()
}));

app.MapGet("/health/ready", (IConfiguration configuration) =>
{
    var readinessMode = configuration["READINESS_MODE"];

    return string.Equals(readinessMode, "fail", StringComparison.OrdinalIgnoreCase)
        ? Results.Json(
            new { status = "not-ready", reason = "READINESS_MODE=fail" },
            statusCode: StatusCodes.Status503ServiceUnavailable)
        : Results.Json(new { status = "ready" });
});

app.MapGet("/diagnostics/request", (HttpContext context) => Results.Ok(new
{
    traceIdentifier = context.TraceIdentifier,
    processId = Environment.ProcessId,
    remoteIp = context.Connection.RemoteIpAddress?.ToString(),
    remotePort = context.Connection.RemotePort,
    localIp = context.Connection.LocalIpAddress?.ToString(),
    localPort = context.Connection.LocalPort,
    scheme = context.Request.Scheme,
    host = context.Request.Host.Value,
    pathBase = context.Request.PathBase.Value,
    rawForwardedFor = context.Request.Headers["X-Forwarded-For"].ToString(),
    rawForwardedProto = context.Request.Headers["X-Forwarded-Proto"].ToString(),
    rawForwardedHost = context.Request.Headers["X-Forwarded-Host"].ToString()
}));

app.MapGet("/work", async (int? delayMs, HttpContext context) =>
{
    var boundedDelay = delayMs ?? 1_000;

    if (boundedDelay is < 0 or > 30_000)
    {
        return Results.BadRequest(new
        {
            error = "delayMs must be between 0 and 30000."
        });
    }

    var startedAt = Stopwatch.GetTimestamp();
    await Task.Delay(TimeSpan.FromMilliseconds(boundedDelay), context.RequestAborted);

    return Results.Ok(new
    {
        requestedDelayMs = boundedDelay,
        elapsedMs = Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 1),
        traceIdentifier = context.TraceIdentifier
    });
});

app.MapGet("/dependency", async (
    IHttpClientFactory clientFactory,
    IConfiguration configuration,
    HttpContext context) =>
{
    var upstreamText = configuration["UPSTREAM_URL"];

    if (!Uri.TryCreate(upstreamText, UriKind.Absolute, out var upstream) ||
        (upstream.Scheme != Uri.UriSchemeHttp && upstream.Scheme != Uri.UriSchemeHttps))
    {
        return Results.Json(
            new
            {
                category = "configuration",
                error = "UPSTREAM_URL must be an absolute HTTP or HTTPS URL."
            },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var startedAt = Stopwatch.GetTimestamp();
    using var deadline = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
    deadline.CancelAfter(TimeSpan.FromSeconds(3));

    try
    {
        var client = clientFactory.CreateClient("upstream");
        using var request = new HttpRequestMessage(HttpMethod.Get, upstream);
        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            deadline.Token);

        return Results.Json(
            new
            {
                category = "http-response",
                upstreamHost = upstream.Host,
                upstreamStatus = (int)response.StatusCode,
                elapsedMs = Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 1)
            },
            statusCode: response.IsSuccessStatusCode
                ? StatusCodes.Status200OK
                : StatusCodes.Status502BadGateway);
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    {
        return Results.StatusCode(499);
    }
    catch (OperationCanceledException)
    {
        return Results.Json(
            new
            {
                category = "timeout",
                upstreamHost = upstream.Host,
                elapsedMs = Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 1)
            },
            statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (HttpRequestException exception)
    {
        return Results.Json(
            new
            {
                category = "transport",
                upstreamHost = upstream.Host,
                httpRequestError = exception.HttpRequestError.ToString(),
                elapsedMs = Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 1)
            },
            statusCode: StatusCodes.Status502BadGateway);
    }
});

app.MapPost("/lab/stop", (IConfiguration configuration, IHostApplicationLifetime lifetime) =>
{
    if (!string.Equals(
            configuration["LAB_ALLOW_STOP"],
            "true",
            StringComparison.OrdinalIgnoreCase))
    {
        return Results.NotFound();
    }

    _ = Task.Run(async () =>
    {
        await Task.Delay(100);
        lifetime.StopApplication();
    });

    return Results.Accepted(value: new { status = "stopping" });
});

app.Run();
