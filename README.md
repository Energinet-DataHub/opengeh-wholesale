# Wholesale

[![`codecov`](https://codecov.io/gh/Energinet-DataHub/opengeh-wholesale/branch/main/graph/badge.svg?token=YG4H2IATQ1)](https://codecov.io/gh/Energinet-DataHub/opengeh-wholesale)

[![SonarCloud Status](https://sonarcloud.io/api/project_badges/measure?project=opengeh-wholesale-python&metric=alert_status)](https://sonarcloud.io/dashboard?id=opengeh-wholesale-python)

[![SonarCloud Status](https://sonarcloud.io/api/project_badges/measure?project=opengeh-wholesale-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=opengeh-wholesale-dotnet)

* [Introduction](#introduction)
* [Understanding the Domain](#understanding-the-domain)
* [Getting Started for Developers](#getting-started-for-developers)
    * [Running Databricks locally](#running-databricks-locally)
    * [Writing Test](#writing-tests)
    * [Committing to Source Control](#committing-to-source-control)
    * [Consuming Integration Events](#consuming-integration-events)
    * [Web API](#web-api)
    * [Calculation Input Data Format](#calculation-input-data-format)
    * [FAQ](#faq)
* [Where can I get more help?](#where-can-i-get-more-help)
* [Domain C4 model](#domain-c4-model)

## Introduction

The wholesale domain is in charge of doing calculations on the time series sent to DataHub and executing the balance and wholesale settlement process.

The main calculations the domain is responsible to process are consumption, production, exchange between grid areas and the current grid loss within a grid area.
All calculations return a result for grid area, balance responsible parties and energy suppliers.

The times series sent to DataHub is processed and prepared for calculations in the (private) migration domain.

The calculated results are packaged and forwarded to the legitimate market participants:

| Market Participants |
| ----------- |
| Grid Access Provider  |
| Balance Responsible Party |
| Energy Supplier |
| eSett |

These are the business processes maintained by this domain:

| Processes |
| ------------ |
| [Submission of calculated energy time series](docs/business-processes/submission-of-calculated-energy-time-series.md) |
| [Request for calculated energy time series](docs/business-processes/request-for-calculated-energy-time-series.md) |
| [Aggregation of wholesale services](docs/business-processes/aggregation-of-wholesale-services.md) |
| [Request for aggregated subscriptions or fees](docs/business-processes/request-for-aggregated-subscriptions-or-fees.md) |
| [Request for aggregated tariffs](docs/business-processes/request-for-aggregated-tariffs.md) |
| [Request for settlement basis](docs/business-processes/request-for-settlement-basis.md) |

## Understanding the Domain

This is the context map.

![Context Map!](docs/images/context-map.drawio.png)

Read about the architecture [here](docs/architecture.md).

![Architecture!](docs/images/architecture.drawio.png)

Event flows in the domain.

![Events!](docs/images/events.drawio.png)

Learn about the wholesale ubiquitous language [here](docs/ubiquitous-language.md).

## Getting Started for Developers

The source code of the repository is provided as-is. We currently do not accept contributions or forks from outside the project driving the current development.

For people on the project please read on for details on how to contribute or integrate with the domain.

### Running Databricks locally

[Databricks readme](source/databricks#readme)

### Writing Tests

In team Mandolorian we have agreed on a test strategy which is located [here](docs/test-strategy.md)

When writing unit tests we strive to write the test method names the following way: `MemberName_WhenSomething_IsOrDoes`. Where member name is usually a method name but can also be a property name. The "WhenSomething" part can be left out in cases where it doesn't add any meaningful value. "IsOrDoes" represents the expected behavior like "returning", "throwing" or a side-effect.
an example can be found [here](source/dotnet/wholesale-api/Batches/Batches.UnitTests/Infrastructure/BatchAggregate/BatchTests.cs)

Read about general QA that applies to the entire Green Energy Hub [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/quality-assurance-and-test.md).

### Committing to Source Control

Contributions are provided by the means of GitHub pull-requests. Pull-requests titles and commit messages should adhere to [this guide](https://github.com/Mech0z/GitHubGuidelines).

### Consuming Integration Events

When a process has been completed the wholesale domain publishes an integration event for each calculation result.
The number of calculation results per completed process may be massive. In the danish electricity market, the number may be thousands of results.
The events contain the data defined by the
[`calculation_result_completed.proto`](source/dotnet/wholesale-api/Infrastructure/IntegrationEvents/calculation_result_completed.proto) Google Protocol Buffers contract.

The process type is specified in the message type meta data of the transport messages according to `ADR-008`.

The set of supported process types can be found in [`calculation_result_completed.proto`](source/dotnet/wholesale-api/Infrastructure/IntegrationEvents/CalculationResultCompleted.cs).

### Web API

Process results can be fetched using [the wholesale web API](source/dotnet/wholesale-api/).

We expose a swagger.json from which a client can be generated using [NSwag](https://github.com/RicoSuter/NSwag). Check out a example [here!](https://github.com/Energinet-DataHub/greenforce-frontend/tree/main/apps/dh/api-dh/source/DataHub.WebApi/Clients/Wholesale/V3)

### Calculation Input Data Format

Read about the contracts [here](docs/inter-domain-integration/README.md).

### FAQ

Read the FAQ for developers [here](docs/developer-faq.md).

## Where can I get more help?

Read about community for Green Energy Hub [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/COMMUNITY.md) and learn about how to get involved and get help.

Please note that we have provided a [Dictionary](https://github.com/Energinet-DataHub/green-energy-hub/tree/main/docs/dictionary-and-concepts) to help understand many of the terms used throughout the repository.

## Domain C4 model

In the DataHub 3 project we use the [C4 model](https://c4model.com/) to document the high-level software design.

The [DataHub 3 base model](https://github.com/Energinet-DataHub/opengeh-arch-diagrams#datahub-3-base-model) describes elements like organizations, software systems and actors. In domain repositories we should `extend` on this model and add additional elements within the DataHub 3.0 Software System (`dh3`).

The domain C4 model and rendered diagrams are located in the folder hierarchy [docs/diagrams/c4-model](./docs/diagrams/c4-model/) and consists of:

* `model.dsl`: Structurizr DSL describing the domain C4 model.
* `views.dsl`: Structurizr DSL extending the `dh3` software system by referencing domain C4 models using `!include`, and describing the views.
* `views.json`: Structurizr layout information for views.
* `/views/*.png`: A PNG file per view described in the Structurizr DSL.

Maintenance of the C4 model should be performed using VS Code and a local version of Structurizr Lite running in Docker. See [DataHub 3 base model](https://github.com/Energinet-DataHub/opengeh-arch-diagrams#datahub-3-base-model) for a description of how to do this.
