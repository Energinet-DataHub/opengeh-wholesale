// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry
{
    /// <summary>
    /// Verify telemetry is configured correctly so we can track http requests and their dependencies.
    /// </summary>
    [TestCaseOrderer(
        ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
        ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
    public class RequestTelemetryScenario : SubsystemTestsBase<TelemetryScenarioFixture<RequestTelemetryScenarioState>>
    {
        public RequestTelemetryScenario(LazyFixtureFactory<TelemetryScenarioFixture<RequestTelemetryScenarioState>> lazyFixtureFactory)
            : base(lazyFixtureFactory)
        {
        }

        [ScenarioStep(0)]
        [SubsystemFact]
        public void Given_ExistingBatchId()
        {
            Fixture.ScenarioState.BatchId = Fixture.ExistingBatchId;
        }

        [ScenarioStep(1)]
        [SubsystemFact]
        public void AndGiven_ExpectedTelemetryEvents()
        {
            Fixture.ScenarioState.ExpectedTelemetryEvents.Add(new TelemetryEventMatch
            {
                Type = "AppRequests",
                Name = "GET Calculation/Get [batchId]",
            });
            Fixture.ScenarioState.ExpectedTelemetryEvents.Add(new TelemetryEventMatch
            {
                Type = "AppDependencies",
                NameContains = "mssqldb-data-wholsal-",
                DependencyType = "SQL",
            });
        }

        [ScenarioStep(2)]
        [SubsystemFact]
        public async Task When_RequestingBatchById()
        {
            await Fixture.WholesaleClient.GetBatchAsync(Fixture.ScenarioState.BatchId);
        }

        [ScenarioStep(3)]
        [SubsystemFact]
        public async Task Then_TelemetryEventsAreLoggedWithinWaitTime()
        {
            var query = $@"
                let OperationIds = AppRequests
                | where AppRoleName contains ""app-webapi-wholsal-""
                | where Url contains ""/v3/batches/{Fixture.ScenarioState.BatchId}""
                | order by TimeGenerated desc
                | take 1
                | project OperationId;
                OperationIds
                | join(union AppRequests, AppDependencies, AppTraces, AppExceptions) on OperationId
                | extend parsedProp = parse_json(Properties)
                | project TimeGenerated, OperationId, ParentId, Id, Type, Name, DependencyType, EventName=parsedProp.EventName, Message, Url, OuterType, OuterMessage, Properties
                | order by TimeGenerated asc";

            var wasEventsLogged = await Fixture.WaitForTelemetryEventsAsync(
                Fixture.ScenarioState.ExpectedTelemetryEvents.AsReadOnly(),
                query,
                queryTimeRange: new QueryTimeRange(TimeSpan.FromMinutes(10)),
                waitTimeLimit: TimeSpan.FromMinutes(10),
                delay: TimeSpan.FromSeconds(30));

            wasEventsLogged.Should().BeTrue("Events was not logged to Application Insights within time limit.");
        }
    }
}
