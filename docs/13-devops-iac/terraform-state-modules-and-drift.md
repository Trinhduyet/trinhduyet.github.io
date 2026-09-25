# Terraform — State, Dependency Graph, Modules & Drift

> [← Module 13 overview](README.md) · [Terraform on Azure →](terraform-on-azure-production.md) · [References](references.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;language · graph · state · plan/apply · modules · drift · testing · recovery</span>
  <span><strong>Baseline</strong>&nbsp;Terraform 1.16.x</span>
</div>

<div class="key-takeaway" markdown>
<strong>Terraform không phải script gọi cloud API theo thứ tự từ trên xuống.</strong>

Terraform đọc configuration, xây dependency graph, so sánh desired configuration với state và remote objects, sau đó tạo execution plan. State là một phần correctness của system, không phải file cache có thể xóa tùy ý.
</div>

---

# 1. Mental model quan trọng nhất

~~~text
Configuration
  ↓
Terraform language evaluation
  ↓
Dependency graph
  ↓
Current state + provider reads
  ↓
Plan
  ↓
Human / policy review
  ↓
Apply
  ↓
Provider APIs
  ↓
Remote infrastructure
  ↓
Updated state
~~~

Ba thứ phải phân biệt:

~~~text
Configuration
= desired intent in code

State
= Terraform's mapping/knowledge about managed objects

Remote infrastructure
= actual cloud objects
~~~

Nếu ba lớp lệch nhau, bạn có drift, import/migration problem hoặc out-of-band change.

---

# 2. HCL — những building block tối thiểu phải biết

Một root module thường có:

~~~text
terraform block
provider configuration
variables
locals
resources
data sources
modules
outputs
moved/import blocks when migrating
~~~

Ví dụ shape:

~~~hcl
terraform {
  required_version = "~> 1.16.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 5.0"
    }
  }
}

variable "environment" {
  type = string
}

locals {
  common_tags = {
    environment = var.environment
    managed_by  = "terraform"
  }
}

resource "azurerm_resource_group" "app" {
  name     = "rg-orders-\${var.environment}"
  location = "southeastasia"
  tags     = local.common_tags
}

output "resource_group_id" {
  value = azurerm_resource_group.app.id
}
~~~

Học HCL theo data flow, không học như template copy-paste.

---

# 3. Resource address và identity

Terraform theo dõi object bằng resource address.

Ví dụ:

~~~text
azurerm_resource_group.app
module.network.azurerm_virtual_network.this
azurerm_subnet.app["api"]
~~~

Address quan trọng vì state map:

~~~text
resource address
→ provider object identity
~~~

Nếu rename resource block mà không khai báo migration, Terraform có thể hiểu:

~~~text
old address removed
new address created
~~~

dù bạn chỉ đổi tên code.

Dùng moved block khi refactor address:

~~~hcl
moved {
  from = azurerm_resource_group.old_name
  to   = azurerm_resource_group.app
}
~~~

Rule:

~~~text
Refactor Terraform address
→ treat as state migration
~~~

---

# 4. Dependency graph — Terraform không chạy file theo thứ tự

Terraform tự suy ra dependency từ references.

~~~hcl
resource "azurerm_virtual_network" "app" {
  resource_group_name = azurerm_resource_group.app.name
}
~~~

Reference trên tạo edge:

~~~text
resource_group
      ↓
virtual_network
~~~

Terraform có thể parallelize objects không phụ thuộc nhau.

Chỉ dùng depends_on khi dependency thực sự tồn tại nhưng không thể biểu diễn qua data reference rõ ràng.

Overusing depends_on làm graph khó reasoning và có thể làm plan conservative hơn cần thiết.

---

# 5. count vs for_each

Cả hai tạo nhiều instances nhưng identity khác.

## count

~~~hcl
resource "example" "item" {
  count = 3
}
~~~

addresses:

~~~text
example.item[0]
example.item[1]
example.item[2]
~~~

Index dễ thay đổi khi list reorder/remove.

## for_each

~~~hcl
resource "example" "item" {
  for_each = {
    api    = {}
    worker = {}
  }
}
~~~

addresses:

~~~text
example.item["api"]
example.item["worker"]
~~~

Khi business identity có key ổn định, for_each thường dễ maintain hơn.

Rule:

~~~text
stable semantic key
→ prefer for_each

