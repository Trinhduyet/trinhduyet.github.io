# Events, gRPC, Webhooks và Contract Interoperability

> [← Module 06](README.md) · [API Styles & Realtime](api-styles-gateway-and-realtime.md) · [References](references.md)

## Hiểu trong 5 phút

Ba interaction styles khác nhau:

```text
Synchronous API / gRPC
caller waits for response

Webhook
producer calls consumer HTTP endpoint when event happens

Message/Event Bus
producer publishes; broker mediates delivery
```

Không chọn bằng trend. Chọn theo:

```text
latency
coupling
delivery durability
consumer ownership
network reachability
retry/replay needs
ordering
schema evolution
```

---

# 1. Webhook là gì?

Webhook là **server-to-server HTTP callback**.

Example:

```text
Payment Provider
      ↓ HTTP POST
Merchant Webhook Endpoint
```

Payload:

```http
POST /webhooks/payments HTTP/1.1
Content-Type: application/json
Webhook-Id: whd_123
Webhook-Timestamp: 1786586400
Webhook-Signature: v1=...
```

```json
{
  "type": "payment.succeeded",
  "data": {
    "paymentId": "pay-123",
    "orderId": "ord-7"
  }
}
```

---

# 2. Webhook delivery is usually at-least-once-ish in practice

Producer có thể retry khi:

```text
connection failed
consumer timeout
non-2xx response
producer uncertain whether response was received
```

Consumer phải assume duplicate:

```text
whd_123
whd_123
whd_123
```

Do đó:

```text
delivery ID
+
dedup/inbox
+
idempotent business handling
```

---

# 3. Webhook consumer pattern

Bad:

```text
receive webhook
↓
run 20-second business workflow
↓
call 3 external services
↓
return 200
```

Producer may timeout and retry while first work is still running.

Better:

```text
receive
↓
verify signature/timestamp
↓
atomically dedup + persist/enqueue intent
↓
return 2xx quickly
↓
background worker handles business process
```

This decouples delivery acknowledgement from slow business work.

---

# 4. Webhook signature

Webhook endpoint is public ingress. Do not trust payload just because URL is obscure.

Common design:

```text
shared secret
+
timestamp
+
raw request body
        ↓
HMAC
```

Pseudo sender:

```text
signedPayload = timestamp + "." + rawBody
signature = HMAC-SHA256(secret, signedPayload)
```

Consumer recomputes and constant-time compares.

C# sketch:

```csharp
static bool VerifyWebhook(
    string secret,
    string timestamp,
    ReadOnlySpan<byte> rawBody,
    string providedHex)
{
    byte[] prefix = Encoding.UTF8.GetBytes(timestamp + ".");
    byte[] message = new byte[prefix.Length + rawBody.Length];

    prefix.CopyTo(message, 0);
    rawBody.CopyTo(message.AsSpan(prefix.Length));

    byte[] expected = HMACSHA256.HashData(
        Encoding.UTF8.GetBytes(secret),
        message);

    byte[] provided = Convert.FromHexString(providedHex);

    return expected.Length == provided.Length &&
           CryptographicOperations.FixedTimeEquals(expected, provided);
}
```

Real provider signature formats differ; follow provider official spec exactly.

---

# 5. Replay protection

Valid signature alone may be replayed by attacker who captured request.

Check:

```text
timestamp within acceptable window
+
delivery ID not processed before
```

Example:

```csharp
DateTimeOffset sentAt = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds);

if (DateTimeOffset.UtcNow - sentAt > TimeSpan.FromMinutes(5))
    return Results.Unauthorized();
```

Clock skew policy must be explicit.

---

# 6. Inbox / dedup

SQL shape:

```sql
CREATE TABLE WebhookInbox (
    Provider         nvarchar(50)  NOT NULL,
    DeliveryId       nvarchar(200) NOT NULL,
    EventType        nvarchar(100) NOT NULL,
    ReceivedAt       datetimeoffset NOT NULL,
    ProcessedAt      datetimeoffset NULL,
    PayloadHash      varbinary(32) NULL,
    CONSTRAINT PK_WebhookInbox PRIMARY KEY (Provider, DeliveryId)
);
```

Atomic insert decides ownership.

Bad:

```text
SELECT exists
then INSERT
```

Race can double-process.

Unique key is stronger evidence.

---

# 7. Webhook endpoint example

