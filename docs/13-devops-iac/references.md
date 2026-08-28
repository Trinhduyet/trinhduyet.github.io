# References — Module 13 DevOps, IaC & Delivery Engineering

> [← Module overview](README.md) · [DevOps → Kubernetes Delivery](devops-kubernetes-production-delivery.md)

## Source policy

DevOps là practice/lifecycle rộng, không có một specification duy nhất. Vì vậy:

- tool behavior/version-sensitive claims dùng official project/platform documentation;
- Kubernetes behavior dùng kubernetes.io;
- roadmap.sh dùng để kiểm tra breadth và learning order;
- production claims cần measured evidence trong project thật.

## CI/CD & GitHub Actions

- [GitHub Actions documentation](https://docs.github.com/en/actions)
- [Workflow artifacts](https://docs.github.com/en/actions/concepts/workflows-and-actions/workflow-artifacts)
- [Deployment environments](https://docs.github.com/en/actions/concepts/workflows-and-actions/deployment-environments)
- [Security hardening for GitHub Actions](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions)

## Terraform / Infrastructure as Code

- [Terraform language](https://developer.hashicorp.com/terraform/language)
- [Terraform state](https://developer.hashicorp.com/terraform/language/state)
- [Terraform modules](https://developer.hashicorp.com/terraform/language/modules/develop)

## Containers & Kubernetes delivery

- [Kubernetes documentation](https://kubernetes.io/docs/)
- [Kubernetes Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Kubernetes Horizontal Pod Autoscaling](https://kubernetes.io/docs/concepts/workloads/autoscaling/horizontal-pod-autoscale/)
- [Helm documentation](https://helm.sh/docs/)
- [Kustomize documentation](https://kubectl.docs.kubernetes.io/references/kustomize/)

## GitOps / Continuous Delivery

- [Argo CD documentation](https://argo-cd.readthedocs.io/en/stable/)
- [Flux documentation](https://fluxcd.io/flux/)

## Learning breadth

- [DevOps Roadmap — roadmap.sh](https://roadmap.sh/devops)
- [Kubernetes Roadmap — roadmap.sh](https://roadmap.sh/kubernetes)
- [DevOps Projects — roadmap.sh](https://roadmap.sh/devops/projects)

Roadmap.sh currently frames DevOps around collaboration/automation plus skills such as programming/scripting, cloud, containerization, CI/CD, infrastructure management and monitoring/logging. Kubernetes is treated as a separate deep technical roadmap for container orchestration. This repository mirrors that relationship: **DevOps lifecycle + Kubernetes specialization**.

## Related repository modules

- [Testing & Code Review](../08-testing-code-review/README.md)
- [Security & DevSecOps](../09-security-devsecops/README.md)
- [Docker](../12-docker/README.md)
- [Kubernetes](../15-kubernetes/README.md)
- [Azure & Platform](../14-cloud/README.md)

## Verification metadata

- Verified: 2026-08-28.
- roadmap.sh DevOps/Kubernetes pages checked against current 2026 scope.
- Official project documentation remains canonical for behavior/version-sensitive claims.
- Claude artifact URL supplied by the learner could not be fetched from this environment; no claims were copied from it.