simple fixed cardinality
→ count can be enough
~~~

---

# 6. Variables, locals và outputs

## Variables

Input contract của module.

Production variables nên có:

- type rõ;
- description;
- validation khi invariant encode được;
- nullable/default có chủ đích;
- sensitive = true cho UX masking khi phù hợp.

~~~hcl
variable "environment" {
  type        = string
  description = "Deployment environment."

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be dev, staging, or prod."
  }
}
~~~

## Locals

Dùng để derive values, normalize naming/tagging, giảm duplicate expression.

Không dùng locals để tạo hidden configuration framework khó đọc.

## Outputs

Output là module API.

Chỉ export thứ consumer cần:

~~~text
resource ID
hostname
principal ID
subnet ID
~~~

Đừng export cả resource object chỉ để "cho tiện".

---

# 7. Providers và dependency lock file

Provider là plugin Terraform dùng để hiểu schema và gọi API.

~~~text
Terraform Core
  ↓
Provider plugin
  ↓
Azure / AWS / GitHub / ...
~~~

Hai version layer khác nhau:

~~~text
Terraform CLI version
!=
Provider version
~~~

File .terraform.lock.hcl ghi provider version/checksum đã selected.

Commit lock file cho root configuration để team/CI dùng cùng provider selection.

Workflow tốt:

~~~text
version constraint in code
+
dependency lock file
+
reviewed upgrade
~~~

Không dùng latest như production identity.

---

# 8. init / fmt / validate / plan / apply

## terraform fmt

Format HCL canonical.

## terraform init

Khởi tạo working directory:

- backend;
- providers;
- modules;
- lock file reconciliation.

## terraform validate

Kiểm configuration internally valid theo provider/module schemas đã init.

Nó không chứng minh:

- permission đủ;
- quota đủ;
- Azure Policy sẽ allow;
- region/SKU có sẵn;
- runtime architecture đúng.

## terraform plan

Tính proposed change.

~~~text
create
update in-place
replace
destroy
read
no-op
~~~

Plan là artifact để review, không phải proof apply chắc chắn thành công.

Giữa plan và apply, remote state/API có thể đổi.

## terraform apply

Thực thi graph qua provider APIs và cập nhật state.

---

# 9. Saved plan là artifact nhạy cảm

~~~bash
terraform plan -out=tfplan
terraform apply tfplan
~~~

Saved plan giúp apply đúng plan đã review.

Nhưng plan/state có thể chứa:

- IDs;
- topology;
- configuration;
- sensitive values;
- provider-private data.

Do đó:

~~~text
plan file
= controlled artifact
~~~

Không commit plan vào Git.
Không upload public artifact.
Có retention/ACL phù hợp trong CI.

---

# 10. State — Terraform biết gì về infrastructure?

State chứa mapping giữa Terraform addresses và remote objects, cùng metadata cần cho planning.

State không phải source of truth duy nhất cho business/platform reality.

~~~text
Configuration says what you want.
State says what Terraform currently tracks.
Provider API says what exists now.
~~~

State có thể chứa sensitive data ngay cả khi CLI che output.

Vì vậy production state cần:

- remote durable storage;
- access control;
- encryption provided by backend/platform;
- versioning/backup strategy;
- locking khi backend hỗ trợ;
- audit;
- blast-radius separation.

---

# 11. Remote backend và locking

Local state phù hợp learning nhỏ, không phù hợp team production.

Production flow:

~~~text
Developer / CI
      ↓
Remote backend
  ├─ state persistence
  ├─ access control
  └─ locking when supported
      ↓
Terraform operation
~~~

State locking ngăn nhiều writers cùng mutate state.

Nếu lock fail, Terraform dừng operation thay vì tiếp tục ghi đồng thời.

Không dùng -lock=false như cách chữa pipeline contention mặc định.

Root cause có thể là:

- concurrent pipelines;
- stale operation;
- architecture state quá lớn;
- emergency operation chưa kết thúc.

---

# 12. force-unlock — emergency tool

force-unlock nguy hiểm vì có thể tạo multiple writers nếu operation thật vẫn chạy.

Safe procedure:

~~~text
1. identify lock owner/run
2. prove original operation stopped
3. capture lock ID
4. investigate backend/run state
5. force-unlock only that stale lock
6. run plan again
7. inspect drift/state carefully
~~~

