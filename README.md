# dh3-infrastructure

This repository contains:

- All infrastructure as code (IaC) for the DataHub 3 system.
- A C4 model and diagrams with focus on deployment.

## Table of contents

1. [Folders structure](#folder-structure)
1. [Infrastructure as Code (IaC)](#infrastructure-as-code-iac)
1. [Deployment diagrams](#deployment-diagrams)

## Folder structure

This repository is structured according to the following sketch:

```txt
.
├── .github/
│   ├── actions/
│   ├── workflows/
│   └── CODEOWNERS
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

## Deployment diagrams

In the DataHub 3 project we use the [C4 model](https://c4model.com/) to document the high-level software design.

The [DataHub base model](https://github.com/Energinet-DataHub/opengeh-arch-diagrams#datahub-base-model) describes elements like organizations, software systems and actors. In domain repositories we should `extend` on this model and add additional elements within the DataHub 3.0 Software System (`dh3`).

In current infrastructure repository we also `extend` on the DataHub 3 base model and references other domains models by using `!include`. We do this to create one big model containing all _containers_, which we can then describe how are deployed onto _deployment nodes_, and visualize in _deployment diagrams_.

> Any models from private repositories must be referenced using a GitHub url and token, which means the link will only be valid for a short period of time. So when ever we want to update the deployment diagrams, we have to update the token (just get the "raw" url from Github, it will contain the token).

The deployment C4 model and rendered diagrams are located in the folder hierarchy [docs/diagrams/c4-model](./docs/diagrams/c4-model/) and consists of:

- `deployment.dsl`: Structurizr DSL extending the `dh3` software system by referencing domain C4 models using `!include`, adding containers to deployment nodes, and describing the views.
- `deployment.json`: Structurizr layout information for views.
- `/deployment/*.png`: A PNG file per view described in the Structurizr DSL.

Maintenance of the C4 model should be performed using VS Code and a local version of Structurizr Lite running in Docker. See [DataHub base model](https://github.com/Energinet-DataHub/opengeh-arch-diagrams#datahub-base-model) for a description of how to do this.
