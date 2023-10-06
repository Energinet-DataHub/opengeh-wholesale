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

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Support testing Wholesale Web API using an authorized Wholesale client.
    /// </summary>
    public sealed class AuthorizedClientFixture : IAsyncLifetime
    {
        private readonly string _topicName = "sbt-sharedres-integrationevent-received";
        private readonly string _subscriptionName = Guid.NewGuid().ToString();
        private readonly TimeSpan _httpTimeout = TimeSpan.FromMinutes(10); // IDatabricksSqlStatementClient can take up to 8 minutes to get ready.

        public AuthorizedClientFixture()
        {
            Configuration = new WholesaleDomainConfiguration();
            UserAuthenticationClient = new B2CUserTokenAuthenticationClient(Configuration.UserTokenConfiguration);
            ServiceBusAdministrationClient = new ServiceBusAdministrationClient(Configuration.ServiceBusFullyQualifiedNamespace, new DefaultAzureCredential());
            ServiceBusClient = new ServiceBusClient(Configuration.ServiceBusConnectionString);
        }

        /// <summary>
        /// The actual client is not created until <see cref="IAsyncLifetime.InitializeAsync"/> has been called by xUnit.
        /// </summary>
        public WholesaleClient_V3 WholesaleClient { get; private set; } = null!;

        /// <summary>
        /// The actual client is not created until <see cref="IAsyncLifetime.InitializeAsync"/> has been called by xUnit.
        /// </summary>
        public ServiceBusReceiver Receiver { get; private set; } = null!;

        public AuthorizedClientFixtureOutput Output { get; private set; } = null!;

        private WholesaleDomainConfiguration Configuration { get; }

        private B2CUserTokenAuthenticationClient UserAuthenticationClient { get; }

        private ServiceBusAdministrationClient ServiceBusAdministrationClient { get; }

        private ServiceBusClient ServiceBusClient { get; }

        async Task IAsyncLifetime.InitializeAsync()
        {
            WholesaleClient = await CreateWholesaleClientAsync();
            await CreateTopicSubscriptionAsync();
            Receiver = CreateServiceBusReceiver();
            Output = new AuthorizedClientFixtureOutput(WholesaleClient, Receiver);
            await Output.InitializeAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            UserAuthenticationClient.Dispose();
            await ServiceBusAdministrationClient.DeleteSubscriptionAsync(_topicName, _subscriptionName);
            await ServiceBusClient.DisposeAsync();
        }

        /// <summary>
        /// The current implementation of <see cref="WholesaleClient"/> is favored to
        /// a usage scenario where the access token has already been retrieved or can
        /// be retrieved synchronously.
        /// However, in current tests we need to retrieve it asynchronously.
        /// </summary>
        private async Task<WholesaleClient_V3> CreateWholesaleClientAsync()
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(m => m.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient { Timeout = _httpTimeout });

            var accessToken = await UserAuthenticationClient.AcquireAccessTokenAsync();

            return new WholesaleClient_V3(
                Configuration.WebApiBaseAddress.ToString(),
                new AuthorizedHttpClientFactory(
                    httpClientFactoryMock.Object,
                    () => $"Bearer {accessToken}").CreateClient(Configuration.WebApiBaseAddress));
        }

        private async Task CreateTopicSubscriptionAsync()
        {
            if (await ServiceBusAdministrationClient.SubscriptionExistsAsync(_topicName, _subscriptionName))
            {
                await ServiceBusAdministrationClient.DeleteSubscriptionAsync(_topicName, _subscriptionName);
            }

            var options = new CreateSubscriptionOptions(_topicName, _subscriptionName)
            {
                AutoDeleteOnIdle = TimeSpan.FromHours(1),
            };

            await ServiceBusAdministrationClient.CreateSubscriptionAsync(options);
        }

        private ServiceBusReceiver CreateServiceBusReceiver()
        {
            var serviceBusReceiverOptions = new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
            };

            return ServiceBusClient.CreateReceiver(_topicName, _subscriptionName, serviceBusReceiverOptions);
        }
    }
}