Không tự động force-unlock sau mỗi failure.

---

# 13. State boundaries

Một state quá lớn tạo:

- blast radius lớn;
- lock contention;
- long plan/apply;
- broad permissions;
- khó delegate ownership.

Một state quá nhỏ tạo:

- excessive cross-state dependencies;
- nhiều pipelines/backend keys;
- orchestration complexity.

Boundary tốt thường theo:

~~~text
ownership
lifecycle
blast radius
deployment frequency
permissions
failure isolation
~~~

Ví dụ:

~~~text
platform-connectivity
platform-observability
orders-prod
payments-prod
~~~

thường dễ reason hơn một enterprise.tfstate duy nhất.

---

# 14. Workspaces không tự động giải quyết environment architecture

Terraform CLI workspaces cho phép nhiều state instances cho cùng configuration.

Nhưng chúng không tự tạo:

- Azure subscription isolation;
- different RBAC;
- different policy;
- different backend account;
- different blast radius;
- clear code review boundary.

Với prod/non-prod, nhiều team chọn explicit root/state boundaries thay vì dùng workspace như environment abstraction duy nhất.

~~~text
workspace = state-selection mechanism
not environment governance strategy
~~~

---

# 15. Modules — reusable infrastructure contract

Root module:

~~~text
owns environment composition
provider/backend wiring
real workload inputs
~~~

Child module:

~~~text
encapsulates reusable infrastructure capability
~~~

Một module tốt có:

- narrow purpose;
- typed variables;
- stable outputs;
- documented assumptions;
- no hidden provider config unless justified;
- tests;
- upgrade/migration story.

Bad module:

~~~text
module "everything"
→ network + database + AKS + front door + monitoring + ...
~~~

Nó trở thành custom cloud framework.

Better:

~~~text
network module
identity module
workload-host module
data module
observability module
~~~

compose ở root/pattern layer.

---

# 16. Module versioning

Public/shared module change có thể ảnh hưởng nhiều environments.

Treat module như library:

~~~text
version
contract
changelog
compatibility
migration
tests
~~~

Consumer không nên tracking moving branch mặc định.

Use immutable/reviewed source identity:

~~~text
registry version
Git tag/commit
approved module release
~~~

---

# 17. Data sources

Data source đọc object ngoài management ownership trực tiếp của resource block hiện tại.

Ví dụ:

~~~text
read existing subscription
read shared VNet
read Key Vault metadata
~~~

Nếu root A đọc internals của root B bằng hàng chục data sources, coupling vẫn cao.

Prefer explicit output/contract hoặc stable platform convention khi ownership tách biệt.

---

# 18. lifecycle meta-arguments

## create_before_destroy

Giúp replacement tạo mới trước xóa cũ khi provider/resource cho phép.

Không có nghĩa zero downtime tự động.

Bạn vẫn cần:

- name uniqueness;
- quota;
- dependency behavior;
- traffic switch;
- data compatibility.

## prevent_destroy

Có ích như guard cho critical resources.

Nhưng không phải backup/DR.

## ignore_changes

Rất dễ bị abuse để "làm plan sạch".

Nếu ignore field quan trọng:

~~~text
Terraform stops detecting desired-state drift for that field
~~~

Chỉ dùng khi có documented external owner/controller.

---

# 19. Import brownfield infrastructure

Brownfield flow tốt:

~~~text
existing resource
→ write matching configuration
→ define import
→ plan
→ reconcile differences
→ import/apply
→ prove next plan is understood
~~~

Modern import blocks làm migration reviewable trong configuration.

Import không biến existing resource thành architecture tốt; nó chỉ đưa object vào Terraform ownership.

---

# 20. moved blocks và refactor-safe changes

Khi:

- rename resource;
- move resource vào module;
- đổi module path;

hãy xem đây là state address migration.

moved block giúp Terraform hiểu continuity.

Evidence cần:

~~~text
plan shows move
not destroy/create
~~~

---

# 21. Drift

Drift là remote reality khác desired configuration/state expectation.

Nguồn drift:

- portal/manual changes;
- external automation;
- policy modification;
- platform default evolution;
- provider behavior;
- emergency hotfix;
- unmanaged controller.

Detection:

~~~bash
terraform plan
terraform plan -refresh-only
~~~

