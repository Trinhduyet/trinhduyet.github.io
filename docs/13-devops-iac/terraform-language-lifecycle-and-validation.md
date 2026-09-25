# Terraform Language, Lifecycle, Validation & Provider Design

> [← Module 13](README.md) · [State, Modules & Drift](terraform-state-modules-and-drift.md) · [Team Workflow & Recovery →](terraform-team-workflows-and-state-recovery.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;HCL · expressions · lifecycle · validation · providers · sensitive/ephemeral data</span>
  <span><strong>Baseline</strong>&nbsp;Terraform 1.16.x</span>
</div>

Terraform production không chỉ là biết viết `resource`. Bạn phải hiểu **configuration language tạo graph như thế nào, identity của resource thay đổi ra sao, lifecycle rule nào có thể phá safety, và validation nằm ở phase nào**.

## 1. Mental model

```text
Inputs
  ↓
HCL expressions
  ↓
Resource/module instances
  ↓
Dependency graph
  ↓
Preconditions
  ↓
Plan
  ↓
Apply
  ↓
Postconditions
  ↓
Checks
```

Không phải mọi validation chạy cùng một lúc.

## 2. Expressions và collection transformations

Những construct cần biết:

```text
conditional expression
for expression
splat
merge
lookup
try / can
toset / tomap
flatten
zipmap
dynamic block
```

Ví dụ derive map có key ổn định:

```hcl
locals {
  subnets = {
    for subnet in var.subnets :
    subnet.name => subnet
  }
}
```

Sau đó:

```hcl
resource "azurerm_subnet" "this" {
  for_each = local.subnets

  name                 = each.key
  address_prefixes     = each.value.address_prefixes
  virtual_network_name = azurerm_virtual_network.this.name
  resource_group_name  = azurerm_resource_group.this.name
}
```

Rule:

```text
business/stable identity
→ semantic key
→ for_each
```

Tránh dùng list index nếu reorder có thể làm identity đổi ngoài ý muốn.

## 3. Unknown values

Terraform có value chưa biết ở plan time.

Ví dụ:

```text
resource ID generated after apply
private IP allocated by provider
computed hostname
```

Không phải unknown value nào cũng là lỗi.

Bạn cần hiểu:

```text
known before plan
known during plan
known only after apply
```

Nếu `for_each` key phụ thuộc value chỉ biết sau apply, Terraform không thể xác định resource instances sớm và plan sẽ fail.

## 4. null, optional và nullable

Ba thứ dễ nhầm:

```text
unset/default
null
empty collection/string
```

Module contract nên explicit:

```hcl
variable "sku" {
  type     = string
  nullable = false
}

variable "tags" {
  type    = map(string)
  default = {}
}
```

Đừng dùng `null`, `""`, `{}` thay nhau như cùng nghĩa.

## 5. Type system cho module contract

Ưu tiên object type rõ:

```hcl
variable "network" {
  type = object({
    address_space = list(string)
    subnets = map(object({
      address_prefixes = list(string)
      private_endpoint_network_policies = optional(string)
    }))
  })
}
```

Lợi ích:

- contract đọc được;
- validation sớm;
- IDE/tooling tốt hơn;
- giảm hidden conventions.

## 6. Variable validation

Dùng khi invariant thuộc input contract:

```hcl
variable "environment" {
  type = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be dev, staging, or prod."
  }
}
```

Variable validation không thay Azure Policy.

```text
module input validation
= local configuration contract

Azure Policy
= cloud governance/enforcement at Azure scope
```

## 7. Preconditions và postconditions

### Precondition

Dùng cho assumption phải đúng trước operation.

```hcl
resource "azurerm_storage_account" "this" {
  # ...

  lifecycle {
    precondition {
      condition     = var.environment != "prod" || var.public_network_access_enabled == false
      error_message = "Production storage must disable public network access."
    }
  }
}
```

### Postcondition

Dùng cho guarantee của object sau read/apply.

```text
precondition
= assumption

postcondition
= guarantee
```

Postcondition failure có thể ngăn downstream resources tiếp tục dựa trên một guarantee không đúng.

## 8. check blocks

`check` phù hợp cho validation ngoài resource lifecycle.

Ví dụ:

```hcl
check "required_tags" {
  assert {
    condition = alltrue([
      contains(keys(local.common_tags), "owner"),
      contains(keys(local.common_tags), "environment")
    ])
    error_message = "owner and environment tags are required."
  }
}
```

Điểm quan trọng:

```text
check failure
→ warning
→ operation continues
```

Do đó không dùng `check` cho invariant phải block deployment.

## 9. Lifecycle meta-arguments

Bạn phải hiểu ít nhất:

```text
create_before_destroy
prevent_destroy
ignore_changes
replace_triggered_by
precondition
postcondition
```

### create_before_destroy

Hữu ích khi replacement có thể tạo object mới trước khi xóa object cũ.

Nhưng không phải resource nào cũng cho phép coexist do global name/unique constraint.

### prevent_destroy

Guardrail chống destroy ngoài ý muốn.

Không xem nó là backup.

Nếu data/resource bị mất ngoài Terraform, `prevent_destroy` không restore được gì.

### ignore_changes

Đây là một trong những option dễ bị lạm dụng nhất.

Bad:

```hcl
lifecycle {
  ignore_changes = all
}
```

Nếu dùng để “làm plan sạch”, bạn có thể che drift thực sự.

Chỉ ignore field khi ownership thật sự thuộc controller khác và boundary được document.

### replace_triggered_by

Dùng khi replacement semantic phụ thuộc resource khác nhưng provider schema không thể hiện đủ dependency replacement.

## 10. Provider configuration

Provider config nên ở root module.

```text
root module
→ owns provider configuration

child module
→ declares provider requirement
→ receives provider from caller
```

Tránh đặt credential/subscription logic bên trong reusable child module.

## 11. Provider aliases

Azure multi-subscription thường cần alias:

```hcl
provider "azurerm" {
  features {}
  subscription_id = var.workload_subscription_id
}

provider "azurerm" {
  alias           = "connectivity"
  features        {}
  subscription_id = var.connectivity_subscription_id
}
```

Pass explicit:

```hcl
module "hub_connection" {
  source = "./modules/hub-connection"

  providers = {
    azurerm.connectivity = azurerm.connectivity
  }
}
```

Provider alias là một phần architecture boundary, không chỉ syntax.

## 12. Provider version strategy

Phân biệt:

```text
Terraform CLI version
provider version
module version
Azure API version
```

Root config có thể pin/constraint chặt hơn.

Reusable child module thường nên declare minimum compatible provider version thay vì hard-pin patch không cần thiết.

Upgrade flow:

```text
review release notes
→ terraform init -upgrade
→ inspect lock-file diff
→ validate/test
→ plan representative environments
→ review replacements/deprecations
→ staged apply
```

## 13. Sensitive != secret removed from state

`sensitive = true` chủ yếu ảnh hưởng display/UI.

Không suy luận:

```text
sensitive = true
→ value is absent from state
```

State vẫn phải được bảo vệ.

## 14. Ephemeral values

Terraform hiện hỗ trợ ephemeral values/resources cho dữ liệu runtime không nên persist vào plan/state ở các context được hỗ trợ.

Mental model:

```text
sensitive
= hide/redact presentation

ephemeral
= do not persist value in state/plan where supported
```

Ephemeral phù hợp cho short-lived token/session data khi provider/resource contract hỗ trợ.

Nó không thay secret manager hay workload identity.

## 15. Write-only arguments

Khi resource/provider hỗ trợ write-only argument, có thể truyền sensitive/ephemeral value mà không persist nó như readable state attribute.

Nhưng vẫn cần:

- secret source;
- rotation;
- audit;
- least privilege;
- failure recovery.

## 16. Data sources

Data source = read remote information.

Không dùng data source như workaround để nhiều Terraform stacks mơ hồ cùng “discover” ownership.

Bạn vẫn cần xác định:

```text
who creates?
who owns lifecycle?
who may destroy?
who consumes?
```

## 17. Module design

Một module tốt nên represent một cohesive capability.

Bad:

```text
module "everything_company_cloud"
```

Better:

```text
network-spoke
private-endpoint
app-service-workload
monitoring-baseline
sql-workload
```

Module API gồm:

```text
inputs
outputs
provider requirements
behavior
migration contract
```

## 18. Composition over mega-module

Root module nên compose modules.

```text
environment root
├─ network
├─ identity
├─ compute
├─ data
└─ observability
```

Đừng tạo boolean matrix:

```text
enable_sql
enable_redis
enable_aks
enable_frontdoor
enable_everything
```

trong một module khổng lồ.

## 19. Dynamic blocks

`dynamic` hữu ích khi provider schema có repeated nested block.

Nhưng overuse làm HCL giống metaprogramming khó review.

Rule:

```text
simple repeated nested block
→ dynamic can help

architecture abstraction
→ module/composition may be clearer
```

## 20. Testing hierarchy

Terraform verification nên có layers:

```text
fmt
↓
validate
↓
terraform test
↓
policy/static checks
↓
plan
↓
apply to disposable environment
↓
runtime verification
↓
destroy/recovery verification
```

`terraform test` không thay integration test thật trên Azure.

Mock providers phù hợp để verify:

- module wiring;
- input/output behavior;
- assertions;
- basic resource intent.

Không chứng minh:

- Azure quota;
- Azure Policy;
- DNS;
- RBAC propagation;
- service availability;
- actual provider/API edge case.

## 21. Test assertions

Ví dụ:

```hcl
run "prod_defaults" {
  command = plan

  assert {
    condition     = azurerm_storage_account.this.public_network_access_enabled == false
    error_message = "Production storage must be private."
  }
}
```

Test business/platform intent, không chỉ “resource count == 1”.

## 22. Policy-as-code boundary

Terraform test/check và Azure Policy khác ownership:

```text
Terraform validation
→ protects module/configuration usage

CI policy
→ protects change workflow

Azure Policy
→ protects Azure platform state across deployment paths
```

Production nên defense-in-depth khi risk đủ lớn.

## 23. Provisioners

`local-exec` / `remote-exec` là escape hatch.

Trước khi dùng, hỏi:

- provider/resource có native capability không?
- cloud-init/extension/config management phù hợp hơn không?
- action có idempotent không?
- retry/unknown outcome xử lý thế nào?
- secret có leak vào command/log không?

Provisioner làm graph khó reason và recovery khó hơn.

## 24. Failure drills

### A — rename resource without moved block

Quan sát plan destroy/create, sau đó thêm `moved`.

### B — unstable count index

Remove item giữa list và quan sát address churn.

### C — ignore_changes hides drift

Tạo drift có chủ đích và chứng minh plan không còn báo field bị ignore.

### D — invalid production policy

Dùng precondition để block public network access.

### E — provider upgrade

Upgrade lock file và review schema/replacement behavior.

## 25. Exit criteria

Bạn hoàn thành chapter khi có thể:

- [ ] explain expression/data flow và unknown values;
- [ ] model module inputs bằng type rõ;
- [ ] distinguish validation/precondition/postcondition/check;
- [ ] explain lifecycle rules và risk của ignore_changes;
- [ ] design provider aliases cho multi-subscription;
- [ ] explain provider version/lock-file strategy;
- [ ] distinguish sensitive và ephemeral;
- [ ] explain root vs child module provider ownership;
- [ ] design focused reusable modules;
- [ ] write meaningful `terraform test` assertions;
- [ ] explain Terraform validation vs Azure Policy.

## Official sources

- Terraform language: https://developer.hashicorp.com/terraform/language
- Validate configuration: https://developer.hashicorp.com/terraform/language/validate
- Lifecycle: https://developer.hashicorp.com/terraform/language/meta-arguments/lifecycle
- Provider block: https://developer.hashicorp.com/terraform/language/block/provider
- Providers in modules: https://developer.hashicorp.com/terraform/language/modules/develop/providers
- Check block: https://developer.hashicorp.com/terraform/language/block/check
- Ephemeral resources: https://developer.hashicorp.com/terraform/language/block/ephemeral
- Variables: https://developer.hashicorp.com/terraform/language/block/variable

## Verification metadata

- Verified: 2026-09-25.
- Terraform 1.16.x repository baseline.
- Version-sensitive provider features must be checked against the provider version used by each root configuration.
