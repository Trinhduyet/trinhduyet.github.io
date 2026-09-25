# Terraform Team Workflows, Environment Topology & State Recovery

> [← Language/Lifecycle](terraform-language-lifecycle-and-validation.md) · [Terraform on Azure](terraform-on-azure-production.md) · [Module 13](README.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;team workflow · environment topology · state ownership · recovery · CI/CD</span>
</div>

Terraform production problems thường không đến từ HCL syntax. Chúng đến từ **hai người apply cùng lúc, state ownership mơ hồ, environment topology sai, plan artifact không được kiểm soát, manual drift, import/move sai, hoặc recovery không có runbook**.

## 1. State ownership trước code ownership

Một root configuration phải trả lời:

```text
Which resources does this state own?
Who may plan?
Who may apply?
Which identity is used?
Which backend/key stores state?
Which environments/subscriptions are in scope?
```

Nếu không trả lời được, chưa nên apply.

## 2. State boundary

Tránh một state chứa toàn enterprise.

Bad:

```text
one terraform state
→ all subscriptions
→ all apps
→ all databases
→ all networks
```

Blast radius rất lớn.

Một practical split:

```text
platform/connectivity
platform/identity
platform/monitoring

workload-a/dev
workload-a/prod

workload-b/dev
workload-b/prod
```

Split theo:

- ownership;
- lifecycle;
- blast radius;
- permission;
- release cadence.

Không split chỉ để có nhiều folder đẹp.

## 3. Environment strategy

Ba pattern thường gặp:

### Separate roots/directories

```text
envs/dev
envs/staging
envs/prod
```

Ưu:

- dễ thấy differences;
- state/backend explicit;
- CI dễ map.

Nhược:

- duplicate config nếu module composition kém.

### Same root + tfvars

Có thể hợp khi topology gần giống nhau.

Risk:

- apply nhầm vars/backend;
- environment-specific difference bị hidden.

### CLI workspaces

CLI workspaces tạo nhiều state instances cho cùng configuration.

Không nên mặc định coi workspace = strong production isolation.

```text
workspace separation
!=
subscription isolation
!=
credential isolation
!=
RBAC isolation
```

Prod thường cần stronger boundaries.

## 4. Backend key design

Azure Blob backend key phải stable và có convention.

Ví dụ:

```text
platform/connectivity/prod.tfstate
workloads/orders/prod.tfstate
workloads/orders/staging.tfstate
```

Không encode secret.

Không để local developer tùy ý chọn key production.

## 5. Remote state security

State có thể chứa:

- resource IDs;
- topology;
- generated values;
- secrets/sensitive values tùy resource;
- connection material.

Protect:

```text
private storage
RBAC
short-lived identity
encryption
logging
soft-delete/versioning where appropriate
network controls
backup/recovery procedure
```

## 6. Locking và concurrency

State locking giúp ngăn concurrent writers.

Mental model:

```text
runner A acquires lock
runner B waits/fails
runner A updates remote objects + state
runner A releases lock
```

Không bypass lock chỉ để pipeline chạy.

Force-unlock chỉ dùng khi xác minh lock stale và không còn active apply.

## 7. Plan/apply separation

Production workflow:

```text
PR
↓
fmt/validate/test
↓
plan
↓
human/policy review
↓
approved change
↓
apply reviewed commit/configuration
↓
post-apply verification
```

Nếu saved plan được dùng:

```text
plan artifact
= sensitive controlled artifact
```

Phải đảm bảo artifact map đúng commit, backend, provider lock file và environment.

## 8. Stale plan

Plan không phải reservation.

Giữa plan và apply có thể có:

- another apply;
- manual change;
- policy update;
- quota/capacity change;
- provider/API behavior change.

Do đó production system phải có rule khi nào re-plan bắt buộc.

## 9. PR workflow

Một Terraform PR nên show:

```text
configuration diff
lock-file diff
module/provider version diff
plan summary
replacement/destroy signals
policy/test results
risk/recovery note
```

Reviewer đặc biệt chú ý:

```text
-/+ replacement
destroy
public endpoint
RBAC widening
policy exemption
network route
DNS change
database/storage delete
state/backend change
provider major upgrade
```

## 10. CI identity vs runtime identity

Tách:

```text
CI/Terraform identity
→ creates/configures resources

runtime managed identity
→ application accesses dependencies
```

Không cho application runtime dùng Contributor chỉ vì Terraform cần quyền provision.

## 11. OIDC/federation

Prefer short-lived federation:

```text
GitHub Actions
→ OIDC token
→ Entra federated credential
→ Azure access token
→ Terraform
```

thay vì long-lived service-principal secret khi platform hỗ trợ.

Benefits:

- giảm secret rotation burden;
- giảm secret leakage risk;
- claim/repository/environment scope rõ hơn.

Vẫn phải least privilege Azure RBAC.

## 12. Apply permissions

Plan job có thể dùng read + required provider permissions.

Apply job cần write permissions.

Environment approval có thể bảo vệ production.

Không cấp Owner ở subscription chỉ vì dễ.

## 13. Module versioning

Shared module change = API change.

Track:

```text
module source
version/ref
input contract
output contract
migration instructions
provider requirements
```

Không point production module tới mutable branch như `main`.

## 14. Provider upgrades

Provider upgrade có thể tạo plan rất lớn dù business intent nhỏ.

Process:

```text
upgrade isolated PR
→ read release notes
→ update lock file
→ test modules
→ plan all representative roots
→ inspect deprecations/replacements
→ apply low-risk env
→ prod
```

Tách provider upgrade khỏi feature change khi có thể.

## 15. Import existing infrastructure

Import chỉ nối remote object với Terraform address/state.

Nó không tự sinh architecture tốt.

Process:

```text
inventory remote object
↓
write matching configuration
↓
import mapping
↓
plan
↓
drive plan toward no-op
↓
then refactor safely
```

Nếu import xong rồi thấy destroy/create lớn, dừng lại.

## 16. moved blocks

Dùng cho refactor address/module path.

Ví dụ:

```hcl
moved {
  from = azurerm_storage_account.logs
  to   = module.observability.azurerm_storage_account.logs
}
```

Migration cần review như schema migration.

## 17. removed blocks và decommission

Removing code không phải lúc nào cũng muốn destroy remote object.

Decommission phải explicit:

```text
destroy intentionally
or
remove from state/ownership intentionally
```

Document handoff nếu resource chuyển sang system khác quản lý.

## 18. State commands

Các command như:

```text
terraform state list
terraform state show
terraform state mv
terraform state rm
terraform import
```

là surgical tools.

Production rule:

```text
backup state
→ understand remote object
→ run focused operation
→ plan immediately
→ verify no unexpected destroy/create
```

Không thao tác state trong hoảng loạn.

## 19. Drift workflow

Drift có thể đến từ:

- portal/manual change;
- policy remediation;
- another IaC controller;
- service-managed mutation;
- incident hotfix.

Khi phát hiện:

```text
observe drift
↓
identify owner
↓
decide source of truth
↓
import configuration change OR revert remote change
↓
plan
↓
review/apply
```

Không auto-revert mọi drift nếu chưa hiểu owner.

## 20. Incident hotfix

Có lúc production incident buộc manual Azure change.

Sau incident:

```text
manual recovery
↓
record exact change
↓
restore service
↓
reconcile Terraform immediately
↓
plan to no unexplained drift
↓
postmortem
```

Manual emergency action không được trở thành permanent invisible config.

## 21. Partial apply

Provider API failure có thể làm:

```text
resource A created
resource B failed
state partially updated
```

Đừng assume apply là transaction.

Recovery:

1. inspect Terraform output/error;
2. inspect state;
3. inspect remote Azure;
4. re-run plan;
5. decide retry/import/manual reconciliation;
6. avoid random state deletion.

## 22. State lost/corrupted

Runbook phải tồn tại trước incident.

Typical path:

```text
stop writers
↓
restore backend version/backup if possible
↓
compare remote resources
↓
validate state lineage/serial
↓
plan in read-only/recovery context
↓
import/reconcile only if necessary
```

Không giải quyết bằng `rm terraform.tfstate`.

## 23. Remote state outputs

Cross-stack dependency qua remote-state outputs tạo coupling.

Use only stable contract outputs.

Bad:

```text
consumer reads dozens of implementation details
```

Better:

```text
network stack exports subnet IDs
identity stack exports principal IDs
workload consumes only those contracts
```

Ở scale lớn, service catalog/config store/API có thể phù hợp hơn state-to-state coupling.

## 24. State backend bootstrap problem

Backend storage bản thân là infrastructure.

Options:

- bootstrap stack;
- platform provisioning process;
- pre-created governed storage.

Không tạo circular dependency:

```text
Terraform needs backend
but backend can only exist after same Terraform starts
```

## 25. Multi-subscription Azure topology

Một enterprise layout có thể:

```text
Management Group
├─ Platform
│  ├─ Connectivity subscription
│  ├─ Identity subscription
│  └─ Management subscription
└─ Landing Zones
   ├─ Orders Prod subscription
   ├─ Orders NonProd subscription
   └─ Payments Prod subscription
```

Terraform roots/state/identity nên phản ánh ownership và blast radius này.

## 26. Policy and Terraform

Azure Policy có thể:

- deny;
- audit;
- append/modify;
- deployIfNotExists;
- remediate.

Terraform plan có thể không thể hiện đầy đủ mutation của policy remediation.

Do đó failure:

```text
Terraform config says X
Azure Policy mutates/enforces Y
next plan sees drift
```

Giải quyết bằng ownership rõ và align module defaults với platform policy.

## 27. Cost review in plan

Terraform plan không phải cost estimate hoàn chỉnh.

Review thêm:

- SKU/tier;
- zone redundancy;
- data transfer;
- retention;
- replicas;
- private endpoints;
- logs;
- backup;
- NAT/firewall;
- reservation/savings plan assumptions.

## 28. Recovery evidence

Một mature repo nên có artifacts:

```text
state ownership map
backend convention
OIDC identity map
plan/apply workflow
import/move runbook
provider upgrade runbook
state recovery runbook
drift drill
destroy/decommission checklist
```

## 29. Failure drills

### A — stale lock

Simulate interrupted operation in disposable backend and document safe investigation.

### B — manual Azure drift

Change a harmless tag out-of-band, run plan and reconcile.

### C — address refactor

Move resource into module using `moved`.

### D — partial apply

Use a failing dependency in disposable environment and inspect next plan.

### E — state recovery tabletop

Assume backend object deleted; walk restore/import decision without destructive commands.

## 30. Exit criteria

- [ ] define state boundary from ownership/blast radius;
- [ ] choose environment topology intentionally;
- [ ] explain workspaces vs stronger isolation;
- [ ] design secure Azure Blob backend;
- [ ] explain locking and force-unlock risk;
- [ ] design PR plan/apply workflow;
- [ ] use OIDC with least privilege;
- [ ] plan provider/module upgrades;
- [ ] import existing Azure resources safely;
- [ ] use moved/state commands with recovery discipline;
- [ ] reconcile manual drift/hotfix;
- [ ] recover from partial apply/state incident;
- [ ] map Terraform roots to Azure subscriptions/platform ownership.

## Official sources

- Terraform state: https://developer.hashicorp.com/terraform/language/state
- State locking: https://developer.hashicorp.com/terraform/language/state/locking
- Backends: https://developer.hashicorp.com/terraform/language/backend
- AzureRM backend: https://developer.hashicorp.com/terraform/language/backend/azurerm
- Import: https://developer.hashicorp.com/terraform/language/import
- moved block: https://developer.hashicorp.com/terraform/language/block/moved
- Dependency lock file: https://developer.hashicorp.com/terraform/language/files/dependency-lock

## Verification metadata

- Verified: 2026-09-25.
- Focus is team/state correctness rather than a specific CI vendor.