Không auto-apply mọi drift.

Classification trước:

~~~text
authorized emergency change?
malicious/unapproved change?
new desired state?
provider normalization?
platform-managed field?
~~~

Response có thể là:

~~~text
revert remote change
update code
import/migrate
document external ownership
~~~

---

# 22. Refresh-only không phải reconciliation magic

Refresh-only giúp review state updates dựa trên remote objects mà không propose normal configuration changes.

Use case:

~~~text
out-of-band change occurred
→ inspect what Terraform now observes
→ update state intentionally if appropriate
~~~

Không dùng refresh-only để che việc configuration không còn đại diện desired state.

---

# 23. Secrets và sensitive values

sensitive = true chủ yếu ảnh hưởng display behavior.

Nó không đảm bảo value không xuất hiện trong:

- state;
- plan;
- provider API;
- debug log;
- downstream resource attributes.

Prefer:

~~~text
workload identity / managed identity
→ no static secret
~~~

Nếu secret bắt buộc:

- inject at runtime;
- secret manager;
- avoid plaintext tfvars committed to Git;
- restrict state/plan access;
- rotate.

---

# 24. CI/CD cho Terraform

PR pipeline:

~~~text
terraform fmt -check
→ init
→ validate
→ test
→ security/policy checks
→ plan
→ plan summary
→ human review
~~~

Apply pipeline:

~~~text
approved change
→ obtain short-lived identity
→ init exact backend
→ apply reviewed change
→ capture outputs
→ post-apply verification
~~~

Quan trọng:

~~~text
CI identity
!= runtime workload identity
~~~

CI chỉ cần quyền deploy phạm vi cần thiết.

---

# 25. Plan review checklist

Không chỉ đọc:

~~~text
Plan: 2 to add, 1 to change, 0 to destroy
~~~

Review:

- replacement nào xảy ra?
- resource critical có destroy không?
- network route/NSG thay đổi không?
- public access thay đổi không?
- identity/RBAC broaden không?
- SKU/capacity/cost thay đổi không?
- data resource có recreate không?
- tag/policy drift có ý nghĩa không?
- provider/module version có đổi không?
- output/API contract có đổi không?

---

# 26. Terraform testing

Testing layers:

~~~text
fmt
↓
validate
↓
terraform test
↓
policy/security checks
↓
plan assertions/review
↓
ephemeral integration environment when justified
~~~

Terraform test có thể test root/child modules.

Provider mocking cho phép kiểm module logic mà không tạo cloud resources hoặc cần credential.

Mock test phù hợp để chứng minh:

- naming;
- tags;
- variable validation;
- composition;
- output shape;
- resource arguments.

Nó không chứng minh Azure API/runtime behavior thật.

---

# 27. Policy as code

Policy gate có thể kiểm:

~~~text
public network disabled?
approved regions?
required tags?
approved SKUs?
TLS/security baseline?
forbidden resource types?
~~~

Nhưng policy không thay architecture review.

Một configuration có thể pass policy nhưng vẫn:

- quá đắt;
- không đạt RTO/RPO;
- bottleneck;
- sai ownership;
- thiếu observability.

---

# 28. Failure modes

| Failure | Meaning | Response |
|---|---|---|
| init/provider download fail | tool/dependency problem | retry bounded, mirror/cache if needed |
| backend auth fail | identity/RBAC/config issue | fix auth; do not bypass backend |
| state lock busy | another writer/stale lock | identify owner before unlock |
| plan unexpected destroy | config/state/address mismatch | stop; inspect migration/import |
| apply partial fail | some remote actions succeeded | re-plan from current reality |
| provider timeout | remote outcome may be uncertain | query remote state/API before retry |
| policy deny | governance rejected resource | fix design/config or approved exception |
| quota/capacity fail | platform capacity boundary | request quota/change design |
| drift after emergency edit | desired/actual mismatch | decide code vs remote authority |
| state lost/corrupt | management metadata failure | restore backend version/backup carefully |

---

# 29. Partial apply — Terraform is not a database transaction

Terraform apply across many cloud resources is not one ACID transaction.

Example:

~~~text
resource A created
resource B created
resource C fails
~~~

Terraform cannot guarantee rollback A/B.

Recovery:

~~~text
inspect actual remote objects
→ inspect state
→ rerun plan
→ repair forward or controlled destroy
~~~

