# Concept Cards — mỗi khái niệm trong 30 giây

> Dùng trang này khi bạn nhớ tên nhưng không nhớ **nó giải quyết vấn đề gì**. Không học thuộc định nghĩa; nhớ **problem + example + failure**.

---

## 1. Transaction

**Một câu:** gom nhiều thay đổi local DB thành một đơn vị commit/rollback.

**Ví dụ:** tạo `Order` và `OrderItem` phải cùng thành công.

**Đừng nhầm:** transaction local không rollback được payment provider ở service khác.

---

## 2. Isolation Level

**Một câu:** quy định transaction của bạn được nhìn thấy thay đổi concurrent đến mức nào.

**Ví dụ:** hai checkout cùng tranh một stock item.

**Đừng nhầm:** isolation mạnh hơn luôn tốt hơn; nó thường đổi concurrency lấy correctness đơn giản hơn.

---

## 3. Index

**Một câu:** cấu trúc dữ liệu giúp DB tìm row nhanh hơn nhưng làm write/storage đắt hơn.

**Ví dụ:** index `(customer_id, created_at)` cho lịch sử order theo customer.

**Đừng nhầm:** query chậm → thêm index bất kỳ.

---

## 4. Execution Plan

**Một câu:** cách database thực sự chọn để chạy query.

**Ví dụ:** `Index Seek → Key Lookup` thay vì `Table Scan`.

**Đừng nhầm:** SQL nhìn ngắn không có nghĩa query rẻ.

---

## 5. Idempotency

**Một câu:** chạy lại cùng logical operation không tạo thêm side effect ngoài ý muốn.

**Ví dụ:** user bấm Checkout hai lần nhưng chỉ có một logical order/payment attempt.

**Đừng nhầm:** response phải byte-for-byte giống nhau.

---

## 6. Authentication

**Một câu:** xác định caller là ai.

**Ví dụ:** access token xác lập principal `customer-123`.

**Đừng nhầm:** authenticated nghĩa là được phép xem mọi order.

---

## 7. Authorization

**Một câu:** caller này có được phép thực hiện action trên resource này không.

**Ví dụ:** `customer-123` chỉ đọc order thuộc chính họ.

**Đừng nhầm:** hide button trên UI là authorization.

---

## 8. OAuth 2.0

**Một câu:** framework để cấp quyền truy cập cho client mà không đưa password user cho client đó.

**Ví dụ:** mobile app lấy access token để gọi API.

**Đừng nhầm:** OAuth tự nó là user authentication; OIDC bổ sung identity layer.

---

## 9. Rate Limiting

**Một câu:** giới hạn lượng request/action caller được phép tạo trong một budget.

**Ví dụ:** 100 checkout attempts/minute/account.

**Đừng nhầm:** rate limit giải quyết mọi overload; DB concurrency vẫn có thể bão hòa.

---

## 10. Cache

**Một câu:** lưu bản copy gần hơn/nhanh hơn để đổi freshness lấy latency và giảm source load.

**Ví dụ:** product detail cache 30 giây.

**Đừng nhầm:** cache là source of truth.

---

## 11. CDN

**Một câu:** cache/serve content từ edge gần user.

**Ví dụ:** ảnh product và static JS/CSS.

**Đừng nhầm:** CDN tự động phù hợp cho mọi response personalized/dynamic.

---

## 12. Load Balancer

**Một câu:** phân phối traffic giữa nhiều healthy instances.

**Ví dụ:** 10 API instances sau một public endpoint.

**Đừng nhầm:** thêm API replicas sẽ tự scale database.

---

## 13. Retry

**Một câu:** thử lại transient operation sau failure có khả năng phục hồi.

**Ví dụ:** retry GET catalog sau connection reset.

**Đừng nhầm:** retry mọi POST/payment một cách mù quáng.

---

## 14. Exponential Backoff + Jitter

**Một câu:** tăng delay giữa retry và thêm randomness để nhiều client không retry cùng lúc.

**Ví dụ:** 100ms → 200ms → 400ms + random jitter.

**Đừng nhầm:** backoff làm unsafe operation trở thành safe.

---

## 15. Circuit Breaker

**Một câu:** khi dependency đang lỗi nhiều, tạm ngừng gọi để fail fast và cho dependency thời gian phục hồi.

**Ví dụ:** payment status provider 80% timeout trong 30s.

**Đừng nhầm:** circuit breaker thay timeout/retry.

---

## 16. Queue

**Một câu:** tách thời điểm producer tạo work khỏi thời điểm consumer xử lý work.

**Ví dụ:** order created → enqueue email notification.

**Đừng nhầm:** queue = infinite capacity.

---

## 17. Backpressure

**Một câu:** phản ứng khi tốc độ work đi vào lớn hơn tốc độ xử lý.

**Ví dụ:** queue tăng 2,000 messages/s → giảm admission, scale consumers hoặc degrade feature.

**Đừng nhầm:** chỉ nhìn CPU mà bỏ queue age/lag.

---

## 18. At-least-once Delivery

**Một câu:** message có thể được giao lại; consumer phải chịu được duplicate.

**Ví dụ:** consumer xử lý xong rồi crash trước ACK.

