# Learning Quality Standard

> Mục tiêu của repository không phải có nhiều trang Markdown. Mục tiêu là giúp người học **hiểu → làm → làm hỏng → debug → giải thích trade-off → tạo evidence**.

Tài liệu này là quality bar chung cho mọi module. Khi một chapter mới không đạt toàn bộ tiêu chí P0 ngay lập tức, phải ghi rõ maturity thay vì tạo cảm giác nội dung đã production-ready.

## 1. Hai loại maturity phải tách riêng

Không dùng một chữ `Done` cho cả nội dung và thực hành.

| Maturity | Nghĩa |
|---|---|
| **Reference** | Có vocabulary, mental model và source để tra cứu |
| **Guided** | Có scenario, code/config mẫu và failure reasoning |
| **Deep** | Có production constraints, debugging, security, operations và trade-off |
| **Runnable** | Có artifact/lab trong repo mà người học có thể chạy |
| **Verified Evidence** | Có expected output, test/failure drill và cách kiểm chứng kết quả |

Một module có thể `Deep` nhưng chưa `Runnable`. Điều đó không phải lỗi nếu được ghi trung thực.

## 2. P0/P1 chapter structure

Một chapter quan trọng nên trả lời theo thứ tự:

```text
Problem thật
  ↓
Mental model
  ↓
Minimal implementation / config
  ↓
Expected state / output
  ↓
Failure experiment
  ↓
Debugging path
  ↓
Production constraints
  ↓
Trade-offs / simpler option
  ↓
Evidence / exit criteria
```

Không bắt buộc tất cả heading phải giống hệt nhau; bắt buộc **learning loop** phải tồn tại.

## 3. Problem trước technology

Bad:

```text
Redis là gì?
Kafka là gì?
Kubernetes là gì?
```

Better:

```text
SQL read path saturates ở peak traffic
→ cache có thể giảm read load
→ Redis là một candidate
```

```text
producer rate > consumer capacity
→ cần durable buffer + backpressure semantics
→ queue/broker là candidate
```

```text
nhiều container cần scheduling, rollout, service discovery, policy
→ orchestration pressure xuất hiện
→ Kubernetes có thể justified
```

Technology không được xuất hiện như đáp án mặc định chỉ vì phổ biến.

## 4. Mọi cơ chế phải có failure model

Nếu chapter dạy một mechanism, ít nhất phải chỉ ra failure quan trọng nhất.

| Mechanism | Failure phải hiểu |
|---|---|
| Retry | amplification, duplicate side effect |
| Cache | stale data, stampede, cache outage |
| Queue | backlog, duplicate, poison message |
| Kubernetes Deployment | bad rollout, incompatible migration |
| HPA | metric lag, capacity thiếu, Pending Pods |
| Secret | over-broad access, rotation, accidental exposure |
| Multi-region | stale/forked state, failover complexity |
| RAG | ACL leak, stale/deleted data, bad retrieval |
| Tool calling | authorization bypass, unknown side-effect outcome |

Nếu không có failure model, người học mới chỉ biết happy path.

## 5. Evidence hierarchy

Evidence mạnh dần:

```text
đọc xong
< trả lời câu hỏi
< viết config/code
< test pass
< reproduce failure
< debug bằng telemetry
< recover đúng
< giải thích trade-off bằng measured evidence
```

Repository ưu tiên 4 loại evidence:

1. **Executable** — code/config chạy được.
2. **Observable** — log/metric/trace/status cho thấy mechanism thật sự hoạt động.
3. **Failure** — có cách cố tình phá hệ thống và expected outcome.
4. **Decision** — có ADR/note giải thích khi nào dùng, khi nào không.

## 6. Lab contract

Một runnable lab nên có tối thiểu:

```text
Goal
Prerequisites
Run commands
Expected output/state
Failure injection
Debug commands
Recovery
Cleanup
Evidence to save
```

Nếu chapter chỉ mô tả commands nhưng repo chưa có artifact tương ứng, gọi nó là **guided exercise**, không gọi là runnable lab.

## 7. Production checklist cho technical chapter

P0/P1 topic nên xem xét các dimension sau khi relevant:

- correctness / invariants;
- security / trust boundary;
- performance / capacity;
- availability / failure;
- observability / diagnostics;
- deployment / rollback;
- state / backup / recovery;
- cost drivers;
- compatibility / versioning;
- operational ownership.

Không ép mọi chapter nói đủ 10 mục nếu không liên quan; nhưng không được bỏ qua dimension quan trọng của topic.

## 8. Version-sensitive content

Tách:

```text
Stable mental model
vs
Version-specific syntax / SKU / API / support status
```

Ví dụ:

```text
Kubernetes reconciliation
= stable concept

apiVersion / feature gate / supported minor
= version-sensitive
```

Version-sensitive claim phải:

- ưu tiên official source;
- có verification date khi material;
- không copy quota/pricing dễ stale nếu link/source hiện hành tốt hơn;
- pin exact version/digest trong executable lab khi reproducibility cần thiết.

→ [Technology Baseline](technology-baseline.md) · [Source Policy](source-policy.md)

## 9. README contract của module

Mỗi module overview nên giúp người học trả lời trong vài phút:

```text
1. Module giải quyết vấn đề gì?
2. Prerequisite thật sự là gì?
3. 5–10 concepts tối thiểu phải hiểu?
4. Learning path theo thứ tự nào?
5. Có scenario xuyên suốt nào?
6. Failure nào bắt buộc phải debug?
7. Repo có runnable lab hay chỉ guided exercise?
8. Exit criteria là gì?
9. Học tiếp đâu?
10. Source nào canonical?
```

Generic boilerplate không thay được câu trả lời cụ thể cho module.

## 10. Diagram standard

Diagram phải trả lời một câu hỏi cụ thể, ví dụ:

```text
request đi qua đâu?
state nằm ở đâu?
component nào own data?
control flow vs data flow thế nào?
failure boundary ở đâu?
```

Không thêm diagram chỉ để trang trông “architecture-like”. Với mental model đơn giản, wiretext thường rõ hơn một SVG nhiều box.

## 11. Definition of Done cho content

### Reference-ready

- vocabulary đúng;
- links canonical;
- không có claim version-sensitive mơ hồ.

### Guided-ready

- có problem/scenario;
- có example;
- có failure reasoning;
- có exit criteria.

### Deep-ready

- có production constraints;
- có debugging/operations;
- có trade-offs và simpler alternative;
- có security/cost/capacity khi relevant.

### Runnable-ready

- artifact tồn tại dưới `labs/` hoặc executable project khác;
- commands repo-relative;
- expected result có thể verify;
- cleanup/recovery rõ.

## 12. Repository-wide rule

Không nâng maturity bằng số trang.

```text
More content
!=
More learning
```

Ưu tiên:

```text
fewer concepts
+ stronger mental model
+ runnable scenario
+ failure evidence
+ clear decision boundary
```

## 13. Review cadence

Khi review repository:

1. audit navigation và prerequisites;
2. audit content maturity vs runnable evidence;
3. audit stale statuses/version baselines;
4. audit weakest module overviews;
5. audit missing production dimensions;
6. chạy MkDocs strict + learning quality audit;
7. tạo backlog theo impact, không theo số file.

## Verification metadata

- Standard introduced: 2026-08-28.
- Applies to: all P0/P1 learning content.
- Enforcement: `scripts/audit-learning-quality.py` reports repository-level gaps; strict enforcement is intentionally phased to avoid pretending legacy debt is already solved.
