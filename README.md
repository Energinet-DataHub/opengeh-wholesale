# dh3-infrastructure

This repository contains:

- All infrastructure as code (IaC) for the DataHub 3 system.
- A C4 model and diagrams with focus on deployment.

## Table of contents

1. [Folders structure](#folder-structure)
2. [Infrastructure as Code (IaC)](#infrastructure-as-code-iac)

## Folder structure

This repository is structured according to the following sketch:

```txt
.
├── .github/
│   ├── actions/
│   ├── workflows/
│   ├── CODEOWNERS
│   └── dependabot.yml
│
├── .vscode/
│
├── docs/
│   └── diagrams/
│      └── c4-model/
│
├── edi/
├── market-participant/
├── migrations/
├── shard-resources/
├── ui/
├── wholesale/
│
├── .editorconfig
├── .gitignore
└── README.md
```

## Infrastructure as Code (IaC)

The IaC is implemented in Terraform, a declarative configuration language by [HashiCorp](https://www.hashicorp.com/).

All Terraform code is organized in the following folder hierarchy:

- EDI domain: [edi/terraform](./edi/terraform/)
- Market Participant domain: [market-participant/terraform](./market-participant/terraform/)
- Migrations domain: [migrations/terraform](./migrations/terraform/)
- Shared resources domain: [shared-resources/terraform](./shared-resources/terraform/)
- UI domain: [ui/terraform](./ui/terraform/)
- Wholesale domain: [wholesale/terraform](./wholesale/terraform/)

Two GitHub workflows (`<domain-name>-ci.yml` and `<domain-name>-cd.yml`) are related to each domain's infrastructure configuration. These workflows are in the `.github/workflows`-folder.

If a new subsystem is added, remember to add it to dependabot.yml for automatic provider updates.
