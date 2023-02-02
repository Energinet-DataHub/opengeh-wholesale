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

using System.Text;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

public class ServiceBusMessageFactory : IServiceBusMessageFactory
{
    private readonly ICorrelationContext _correlationContext;
    private readonly IJsonSerializer _jsonSerializer;

    public ServiceBusMessageFactory(
        ICorrelationContext correlationContext,
        IJsonSerializer jsonSerializer)
    {
        _correlationContext = correlationContext;
        _jsonSerializer = jsonSerializer;
    }

    public ServiceBusMessage Create(DomainEventDto domainEventDto, string messageType)
    {
        var serializedDto = _jsonSerializer.Serialize(domainEventDto);
        var body = Encoding.UTF8.GetBytes(serializedDto);
        return CreateServiceBusMessage(body, messageType, _correlationContext.Id);
    }

    public ServiceBusMessage Create(ProcessCompleted integrationEventDto, string messageType)
    {
        var serializedDto = _jsonSerializer.Serialize(integrationEventDto);
        var body = Encoding.UTF8.GetBytes(serializedDto);
        return CreateServiceBusMessage(body, messageType, _correlationContext.Id);
    }

    public IEnumerable<ServiceBusMessage> Create(IEnumerable<DomainEventDto> domainEvents, string messageType)
    {
        var bodies = domainEvents
            .Select(events => _jsonSerializer.Serialize(events))
            .Select(body => Encoding.UTF8.GetBytes(body));
        return bodies.Select(message => CreateServiceBusMessage(message, messageType, _correlationContext.Id));
    }

    /// <summary>
    /// This method is made public to use it in integration test(s) for simplicity.
    /// </summary>
    public static ServiceBusMessage CreateServiceBusMessage(
        byte[] body,
        string messageType,
        string operationCorrelationId)
    {
        var serviceBusMessage = new ServiceBusMessage
        {
            Body = new BinaryData(body),
            Subject = messageType,
        };
        serviceBusMessage.SetOperationCorrelationId(operationCorrelationId);
        serviceBusMessage.SetMessageType(messageType);
        return serviceBusMessage;
    }
}
