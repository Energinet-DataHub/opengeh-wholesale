# dh3-infrastructure

This repository contains all infrastructure as code (IaC) for the DataHub 3 system. The IaC is implemented in Terraform, a declarative configuration language by [HashiCorp](https://www.hashicorp.com/).

All Terraform code is organized in the following folder hierarchy:

- EDI domain: [edi/terraform](./edi/terraform/)
- Market Participant domain: [market-participant/terraform](./market-participant/terraform/)
- Migrations domain: [migrations/terraform](./migrations/terraform/)
- Shared resources domain: [shared-resources/terraform](./shared-resources/terraform/)
- UI domain: [ui/terraform](./ui/terraform/)
- Wholesale domain: [wholesale/terraform](./wholesale/terraform/)

Two GitHub workflows (`<domain-name>-ci.yml` and `<domain-name>-cd.yml`) are related to each domain's infrastructure configuration. These workflows are in the `.github/workflows`-folder.
