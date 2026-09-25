# Terraform on Azure — Production State, Identity, Modules & Delivery

> [← Terraform fundamentals](terraform-state-modules-and-drift.md) · [Azure platform →](../14-cloud/README.md) · [References](references.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;AzureRM · AzAPI · AVM · Blob backend · OIDC · subscriptions · policy · CI/CD</span>
  <span><strong>Baseline</strong>&nbsp;Terraform 1.16.4 · AzureRM 5.6.0</span>
</div>

<div class="key-takeaway" markdown>
<strong>Terraform on Azure là bài toán ownership + identity + state + Azure control plane.</strong>

Viết một azurerm_resource_group chỉ là bước đầu. Production path phải trả lời: state nằm đâu, ai lock/apply được, subscription nào là blast radius, Azure Policy có thể deny gì, provider/module version nào được pin, identity nào deploy và khi apply fail giữa chừng thì recovery ra sao.
</div>

---

# 1. Mental model

~~~text
Git / Pull Request
      ↓
Terraform root module
      ↓
Terraform Core
      ↓
Azure provider adapter
  ├─ AzureRM
  └─ AzAPI when needed
      ↓
Azure Resource Manager control plane
      ↓
Subscriptions / Resource Groups / Resources

Remote state
      ↕
Azure Blob backend

CI identity
      ↓
Microsoft Entra ID
      ↓
Azure RBAC
~~~

Terraform không thay Azure governance. Nó hoạt động bên trong governance đó.

---

# 2. AzureRM, AzAPI và Azure Verified Modules

## AzureRM

HashiCorp AzureRM provider cung cấp typed resources/data sources cho Azure Resource Manager APIs.

Use when:

- resource được support tốt;
- schema/provider lifecycle phù hợp;
- team muốn stable typed Terraform interface.

## AzAPI

AzAPI expose Azure Resource Manager surface gần API hơn.

Use when:

- Azure feature/API mới chưa có AzureRM resource phù hợp;
- cần control-plane property mới;
- module/pattern standard của tổ chức chọn AzAPI.

Trade-off:

~~~text
closer to ARM API
→ faster feature coverage
→ less provider-specific abstraction
→ more responsibility to understand API schema/version
~~~

## Azure Verified Modules

AVM là Microsoft-driven module catalog/specification cho reusable Azure IaC.

AVM có:

~~~text
Resource Modules
Pattern Modules
Utility Modules
~~~

Use AVM khi module contract phù hợp thay vì copy một community module không rõ lifecycle.

Không có nghĩa:

~~~text
AVM → no architecture review needed
~~~

Bạn vẫn phải review:

- inputs;
- networking;
- identity;
- cost;
- version;
- blast radius;
- upgrade path.

---

# 3. Provider pinning

Pin Terraform Core và provider separately.

~~~hcl
terraform {
  required_version = "~> 1.16.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "= 5.6.0"
    }
  }
}

provider "azurerm" {
  features {}
}
~~~

Root module nên commit .terraform.lock.hcl.

Upgrade flow:

~~~text
read provider release/upgrade notes
→ update constraint intentionally
→ terraform init -upgrade
→ review lock file
→ validate/test
→ plan non-prod
→ inspect replacements
→ staged production rollout
~~~

Major provider upgrade không phải dependency bump mù.

---

# 4. Azure authentication — local vs CI

## Local development

Local engineer có thể dùng Azure CLI/Entra-backed login theo provider-supported flow.

Mục tiêu:

~~~text
human identity
→ MFA / conditional access
→ short-lived token
→ scoped Azure permission
~~~

Không để long-lived client secret trong tfvars.

## CI/CD

Prefer workload identity federation / OIDC.

~~~text
GitHub Actions
      ↓ OIDC token
Microsoft Entra federated credential
      ↓
service principal / user-assigned managed identity
      ↓
Azure RBAC
      ↓
Terraform provider/backend
~~~

Lợi ích:

- không lưu Azure client secret lâu dài;
- credential short-lived;
- trust bound theo repo/environment/branch claims;
- dễ rotate hơn static secret.

