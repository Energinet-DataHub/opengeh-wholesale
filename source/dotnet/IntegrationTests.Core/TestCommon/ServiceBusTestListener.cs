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

namespace Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon
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
                .VerifyOnceAsync(receivedMessage =>
                {
                    result.Body = receivedMessage.Body;
                    result.CorrelationId = receivedMessage.CorrelationId;
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
            return result;
        }

        public async Task<EventualServiceBusMessage> ListenForMessageAsync(string correlationId)
        {
            var result = new EventualServiceBusMessage();
            result.MessageAwaiter = await _serviceBusListenerMock
                .WhenCorrelationId(correlationId)
                .VerifyOnceAsync(receivedMessage =>
                {
                    result.Body = receivedMessage.Body;
                    result.CorrelationId = receivedMessage.CorrelationId;
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
            return result;
        }

        public async Task<EventualServiceBusEvents> ListenForEventsAsync(
            string correlationId,
            int expectedCount)
        {
            var result = new EventualServiceBusEvents();
            result.CountdownEvent = await _serviceBusListenerMock
                .WhenCorrelationId(correlationId)
                .VerifyCountAsync(expectedCount, receivedMessage =>
                {
                    result.Body = receivedMessage.Body;
                    result.CorrelationId = receivedMessage.CorrelationId;
                    return Task.CompletedTask;
                })
                .ConfigureAwait(false);

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
