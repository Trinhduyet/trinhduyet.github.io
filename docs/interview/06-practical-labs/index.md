# Practical Labs — không chỉ đọc câu trả lời

<div class="lesson-meta"><span>Executable evidence</span><span>Failures & recovery</span><span>Không dùng dữ liệu nội bộ</span></div>

## Lab bắt đầu ngay

<div class="course-grid" markdown>

[**SQL Query Optimization** — script T-SQL đầy đủ tạo synthetic data, so index, keyset và execution plan.](sql-performance-lab.md){ .course-card }

[**.NET Runtime & Async** — chạy ví dụ trong module thực hành đã có trên website.](../../03-dotnet/README.md){ .course-card }

[**Terraform + Azure** — runnable lab, format/validate/test theo pipeline website.](../../13-devops-iac/terraform-on-azure-production.md){ .course-card }

[**Kubernetes Debugging** — Deployment/Service/probes, wrong selector và recovery.](../../15-kubernetes/kubectl-debugging-and-ckad-practice.md){ .course-card }

</div>

## Các bài guided practice chưa có code chạy sẵn

| Bài | Làm gì? | Đầu ra cần có |
| --- | --- | --- |
| HTTP cancellation | ASP.NET Core + fake slow dependency | Tracing, status, timeout contract |
| Idempotent consumer | Message broker + SQL unique operation | Crash-after-commit test |
| Deployment readiness | New revision fail startup | CI fail và rollback evidence |
| Architecture review | Design 10k notifications | Diagram, assumptions, failure matrix, ADR |

**Guided ≠ Runnable.** Chỉ đánh dấu hoàn thành khi người học có logs, test, plan hoặc trace thực tế. Không chạy lệnh deploy/SQL DDL trên môi trường production.

## Báo cáo sau mỗi lab

```text
Question / target behavior:
Environment (versions):
Commands or code:
Expected:
Observed:
Failure injected:
Recovery:
Trade-off / next action:
```

**Luyện trình bày:** [Mock Interview](../05-interview-preparation/mock-interview.md).
