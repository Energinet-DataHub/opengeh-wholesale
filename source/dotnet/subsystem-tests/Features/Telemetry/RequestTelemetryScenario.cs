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
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Requests.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Requests
{
    /// <summary>
    /// Verify telemetry is configured correctly so we can track http request and their dependencies.
    /// </summary>
    [TestCaseOrderer(
        ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
        ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
    public class RequestTelemetryScenario : SubsystemTestsBase<RequestTelemetryScenarioFixture>
    {
        public RequestTelemetryScenario(LazyFixtureFactory<RequestTelemetryScenarioFixture> lazyFixtureFactory)
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
        public void AndGiven_ApplicationInsightsTelemetryIsConfigured()
        {
            // Configured in startup
        }

        [ScenarioStep(2)]
        [SubsystemFact]
        public async Task When_RequestingBatchById()
        {
            await Fixture.WholesaleClient.GetBatchAsync(Fixture.ScenarioState.BatchId);
        }

////        [ScenarioStep(3)]
////        [SubsystemFact]
////        public async Task Then_MatchingRequestTelemetryShouldExist()
////        {
////            var query = $@"
////AppTraces
////| where AppRoleName == ""dbr-calculation-engine""
////| where SeverityLevel == 1 // Information
////| where Message startswith_cs ""Calculation arguments:""
////| where OperationId != ""00000000000000000000000000000000""
////| where Properties.Domain == ""wholesale""
////| where Properties.calculation_id == ""{Fixture.ScenarioState.BatchId}""
////| where Properties.CategoryName == ""Energinet.DataHub.package.calculator_job""
////| count";

////            // Assert
////            var actual = await Fixture.QueryLogAnalyticsAsync(query, new QueryTimeRange(TimeSpan.FromMinutes(60)));

////            using var assertionScope = new AssertionScope();
////            actual.Value.Table.Rows[0][0].Should().Be(1); // count == 1
////        }
    }
}
