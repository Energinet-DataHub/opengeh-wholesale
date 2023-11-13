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

using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Moq;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Support calling the Wholesale Web API using an authorized Wholesale client.
    /// </summary>
    public sealed class AuthorizedClientFixture : LazyFixtureBase
    {
        private readonly TimeSpan _httpTimeout = TimeSpan.FromMinutes(10); // IDatabricksSqlStatementClient can take up to 8 minutes to get ready.

        public AuthorizedClientFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            Configuration = new WholesaleDomainConfiguration();
            UserAuthenticationClient = new B2CUserTokenAuthenticationClient(Configuration.UserTokenConfiguration);
        }

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        public WholesaleClient_V3 WholesaleClient { get; private set; } = null!;

        private WholesaleDomainConfiguration Configuration { get; }

        private B2CUserTokenAuthenticationClient UserAuthenticationClient { get; }

        protected override async Task OnInitializeAsync()
        {
            WholesaleClient = await CreateWholesaleClientAsync();
        }

        protected override Task OnDisposeAsync()
        {
            UserAuthenticationClient.Dispose();

            return Task.CompletedTask;
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
    }
}