CI deploy identity và runtime managed identity là hai principal khác nhau.

---

# 5. Remote state trên Azure Blob

HashiCorp azurerm backend lưu state trong Azure Blob Storage và hỗ trợ state locking/consistency checking bằng Azure Storage capabilities.

Conceptual backend:

~~~hcl
terraform {
  backend "azurerm" {}
}
~~~

Runtime/backend config supply:

~~~text
resource_group_name
storage_account_name
container_name
key
tenant/client/subscription identity settings
~~~

Prefer Microsoft Entra ID authentication.

Không hard-code secret vào backend config.

---

# 6. Bootstrap problem

Terraform state storage phải tồn tại trước khi workload root có thể dùng nó.

Do đó tách bootstrap:

~~~text
Bootstrap
→ state resource group
→ storage account
→ blob container
→ access/RBAC
→ optional logging/private networking

Then

Workload Terraform
→ uses remote backend
~~~

Không để root workload vừa create backend storage vừa phụ thuộc backend đó trong cùng lifecycle.

Bootstrap có thể dùng:

- small dedicated Terraform root with temporary/local state then migrated;
- Bicep/CLI governed bootstrap;
- centrally provisioned platform capability.

Quan trọng là owner + recovery rõ.

---

# 7. Backend identity và least privilege

Backend access là data-plane access tới state blob.

State chứa sensitive infrastructure metadata nên identity phải tối thiểu quyền cần thiết.

Separate concerns:

~~~text
Backend state access
!=
Azure infrastructure deployment rights
~~~

Một CI principal có thể cần cả hai, nhưng review quyền theo từng surface.

Không cấp Owner toàn subscription chỉ vì "Terraform cần quyền".

---

# 8. State key design

Bad:

~~~text
terraform.tfstate
~~~

cho mọi workload/environment.

Better:

~~~text
platform/connectivity/prod.tfstate
orders/dev.tfstate
orders/prod.tfstate
payments/prod.tfstate
~~~

Key naming phải phản ánh:

- owner;
- environment;
- state boundary;
- restore target.

State split không nên phụ thuộc chỉ vào folder cosmetics.

---

# 9. Subscription boundary

Azure hierarchy:

~~~text
Tenant
↓
Management Group
↓
Subscription
↓
Resource Group
↓
Resource
~~~

Terraform root thường nên map vào governance boundary rõ.

Một root có thể target nhiều subscriptions, nhưng càng cross-subscription:

- permission càng rộng;
- blast radius càng lớn;
- provider aliases tăng;
- plan khó review;
- ownership dễ mơ hồ.

Prefer separate roots/states khi lifecycle/owner khác nhau.

---

# 10. Multiple subscriptions và provider aliases

Khi thật sự cần shared platform + workload subscription:

~~~hcl
provider "azurerm" {
  features {}
  subscription_id = var.workload_subscription_id
}

provider "azurerm" {
  alias           = "connectivity"
  features {}
  subscription_id = var.connectivity_subscription_id
}
~~~

Child module nhận provider explicitly khi cần.

Rule:

~~~text
cross-subscription dependency
→ explicit ownership + explicit provider wiring
~~~

Đừng để module tự chọn subscription ngầm.

---

# 11. Resource Group không phải state boundary mặc định

Một state có thể chứa nhiều RG; một RG cũng có thể bị nhiều automation chạm vào.

Hỏi:

~~~text
Which Terraform root owns this field/resource?
~~~

Không hỏi chỉ:

~~~text
Which resource group is it in?
~~~

Ownership phải rõ ở code/pipeline/state.

---

# 12. Azure Policy và Terraform plan

Azure Policy có thể audit/deny/modify/deploy behavior ở platform scope.

Terraform plan không phải guarantee Policy sẽ accept apply.

Flow:

~~~text
terraform plan
→ looks valid
→ apply
→ ARM request
→ Azure Policy evaluation
→ allow / deny / modify / deploy effect
~~~

Do đó test non-prod với same governance baseline.

