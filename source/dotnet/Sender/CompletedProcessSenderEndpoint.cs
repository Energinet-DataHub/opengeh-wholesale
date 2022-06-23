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
using Energinet.DataHub.Wholesale.Domain.Dtos.CompletedProcessSenderEvents;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Sender;

public class CompletedProcessSenderEndpoint
{
    private const string FunctionName = nameof(CompletedProcessSenderEndpoint);
    private readonly JsonSerializer _jsonSerializer;

    public CompletedProcessSenderEndpoint(JsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    [Function(FunctionName)]
    [ServiceBusOutput(EnvironmentSettingNames.DataAvailableQueueName, ServiceBusEntityType.Queue, Connection = EnvironmentSettingNames.DataAvailableServiceBusConnectionString)]
    public async Task<CompletedProcess> RunAsync(
        [ServiceBusTrigger(
            "%" + EnvironmentSettingNames.CompletedProcessTopicName + "%",
            "%" + EnvironmentSettingNames.CompletedProcessSubscriptionName + "%",
            Connection = EnvironmentSettingNames.ServiceBusConnectionString)]
        byte[] message)
    {
        return await DeserializeByteArrayAsync<CompletedProcess>(message).ConfigureAwait(false);
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