```csharp
app.MapPost("/webhooks/payment-provider", async (
    HttpRequest request,
    WebhookInbox inbox,
    CancellationToken cancellationToken) =>
{
    string? deliveryId = request.Headers["Webhook-Id"].FirstOrDefault();
    string? signature = request.Headers["Webhook-Signature"].FirstOrDefault();
    string? timestamp = request.Headers["Webhook-Timestamp"].FirstOrDefault();

    if (deliveryId is null || signature is null || timestamp is null)
        return Results.Unauthorized();

    using MemoryStream buffer = new();
    await request.Body.CopyToAsync(buffer, cancellationToken);
    byte[] raw = buffer.ToArray();

    if (!VerifyProviderSignature(timestamp, raw, signature))
        return Results.Unauthorized();

    bool accepted = await inbox.TryAcceptAsync(
        deliveryId,
        raw,
        cancellationToken);

    // duplicate delivery is already acknowledged
    return accepted
        ? Results.Accepted()
        : Results.Ok();
});
```

Business worker processes accepted inbox rows asynchronously.

---

# 8. Webhook retries

Producer retry policy usually needs:

```text
bounded attempts
exponential backoff
jitter
retryable status classification
max retention window
DLQ/manual replay path
```

Consumer should not rely on exact retry schedule unless provider contract guarantees it.

Store provider event ID and event creation timestamp separately from delivery ID if provider exposes both.

---

# 9. Webhook ordering

Two events:

```text
order.updated version 11
order.updated version 12
```

Network may deliver:

```text
12
then 11
```

Consumer must define ordering strategy:

```text
sequence/version check
current-state fetch
commutative operation
per-aggregate ordering
ignore stale event
```

Do not assume HTTP arrival order equals business order.

---

# 10. Event contract envelope

Useful envelope:

```json
{
  "eventId": "evt-123",
  "eventType": "order.confirmed",
  "schemaVersion": 2,
  "occurredAt": "2026-08-13T03:00:00Z",
  "correlationId": "corr-7",
  "aggregateId": "ord-9",
  "aggregateVersion": 12,
  "data": {}
}
```

Why:

```text
eventId → dedup
schemaVersion → evolution
occurredAt → event time
correlationId → tracing
aggregateVersion → ordering/staleness
```

---

# 11. Event is not DB row dump

Bad:

```json
{
  "OrderTable": {
    "InternalRowVersion": "...",
    "DeletedFlag": 0,
    "DbShardId": 4
  }
}
```

Public event should express business fact:

```json
{
  "eventType": "order.confirmed",
  "orderId": "ord-9",
  "confirmedAt": "2026-08-13T03:00:00Z"
}
```

Contract ownership survives persistence refactor.

---

# 12. gRPC recap

`.proto`:

```proto
syntax = "proto3";

package payments.v1;

service Payments {
  rpc GetPayment(GetPaymentRequest) returns (PaymentReply);
  rpc WatchPayment(WatchPaymentRequest) returns (stream PaymentUpdate);
}

message GetPaymentRequest {
  string payment_id = 1;
}

message PaymentReply {
  string payment_id = 1;
  PaymentStatus status = 2;
}

enum PaymentStatus {
  PAYMENT_STATUS_UNSPECIFIED = 0;
  PAYMENT_STATUS_PENDING = 1;
  PAYMENT_STATUS_SUCCEEDED = 2;
  PAYMENT_STATUS_FAILED = 3;
}
```

gRPC supports unary and streaming patterns through strongly defined service methods/messages.

---

# 13. Protobuf evolution rules — critical

Field numbers are wire identity.

Original:

```proto
message PaymentReply {
  string payment_id = 1;
  string status = 2;
}
```

Do not later reuse `2` for unrelated meaning after removal.

Safer:

```proto
message PaymentReply {
  string payment_id = 1;
  reserved 2;
  PaymentStatus status_code = 3;
}
```

Also reserve old field names where appropriate to prevent accidental reuse.

Generated-code compatibility depends on schema evolution discipline.

---

# 14. gRPC deadline and cancellation

Service-to-service RPC without deadlines can hang resources during dependency degradation.

Client should set meaningful deadline/time budget.

Server propagates cancellation when possible.

Mental model:

```text
user/API request deadline
       ↓ remaining budget
service A gRPC
       ↓ remaining budget
service B
```

Do not reset every hop to 30 seconds.

---

# 15. gRPC status vs domain error

gRPC has transport/RPC status model, nhưng domain contract still needs stable business semantics.

Example:

```text
NOT_FOUND
PERMISSION_DENIED
RESOURCE_EXHAUSTED
UNAVAILABLE
DEADLINE_EXCEEDED
```

