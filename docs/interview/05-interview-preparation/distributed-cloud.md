# Distributed Systems, Azure & DevOps — câu hỏi có lời giải

<div class="lesson-meta"><span>Messaging · Azure · Reliability</span><span>9 câu</span></div>

## DIST-01. Event Hubs, Service Bus, RabbitMQ khác nhau?

| | Azure Event Hubs | Azure Service Bus | RabbitMQ |
| --- | --- | --- | --- |
| Abstraction | Partitioned event stream | Queue/topic broker | Exchanges, queues, bindings |
| Consumption | Partition offsets/checkpoints | Receive/complete/abandon, DLQ | Consumer ACK/NACK |
| Replay | Trong retention | Re-delivery/deferral theo semantics | Queue redelivery / plugin-specific replay |
| Use case | High-throughput telemetry/market feed | Business commands/events/workflows | Flexible message routing |

Không chọn broker chỉ vì dự án trước dùng; phân tích durability, ordering, volume, consumer groups, operations và cost.

## DIST-02. Checkpoint hoặc ACK có nghĩa exactly-once không?

**Không.** Consumer có thể commit DB rồi chết trước checkpoint/ACK; message sẽ được xử lý lại. Cần unique business operation key, transactional inbox hoặc idempotent write. Broker duplicate detection nếu bật vẫn có retention/window giới hạn và **không** bao phủ external side effect.

**Follow-up:** Nếu email provider nhận request nhưng response timeout, có chắc chưa gửi không? Không; reconciliation và provider contract là yếu tố quyết định.

## DIST-03. Retry và circuit breaker có ngăn outage không?

Retry phù hợp transient failure khi operation **an toàn để lặp** hoặc có idempotency contract. Exponential backoff+jitter giới hạn tổng deadline, concurrency và retry budget. Circuit breaker từ chối nhanh trong outage, không sửa dữ liệu đã thay đổi. Retry storm có thể tăng tải lên dependency đang hỏng.

## DIST-04. Redis cache-aside có consistency không?

Cache-aside có cửa sổ stale và race: reader lấy DB snapshot cũ, writer commit+invalidate, reader mới set lại cache cũ. Dùng generation/version, atomic projection hoặc bounded TTL theo freshness contract; authorization/consent cần fail-closed. **Không** dùng cached wallet balance để quyết định debit.

## DIST-05. Azure App Service, Container Apps hay AKS?

- App Service: thuận lợi web APIs, ít vận hành.
- Container Apps: containers/revisions/event scaling với complexity vừa phải.
- AKS: Kubernetes orchestration + network/RBAC/node lifecycle overhead.

**Senior answer:** đánh giá release strategy, cost, workload, SLO, secret/identity, ingress/egress và năng lực vận hành trước. AKS không phải default cho 3 APIs.

## DIST-06. Managed Identity khác Key Vault?

**Managed Identity** là workload identity để lấy token, tránh hardcoded client secrets khi gọi supported Azure services. **Key Vault** lưu/truy cập secrets/keys/certs theo policies/RBAC. Có Managed Identity vẫn phải cấu hình quyền data-plane đúng; private networking không thay authZ.

## DIST-07. Azure Container Apps deploy thành công nhưng Revision NotReady?

CLI accepted/deployment operation hoàn tất **không** đủ chứng minh app ready. Kiểm tra readiness probe/startup, image, ingress target, auth, dependency, logs, revision traffic, rồi smoke test. CI phải fail rõ khi revision fail/timeout, không báo green dựa mỗi lệnh deploy. Nếu DB schema đã migrate không tương thích, rollback image chưa đủ.

## DIST-08. Kubernetes Pods Running nhưng Service trả 503?

Kiểm tra labels/selectors và EndpointSlices, readiness, app bind/port, service targetPort, ingress/gateway policies, network/TLS. Scale replicas không giải quyết wrong selector. Dùng `kubectl describe`/`logs`/events và kiểm tra routing trước khi sửa.

**Follow-up:** HPA scale pod nhưng nodes full? Requests/scheduling/node autoscaler khác HPA; DB hoặc external quota có thể là bottleneck.

## DIST-09. Terraform state/plan là gì, có được commit plan không?

State lưu mapping resource → infrastructure state, có thể chứa sensitive data. Plan là đề xuất delta ở thời điểm refresh; đánh giá destroy/replace, drift, provider version, locking và authorization. Không coi mọi plan file an toàn để commit vì nội dung có thể chứa credentials/secrets.

**Đọc sâu:** [Azure service selection](../../14-cloud/azure-service-selection-and-workload-architecture.md) · [Kubernetes](../../15-kubernetes/README.md) · [Terraform state](../../13-devops-iac/terraform-state-modules-and-drift.md).
