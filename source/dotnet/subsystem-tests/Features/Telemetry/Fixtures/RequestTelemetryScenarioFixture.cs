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

using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Requests.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Requests.Fixtures
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

        public Task<Response<LogsQueryResult>> QueryLogAnalyticsAsync(string query, QueryTimeRange queryTimeRange)
        {
            return LogsQueryClient.QueryWorkspaceAsync(Configuration.LogAnalyticsWorkspaceId, query, queryTimeRange);
        }

        protected override async Task OnInitializeAsync()
        {
            WholesaleClient = await WholesaleClientFactory.CreateAsync(Configuration, useAuthentication: true);
        }

        protected override Task OnDisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
