﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Azure.Monitor.Query;
using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Core.TestCommon.Xunit.LazyFixture;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.States;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry;

/// <summary>
/// Verify telemetry is configured correctly so we can track http requests, dependencies, exceptions and traces.
/// See also: https://learn.microsoft.com/en-us/azure/azure-monitor/app/data-model-complete
/// </summary>
[ExecutionContext(AzureEnvironment.AllDev)]
[TestCaseOrderer(
    ordererTypeName: TestCaseOrdererLocation.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererLocation.OrdererAssemblyName)]
public class RequestTelemetryScenario : SubsystemTestsBase<TelemetryScenarioFixture<RequestTelemetryScenarioState>>
{
    public RequestTelemetryScenario(LazyFixtureFactory<TelemetryScenarioFixture<RequestTelemetryScenarioState>> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_UnknownCalculationId()
    {
        Fixture.ScenarioState.CalculationId = Guid.NewGuid();
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public void AndGiven_ExpectedTelemetryEvents()
    {
        Fixture.ScenarioState.ExpectedTelemetryEvents.Add(new AppRequestMatch
        {
            AppVersionContains = "PR:",
            Subsystem = "wholesale-dotnet",
            Name = "GET Calculation/Get [calculationId]",
        });
        Fixture.ScenarioState.ExpectedTelemetryEvents.Add(new AppDependencyMatch
        {
            AppVersionContains = "PR:",
            Subsystem = "wholesale-dotnet",
            NameContains = "mssqldb-data-wholsal-",
            DependencyType = "SQL",
        });
        Fixture.ScenarioState.ExpectedTelemetryEvents.Add(new AppExceptionMatch
        {
            AppVersionContains = "PR:",
            Subsystem = "wholesale-dotnet",
            EventName = "ApplicationError",
            OuterType = "System.InvalidOperationException",
            OuterMessage = "Sequence contains no elements.",
        });
    }

    [ScenarioStep(2)]
    [SubsystemFact]
    public async Task When_RequestingCalculationById()
    {
        var act = async () => await Fixture.WholesaleWebApiClient.GetCalculationAsync(Fixture.ScenarioState.CalculationId);

        // Assert request is failing
        await act.Should().ThrowAsync<Clients.v3.ApiException>();
    }

    [ScenarioStep(3)]
    [SubsystemFact(Skip = "Telemetry events are not logged within time limit. Maybe because of sampling")]
    public async Task Then_TelemetryEventsAreLoggedWithinWaitTime()
    {
        var query = $@"
                 let OperationIds = AppRequests
                 | where AppRoleName contains ""api-wholsal-""
                 | where Url contains ""/v3/calculations/{Fixture.ScenarioState.CalculationId}""
                 | order by TimeGenerated desc
                 | take 1
                 | project OperationId;
                 OperationIds
                 | join(union AppRequests, AppDependencies, AppTraces, AppExceptions) on OperationId
                 | extend parsedProp = parse_json(Properties)
                 | project TimeGenerated, OperationId, ParentId, Id, Type, AppVersion, Subsystem=parsedProp.Subsystem, Name, DependencyType, EventName=parsedProp.EventName, Message, Url, OuterType, OuterMessage, Properties
                 | order by TimeGenerated asc";

        var wasEventsLogged = await Fixture.WaitForTelemetryEventsAsync(
            Fixture.ScenarioState.ExpectedTelemetryEvents.AsReadOnly(),
            query,
            queryTimeRange: new QueryTimeRange(TimeSpan.FromMinutes(12)),
            waitTimeLimit: TimeSpan.FromMinutes(12),
            delay: TimeSpan.FromSeconds(30));

        wasEventsLogged.Should().BeTrue($"{nameof(Fixture.ScenarioState.ExpectedTelemetryEvents)} was not logged to Application Insights within time limit.");
    }
}
