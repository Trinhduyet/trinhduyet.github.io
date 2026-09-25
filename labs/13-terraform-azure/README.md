# Terraform + Azure Runnable Lab

This lab verifies Terraform configuration and Azure resource composition **without Azure credentials and without creating cloud resources**.

It uses Terraform provider mocking to keep CI deterministic.

## What it teaches

~~~text
Terraform Core
→ provider schema
→ Azure resource graph
→ typed variables
→ naming/tags
→ outputs
→ terraform test
~~~

The live production path is covered in:

- docs/13-devops-iac/terraform-state-modules-and-drift.md
- docs/13-devops-iac/terraform-language-lifecycle-and-validation.md
- docs/13-devops-iac/terraform-team-workflows-and-state-recovery.md
- docs/13-devops-iac/terraform-on-azure-production.md
- docs/14-cloud/azure-core-platform-control-plane-and-governance.md
- docs/14-cloud/azure-service-selection-and-workload-architecture.md
- docs/14-cloud/azure-governance-security-and-operations-playbook.md

## Baseline

~~~text
Terraform CLI 1.16.4
AzureRM provider 5.6.0
~~~

## Run

From repository root:

~~~bash
terraform -chdir=labs/13-terraform-azure fmt -check -recursive

terraform -chdir=labs/13-terraform-azure init \
  -backend=false \
  -input=false

terraform -chdir=labs/13-terraform-azure validate

terraform -chdir=labs/13-terraform-azure test
~~~

Expected final test shape:

~~~text
Success! 3 passed, 0 failed.
~~~

## Why no Azure login?

The test file contains:

~~~hcl
mock_provider "azurerm" {}
~~~

Terraform still loads the real provider schema, but the test does not call Azure APIs.

This proves:

- HCL/schema validity;
- variable contracts;
- naming/tagging invariants;
- resource relationships;
- output/resource shape;
- negative variable-contract behavior through `expect_failures`.

It does **not** prove:

- Azure RBAC;
- Azure Policy;
- quota/capacity;
- region/SKU availability;
- Private Link/DNS behavior;
- actual create/update/delete behavior.

Those require an isolated Azure integration environment.

## Exercise 1 — break naming

Change the resource group naming expression so the test fails.

Observe:

~~~text
configuration still parses
but architecture contract test fails
~~~

## Exercise 2 — remove dependency reference

Replace a resource reference with a hard-coded name.

Compare the Terraform dependency graph and explain why implicit references are safer than hidden ordering assumptions.

## Exercise 3 — add a tag invariant

Add:

~~~text
cost_center
criticality
~~~

to variables/tags, then assert them in the test.

## Exercise 4 — remote backend design

Read backend.azurerm.hcl.example and design the real bootstrap path:

~~~text
state RG
→ Storage Account
→ private/public access decision
→ blob container
→ versioning/retention
→ Entra data-plane access
→ CI OIDC
~~~

Do not add real secrets to the example file.

## Exercise 5 — live sandbox extension

Only after the mock test is green:

1. create a dedicated Azure sandbox subscription/resource group;
2. configure short-lived identity;
3. use remote Blob backend;
4. run plan;
5. review create/update/destroy;
6. apply;
7. make a harmless portal tag drift;
8. run plan again;
9. reconcile from code;
10. destroy and verify cleanup.

Never use a production subscription for first-time Terraform experiments.

## Evidence

A completed learner submission should include:

~~~text
terraform version
terraform providers
fmt result
validate result
test result
dependency graph explanation
state boundary ADR
OIDC/RBAC design
drift experiment
recovery notes
~~~
