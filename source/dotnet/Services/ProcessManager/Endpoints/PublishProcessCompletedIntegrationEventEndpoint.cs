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
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.ProcessCompleted;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;

namespace Energinet.DataHub.Wholesale.ProcessManager.Endpoints;

public class PublishProcessCompletedIntegrationEventEndpoint
{
    private const string FunctionName = nameof(PublishProcessCompletedIntegrationEventEndpoint);
    private readonly IJsonSerializer _jsonSerializer;

    public PublishProcessCompletedIntegrationEventEndpoint(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    [Function(FunctionName)]
    [return: ServiceBus("%" + EnvironmentSettingNames.IntegrationEventsTopicName + "%", Connection = EnvironmentSettingNames.ServiceBusSendConnectionString)]
    public async Task<ProcessCompleted> RunAsync(
        [Microsoft.Azure.Functions.Worker.ServiceBusTrigger(
            "%" + EnvironmentSettingNames.DomainEventsTopicName + "%",
            "%" + EnvironmentSettingNames.PublishProcessesCompletedIntegrationEventWhenProcessCompletedSubscriptionName + "%",
            Connection = EnvironmentSettingNames.ServiceBusListenConnectionString)]
        byte[] message)
    {
        var processCompletedEvent = await DeserializeByteArrayAsync<ProcessCompletedEventDto>(message).ConfigureAwait(false);
        return Map(processCompletedEvent);
    }

    // TODO: Unit test
    private static ProcessCompleted Map(ProcessCompletedEventDto processCompletedEvent)
    {
        return new ProcessCompleted()
        {
            BatchId = processCompletedEvent.BatchId.ToString(),
            ProcessType = ProcessCompleted.Types.ProcessType.PtBalancefixing,
            GridAreaCode = processCompletedEvent.GridAreaCode,
        };
    }

    private async Task<T> DeserializeByteArrayAsync<T>(byte[] data)
    {
        var stream = new MemoryStream(data);
        await using (stream.ConfigureAwait(false))
        {
            return (T)await _jsonSerializer.DeserializeAsync(stream, typeof(T)).ConfigureAwait(false);
        }
    }
}