Policy deny phải được fix ở design/config hoặc có exception workflow rõ; không bypass bằng portal.

---

# 13. RBAC propagation và eventual consistency

Azure identity/RBAC operations có thể có propagation delay.

Common sequence:

~~~text
create managed identity
→ assign role
→ immediately use role
→ authorization may not be visible yet
~~~

Đừng chữa mọi eventual-consistency issue bằng sleep dài tùy ý.

Prefer:

- provider-native polling/retry behavior;
- split lifecycle khi cần;
- verify principal/resource IDs;
- bounded retry at correct boundary.

---

# 14. Resource locks vs Terraform lifecycle

Azure resource locks và Terraform prevent_destroy giải quyết hai lớp khác nhau.

~~~text
Azure resource lock
= platform guard against delete/update operations

Terraform prevent_destroy
= configuration-level safety against planned destroy
~~~

Cả hai không phải backup.

Nếu Terraform cần destroy resource có CanNotDelete lock:

~~~text
apply fails
~~~

Do đó lock ownership và emergency removal procedure phải documented.

---

# 15. Private networking changes are architecture changes

Terraform dễ làm network config trông như vài HCL lines, nhưng change có thể cắt traffic production.

Review đặc biệt:

- VNet address space;
- subnet;
- route table;
- NSG;
- firewall;
- private endpoint;
- public network access;
- DNS zone/link;
- NAT/outbound path;
- Front Door/App Gateway/APIM connectivity.

Network plan review phải gắn với traffic path diagram.

---

# 16. Private Endpoint + DNS

Private Endpoint tạo private IP path tới PaaS service.

Nhưng connectivity cần DNS resolve đúng FQDN tới private IP.

~~~text
Application
→ DNS
→ private zone / resolver
→ Private Endpoint IP
→ PaaS service
~~~

Terraform change chỉ tạo endpoint mà quên DNS link/routing là incomplete.

Treat:

~~~text
Private Link
= network path

RBAC / service auth
= caller authorization
~~~

Cần cả hai khi service yêu cầu.

---

# 17. Managed Identity first

Prefer runtime path:

~~~text
App Service / Container Apps / Function / VM / AKS workload
      ↓
Managed Identity / Workload Identity
      ↓
Azure RBAC / Entra auth
      ↓
Storage / SQL / Service Bus / Key Vault
~~~

Terraform nên provision identity + role relationship, nhưng không biến static secret thành default integration mechanism.

---

# 18. Secrets and Terraform state

Bad:

~~~hcl
variable "sql_admin_password" {
  type = string
}
~~~

rồi lưu plaintext trong committed tfvars.

Even sensitive variables can end up in state depending on resource/provider semantics.

Prefer architecture không cần secret:

~~~text
Managed Identity
→ Entra-based data access
~~~

Nếu resource bắt buộc bootstrap secret:

- generate/handle through controlled secret path;
- restrict state;
- avoid outputting it;
- rotate;
- migrate away when possible.

---

# 19. Tags and naming

Use locals/module conventions:

~~~hcl
locals {
  tags = {
    application = var.application
    environment = var.environment
    owner       = var.owner
    managed_by  = "terraform"
  }
}
~~~

Tags support:

- cost allocation;
- ownership;
- inventory;
- policy.

Tags không phải security boundary.

Naming conventions cần account cho Azure per-resource naming constraints.

---

# 20. Azure quotas are deployment dependencies

Terraform config có thể đúng nhưng apply fail do:

- regional capacity;
- subscription quota;
- SKU availability;
- service feature availability;
- policy;
- provider registration/API behavior.

Preflight production deployment nên kiểm các capacity/region assumptions quan trọng.

---

# 21. ARM long-running operations

Nhiều Azure operations async/long-running.

Terraform provider có polling/timeouts.

Failure semantics:

~~~text
client/provider timeout
!=
Azure definitely did nothing
~~~

Khi apply timeout:

1. query Azure resource state read-only;
2. inspect provider/Terraform state;
3. rerun plan;
4. decide retry/repair/import.

Không blind retry destructive operation.

