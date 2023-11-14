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
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    public sealed class CalculationScenarioFixture : LazyFixtureBase
    {
        private readonly string _subscriptionName = Guid.NewGuid().ToString();

        public CalculationScenarioFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            Configuration = new WholesaleDomainConfiguration();
            ServiceBusAdministrationClient = new ServiceBusAdministrationClient(Configuration.ServiceBusFullyQualifiedNamespace, new DefaultAzureCredential());
            ServiceBusClient = new ServiceBusClient(Configuration.ServiceBusConnectionString);
            Scenario = new CalculationScenario();
        }

        /// <summary>
        /// Scenario state.
        /// </summary>
        public CalculationScenario Scenario { get; }

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        private WholesaleClient_V3 WholesaleClient { get; set; } = null!;

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        private ServiceBusReceiver Receiver { get; set; } = null!;

        private WholesaleDomainConfiguration Configuration { get; }

        private ServiceBusAdministrationClient ServiceBusAdministrationClient { get; }

        private ServiceBusClient ServiceBusClient { get; }

        public async Task<Guid> StartCalculationAsync(BatchRequestDto calculationInput)
        {
            var calculationId = await WholesaleClient.CreateBatchAsync(calculationInput);
            DiagnosticMessageSink.WriteDiagnosticMessage($"Calculation for {calculationInput.ProcessType} with id '{calculationId}' started.");

            return calculationId;
        }

        public async Task<(bool IsState, BatchDto? Batch)> WaitForCalculationStateAsync(
            Guid calculationId,
            BatchState waitForState,
            TimeSpan waitTimeLimit)
        {
            var delay = TimeSpan.FromSeconds(30);

            BatchDto? batch = null;
            var isState = await Awaiter.TryWaitUntilConditionAsync(
                async () =>
                {
                    batch = await WholesaleClient.GetBatchAsync(calculationId);
                    return batch?.ExecutionState == waitForState;
                },
                waitTimeLimit,
                delay);

            DiagnosticMessageSink.WriteDiagnosticMessage($"Wait for calculation with id '{calculationId}' completed with '{nameof(isState)}={isState}'.");

            return (isState, batch);
        }

        protected override async Task OnInitializeAsync()
        {
            WholesaleClient = await WholesaleClientFactory.CreateWholesaleClientAsync(Configuration, useAuthentication: true);
            await CreateTopicSubscriptionAsync();
            Receiver = ServiceBusClient.CreateReceiver(Configuration.DomainRelayTopicName, _subscriptionName);
        }

        protected override async Task OnDisposeAsync()
        {
            await ServiceBusAdministrationClient.DeleteSubscriptionAsync(Configuration.DomainRelayTopicName, _subscriptionName);
            await ServiceBusClient.DisposeAsync();
        }

        private async Task CreateTopicSubscriptionAsync()
        {
            if (await ServiceBusAdministrationClient.SubscriptionExistsAsync(Configuration.DomainRelayTopicName, _subscriptionName))
            {
                await ServiceBusAdministrationClient.DeleteSubscriptionAsync(Configuration.DomainRelayTopicName, _subscriptionName);
            }

            var options = new CreateSubscriptionOptions(Configuration.DomainRelayTopicName, _subscriptionName)
            {
                AutoDeleteOnIdle = TimeSpan.FromHours(1),
            };

            await ServiceBusAdministrationClient.CreateSubscriptionAsync(options);
        }
    }
}
