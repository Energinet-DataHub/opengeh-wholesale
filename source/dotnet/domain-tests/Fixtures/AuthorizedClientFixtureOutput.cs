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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Microsoft.IdentityModel.Tokens;
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

        public bool CalculationIsComplete { get; private set; }

        public List<CalculationResultCompleted>? CalculationResults { get; private set; }

        private Guid CalculationId { get; set; }

        public async Task InitializeAsync()
        {
            CalculationId = await StartCalculation();
            CalculationIsComplete = await WaitForCalculationToComplete(CalculationId);
            CalculationResults = await GetListOfResultsFromServiceBus(CalculationId);
        }

        private async Task<Guid> StartCalculation()
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

        private async Task<bool> WaitForCalculationToComplete(Guid calculationId)
        {
        var defaultTimeout = TimeSpan.FromMinutes(15);
        var defaultDelay = TimeSpan.FromSeconds(30);
        var isCompleted = await Awaiter.TryWaitUntilConditionAsync(
                async () =>
                {
                    var batchResult = await _wholesaleClient.GetBatchAsync(calculationId);
                    return batchResult?.ExecutionState == BatchState.Completed;
                },
                defaultTimeout,
                defaultDelay);
        return isCompleted;
        }

        private async Task<List<CalculationResultCompleted>?> GetListOfResultsFromServiceBus(Guid calculationId)
        {
            var messageHasValue = true;
            var results = new List<CalculationResultCompleted>();
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromMinutes(5));
                while (messageHasValue)
                {
                    var message = await _receiver.ReceiveMessageAsync();
                    if (message?.Body == null)
                    {
                        if (!results.IsNullOrEmpty())
                        {
                            messageHasValue = false;
                        }
                    }
                    else
                    {
                        var data = message.Body.ToArray();
                        var result = CalculationResultCompleted.Parser.ParseFrom(data);
                        if (result.BatchId == calculationId.ToString())
                        {
                            results.Add(result);
                        }
                    }

                    if (cts.Token.IsCancellationRequested)
                    {
                        messageHasValue = false;
                    }
                }
            }

            return results;
        }
    }
}
