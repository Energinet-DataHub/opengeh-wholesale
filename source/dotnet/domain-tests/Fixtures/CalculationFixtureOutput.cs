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
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Extensions;
using Xunit.Abstractions;
using ProcessType = Energinet.DataHub.Wholesale.DomainTests.Clients.v3.ProcessType;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Support testing Wholesale Web API using an authorized Wholesale client.
    /// </summary>
    public sealed class CalculationFixtureOutput
    {
        private readonly IMessageSink _diagnosticMessageSink;
        private readonly WholesaleClient_V3 _wholesaleClient;
        private readonly ServiceBusReceiver _receiver;

        public CalculationFixtureOutput(IMessageSink diagnosticMessageSink, WholesaleClient_V3 wholesaleClient, ServiceBusReceiver receiver)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
            _wholesaleClient = wholesaleClient;
            _receiver = receiver;
        }

        public bool BalanceFixingCalculationIsComplete { get; private set; }

        public bool WholesaleFixingCalculationIsComplete { get; private set; }

        public List<CalculationResultCompleted> CalculationResultCompletedFromBalanceFixing { get; } = new();

        public List<CalculationResultCompleted> CalculationResultCompletedFromWholesaleFixing { get; } = new();

        public List<EnergyResultProducedV2> EnergyResultProducedFromBalanceFixing { get; } = new();

        public List<EnergyResultProducedV2> EnergyResultProducedFromWholesaleFixing { get; } = new();

        public List<AmountPerChargeResultProducedV1> AmountPerChargeResultProduced { get; } = new();

        public List<MonthlyAmountPerChargeResultProducedV1> MonthlyAmountPerChargeResultProduced { get; } = new();

        private Guid BalanceFixingCalculationId { get; set; }

        private Guid WholesaleFixingCalculationId { get; set; }

        public async Task InitializeAsync()
        {
            await Task.WhenAll(
                HandleBalanceFixingCalculationAsync(calculationTimeLimit: TimeSpan.FromMinutes(15)),
                HandleWholesaleFixingCalculationAsync(calculationTimeLimit: TimeSpan.FromMinutes(25)));

            await ReceiveServiceBusMessagesAsync(loopTimeLimit: TimeSpan.FromMinutes(10));
        }

        private async Task HandleBalanceFixingCalculationAsync(TimeSpan calculationTimeLimit)
        {
            BalanceFixingCalculationId = await StartBalanceFixingCalculationAsync();
            _diagnosticMessageSink.WriteDiagnosticMessage($"Calculation for BalanceFixing with id '{BalanceFixingCalculationId}' started.");

            var stopwatch = Stopwatch.StartNew();
            BalanceFixingCalculationIsComplete = await WaitForCalculationToCompleteAsync(BalanceFixingCalculationId, calculationTimeLimit);
            stopwatch.Stop();
            _diagnosticMessageSink.WriteDiagnosticMessage($"Calculation for BalanceFixing completed. Calculation took '{stopwatch.Elapsed}'.");
        }

        private async Task HandleWholesaleFixingCalculationAsync(TimeSpan calculationTimeLimit)
        {
            WholesaleFixingCalculationId = await StartWholesaleFixingCalculationAsync();
            _diagnosticMessageSink.WriteDiagnosticMessage($"Calculation for WholesaleFixing with id '{WholesaleFixingCalculationId}' started.");

            var stopwatch = Stopwatch.StartNew();
            WholesaleFixingCalculationIsComplete = await WaitForCalculationToCompleteAsync(WholesaleFixingCalculationId, calculationTimeLimit);
            stopwatch.Stop();
            _diagnosticMessageSink.WriteDiagnosticMessage($"Calculation for WholesaleFixing completed. Calculation took '{stopwatch.Elapsed}'.");
        }

        private Task<Guid> StartBalanceFixingCalculationAsync()
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
            return _wholesaleClient.CreateBatchAsync(batchRequestDto);
        }

        private Task<Guid> StartWholesaleFixingCalculationAsync()
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
            return _wholesaleClient.CreateBatchAsync(batchRequestDto);
        }

        private async Task<bool> WaitForCalculationToCompleteAsync(Guid calculationId, TimeSpan timeLimit)
        {
            var delay = TimeSpan.FromSeconds(30);
            var isCompleted = await Awaiter.TryWaitUntilConditionAsync(
                async () =>
                {
                    var batchResult = await _wholesaleClient.GetBatchAsync(calculationId);
                    return batchResult?.ExecutionState == BatchState.Completed;
                },
                timeLimit,
                delay);
            return isCompleted;
        }

        private async Task ReceiveServiceBusMessagesAsync(TimeSpan loopTimeLimit)
        {
            using var cts = new CancellationTokenSource(loopTimeLimit);

            var stopwatch = Stopwatch.StartNew();

            var messageCount = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                var messageOrNull = await _receiver.ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(1));
                if (messageOrNull?.Body == null)
                {
                    if (messageCount > 0)
                        break;
                }
                else
                {
                    if (await TryHandleMessageAsync(messageOrNull))
                    {
                        messageCount++;
                    }
                }
            }

            stopwatch.Stop();

            _diagnosticMessageSink.WriteDiagnosticMessage($"Message receiver loop took '{stopwatch.Elapsed}' to complete. It handled a total of '{messageCount}' messages spanning various event types.");
            if (stopwatch.Elapsed >= loopTimeLimit)
            {
                _diagnosticMessageSink.WriteDiagnosticMessage($"No messages received within the time limit of '{loopTimeLimit}'. The loop was stopped.");
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if we handled the message type; otherwise <see langword="false"/> .
        /// </summary>
        private async Task<bool> TryHandleMessageAsync(ServiceBusReceivedMessage message)
        {
            var messageHandled = false;
            var data = message.Body.ToArray();

            switch (message.Subject)
            {
                case CalculationResultCompleted.EventName:
                    var calculationResultCompleted = CalculationResultCompleted.Parser.ParseFrom(data);
                    if (calculationResultCompleted.BatchId == BalanceFixingCalculationId.ToString())
                    {
                        CalculationResultCompletedFromBalanceFixing.Add(calculationResultCompleted);
                        messageHandled = true;
                    }
                    else if (calculationResultCompleted.BatchId == WholesaleFixingCalculationId.ToString())
                    {
                        CalculationResultCompletedFromWholesaleFixing.Add(calculationResultCompleted);
                        messageHandled = true;
                    }

                    break;
                case EnergyResultProducedV2.EventName:
                    var energyResultProduced = EnergyResultProducedV2.Parser.ParseFrom(data);
                    if (energyResultProduced.CalculationId == BalanceFixingCalculationId.ToString())
                    {
                        EnergyResultProducedFromBalanceFixing.Add(energyResultProduced);
                        messageHandled = true;
                    }
                    else if (energyResultProduced.CalculationId == WholesaleFixingCalculationId.ToString())
                    {
                        EnergyResultProducedFromWholesaleFixing.Add(energyResultProduced);
                        messageHandled = true;
                    }

                    break;
                case AmountPerChargeResultProducedV1.EventName:
                    var amountPerChargeResultProduced = AmountPerChargeResultProducedV1.Parser.ParseFrom(data);
                    if (amountPerChargeResultProduced.CalculationId == WholesaleFixingCalculationId.ToString())
                    {
                        AmountPerChargeResultProduced.Add(amountPerChargeResultProduced);
                        messageHandled = true;
                    }

                    break;
                case MonthlyAmountPerChargeResultProducedV1.EventName:
                    var monthlyAmountPerChargeResultProduced = MonthlyAmountPerChargeResultProducedV1.Parser.ParseFrom(data);
                    if (monthlyAmountPerChargeResultProduced.CalculationId == WholesaleFixingCalculationId.ToString())
                    {
                        MonthlyAmountPerChargeResultProduced.Add(monthlyAmountPerChargeResultProduced);
                        messageHandled = true;
                    }

                    break;
            }

            if (messageHandled)
            {
                await _receiver.CompleteMessageAsync(message);
                return true;
            }
            else
            {
                await _receiver.AbandonMessageAsync(message);
                return false;
            }
        }
    }
}
