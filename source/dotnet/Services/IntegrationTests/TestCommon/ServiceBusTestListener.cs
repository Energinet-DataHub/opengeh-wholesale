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

using System.Text.Json;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon
{
    public class ServiceBusTestListener : IAsyncDisposable
    {
        private readonly ServiceBusListenerMock _serviceBusListenerMock;

        public ServiceBusTestListener(ServiceBusListenerMock serviceBusListenerMock)
        {
            _serviceBusListenerMock = serviceBusListenerMock;
        }

        public async Task<EventualServiceBusMessage> ListenForMessageAsync<TMessage>(Func<TMessage, bool> predicate)
        {
            var result = new EventualServiceBusMessage();
            result.MessageAwaiter = await _serviceBusListenerMock
                .When(message =>
                {
                    var deserializedMessage = JsonSerializer.Deserialize<TMessage>(message.Body);
                    return predicate(deserializedMessage!);
                })
                .VerifyOnceAsync(message =>
                {
                    result.Body = message.Body;
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
            return result;
        }

        public async Task<EventualServiceBusMessage> ListenForMessageByCorrelationIdAsync(string correlationId)
        {
            var result = new EventualServiceBusMessage();
            result.MessageAwaiter = await _serviceBusListenerMock
                .WhenCorrelationId(correlationId)
                .VerifyOnceAsync(receivedMessage =>
                {
                    result.Body = receivedMessage.Body;
                    result.CorrelationId = (string)receivedMessage.ApplicationProperties[MessageMetaDataConstants.CorrelationId];
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Reset handlers and received messages.
        /// </summary>
        /// <remarks>Use this between tests.</remarks>
        public void Reset()
        {
            _serviceBusListenerMock.ResetMessageHandlersAndReceivedMessages();
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusListenerMock.DisposeAsync();
        }
    }
}
