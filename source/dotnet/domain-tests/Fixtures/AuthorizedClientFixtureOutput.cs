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

        public bool EnergyCalculationIsComplete { get; private set; }

        public bool WholesaleCalculationIsComplete { get; private set; }

        public List<CalculationResultCompleted> EnergyCalculationResults { get; } = new();

        public List<CalculationResultCompleted> WholesaleCalculationResults { get; } = new();

        private Guid EnergyCalculationId { get; set; }

        private Guid WholesaleCalculationId { get; set; }

        public async Task InitializeAsync()
        {
            EnergyCalculationId = await StartEnergyCalculation();
            WholesaleCalculationId = await StartWholesaleCalculation();

            EnergyCalculationIsComplete = await WaitForCalculationToComplete(EnergyCalculationId);
            WholesaleCalculationIsComplete = await WaitForCalculationToComplete(WholesaleCalculationId);

            await CollectResultsFromServiceBus();
        }

        private async Task<Guid> StartEnergyCalculation()
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

        private async Task<Guid> StartWholesaleCalculation()
        {
            var startDate = new DateTimeOffset(2021, 12, 31, 23, 0, 0, TimeSpan.Zero);
            var endDate = new DateTimeOffset(2022, 1, 31, 23, 0, 0, TimeSpan.Zero);
            var batchRequestDto = new BatchRequestDto
            {
                ProcessType = ProcessType.WholesaleFixing,
                GridAreaCodes = new List<string> { "543" },
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
                    // No more messages in the queue AND expected results are anticipated to have been received
                    if (EnergyCalculationResults.Any() || WholesaleCalculationResults.Any())
                        break;
                }
                else
                {
                    var data = message.Body.ToArray();
                    var result = CalculationResultCompleted.Parser.ParseFrom(data);

                    if (result.BatchId == EnergyCalculationId.ToString())
                    {
                        EnergyCalculationResults.Add(result);
                    }

                    if (result.BatchId == WholesaleCalculationId.ToString())
                    {
                        WholesaleCalculationResults.Add(result);
                    }
                }
            }

            stopwatch.Stop();

            Console.WriteLine($"LOOK AT ME: the loop took {stopwatch.Elapsed} to complete and received {EnergyCalculationResults.Count} messages");
        }
    }
}