Do not assume failed apply means "nothing changed".

---

# 30. Provisioners are last resort

remote-exec/local-exec provisioners couple infrastructure lifecycle to imperative scripts.

Prefer:

~~~text
cloud-init / image build
managed service config
application deployment system
configuration management
dedicated pipeline step
~~~

Use provisioner only when no better lifecycle owner exists and failure/idempotency is understood.

---

# 31. Repository structure

Một practical layout:

~~~text
infra/
├── modules/
│   ├── network/
│   ├── workload/
│   └── observability/
├── environments/
│   ├── dev/
│   ├── staging/
│   └── prod/
└── tests/
~~~

Alternative layouts are valid.

Quality test:

~~~text
Can a new engineer answer:
- which state owns this resource?
- which pipeline applies it?
- which module defines it?
- which identity has permission?
- how is prod isolated?
~~~

---

# 32. What Terraform should own?

Good Terraform ownership:

- cloud resource topology;
- networking;
- managed services;
- identities/role assignments where lifecycle fits;
- policies/config that are infrastructure desired state.

Be cautious with:

- frequently changing application runtime data;
- database records/business data;
- secrets whose value lifecycle belongs elsewhere;
- Kubernetes workload fields concurrently reconciled by GitOps;
- resources controlled by another authoritative controller.

~~~text
one field
→ one clear desired-state owner
~~~

---

# 33. Debugging workflow

When plan looks wrong:

~~~text
1. terraform version
2. inspect .terraform.lock.hcl
3. terraform providers
4. verify workspace/backend
5. terraform state list
6. terraform state show <address>
7. inspect configuration/address changes
8. query remote cloud object read-only
9. terraform plan
10. classify config/state/remote mismatch
~~~

Do not start with state rm or manual state editing.

State mutation commands are surgical tools, not normal debugging shortcuts.

---

# 34. Hands-on Lab

Repository có lab không cần Azure credential:

~~~bash
terraform -chdir=labs/13-terraform-azure init -backend=false
terraform -chdir=labs/13-terraform-azure validate
terraform -chdir=labs/13-terraform-azure test
~~~

Lab dùng mocked AzureRM provider để kiểm:

- typed inputs;
- naming;
- tags;
- resource graph;
- outputs;

mà không tạo Azure resource thật.

Sau đó học production path:

→ [Terraform on Azure](terraform-on-azure-production.md)

---

# 35. Exit Criteria

Bạn hoàn thành chapter khi có thể:

- [ ] giải thích configuration vs state vs remote reality;
- [ ] giải thích dependency graph và implicit dependency;
- [ ] đọc resource address và plan replacement;
- [ ] dùng typed variables/validation/locals/outputs;
- [ ] giải thích provider version vs Terraform version;
- [ ] quản lý dependency lock file;
- [ ] thiết kế remote state + locking;
- [ ] chọn state boundary theo ownership/blast radius;
- [ ] giải thích vì sao workspaces không thay environment governance;
- [ ] thiết kế module có narrow contract;
- [ ] dùng moved/import blocks cho migration;
- [ ] phát hiện và phân loại drift;
- [ ] giải thích partial apply/unknown outcome;
- [ ] bảo vệ plan/state như sensitive artifact;
- [ ] xây PR plan/apply pipeline;
- [ ] chạy terraform test với mocked provider;
- [ ] biết khi nào không nên cho Terraform ownership.

## Official sources

- Terraform language: https://developer.hashicorp.com/terraform/language
- State: https://developer.hashicorp.com/terraform/language/state
- State locking: https://developer.hashicorp.com/terraform/language/state/locking
- Backends: https://developer.hashicorp.com/terraform/language/backend
- Dependency lock file: https://developer.hashicorp.com/terraform/language/files/dependency-lock
- Modules: https://developer.hashicorp.com/terraform/language/modules
- Import: https://developer.hashicorp.com/terraform/language/import
- moved block: https://developer.hashicorp.com/terraform/language/block/moved
- Testing: https://developer.hashicorp.com/terraform/language/tests
- Provider mocking: https://developer.hashicorp.com/terraform/language/tests/mocking

## Verification metadata

- Verified: 2026-09-25.
- Terraform stable baseline: 1.16.4.
- Terraform 1.17 is prerelease at verification time and is not used as the runnable baseline.
