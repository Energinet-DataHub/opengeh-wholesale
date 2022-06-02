//  Copyright 2020 Energinet DataHub A/S
// 
//  Licensed under the Apache License, Version 2.0 (the "License2");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.Core.JsonSerialization;

namespace Infrastructure.Core.MessagingExtensions;

/// <summary>
/// This class enables us to kind of handle <see cref="EventHubSender"/>s generically
/// and thus enabling dependency injection. Should be registered as a singleton
/// </summary>
public class EventHubSender : IEventHubSender
{
    private readonly EventHubProducerClient _instance;
    private readonly IJsonSerializer _jsonSerializer;

    public EventHubSender(EventHubProducerClient instance, IJsonSerializer jsonSerializer)
    {
        _instance = instance;
        _jsonSerializer = jsonSerializer;
    }
    
    public async Task SendAsync(object body)
    {
        var batch =  await _instance.CreateBatchAsync().ConfigureAwait(false);
        batch.TryAdd(new EventData(_jsonSerializer.Serialize(body)));
        await _instance.SendAsync(batch).ConfigureAwait(false);
    }
}