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
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;

namespace Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

public class ServiceBusMessageFactory : IServiceBusMessageFactory
{
    private readonly ICorrelationContext _correlationContext;
    private readonly IDictionary<Type, string> _messageTypes;

    public ServiceBusMessageFactory(ICorrelationContext correlationContext, IDictionary<Type, string> messageTypes)
    {
        _correlationContext = correlationContext;
        _messageTypes = messageTypes;
    }

    public IEnumerable<ServiceBusMessage> Create<TMessage>(IEnumerable<TMessage> messages)
    {
        return messages.Select(CreateServiceBusMessage);
    }

    public static ServiceBusMessage CreateServiceBusMessage<TMessage>(
        TMessage message,
        string messageType,
        string correlationContextId)
    {
        return new ServiceBusMessage
        {
            Body = new BinaryData(message),
            Subject = messageType,
            ApplicationProperties =
            {
                new KeyValuePair<string, object>(MessageMetaDataConstants.CorrelationId, correlationContextId),
                new KeyValuePair<string, object>(MessageMetaDataConstants.MessageType, messageType),
            },
        };
    }

    private ServiceBusMessage CreateServiceBusMessage<TMessage>(TMessage message)
    {
        if (!_messageTypes.ContainsKey(typeof(TMessage)))
            throw new NotImplementedException($"No message type identifier has been registered for message of type {typeof(TMessage).FullName}");

        var messageType = _messageTypes[typeof(TMessage)];

        return CreateServiceBusMessage(message, messageType, _correlationContext.Id);
    }
}