Do not encode every business outcome as generic INTERNAL.

For rich domain detail, define structured error/details convention compatible with client ecosystem.

---

# 16. REST ↔ gRPC gateway

Possible architecture:

```text
Public Client
   ↓ REST/JSON
API Gateway/BFF
   ↓ gRPC
Internal Services
```

Good when:

```text
public consumers benefit from HTTP/JSON
internal services benefit from typed RPC
```

But transformation layer owns:

```text
status mapping
field mapping
timeout/deadline mapping
error translation
version compatibility
```

That layer is real code with contract tests.

---

# 17. Webhook vs event bus

Webhook:

```text
producer knows consumer URL
cross-organization / internet friendly
HTTP delivery
consumer owns ingress endpoint
```

Event bus:

```text
producer publishes to broker/topic
consumer subscription decoupled from producer endpoint knowledge
replay/retention may be stronger
internal platform ownership
```

For external SaaS integration, webhook often simpler. For internal many-consumer event distribution, broker may fit better.

---

# 18. Webhook vs polling

Polling:

```text
GET /events?since=cursor every 30s
```

Pros:

```text
consumer controls schedule
firewall/simple outbound only
recovery cursor can be explicit
```

Costs:

```text
latency
empty requests
provider load
```

Webhook:

```text
lower event latency
producer pushes immediately
```

Costs:

```text
public callback endpoint
signature/security
retry/dedup
```

Hybrid is common:

```text
webhook for notification
GET API for reconciliation/source of truth
```

This is especially strong when webhook payload may be missed/out-of-order.

---

# 19. Reconciliation pattern

Webhook tells you:

```text
"payment changed"
```

Consumer can fetch authoritative state:

```text
Webhook evt pay-123
      ↓
GET /payments/pay-123
      ↓
authoritative current status
```

Benefits:

```text
handles out-of-order events
reduces payload trust
supports recovery after missed delivery
```

Cost: extra API calls and provider dependency.

---

# 20. Contract interoperability

Each boundary should have an explicit artifact:

```text
REST     → OpenAPI
GraphQL  → GraphQL schema
 gRPC    → .proto
Event    → JSON Schema/Avro/Protobuf/custom schema registry contract
Webhook  → payload schema + signature/retry contract
```

Contract artifact should be version-controlled and testable.

---

# 21. Schema compatibility gate

CI pipeline:

```text
old schema
   ↓ compare
new schema
   ↓
compatible?
  ├─ yes → tests/build
  └─ no  → explicit migration/version approval
```

For event schema, producer and consumers may deploy at different times. Compatibility matrix matters more than same-repo build.

---

# 22. Observability

Webhook metrics:

```text
received_total
signature_failure_total
duplicate_total
processing_lag
processing_failure_total
oldest_pending_age
```

Producer metrics:

```text
delivery_attempts
2xx/4xx/5xx
timeout
retry queue depth
DLQ count
```

gRPC metrics:

```text
method latency
status code counts
deadline exceeded
message size
active streams
```

Correlation ID/trace context should connect cross-protocol boundaries where supported.

---

# 23. Failure experiments

## A — duplicate webhook

Send same delivery ID 10 times concurrently. Verify one business side effect.

## B — signature tampering

Change one byte in raw body after signing. Verify 401/400 according to contract and zero side effect.

## C — replay old signed request

Valid signature but timestamp outside acceptance window. Verify rejected.

## D — out-of-order version

Deliver aggregate version 12 then 11. Verify stale handling.

## E — crash after inbox commit before processing

Restart worker; verify pending row resumes.

## F — gRPC deadline

Make server exceed deadline. Verify cancellation/deadline behavior and no unbounded work leak.

## G — schema compatibility

Attempt incompatible proto/event change; verify automated gate catches it.

---

# 24. Exit criteria

- [ ] design webhook signature + timestamp + replay policy;
- [ ] use delivery ID + atomic dedup;
- [ ] ACK webhook quickly and process async;
- [ ] handle duplicate/out-of-order event;
- [ ] define event envelope and schema version;
- [ ] explain webhook vs polling vs broker;
- [ ] write/read basic `.proto` contract;
- [ ] explain gRPC deadline/streaming trade-offs;
- [ ] preserve Protobuf field-number compatibility;
- [ ] run contract compatibility + failure tests.

## Verification metadata

- Verified: **2026-08-13**.
- Provider-specific webhook signatures/retry schedules must follow each provider's official documentation.
