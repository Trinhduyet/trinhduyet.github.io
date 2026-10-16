# Practical Labs — học bằng lỗi có kiểm soát

**Đề bài thực hành** — không khẳng định đã chạy trong environment của người học.

## Lab A — SQL performance

Dựng bảng synthetic 100k+ rows, query theo tenant/time range, đo actual plan và `SET STATISTICS IO, TIME ON`, thêm composite index rồi so sánh. Test keyset pagination khi có duplicate timestamp.

## Lab B — ASP.NET Core resilience

Minimal API + fake slow dependency; truyền CancellationToken, kiểm tra timeout và retry giới hạn. Load test bằng k6/JMeter; thu p95/p99, RPS và 5xx.

## Lab C — Messaging failure

Queue + consumer + local SQL; cố tình crash sau DB commit trước ACK, đảm bảo duplicate delivery không tạo duplicate business action.

## Lab D — CI/CD and rollback

Build Docker image, release staging, cố tình readiness fail, xác minh deploy job báo fail. Luyện expand-contract migration với 2 app revisions.

## Lab E — Kubernetes debugging

Deployment → ReplicaSet → Pods; thay sai selector Service để tạo 503, quan sát Ready endpoints rồi recover.

## Exit criteria

Mỗi lab cần: lệnh chạy, expected output, negative test, metrics/capture và giải thích trade-off. Không kết luận “pass” khi chưa thực sự chạy.

[Existing runnable labs](../../00-roadmap/practical-mini-labs.md) · [Terraform delivery](../../13-devops-iac/devops-kubernetes-production-delivery.md).
