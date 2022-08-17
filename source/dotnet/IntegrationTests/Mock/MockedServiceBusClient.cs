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

namespace Energinet.DataHub.Wholesale.IntegrationTests.Mock;

internal sealed class MockedServiceBusClient : ServiceBusClient
{
    private static readonly MockedServiceBusSender _globalSender = new();

    public override ServiceBusSender CreateSender(string queueOrTopicName)
    {
        return _globalSender;
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync().ConfigureAwait(false);
        }
        catch (NullReferenceException)
        {
            // Dispose error during integration test.
        }
    }

    private sealed class MockedServiceBusSender : ServiceBusSender
    {
        public override Task SendMessageAsync(
            ServiceBusMessage message,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendMessagesAsync(
            IEnumerable<ServiceBusMessage> messages,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                await base.DisposeAsync().ConfigureAwait(false);
            }
            catch (NullReferenceException)
            {
                // Dispose error during integration test.
            }
        }
    }
}
