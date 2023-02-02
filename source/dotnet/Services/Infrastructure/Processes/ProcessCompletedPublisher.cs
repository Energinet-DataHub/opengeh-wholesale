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
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Infrastructure.Batches;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

public class ProcessCompletedPublisher : IProcessCompletedPublisher
{
    private readonly IOptions<DomainEventTopicSettings> _settings;
    private readonly ServiceBusSender _serviceBusSender;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;

    public ProcessCompletedPublisher(
        IOptions<DomainEventTopicSettings> settings,
        DomainEventTopicServiceBusSender serviceBusSender,
        IServiceBusMessageFactory serviceBusMessageFactory)
    {
        _settings = settings;
        _serviceBusSender = serviceBusSender;
        _serviceBusMessageFactory = serviceBusMessageFactory;
    }

    public async Task PublishAsync(List<ProcessCompletedEventDto> completedProcesses)
    {
        var messages = _serviceBusMessageFactory.Create(completedProcesses, _settings.Value.TopicName);
        await _serviceBusSender.SendMessagesAsync(messages).ConfigureAwait(false);
    }
}
