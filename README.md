# dh3-infrastructure

This repository contains all infrastructure as code (IaC) for the DataHub 3 system. The IaC is implemented in Terraform, a declarative configuration language by [HashiCorp](https://www.hashicorp.com/).

All Terraform code is organized in the following folder hierarchy:

- Charges domain: [charges/terraform](./charges/terraform/)
- ...

Two GitHub workflows (`<domain-name>-ci.yml` and `<domain-name>-cd.yml`) are related to each domain's infrastructure configuration. These workflows are in the `.github/workflows`-folder.
