# dh3-infrastructure

## Infrastructure as Code (IaC)

This repository contains all infrastructure as code (IaC) for the DataHub 3 system. The IaC is implemented in Terraform, a declarative configuration language by [HashiCorp](https://www.hashicorp.com/).

All Terraform code is organized in the following folder hierarchy:

- EDI domain: [edi/terraform](./edi/terraform/)
- Market Participant domain: [market-participant/terraform](./market-participant/terraform/)
- Migrations domain: [migrations/terraform](./migrations/terraform/)
- Shared resources domain: [shared-resources/terraform](./shared-resources/terraform/)
- UI domain: [ui/terraform](./ui/terraform/)
- Wholesale domain: [wholesale/terraform](./wholesale/terraform/)

Two GitHub workflows (`<domain-name>-ci.yml` and `<domain-name>-cd.yml`) are related to each domain's infrastructure configuration. These workflows are in the `.github/workflows`-folder.

## Deployment diagrams

The repository does also contain C4 models and diagrams with focus on deployment diagrams. These are located in the folder [c4-model](./c4-model/).

Since we have a branch protection rule that requires a branch policy status check we need a job to run when we make any changes to the `c4-model` folder. This is handled by the workflows files `c4-model-ci.yml` and `c4-model-status-check.yml`.