---

# 22. AzureRM vs portal drift

Portal hotfix:

~~~text
operator changes production setting
↓
service recovers
↓
Terraform code still old
~~~

Sau incident phải reconcile:

~~~text
remote change becomes desired
→ update Terraform code

or

remote change was temporary
→ Terraform restores declared config
~~~

Không để permanent "temporary" portal drift.

---

# 23. Brownfield Azure adoption

Safe sequence:

~~~text
inventory existing Azure resources
→ classify ownership
→ write matching Terraform configuration
→ import
→ plan
→ eliminate unexpected change
→ move to normal PR/apply workflow
~~~

Do not import everything in a subscription into one state.

Start by bounded workload/platform slice.

---

# 24. Azure Verified Modules adoption

Decision:

~~~text
Need common Azure resource/pattern
      ↓
AVM module fits requirements?
  ├─ yes → pin/review module version
  └─ no  → compose native provider resources / custom module
~~~

Review AVM input defaults instead of assuming "verified" means your NFRs are satisfied.

Pattern modules can be useful for repeatable landing-zone/workload patterns.

---

# 25. Root module vs reusable module on Azure

Recommended ownership:

~~~text
Root module
├─ subscription/environment inputs
├─ provider aliases
├─ backend
├─ workload composition
└─ production-specific policy decisions

Child modules
├─ network capability
├─ identity capability
├─ compute capability
├─ data capability
└─ observability capability
~~~

Provider/backend configuration belongs at composition/root boundary unless module design has explicit reason otherwise.

---

# 26. PR pipeline

~~~text
Pull Request
↓
terraform fmt -check
↓
terraform init -backend=false or safe backend init
↓
terraform validate
↓
terraform test
↓
security/policy scan
↓
terraform plan
↓
publish controlled plan summary/artifact
↓
review
~~~

Do not run production apply on every PR.

---

# 27. Apply pipeline with OIDC

Conceptual GitHub Actions:

~~~yaml
permissions:
  contents: read
  id-token: write

steps:
  - checkout
  - azure/login using OIDC
  - setup terraform
  - terraform init
  - terraform plan
  - approval/environment gate
  - terraform apply
~~~

Federation trust should be scoped to expected repository/environment/branch model.

Apply identity should not have unrelated subscription-wide rights.

---

# 28. Plan artifact promotion

If organization saves binary plan:

~~~text
PR commit SHA
+ provider lock file
+ plan
+ target state/backend
+ identity/environment
~~~

must remain traceable.

Never apply a stale plan after material changes without policy designed for that workflow.

Some teams re-plan at apply time and gate the new plan; others apply a saved reviewed plan. Pick one model explicitly.

---

# 29. Separate platform and workload lifecycles

Platform root:

~~~text
management groups
subscriptions
hub/VWAN
DNS
firewall
shared monitoring
policy
~~~

Workload root:

~~~text
app resource groups
compute
database
storage
service bus
private endpoints
workload diagnostics
~~~

Platform changes usually need stricter ownership/approval and lower frequency.

Do not make every app deployment depend on re-applying the whole platform landing zone.

---

# 30. Terraform vs Bicep

Both can deploy Azure control-plane resources.

Terraform strengths:

- multi-provider ecosystem;
- consistent state/plan workflow;
- reusable modules across platform tooling;
- broader cross-cloud/tool integrations.

Bicep strengths:

- Azure-native ARM language;
- no separate state file for standard ARM deployments;
- fast exposure of Azure resource schema;
- strong Azure integration.

Decision is organizational/operational, not "one is production and one is not".

Avoid mixing ownership for the same Azure field/resource without a clear boundary.

---

# 31. Terraform vs GitOps/Kubernetes

Terraform:

~~~text
Azure platform infrastructure
→ VNet
→ ACR
→ managed DB
→ AKS cluster
→ identities
~~~

GitOps/Kubernetes:

~~~text
workload objects inside cluster
→ Deployment
→ Service
→ HPA
→ NetworkPolicy
~~~

Avoid:

