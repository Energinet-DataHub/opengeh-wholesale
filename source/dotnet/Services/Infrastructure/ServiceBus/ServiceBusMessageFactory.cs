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

namespace Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

public class ServiceBusMessageFactory : IServiceBusMessageFactory
{
    private readonly ICorrelationContext _correlationContext;
    private readonly IDictionary<Type, string> _messageTypes;
    private readonly IJsonSerializer _jsonSerializer;

    public ServiceBusMessageFactory(ICorrelationContext correlationContext, IDictionary<Type, string> messageTypes, IJsonSerializer jsonSerializer)
    {
        _correlationContext = correlationContext;
        _messageTypes = messageTypes;
        _jsonSerializer = jsonSerializer;
    }

    public ServiceBusMessage Create(byte[] body, string messageType)
    {
        return CreateServiceBusMessage(body, messageType, _correlationContext.Id);
    }

    public ServiceBusMessage Create<TMessage>(TMessage message, string messageType)
    {
        return CreateServiceBusMessage(message, messageType);
    }

    public IEnumerable<ServiceBusMessage> Create<TMessage>(IEnumerable<TMessage> messages, string messageType)
    {
        return messages.Select(message => CreateServiceBusMessage(message, messageType));
    }

    public static ServiceBusMessage CreateServiceBusMessage(
        byte[] body,
        string messageType,
        string correlationContextId)
    {
        return new ServiceBusMessage
        {
            Body = new BinaryData(body),
            Subject = messageType,
            ApplicationProperties =
            {
                new KeyValuePair<string, object>(MessageMetaDataConstants.CorrelationId, correlationContextId),
                new KeyValuePair<string, object>(MessageMetaDataConstants.MessageType, messageType),
            },
        };
    }

    private ServiceBusMessage CreateServiceBusMessage<TMessage>(TMessage message, string messageType)
    {
        if (!_messageTypes.ContainsKey(typeof(TMessage)))
            throw new NotImplementedException($"No message type identifier has been registered for message of type {typeof(TMessage).FullName}");

        var messageType = _messageTypes[typeof(TMessage)];

        var body = _jsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(body);
        return CreateServiceBusMessage(bytes, messageType, _correlationContext.Id);
    }
}
