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

using Azure.Identity;
using Azure.Monitor.Query;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.Fixtures
{
    public sealed class RequestTelemetryScenarioFixture : LazyFixtureBase
    {
        public RequestTelemetryScenarioFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            Configuration = new WholesaleSubsystemConfiguration();
            LogsQueryClient = new LogsQueryClient(new DefaultAzureCredential());
            ExistingBatchId = Configuration.Root.GetValue<Guid>("EXISTING_BATCH_ID");

            ScenarioState = new RequestTelemetryScenarioState();
        }

        public RequestTelemetryScenarioState ScenarioState { get; }

        /// <summary>
        /// Support calling the Wholesale Web API using an authorized Wholesale client.
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        public WholesaleClient_V3 WholesaleClient { get; private set; } = null!;

        public Guid ExistingBatchId { get; }

        private WholesaleSubsystemConfiguration Configuration { get; }

        private LogsQueryClient LogsQueryClient { get; }

        public async Task<int> WaitForTelemetryEventsAsync(
            IReadOnlyCollection<TelemetryEventMatch> expectedEvents,
            string query,
            QueryTimeRange queryTimeRange,
            TimeSpan waitTimeLimit,
            TimeSpan delay)
        {
            var actualCount = 0;
            var wasEventsLogged = await Awaiter
                .TryWaitUntilConditionAsync(
                    async () =>
                    {
                        var actualResponse = await LogsQueryClient.QueryWorkspaceAsync<TelemetryQueryResult>(
                            Configuration.LogAnalyticsWorkspaceId,
                            query,
                            queryTimeRange);

                        actualCount = actualResponse.Value.Count;
                        return ContainsExpectedEvents(expectedEvents, actualResponse.Value);
                    },
                    waitTimeLimit,
                    delay);

            // We can only trust actual count if the events was logged, and the wait didn't timeout
            return wasEventsLogged
                ? actualCount
                : 0;
        }

        protected override async Task OnInitializeAsync()
        {
            WholesaleClient = await WholesaleClientFactory.CreateAsync(Configuration, useAuthentication: true);
        }

        protected override Task OnDisposeAsync()
        {
            return Task.CompletedTask;
        }

        private bool ContainsExpectedEvents(IReadOnlyCollection<TelemetryEventMatch> expectedEvents, IReadOnlyList<TelemetryQueryResult> actualResults)
        {
            if (actualResults.Count < expectedEvents.Count)
                return false;

            foreach (var expected in expectedEvents)
            {
                switch (expected.Type)
                {
                    case "AppRequests":
                        var appRequestsExists = actualResults.Any(actual =>
                            actual.Name == expected.Name);

                        if (!appRequestsExists)
                        {
                            DiagnosticMessageSink.WriteDiagnosticMessage($"Did not find expected AppRequests: Name='{expected.Name}'");
                            return false;
                        }

                        break;

                    case "AppDependencies":
                        var appDependenciesExists = false;

                        if (!string.IsNullOrEmpty(expected.NameContains))
                        {
                            // Compare using NameContains
                            appDependenciesExists = actualResults.Any(actual =>
                                actual.Name.Contains(expected.NameContains)
                                && actual.DependencyType == expected.DependencyType);
                        }
                        else
                        {
                            // Compare using Name
                            appDependenciesExists = actualResults.Any(actual =>
                                actual.Name == expected.Name
                                && actual.DependencyType == expected.DependencyType);
                        }

                        if (!appDependenciesExists)
                        {
                            DiagnosticMessageSink.WriteDiagnosticMessage($"Did not find expected AppDependencies: Name='{expected.Name}' NameContains='{expected.NameContains}' DependencyType='{expected.DependencyType}'");
                            return false;
                        }

                        break;

                    // "AppTraces"
                    default:
                        var appTracesExists = actualResults.Any(actual =>
                            actual.EventName == expected.EventName
                            && actual.Message.StartsWith(expected.Message));

                        if (!appTracesExists)
                        {
                            DiagnosticMessageSink.WriteDiagnosticMessage($"Did not find expected AppTrace: EventName='{expected.EventName}' Message='{expected.Message}'");
                            return false;
                        }

                        break;
                }
            }

            return true;
        }
    }
}
