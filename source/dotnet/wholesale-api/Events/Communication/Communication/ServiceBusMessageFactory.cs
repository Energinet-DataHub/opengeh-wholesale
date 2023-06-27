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

namespace Energinet.DataHub.Core.Messaging.Communication;

public class ServiceBusMessageFactory : IServiceBusMessageFactory
{
    public ServiceBusMessage Create(IntegrationEvent @event)
    {
        var serviceBusMessage = new ServiceBusMessage
        {
            Body = new BinaryData(@event.Message),

            // Subject (message name) can be used in filters i.e. <prefix-domain>MeteringPointCreated
            Subject = @event.MessageName,
            MessageId = @event.EventIdentification.ToString(),
        };

        // The Operation Time Stamp is when the event was actually completed (in the business process logic)
        serviceBusMessage.ApplicationProperties.Add("OperationTimeStamp", @event.OperationTimeStamp);
        serviceBusMessage.ApplicationProperties.Add("MessageVersion", @event.MessageVersion);
        return serviceBusMessage;
    }
}
