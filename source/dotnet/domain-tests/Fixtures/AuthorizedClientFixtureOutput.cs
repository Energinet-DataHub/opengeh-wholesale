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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using ProcessType = Energinet.DataHub.Wholesale.DomainTests.Clients.v3.ProcessType;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Support testing Wholesale Web API using an authorized Wholesale client.
    /// </summary>
    public sealed class AuthorizedClientFixtureOutput
    {
        private readonly WholesaleClient_V3 _wholesaleClient;
        private readonly ServiceBusReceiver _receiver;

        public AuthorizedClientFixtureOutput(WholesaleClient_V3 wholesaleClient, ServiceBusReceiver receiver)
        {
            _wholesaleClient = wholesaleClient;
            _receiver = receiver;
        }

        public bool BalanceFixingCalculationIsComplete { get; private set; }

        public bool WholesaleFixingCalculationIsComplete { get; private set; }

        public List<CalculationResultCompleted> CalculationResultCompletedFromBalanceFixing { get; } = new();

        public List<CalculationResultCompleted> CalculationResultCompletedFromWholesaleFixing { get; } = new();

        public List<EnergyResultProducedV1> EnergyResultProducedFromBalanceFixing { get; } = new();

        public List<EnergyResultProducedV1> EnergyResultProducedFromWholesaleFixing { get; } = new();

        private Guid BalanceFixingCalculationId { get; set; }

        private Guid WholesaleFixingCalculationId { get; set; }

        public async Task InitializeAsync()
        {
            BalanceFixingCalculationId = await StartBalanceFixingCalculation();
            WholesaleFixingCalculationId = await StartWholesaleFixingCalculation();

            BalanceFixingCalculationIsComplete = await WaitForCalculationToComplete(BalanceFixingCalculationId);
            WholesaleFixingCalculationIsComplete = await WaitForCalculationToComplete(WholesaleFixingCalculationId);

            await CollectResultsFromServiceBus();
        }

        private async Task<Guid> StartBalanceFixingCalculation()
        {
            var startDate = new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero);
            var endDate = new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero);
            var batchRequestDto = new BatchRequestDto
            {
                ProcessType = ProcessType.BalanceFixing,
                GridAreaCodes = new List<string> { "543" },
                StartDate = startDate,
                EndDate = endDate,
            };
            return await _wholesaleClient.CreateBatchAsync(batchRequestDto);
        }

        private async Task<Guid> StartWholesaleFixingCalculation()
        {
            var startDate = new DateTimeOffset(2023, 1, 31, 23, 0, 0, TimeSpan.Zero);
            var endDate = new DateTimeOffset(2023, 2, 28, 23, 0, 0, TimeSpan.Zero);
            var batchRequestDto = new BatchRequestDto
            {
                ProcessType = ProcessType.WholesaleFixing,
                GridAreaCodes = new List<string> { "804" },
                StartDate = startDate,
                EndDate = endDate,
            };
            return await _wholesaleClient.CreateBatchAsync(batchRequestDto);
        }

        private async Task<bool> WaitForCalculationToComplete(Guid calculationId)
        {
            var defaultTimeout = TimeSpan.FromMinutes(15);
            var defaultDelay = TimeSpan.FromSeconds(30);
            var stopwatch = Stopwatch.StartNew();
            var isCompleted = await Awaiter.TryWaitUntilConditionAsync(
                async () =>
                {
                    var batchResult = await _wholesaleClient.GetBatchAsync(calculationId);
                    return batchResult?.ExecutionState == BatchState.Completed;
                },
                defaultTimeout,
                defaultDelay);
            stopwatch.Stop();
            Console.WriteLine($"LOOK AT ME: Calculation took {stopwatch.Elapsed} to complete");
            return isCompleted;
        }

        private async Task CollectResultsFromServiceBus()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15));

            var stopwatch = Stopwatch.StartNew();

            while (!cts.Token.IsCancellationRequested)
            {
                var message = await _receiver.ReceiveMessageAsync();
                if (message?.Body == null)
                {
                    if (HasAlreadyReceivedMessage())
                        break;
                }
                else
                {
                    HandleMessage(message);
                }
            }

            stopwatch.Stop();

            Console.WriteLine($"LOOK AT ME: the loop took {stopwatch.Elapsed} to complete and received {CalculationResultCompletedFromBalanceFixing.Count} messages");
        }

        private void HandleMessage(ServiceBusReceivedMessage message)
        {
            var data = message.Body.ToArray();
            switch (message.Subject)
            {
                case CalculationResultCompleted.EventName:
                    var calculationResultCompleted = CalculationResultCompleted.Parser.ParseFrom(data);
                    if (calculationResultCompleted.BatchId == BalanceFixingCalculationId.ToString())
                        CalculationResultCompletedFromBalanceFixing.Add(calculationResultCompleted);
                    else if (calculationResultCompleted.BatchId == WholesaleFixingCalculationId.ToString())
                        CalculationResultCompletedFromWholesaleFixing.Add(calculationResultCompleted);
                    break;
                case EnergyResultProducedV1.EventName:
                    var energyResultProduced = EnergyResultProducedV1.Parser.ParseFrom(data);
                    if (energyResultProduced.CalculationId == BalanceFixingCalculationId.ToString())
                        EnergyResultProducedFromBalanceFixing.Add(energyResultProduced);
                    else if (energyResultProduced.CalculationId == WholesaleFixingCalculationId.ToString())
                        EnergyResultProducedFromWholesaleFixing.Add(energyResultProduced);
                    break;
            }
        }

        private bool HasAlreadyReceivedMessage()
        {
            return CalculationResultCompletedFromBalanceFixing.Any() || CalculationResultCompletedFromWholesaleFixing.Any();
        }
    }
}
