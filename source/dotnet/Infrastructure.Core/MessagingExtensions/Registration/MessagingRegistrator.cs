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
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration
{
    public class MessagingRegistrator
    {
        private readonly IServiceCollection _services;

        internal MessagingRegistrator(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="MessageExtractor{TInboundMessage}"/>.
        /// Which is intended to extract a message outside of the Charges domain.
        /// </summary>
        public MessagingRegistrator AddExternalMessageExtractor<TInboundMessage>()
            where TInboundMessage : IInboundMessage
        {
            _services.AddScoped<MessageExtractor<TInboundMessage>>();
            _services.AddScoped<MessageDeserializer<TInboundMessage>, JsonMessageDeserializer<TInboundMessage>>();

            return this;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="MessageExtractor{TInboundMessage}"/>.
        /// Which is intended to extract a message from inside of the Charges domain.
        /// </summary>
        public MessagingRegistrator AddInternalMessageExtractor<TInboundMessage>()
            where TInboundMessage : IInboundMessage
        {
            _services.AddScoped<JsonMessageDeserializer<TInboundMessage>>();

            return this;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="IMessageDispatcher{TOutboundMessage}"/>.
        /// Which is used when sending messages out of the Charges domain.
        /// </summary>
        public MessagingRegistrator AddExternalMessageDispatcher<TOutboundMessage>(
            string serviceBusConnectionString,
            string serviceBusTopicName)
            where TOutboundMessage : IOutboundMessage
        {
            _services.AddScoped<IMessageDispatcher<TOutboundMessage>, MessageDispatcher<TOutboundMessage>>();
            _services.AddScoped<Channel<TOutboundMessage>, ServiceBusChannel<TOutboundMessage>>();

            // Must be a singleton as per documentation of ServiceBusClient and ServiceBusSender
            _services.AddSingleton<IServiceBusSender<TOutboundMessage>>(
                _ =>
                {
                    var client = new ServiceBusClient(serviceBusConnectionString);
                    var instance = client.CreateSender(serviceBusTopicName);
                    return new ServiceBusSender<TOutboundMessage>(instance);
                });

            return this;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="IMessageDispatcher{TInboundMessage}"/>.
        /// Which is used when sending messages within of the Charges domain.
        /// </summary>
        public MessagingRegistrator AddInternalMessageDispatcher<TOutboundMessage>(
            string serviceBusConnectionString,
            string serviceBusTopicName)
            where TOutboundMessage : IOutboundMessage
        {
            _services.AddScoped<IMessageDispatcher<TOutboundMessage>, InternalMessageDispatcher<TOutboundMessage>>();
            _services.AddScoped<Channel<TOutboundMessage>, ServiceBusChannel<TOutboundMessage>>();

            // Must be a singleton as per documentation of ServiceBusClient and ServiceBusSender
            _services.AddSingleton<IServiceBusSender<TOutboundMessage>>(
                _ =>
                {
                    var client = new ServiceBusClient(serviceBusConnectionString);
                    var instance = client.CreateSender(serviceBusTopicName);
                    return new ServiceBusSender<TOutboundMessage>(instance);
                });

            return this;
        }
    }
}
