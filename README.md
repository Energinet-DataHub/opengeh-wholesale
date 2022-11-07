# dh3-infrastructure

This repository contains all infrastructure as code (IaC) for the DataHub 3 system. The IaC is implemented in Terraform, a declarative configuration language by [HashiCorp](https://www.hashicorp.com/).

All Terraform code is organized in the following folder hierarchy:

- Charges domain: [charges/terraform](./charges/terraform/)
- Market Participant domain: [market-participant/terraform](./market-participant/terraform/)
- Message Archive domain: [message-archive/terraform](./message-archive/terraform/)
- Shared resources domain: [shared-resources/terraform](./shared-resources/terraform/)
- Post Office domain: [post-office/terraform](./post-office/terraform/)
- Timeseries domain: [timeseries/terraform](./timeseries/terraform/)
- Wholesale domain: [wholesale/terraform](./wholesale/terraform/)
- Watt domain: [watt/terraform](./watt/terraform/)
- ...

Two GitHub workflows (`<domain-name>-ci.yml` and `<domain-name>-cd.yml`) are related to each domain's infrastructure configuration. These workflows are in the `.github/workflows`-folder.
