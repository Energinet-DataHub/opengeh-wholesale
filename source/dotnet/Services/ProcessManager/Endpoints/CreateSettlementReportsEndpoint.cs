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

using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.ProcessManager.Endpoints;

public class CreateSettlementReportsEndpoint
{
    private const string FunctionName = nameof(CreateSettlementReportsEndpoint);
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ISettlementReportApplicationService _settlementReportApplicationService;

    public CreateSettlementReportsEndpoint(IJsonSerializer jsonSerializer, ISettlementReportApplicationService settlementReportApplicationService)
    {
        _jsonSerializer = jsonSerializer;
        _settlementReportApplicationService = settlementReportApplicationService;
    }

    [Function(FunctionName)]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "%" + EnvironmentSettingNames.DomainEventsTopicName + "%",
            "%" + EnvironmentSettingNames.CreateSettlementReportsWhenCompletedBatchSubscriptionName + "%",
            Connection = EnvironmentSettingNames.ServiceBusListenConnectionString)]
        byte[] message)
    {
        var batchCompletedEvent = await _jsonSerializer.DeserializeAsync<BatchCompletedEventDto>(message).ConfigureAwait(false);
        await _settlementReportApplicationService.CreateSettlementReportAsync(batchCompletedEvent).ConfigureAwait(false);
    }
}
