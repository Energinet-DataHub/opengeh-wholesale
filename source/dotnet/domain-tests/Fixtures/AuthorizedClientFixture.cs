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
using Azure.Security.KeyVault.Secrets;
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
        private readonly string _subscriptionName = "test-sub";

        public AuthorizedClientFixture()
        {
            Configuration = new WholesaleDomainConfiguration();
            UserAuthenticationClient = new B2CUserTokenAuthenticationClient(Configuration.UserTokenConfiguration);

            // Initially mock client, and set it later when 'InitializeAsync' is called.
            // WholesaleClient = Mock.Of<IWholesaleClient>();
        }

        public WholesaleDomainConfiguration Configuration { get; }

        /// <summary>
        /// The actual client is not created until <see cref="IAsyncLifetime.InitializeAsync"/> has been called by xUnit.
        /// </summary>
        public WholesaleClient_V3 WholesaleClient { get; private set; } = null!;

        public ServiceBusReceiver Receiver { get;  private set; } = null!;

        private B2CUserTokenAuthenticationClient UserAuthenticationClient { get; }

        async Task IAsyncLifetime.InitializeAsync()
        {
            WholesaleClient = await CreateWholesaleClientAsync();
            Receiver = await CreateSubscriptionAndServiceBusReceiver();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            UserAuthenticationClient.Dispose();
            var serviceBusAdministrationClient = await CreateServiceBusAdministrationClient(GetSecretClient());
            await serviceBusAdministrationClient.DeleteSubscriptionAsync(_topicName, _subscriptionName);
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
                .Returns(new HttpClient());

            var accessToken = await UserAuthenticationClient.AcquireAccessTokenAsync();

            return new WholesaleClient_V3(
                Configuration.WebApiBaseAddress.ToString(),
                new AuthorizedHttpClientFactory(
                    httpClientFactoryMock.Object,
                    () => $"Bearer {accessToken}").CreateClient(Configuration.WebApiBaseAddress));
        }

        private async Task<ServiceBusReceiver> CreateSubscriptionAndServiceBusReceiver()
        {
            var secretClient = GetSecretClient();
            var serviceBusAdministrationClient = await CreateServiceBusAdministrationClient(secretClient);
            var serviceBusConnectionString = (await secretClient.GetSecretAsync("sb-domain-relay-listen-connection-string")).Value.Value;
            var client = new ServiceBusClient(serviceBusConnectionString);
            if (await serviceBusAdministrationClient.SubscriptionExistsAsync(_topicName, _subscriptionName))
            {
                await serviceBusAdministrationClient.DeleteSubscriptionAsync(_topicName, _subscriptionName);
            }

            await serviceBusAdministrationClient.CreateSubscriptionAsync(_topicName, _subscriptionName);
            var serviceBusReceiverOptions = new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete };
            var receiver = client.CreateReceiver(_topicName, _subscriptionName, serviceBusReceiverOptions);
            return receiver;
        }

        private async Task<ServiceBusAdministrationClient> CreateServiceBusAdministrationClient(SecretClient secretClient)
        {
            var serviceBusNamespace = (await secretClient.GetSecretAsync("sb-domain-relay-namespace-name")).Value.Value;
            var fullyQualifiedNamespace = $"{serviceBusNamespace}.servicebus.windows.net";
            return new ServiceBusAdministrationClient(fullyQualifiedNamespace, new DefaultAzureCredential());
        }

        private SecretClient GetSecretClient()
        {
            var kv = new SecretClient(
                Configuration.SharedKeyVaultUri,
                new DefaultAzureCredential());
            return kv;
        }
    }
}