**Đừng nhầm:** broker bug khi duplicate xuất hiện.

---

## 19. Outbox

**Một câu:** ghi business state và message-to-publish trong cùng local DB transaction.

**Ví dụ:** `Order` + `OrderCreated` outbox row cùng commit.

**Đừng nhầm:** outbox loại bỏ duplicate publish.

---

## 20. Inbox / Dedup

**Một câu:** consumer nhớ message nào đã xử lý để duplicate delivery không lặp side effect.

**Ví dụ:** `message_id` unique trong `inbox_messages`.

**Đừng nhầm:** dedup chỉ dùng in-memory cache nếu correctness phải survive restart.

---

## 21. Eventual Consistency

**Một câu:** các view/service có thể tạm lệch nhau nhưng cuối cùng hội tụ nếu không có failure kéo dài.

**Ví dụ:** order đã `PAID` nhưng analytics view cập nhật sau 2 giây.

**Đừng nhầm:** eventual consistency = dữ liệu sai tùy ý và không có convergence target.

---

## 22. Saga

**Một câu:** quản lý business workflow nhiều local transactions bằng bước tiến và compensation/recovery.

**Ví dụ:** reserve inventory → charge payment → shipment; shipment fail → refund + release.

**Đừng nhầm:** compensation = database rollback.

---

## 23. Reconciliation

**Một câu:** hỏi source of truth để sửa trạng thái khi local system không biết kết quả thật.

**Ví dụ:** payment timeout → query provider bằng stable `PaymentAttemptId`.

**Đừng nhầm:** timeout = failed.

---

## 24. Microservice Boundary

**Một câu:** boundary business capability + owned data + contract + independent lifecycle.

**Ví dụ:** Payment Service sở hữu payment attempts và status contract.

**Đừng nhầm:** mỗi project/container là một microservice.

---

## 25. API Gateway

**Một câu:** front door cho routing và cross-cutting edge policy.

**Ví dụ:** TLS, auth token validation, rate limiting, routing.

**Đừng nhầm:** đặt discount/order workflow vào gateway.

---

## 26. Readiness

**Một câu:** instance có nên nhận traffic ngay bây giờ không.

**Ví dụ:** process chạy nhưng dependency/config chưa sẵn sàng → readiness fail.

**Đừng nhầm:** process alive = ready.

---

## 27. Liveness

**Một câu:** process có đang ở trạng thái cần restart không.

**Ví dụ:** deadlock/hung runtime làm health loop không progress.

**Đừng nhầm:** dependency DB down là lý do restart tất cả Pods.

---

## 28. SLI / SLO

**Một câu:** SLI là metric đo behavior; SLO là target mong muốn của metric đó.

**Ví dụ:** SLI = successful checkout ratio; SLO = 99.95%/month.

**Đừng nhầm:** uptime của một VM = reliability của user journey.

---

## 29. RTO / RPO

**Một câu:** RTO = mất bao lâu để phục hồi; RPO = chấp nhận mất bao nhiêu dữ liệu theo thời gian.

**Ví dụ:** RTO 30 phút, RPO 5 phút.

**Đừng nhầm:** có backup nghĩa là đạt RTO/RPO; phải restore-test.

---

## 30. Partition / Shard

**Một câu:** chia data/workload thành nhiều phần theo key để scale hoặc isolate.

**Ví dụ:** orders partition theo `tenant_id` hoặc hash customer id.

**Đừng nhầm:** sharding là bước đầu tiên khi database lớn.

---

## 31. Replication

**Một câu:** giữ nhiều copies của dữ liệu cho read scale/availability/recovery.

**Ví dụ:** primary + read replicas.

**Đừng nhầm:** replica luôn đọc dữ liệu mới nhất; replication lag tồn tại.

---

## 32. RAG

**Một câu:** tìm context liên quan từ dữ liệu ngoài model rồi đưa nó vào generation.

**Ví dụ:** tìm policy/document theo tenant ACL trước khi LLM trả lời.

**Đừng nhầm:** vector database = toàn bộ RAG architecture.

---

## 33. Tool Calling

**Một câu:** model chọn một capability có schema, application thực thi capability đó.

**Ví dụ:** `GetOrderStatus(orderId)`.

**Đừng nhầm:** model được quyền bypass authorization vì tool đã được chọn.

---

## 34. AI Evaluation

**Một câu:** test behavior AI trên dataset/cases trước và sau thay đổi.

**Ví dụ:** prompt mới phải không làm giảm groundedness/tool accuracy trên regression set.

**Đừng nhầm:** một vài manual prompts đẹp = production quality.

---

## 35. AI Coding Agent

**Một câu:** agent dùng repo context + tools để lập kế hoạch, sửa code và tạo executable evidence.

**Ví dụ:** issue → inspect → edit → build/test → diff → PR.

**Đừng nhầm:** code được generate = task hoàn thành.

---

# Cách dùng cards

Khi học một chapter, chỉ cần chọn 3–5 cards liên quan.

Ví dụ Checkout:

```text
Idempotency
Transaction
Retry
Reconciliation
Saga
```

Tự kể một flow dùng cả 5 từ. Nếu kể được bằng ví dụ mà không nhìn tài liệu, bạn đang giữ được mental model.
