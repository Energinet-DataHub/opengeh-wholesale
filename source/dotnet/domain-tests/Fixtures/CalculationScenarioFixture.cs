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

using System.Diagnostics;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents;
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
            ScenarioState = new CalculationScenarioState();
        }

        public CalculationScenarioState ScenarioState { get; }

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

        public async Task<IReadOnlyCollection<IEventMessage>> WaitForIntegrationEventsAsync(
            Guid calculationId,
            IReadOnlyCollection<string> integrationEventNames,
            TimeSpan waitTimeLimit)
        {
            using var cts = new CancellationTokenSource(waitTimeLimit);
            var stopwatch = Stopwatch.StartNew();

            var receivedIntegrationEvents = new List<IEventMessage>();
            while (!cts.Token.IsCancellationRequested)
            {
                var messageOrNull = await Receiver.ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(1));
                if (messageOrNull?.Body == null)
                {
                    if (receivedIntegrationEvents.Count > 0)
                        break;
                }
                else
                {
                    var result = ShouldHandleMessage(messageOrNull, calculationId, integrationEventNames);
                    if (result.ShouldHandle)
                    {
                        await Receiver.CompleteMessageAsync(messageOrNull);
                        receivedIntegrationEvents.Add(result.EventMessage!);
                    }
                    else
                    {
                        await Receiver.AbandonMessageAsync(messageOrNull);
                    }
                }
            }

            stopwatch.Stop();
            DiagnosticMessageSink.WriteDiagnosticMessage($"Message receiver loop for calculation with id '{calculationId}' took '{stopwatch.Elapsed}' to complete. It handled a total of '{receivedIntegrationEvents.Count}' messages spanning various event types.");

            return receivedIntegrationEvents;
        }

        protected override async Task OnInitializeAsync()
        {
            await DatabricksWorkspaceManager.StartDatabrickWarehouseAsync(Configuration.DatabricksWorkspace);
            WholesaleClient = await WholesaleClientFactory.CreateAsync(Configuration, useAuthentication: true);
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

        /// <summary>
        /// Returns <see langword="true"/> if we should handle the message type; otherwise <see langword="false"/> .
        /// </summary>
        private static (bool ShouldHandle, IEventMessage? EventMessage) ShouldHandleMessage(ServiceBusReceivedMessage message, Guid calculationId, IReadOnlyCollection<string> integrationEventNames)
        {
            var shouldHandle = false;
            IEventMessage? eventMessage = null;

            if (integrationEventNames.Contains(message.Subject))
            {
                var data = message.Body.ToArray();

                switch (message.Subject)
                {
                    case CalculationResultCompleted.EventName:
                        var calculationResultCompleted = CalculationResultCompleted.Parser.ParseFrom(data);
                        if (calculationResultCompleted.BatchId == calculationId.ToString())
                        {
                            eventMessage = calculationResultCompleted;
                            shouldHandle = true;
                        }

                        break;
                    case EnergyResultProducedV2.EventName:
                        var energyResultProduced = EnergyResultProducedV2.Parser.ParseFrom(data);
                        if (energyResultProduced.CalculationId == calculationId.ToString())
                        {
                            eventMessage = energyResultProduced;
                            shouldHandle = true;
                        }

                        break;
                    case AmountPerChargeResultProducedV1.EventName:
                        var amountPerChargeResultProduced = AmountPerChargeResultProducedV1.Parser.ParseFrom(data);
                        if (amountPerChargeResultProduced.CalculationId == calculationId.ToString())
                        {
                            eventMessage = amountPerChargeResultProduced;
                            shouldHandle = true;
                        }

                        break;
                    case MonthlyAmountPerChargeResultProducedV1.EventName:
                        var monthlyAmountPerChargeResultProduced = MonthlyAmountPerChargeResultProducedV1.Parser.ParseFrom(data);
                        if (monthlyAmountPerChargeResultProduced.CalculationId == calculationId.ToString())
                        {
                            eventMessage = monthlyAmountPerChargeResultProduced;
                            shouldHandle = true;
                        }

                        break;
                }
            }

            return (shouldHandle, eventMessage);
        }
    }
}