~~~text
Terraform continuously owns Deployment
while Argo CD also owns Deployment
~~~

unless ownership split is explicit and tested.

---

# 32. Failure drills

## Drill A — stale state lock

Simulate interrupted local/CI operation in safe environment.

Evidence:

- identify lock owner;
- no blind force-unlock;
- correct recovery.

## Drill B — portal drift

Change a harmless tag manually.

Run plan.

Explain:

~~~text
remote change
→ plan reconciliation
~~~

## Drill C — provider major upgrade

Upgrade non-prod 4.x → 5.x or between controlled major lines only in lab.

Evidence:

- upgrade guide read;
- lock file diff;
- no unexpected replacement;
- regression plan.

## Drill D — policy deny

Use sandbox policy or documented simulation.

Show:

~~~text
valid Terraform schema
!=
governance permission
~~~

## Drill E — OIDC permission failure

Remove/limit role in lab subscription.

Expected:

~~~text
authentication succeeds
authorization fails
~~~

Distinguish AuthN vs AuthZ.

---

# 33. Production reference layout

~~~text
infra/
├── bootstrap/
│   └── state/
├── modules/
│   ├── network/
│   ├── identity/
│   ├── workload-host/
│   └── observability/
├── environments/
│   ├── nonprod/
│   └── prod/
└── tests/
~~~

State:

~~~text
tfstate storage
├── platform/connectivity.tfstate
├── platform/management.tfstate
├── orders/nonprod.tfstate
└── orders/prod.tfstate
~~~

Pipelines map 1:1 hoặc predictably tới state owners.

---

# 34. Runnable repository lab

The repository includes:

~~~text
labs/13-terraform-azure
~~~

No Azure credentials required for baseline verification.

~~~bash
terraform -chdir=labs/13-terraform-azure fmt -check -recursive
terraform -chdir=labs/13-terraform-azure init -backend=false
terraform -chdir=labs/13-terraform-azure validate
terraform -chdir=labs/13-terraform-azure test
~~~

terraform test uses a mocked AzureRM provider.

This proves configuration/module logic, not live Azure behavior.

---

# 35. Exit Criteria

- [ ] explain AzureRM vs AzAPI vs AVM;
- [ ] pin Terraform/provider versions and lock file;
- [ ] design Azure Blob remote state;
- [ ] explain backend bootstrap;
- [ ] use Entra/OIDC instead of long-lived CI secret;
- [ ] separate deploy identity from runtime identity;
- [ ] design state/subscription boundaries;
- [ ] wire provider aliases explicitly;
- [ ] explain Azure Policy/RBAC/resource-lock interaction;
- [ ] reason about Private Endpoint + DNS;
- [ ] handle RBAC eventual consistency and ARM long-running operations;
- [ ] import brownfield resources safely;
- [ ] design PR plan/apply pipeline;
- [ ] separate platform and workload lifecycles;
- [ ] define Terraform vs Bicep vs GitOps ownership;
- [ ] run a drift/recovery drill;
- [ ] pass the repository Terraform lab.

## Official sources

- AzureRM provider: https://registry.terraform.io/providers/hashicorp/azurerm/latest
- AzureRM backend: https://developer.hashicorp.com/terraform/language/backend/azurerm
- Azure Terraform overview: https://learn.microsoft.com/en-us/azure/developer/terraform/
- GitHub Actions + Azure workload identity federation sample: https://learn.microsoft.com/en-us/samples/azure-samples/github-terraform-oidc-ci-cd/github-terraform-oidc-ci-cd/
- Azure Verified Modules: https://azure.github.io/Azure-Verified-Modules/
- AVM Terraform solution development: https://azure.github.io/Azure-Verified-Modules/usage/solution-development/terraform/
- Azure landing zone Terraform: https://azure.github.io/Azure-Landing-Zones/terraform/

## Verification metadata

- Verified: 2026-09-25.
- Terraform stable: 1.16.4.
- AzureRM latest stable release verified: 5.6.0.
- AzureRM 5.x is a major line; upgrades from 4.x require upgrade-note review.
