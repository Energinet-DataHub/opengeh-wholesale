# Wholesale - Test Strategy (Draft)

This document describes the test strategy for the Wholesale domain in Datahub 3.0 at Energinet

## Motivation

Why do we test?

The business has zero tolerance for calculation errors.

To prove that the system works as described by the business and continues to work as described.

Tests give the confidence that is required to do continuous delivery.

## Testing Methods

Our test suite can be divided into these categories:

- Unit test

- Integration test

- Domain test

- Infrastructure test

- Performance testing

We strive to follow the ‘Shift Left’ principle, where we test at the early stages in the pipeline. In practice, this means that if it serves the purpose, we prefer unit test over integration test and integration tests over domain tests. However, when deciding on a test method always consider the workload involved - do the cost/benefit consideration. Note: we also want to do test in all stages of the development process.

## Unit Test

### Definition

To avoid confusion or ambiguous interpretation of the term “Unit”, the term Unit Test refers solely to test done to an individual semantical unit of written or produced piece of code or text.  A unit test is hence a test that test:

Test of a single method, property or class.

Test contracts adherence.

All dependencies of the unit under test are presumed mocked.

### Objective

By isolating and executing a single unit of code, we can analyse files before system execution.

### Methods and Implementation

C# Mocked test using xUnit and Moq

Python Mocked tests using Pytest and unittest.mock

### Metrics and Evaluation

.NET code Coverage is performed by CodeCov

We don’t evaluate it, and we have no targets.

## Integration Test

### Definition

Test that a component in combination with other components in the system. Examples of components are Azure Functions and App Services

### Objective

Ensure that the connectivity between components meets the requirements

Verify that units inside a component work well together when combined. No extensively testing is performed, only happy flow is verified.

### Methods and Implementation (.NET)

We normally act and assert on the components' external boundaries.

The integration tests always involve interaction with its dependencies (other components). These dependencies can be set up using:

Stub/spy/mocks: They resemble the actual dependency they are replacing, but with as little functionality as possible to serve as a stand-in during testing. Examples: DatabricksTestHttpListener class which emulates as a web server for Databricks REST API.

Third party emulators:

Azurite: open-source emulator provides a free local environment for testing Azure Blob, Queue Storage, and Table Storage applications.

LocalDB in integration tests to emulate Azure SQL database (available from SqlServerDataManager in TestCommon).

Resource provisioning: sometimes we don’t have an easy way to emulate dependencies. In such cases we use an in-house developed library called TestCommon. This library enables us to test components dependent on Azure resources.

Whenever possible we want our integration test to have short feedback loops and not be fragile. Therefore, we use TestCommon tests that rely on provisioned resources as a last resort - they are slow and require Azure access.

### Methods and Implementation (Python)

Integration testing on our Python codebase diverts a bit from the approach used with .NET.

The goal for the tests is to verify the interaction between the python code (which is later deployed as a wheel to the Databricks workspace) and Spark. This is performed to conform to the business wish to have zero calculation errors.

Test are executed on a Spark cluster provisioned within a Docker container.

### Metrics and Evaluation

None at the moment.

### Performance issues

To ensure short feedback only a single calculation is executed with multiple tests later asserting on the result.

## Domain Test

### Definition

End2end test within the Wholesale domain. The relevant artifacts are deployed and tested on a live Azure environment.

### Objective

Catch bugs that are not found in integration and unit test

Build confidence in that the Wholesale domain works end2end

Guard T001 environment against potential against breaking

The intension is not to do exhaustive testing but rather to do smoke tests end2end via endpoints

### Methods and Implementation

The domain tests act on the external interfaces of the Wholesale domain. This could be the http endpoints that are available via the wholesale NuGet package. It could also be interactions with events or queues. Tests are located in a dedicated project in the wholesale repository (see here). They are executed as part of the CD pipeline in U001. If the domain test fails then the CD step fails, and there will be no deployment to T001.

Example:

Act: starts the calculation of a batch

Assert: Verify that calculation reaches ‘Completed’ state before timeout. We don’t check the content of the calculation - that is assumed to be covered by other types of tests.

Domain tests are currently the only tests that use Databricks - our integration tests use Spark, but not in a Databricks context.

Before adding new domain test, always consider if you can shift-left: can the test be done as a unit or integration test? Domain tests give late feedback, and the code is already in main, so only add new domain tests if there are no good alternatives.

### Metrics and Evaluation

None at the moment.

### Infrastructure Test

### Definition

Ensuring and validating any provisioned infrastructure.

Presence of services and/or resources and also validating their state and configuration.

### Objective

Validate system state on a live environment before reaching test or production environments

Deployment verification in a CD Pipeline

### Methods and Implementation

- HealthCheck's
    - Usually HealthCheck's are implemented for monitoring, but in this case, we also use it as a way of verifying connectivity and authorization between components.

    - As a rule of thumb, when creating a resource capable of a HealthCheck, one should be implemented.

    - Currently Azure-functions and Web Apps are covered. Databricks workspace is uncovered.

All HealthCheck's are called in the CD pipeline when the development environment(U001) is propagated. If any checks fail, further deployment is hindered. This is to protect the deploying faulty code to the test and production environment.

Example: An Azure-functions HealthCheck would consist of checking its connection to a service bus and a SQL server database.

### Metrics and Evaluation

None at the moment.

## Performance Test

### Definition

Test that verifies that the system meets a business time (performance?) requirement.

### Objective

- Verify that the system meets the timely requirements defined by the business.

- Verify that changes to the system don’t deteriorate the system’s performance

### Methods and Implementation

There is as of, yet no chosen method, or implementations made.

### Metrics and Evaluation

None at the moment.
