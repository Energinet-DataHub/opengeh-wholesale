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
    public sealed class TelemetryScenarioFixture<TState> : LazyFixtureBase
        where TState : new()
    {
        public TelemetryScenarioFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            Configuration = new WholesaleSubsystemConfiguration();
            LogsQueryClient = new LogsQueryClient(new DefaultAzureCredential());
            ExistingCalculationId = Configuration.Root.GetValue<Guid>("EXISTING_BATCH_ID");

            ScenarioState = new TState();
        }

        public TState ScenarioState { get; }

        /// <summary>
        /// Support calling the Wholesale Web API using an authorized Wholesale client.
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        public WholesaleClient_V3 WholesaleClient { get; private set; } = null!;

        public Guid ExistingCalculationId { get; }

        private WholesaleSubsystemConfiguration Configuration { get; }

        private LogsQueryClient LogsQueryClient { get; }

        public async Task<Guid> StartCalculationAsync(CalculationRequestDto calculationInput)
        {
            var calculationId = await WholesaleClient.CreateCalculationAsync(calculationInput);
            DiagnosticMessageSink.WriteDiagnosticMessage($"Fixture {GetType().Name} - Calculation for {calculationInput.CalculationType} with id '{calculationId}' started.");

            return calculationId;
        }

        public async Task<bool> WaitForTelemetryEventsAsync(
            IReadOnlyCollection<TelemetryEventMatch> expectedEvents,
            string query,
            QueryTimeRange queryTimeRange,
            TimeSpan waitTimeLimit,
            TimeSpan delay)
        {
            var wasEventsLogged = await Awaiter
                .TryWaitUntilConditionAsync(
                    async () =>
                    {
                        var actualResponse = await LogsQueryClient.QueryWorkspaceAsync<TelemetryQueryResult>(
                            Configuration.LogAnalyticsWorkspaceId,
                            query,
                            queryTimeRange);

                        return TelemetryScenarioFixture<TState>.ContainsExpectedEvents(expectedEvents, actualResponse.Value);
                    },
                    waitTimeLimit,
                    delay);

            return wasEventsLogged;
        }

        protected override async Task OnInitializeAsync()
        {
            WholesaleClient = await WholesaleClientFactory.CreateAsync(Configuration, useAuthentication: true);
        }

        protected override Task OnDisposeAsync()
        {
            return Task.CompletedTask;
        }

        private static bool ContainsExpectedEvents(IReadOnlyCollection<TelemetryEventMatch> expectedEvents, IReadOnlyList<TelemetryQueryResult> actualResults)
        {
            if (actualResults.Count < expectedEvents.Count)
                return false;

            foreach (var expected in expectedEvents)
            {
                if (!actualResults.Any(actual => expected.IsMatch(actual)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
