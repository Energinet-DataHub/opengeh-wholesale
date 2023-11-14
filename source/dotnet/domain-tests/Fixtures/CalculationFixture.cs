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
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Identity;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using Moq;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    public sealed class CalculationFixture : LazyFixtureBase
    {
        private readonly string _subscriptionName = Guid.NewGuid().ToString();
        private readonly TimeSpan _httpTimeout = TimeSpan.FromMinutes(10); // IDatabricksSqlStatementClient can take up to 8 minutes to get ready.

        public CalculationFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            Configuration = new WholesaleDomainConfiguration();
            UserAuthenticationClient = new B2CUserTokenAuthenticationClient(Configuration.UserTokenConfiguration);
            ServiceBusAdministrationClient = new ServiceBusAdministrationClient(Configuration.ServiceBusFullyQualifiedNamespace, new DefaultAzureCredential());
            ServiceBusClient = new ServiceBusClient(Configuration.ServiceBusConnectionString);
        }

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        public WholesaleClient_V3 WholesaleClient { get; private set; } = null!;

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        public ServiceBusReceiver Receiver { get; private set; } = null!;

        public CalculationFixtureOutput Output { get; private set; } = null!;

        private WholesaleDomainConfiguration Configuration { get; }

        private B2CUserTokenAuthenticationClient UserAuthenticationClient { get; }

        private ServiceBusAdministrationClient ServiceBusAdministrationClient { get; }

        private ServiceBusClient ServiceBusClient { get; }

        protected override async Task OnInitializeAsync()
        {
            WholesaleClient = await CreateWholesaleClientAsync();
            await CreateTopicSubscriptionAsync();
            Receiver = CreateServiceBusReceiver();
            Output = new CalculationFixtureOutput(DiagnosticMessageSink, WholesaleClient, Receiver);
            await Output.InitializeAsync();
        }

        protected override async Task OnDisposeAsync()
        {
            UserAuthenticationClient.Dispose();
            await ServiceBusAdministrationClient.DeleteSubscriptionAsync(Configuration.DomainRelayTopicName, _subscriptionName);
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
            var accessToken = await UserAuthenticationClient.AcquireAccessTokenAsync();

            var httpClient = new HttpClient();
            httpClient.BaseAddress = Configuration.WebApiBaseAddress;
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            return new WholesaleClient_V3(
                Configuration.WebApiBaseAddress.ToString(),
                httpClient);
        }

        private async Task CreateTopicSubscriptionAsync()
        {
            if (await ServiceBusAdministrationClient.SubscriptionExistsAsync(Configuration.DomainRelayTopicName, _subscriptionName))
            {
                await ServiceBusAdministrationClient.DeleteSubscriptionAsync(Configuration.DomainRelayTopicName, _subscriptionName);
            }

            var options = new CreateSubscriptionOptions(Configuration.DomainRelayTopicName, _subscriptionName)
            {
                AutoDeleteOnIdle = TimeSpan.FromHours(1),
            };

            await ServiceBusAdministrationClient.CreateSubscriptionAsync(options);
        }

        private ServiceBusReceiver CreateServiceBusReceiver()
        {
            return ServiceBusClient.CreateReceiver(Configuration.DomainRelayTopicName, _subscriptionName);
        }
    }
}
