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
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Sender.Configuration;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Sender.Endpoints;

public class DataAvailableSenderEndpoint
{
    private const string FunctionName = nameof(DataAvailableSenderEndpoint);
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDataAvailableNotifier _dataAvailableNotifier;

    public DataAvailableSenderEndpoint(IJsonSerializer jsonSerializer, IDataAvailableNotifier dataAvailableNotifier)
    {
        _jsonSerializer = jsonSerializer;
        _dataAvailableNotifier = dataAvailableNotifier;
    }

    [Function(FunctionName)]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "%" + EnvironmentSettingNames.ProcessCompletedTopicName + "%",
            "%" + EnvironmentSettingNames.ProcessCompletedSubscriptionName + "%",
            Connection = EnvironmentSettingNames.ServiceBusListenConnectionString)]
        byte[] message)
    {
        var completedProcessEvent = await DeserializeByteArrayAsync<ProcessCompletedEventDto>(message).ConfigureAwait(false);
        await _dataAvailableNotifier.NotifyAsync(completedProcessEvent).ConfigureAwait(false);
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
