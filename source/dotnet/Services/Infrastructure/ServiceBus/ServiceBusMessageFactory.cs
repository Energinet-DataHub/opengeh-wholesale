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
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;

namespace Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

public class ServiceBusMessageFactory : IServiceBusMessageFactory
{
    private readonly ICorrelationContext _correlationContext;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly MessageTypeDictionary _messageTypes;

    public ServiceBusMessageFactory(
        ICorrelationContext correlationContext,
        IJsonSerializer jsonSerializer,
        MessageTypeDictionary messageTypes)
    {
        _correlationContext = correlationContext;
        _jsonSerializer = jsonSerializer;
        _messageTypes = messageTypes;
    }

    public ServiceBusMessage Create<TDomainEventDto>(TDomainEventDto domainEvent)
        where TDomainEventDto : DomainEventDto
    {
        var messageType = GetMessageType(domainEvent);
        var serializedDto = _jsonSerializer.Serialize(domainEvent);
        var body = Encoding.UTF8.GetBytes(serializedDto);
        return CreateServiceBusMessage(body, messageType, _correlationContext.Id);
    }

    public ServiceBusMessage CreateProcessCompleted(byte[] bytes, string messageType)
    {
        return CreateServiceBusMessage(bytes, messageType, _correlationContext.Id);
    }

    public IEnumerable<ServiceBusMessage> Create<TDomainEventDto>(IList<TDomainEventDto> domainEvents)
        where TDomainEventDto : DomainEventDto
    {
        foreach (var domainEvent in domainEvents)
        {
            var body = _jsonSerializer.Serialize(domainEvent);
            var bytes = Encoding.UTF8.GetBytes(body);
            var messageType = GetMessageType(domainEvent);
            yield return CreateServiceBusMessage(bytes, messageType, _correlationContext.Id);
        }
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

    private string GetMessageType(DomainEventDto domainEvent)
    {
        return _messageTypes[domainEvent.GetType()];
    }
}
