# Investor Relations — case study công khai

Đây là **một domain tham khảo** để luyện API, dữ liệu thị trường và bất đồng bộ; không phải bản sao kiến trúc nội bộ của bất kỳ công ty nào.

## Bài toán giả định

Một công ty niêm yết cần hiển thị giá/historical chart, financial calendar, company announcements và thông báo đăng ký.

```text
Market feed → stream ingestion → read model → public API
Announcement publish → durable event → subscriber matching → notification worker
```

## Câu hỏi kỹ thuật

1. Instrument ID khác company ID thế nào khi công ty có nhiều niêm yết?
2. Delayed quote cần trường `asOf` và data freshness policy ra sao?
3. Truy vấn historical prices trên bảng rất lớn cần composite index và giới hạn khoảng thời gian như thế nào?
4. Subscriber unsubscribe giữa lúc job fan-out cần điều kiện kiểm tra gì trước khi gửi?
5. Khi email provider timeout, `accepted` có đồng nghĩa `delivered` không?

**Góc nhìn Architect:** chọn read model và event retention theo SLA; tránh dùng quote cache làm nguồn sự thật cho giao dịch tiền.

Xem [data & messaging](../02-data-and-distributed-systems/index.md) và [system design](../../24-system-design/README.md).
