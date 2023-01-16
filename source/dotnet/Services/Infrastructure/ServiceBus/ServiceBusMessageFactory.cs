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

    public ServiceBusMessageFactory(ICorrelationContext correlationContext)
    {
        _correlationContext = correlationContext;
    }

    public ServiceBusMessage Create(byte[] body, string messageType)
    {
        return CreateServiceBusMessage(body, messageType, _correlationContext.Id);
    }

    public IEnumerable<ServiceBusMessage> Create(IEnumerable<byte[]> messages, string messageType)
    {
        return messages.Select(message => CreateServiceBusMessage(message, messageType, _correlationContext.Id));
    }

    /// <summary>
    /// This method is made public to use it in integration test(s) for simplicity.
    /// </summary>
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
}
