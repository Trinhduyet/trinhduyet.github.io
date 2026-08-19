# Azure Foundations — Resource Hierarchy, Landing Zones và Governance Boundaries

> [← Cloud & Azure](README.md)

<div class="lesson-meta">
  <span><strong>Scope</strong>&nbsp;Tenant → Management Group → Subscription → Resource Group</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Goal</strong>&nbsp;biết đặt workload vào đúng ownership / blast-radius boundary</span>
</div>

## Câu hỏi chính

Trước khi tạo App Service, AKS hay Azure SQL, architect phải trả lời:

```text
Who owns this workload?
Who pays for it?
Who can administer it?
Which policies must be inherited?
What is the blast radius if someone misconfigures it?
How do dev / staging / prod separate?
```

Nếu câu trả lời chỉ là “tạo một Resource Group mới” thì governance model còn quá nông.

## 1. Resource hierarchy theo mental model

```text
Microsoft Entra tenant
        ↓ identity directory / trust context
Management Groups
        ↓ governance inheritance
Subscriptions
        ↓ billing + quota + deployment boundary
Resource Groups
        ↓ lifecycle / deployment grouping
Resources
        ↓ App Service / SQL / VNet / Key Vault / ...
```

### Microsoft Entra tenant

Tenant là identity context chứa users, groups, service principals và managed identities liên quan đến tổ chức. Đừng hiểu tenant đơn giản là “folder trên cùng của Azure resources”. Nó là **identity/control context** có quan hệ với Azure management hierarchy nhưng không đồng nghĩa mọi resource governance rule đều nằm trong tenant object.

### Management Group

Management Group giúp tổ chức nhiều subscriptions theo hierarchy và áp governance inheritance như Azure Policy/RBAC ở cấp cao hơn.

Ví dụ:

```text
Tenant Root
├── Platform
│   ├── Connectivity Subscription
│   ├── Identity Subscription
│   └── Management Subscription
└── Landing Zones
    ├── Corp
    │   ├── Payments-Prod
    │   └── ERP-Prod
    └── Online
        ├── Commerce-Prod
        └── Public-API-Prod
```

Tên cụ thể không phải chuẩn bắt buộc. Điều quan trọng là hierarchy phản ánh **governance intent**.

### Subscription

Subscription thường là một boundary mạnh hơn Resource Group vì nó liên quan tới:

- billing/cost attribution;
- quotas và limits;
- RBAC scope;
- Azure Policy scope;
- blast radius của một số operational actions;
- deployment/ownership separation.

Architect không nên nhét toàn enterprise vào một subscription chỉ vì “resource group đã đủ để tách”.

### Resource Group

Resource Group nên gom resources có lifecycle/ownership/deployment relationship hợp lý.

Không nên dùng Resource Group như database folder tùy hứng:

```text
rg-all-databases
rg-all-apps
rg-all-storage
```

nếu các resource thuộc các product khác nhau, team khác nhau và lifecycle khác nhau.

Một cách dễ reasoning hơn:

```text
rg-checkout-prod-app
rg-checkout-prod-data
rg-checkout-prod-observability
```

hoặc ít nhóm hơn nếu workload nhỏ. Mục tiêu là **operational clarity**, không phải đạt một template tuyệt đối.

## 2. Landing Zone là gì?

Landing Zone là tập hợp design decisions và guardrails để workload có một “nơi hạ cánh” an toàn trên Azure.

Mental model:

```text
Landing Zone
= identity model
+ subscription placement
+ network topology
+ policy guardrails
+ logging / monitoring baseline
+ security baseline
+ resource naming / tagging
+ cost ownership
+ deployment automation
```

Không phải:

```text
Landing Zone = một VNet lớn
```

### Platform Landing Zone vs Application Landing Zone

Một enterprise thường cần phân biệt:

```text
Platform capabilities
- shared connectivity
- DNS
- identity integration
- security/monitoring baseline

Application workload
- checkout
- notification
- investor portal
- internal ERP
```

Nếu platform team và product team có ownership khác nhau, ranh giới này giúp tránh việc product team tự quản shared network/firewall/DNS một cách tùy hứng.

## 3. RBAC vs Azure Policy

Đây là hai khái niệm rất hay bị trộn.

### RBAC — ai được làm gì?

```text
Principal
+ Role
+ Scope
= allowed management/data action
```

Ví dụ:

```text
Group: checkout-prod-operators
Role: Reader
Scope: checkout-prod subscription
```

