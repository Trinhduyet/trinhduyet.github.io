# Azure Identity, Networking & Zero Trust

> [← Azure Foundations](azure-foundations-resource-hierarchy-and-landing-zones.md) · [Cloud & Azure](README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;Entra · Managed Identity · VNet · Private Link · Edge</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Rule</strong>&nbsp;network private != authorized</span>
</div>

## Mental model

Một request production không chỉ đi qua “Internet → API”. Nó đi qua nhiều **trust boundaries**:

```text
User / Client
    ↓
DNS / Global Edge
    ↓
WAF / DDoS controls
    ↓
API / Ingress boundary
    ↓
Application identity
    ↓
Private service/data endpoint
    ↓
Authorization at target resource
```

Hai câu hỏi phải tách biệt:

```text
Can traffic reach this endpoint?
            !=
Is this caller allowed to do this action?
```

## 1. Microsoft Entra ID

Microsoft Entra ID là identity provider/control plane quan trọng cho Azure workloads.

Các actor thường gặp:

```text
Human user
Group
Application / service principal
Managed identity
External/federated identity
```

Ở mức architect, đừng chỉ hỏi “đã bật authentication chưa?”. Hỏi:

```text
Who is the principal?
How is it authenticated?
Which token/audience is valid?
Where is authorization enforced?
How is privilege reviewed/revoked?
```

## 2. Managed Identity — bỏ secret tĩnh khi có thể

Bad:

```text
appsettings.json
  SQL_PASSWORD=...
  SERVICE_BUS_KEY=...
  STORAGE_KEY=...
```

Better mental model:

```text
.NET workload
   ↓ gets workload identity
Managed Identity
   ↓ token
Azure resource
   ↓ checks RBAC / data permission
Allow / Deny
```

Managed Identity không có nghĩa “app tự động có quyền”. Nó chỉ giải quyết identity credential lifecycle; **RBAC/data-plane authorization vẫn phải cấu hình đúng**.

### Example

Checkout API cần đọc Blob receipt template.

```text
Checkout API Managed Identity
        ↓
Storage Blob Data Reader
        ↓ scope
specific storage account/container as appropriate
```

Không cấp `Owner` hoặc storage account key chỉ vì cấu hình nhanh.

## 3. Key Vault

Key Vault phù hợp để quản lý secrets, keys, certificates cần thiết, nhưng mục tiêu tốt hơn vẫn là **giảm số secret phải tồn tại**.

```text
Can use Managed Identity directly?
   ↓ yes
Don't create secret.

Need external provider secret/certificate?
   ↓
Key Vault + least privilege + rotation + audit
```

Failure cần nghĩ tới:

- Key Vault throttling/outage;
- expired certificate;
- rotation làm app không reload;
- startup phụ thuộc secret store khiến whole fleet restart fail;
- broad access policy/RBAC.

## 4. VNet, subnet và network boundary

VNet là logical network boundary trên Azure. Subnet giúp partition address space và áp routing/security controls phù hợp.

Không thiết kế network theo sơ đồ đẹp; thiết kế theo traffic matrix:

| From | To | Protocol/port | Public/private | Reason |
|---|---|---|---|---|
| Internet | Front Door | HTTPS | public | customer ingress |
| Front Door | API ingress | HTTPS | controlled | origin traffic |
| Checkout API | Azure SQL | TDS | private preferred | source of truth |
| Worker | Service Bus | AMQP/HTTPS | private where required | messaging |
| API | payment provider | HTTPS | outbound public/controlled | external dependency |

Traffic matrix giúp review firewall/NSG/routing rõ hơn “VNet diagram”.

## 5. Private Endpoint / Private Link

Private Endpoint đưa một supported PaaS endpoint vào private IP context của VNet.

Mental model:

```text
App subnet
   ↓ private DNS resolution
Private Endpoint IP
   ↓ Private Link
PaaS resource
```

Nhưng nhớ:

```text
Private Endpoint
!=
Authorization
```

Một caller reach được private IP nhưng thiếu token/permission vẫn phải bị deny.

### DNS là phần của architecture

Private endpoint rất hay fail vì DNS:

```text
private endpoint created
→ app still resolves public hostname incorrectly
→ connection timeout / unexpected public route
```

Do đó design phải ghi rõ:

```text
DNS zone
link to VNet
resolution path
on-prem forwarding if hybrid
recovery/runbook
```

## 6. NSG và firewall — defense in depth

Network Security Group kiểm soát network traffic theo rule ở subnet/NIC scope phù hợp. Azure Firewall hoặc firewall/NVA khác có thể dùng cho centralized network control theo topology.

Không dùng NSG như authorization engine cho business action.

```text
NSG: can packet flow?
AuthZ: can principal perform operation?
```

## 7. Global edge, regional ingress và API boundary

Các service thường gặp:

### Azure Front Door

Dùng khi cần global HTTP(S) edge/routing, acceleration, health-based routing và WAF capabilities phù hợp.

### Application Gateway

Regional Layer-7 load balancing/gateway trong Azure region/VNet scenarios, thường đi với WAF khi cần regional application ingress.

### API Management

API boundary/policy layer cho API lifecycle, auth/policy, quota/rate limit, transformation, developer exposure và governance.

Không coi ba service này là interchangeable chỉ vì đều “đứng trước API”.

Mental model:

```text
Global traffic problem?    → Front Door class
Regional L7 ingress?       → Application Gateway class
API product/policy problem?→ API Management class
```

Một architecture có thể dùng nhiều lớp khi requirements justify, nhưng đừng stack chúng mặc định.

## 8. Zero Trust mental model

Đừng dùng:

```text
inside VNet = trusted
```

Dùng:

```text
verify identity
verify authorization
minimize network exposure
least privilege
assume breach
observe decisions
```

Ví dụ checkout worker gọi SQL:

```text
Worker has Managed Identity
+ SQL grants only required role
+ SQL public network disabled where appropriate
+ Private Endpoint provides private path
+ logs/audit show access
```

Security đến từ **nhiều lớp**, không phải một checkbox.

## 9. Common failure scenarios

### A. Managed Identity chưa được grant data-plane role

```text
deploy succeeds
health check shallow = green
first business request → 403
```

Test readiness/contract phải chạm dependency ở mức phù hợp hoặc có startup validation riêng, nhưng tránh làm health probe gây load/failure cascade.

### B. Private DNS sai

```text
app resolves SQL/Storage public endpoint
→ route blocked
→ timeout
```

### C. Secret rotation

```text
secret rotated
→ old app instances cache old value
→ partial fleet fails
```

Cần rotation strategy + reload/restart behavior + observability.

### D. WAF false positive

```text
legitimate payload
→ WAF rule blocks
→ user sees 403
```

Security control cũng có failure mode và cần tuning/evidence.

### E. APIM retry non-idempotent operation

Nếu gateway/policy retry một POST có side effect mà backend không có idempotency semantics, có thể tạo duplicate business action.

Network/gateway resilience không được phá correctness.

## 10. Review checklist

- [ ] Human identity và workload identity tách rõ.
- [ ] Managed Identity được ưu tiên thay secret tĩnh.
- [ ] RBAC scope nhỏ nhất hợp lý.
- [ ] Data-plane authorization được review riêng management-plane RBAC.
- [ ] Public endpoints inventory rõ.
- [ ] VNet/subnet dựa trên traffic/ownership, không chỉ naming.
- [ ] Private Endpoint có DNS design và runbook.
- [ ] Edge/gateway layer có requirement rõ.
- [ ] WAF/rate-limit/retry policies không phá business semantics.
- [ ] Audit/telemetry không leak secrets/token.

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

Azure network giảm **reachability**; Microsoft Entra/RBAC/application policy kiểm soát **authority**. Production security cần cả hai, cộng với observability và operational recovery.
</div>

## Tiếp theo

→ [Azure Compute, Data, Messaging & Integration](azure-compute-data-messaging-and-integration.md)