hoặc Managed Identity được quyền đọc secret cụ thể / truy cập storage theo least privilege.

### Azure Policy — resource/config có được phép tồn tại như vậy không?

Policy dùng để audit/deny/modify/deployIfNotExists theo policy definition/assignment phù hợp.

Ví dụ governance intent:

```text
- production storage không public access
- resources phải có cost-center tag
- chỉ deploy ở approved regions
- diagnostic settings phải bật theo baseline
```

Mental model:

```text
RBAC  = actor permission
Policy = resource compliance / guardrail
```

## 4. Naming và tagging — đừng biến thành “cosmetic standard”

Tag chỉ có giá trị khi phục vụ câu hỏi vận hành:

```text
owner
application
environment
cost-center
data-classification
criticality
```

Ví dụ:

```text
application = checkout
owner       = commerce-platform
cost-center = CC-4310
criticality = tier-1
environment = prod
```

Tag không nên là source of truth duy nhất cho security boundary. Nhưng chúng rất hữu ích cho cost allocation, inventory và automation.

## 5. Environment isolation

Có nhiều mức tách:

```text
same resource, different config        ← yếu
same resource group                    ← vẫn share nhiều blast radius
same subscription                      ← tách RG nhưng share quota/governance scope
separate subscription                  ← mạnh hơn
separate tenant                        ← cực mạnh, cost/complexity cao
```

Không có luật “prod luôn phải tenant riêng”. Quyết định phải dựa trên:

- compliance;
- blast radius;
- organizational trust;
- identity federation;
- operating model;
- cost/complexity.

## 6. Example — Checkout platform

Giả sử có:

```text
checkout-dev
checkout-staging
checkout-prod
```

Một production-aware setup có thể là:

```text
Management Group: Online

Subscription: commerce-nonprod
├── rg-checkout-dev
└── rg-checkout-staging

Subscription: commerce-prod
├── rg-checkout-prod-app
├── rg-checkout-prod-data
└── rg-checkout-prod-ops
```

Tại `Online` management group:

```text
Policy:
- approved regions
- require tags
- diagnostic baseline
```

Tại `commerce-prod` subscription:

```text
Policy:
- deny public DB endpoint where prohibited
- stronger SKU/security baseline
- tighter RBAC
```

## 7. Failure modes governance hay bị bỏ quên

### Case A — Owner có quyền Contributor toàn subscription

Rủi ro:

```text
one accidental script
→ deletes/mutates unrelated product resources
```

Giải pháp không phải chỉ “cẩn thận hơn”, mà phải review scope/RBAC + deployment process.

### Case B — Shared subscription quota bị product khác consume hết

```text
Product A scale burst
→ consumes regional/core quota
→ Product B cannot scale/deploy
```

Quota cũng là shared failure surface.

### Case C — Policy bật quá gấp ở parent scope

```text
new deny policy
→ inherited by many subscriptions
→ deployment pipeline across org breaks
```

Governance cũng cần rollout/canary/change management.

### Case D — delete Resource Group như cleanup

Nếu grouping không phản ánh lifecycle, cleanup có thể xóa cả resource đang được workload khác dùng chung.

## 8. IaC requirement

Landing zone/workload baseline nên được quản lý bằng Infrastructure as Code khi scale/team count tăng.

Điều cần version-control:

```text
subscription/bootstrap assumptions
policy assignments
RBAC assignments
network baseline
resource declarations
monitoring baseline
```

Nhưng IaC không tự đảm bảo architecture đúng. Nó chỉ làm **architecture decision reproducible**.

## 9. Architect checklist

- [ ] Tenant/identity ownership rõ.
- [ ] Management-group hierarchy có business/governance reason.
- [ ] Subscription boundary phản ánh ownership/cost/blast radius.
- [ ] Dev/staging/prod isolation có lý do rõ.
- [ ] RBAC dùng least privilege, không default Owner/Contributor rộng.
- [ ] Policy assignments có rollout/test plan.
- [ ] Resource Group phản ánh lifecycle hợp lý.
- [ ] Tags phục vụ cost/owner/inventory.
- [ ] Quota được coi là capacity constraint.
- [ ] IaC + review + rollback path tồn tại.

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

Azure architecture bắt đầu ở **governance boundary** trước khi bắt đầu ở compute service. Một workload có code tốt nhưng nằm trong subscription/RBAC/network model tệ vẫn là một production architecture tệ.
</div>

## Tiếp theo

→ [Azure Identity, Networking & Zero Trust](azure-identity-networking-and-zero-trust.md)
