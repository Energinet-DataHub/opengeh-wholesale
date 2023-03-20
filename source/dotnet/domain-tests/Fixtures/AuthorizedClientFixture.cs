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

using Energinet.DataHub.Wholesale.Client;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Support testing Wholesale Web API using an authorized Wholesale client.
    /// </summary>
    public sealed class AuthorizedClientFixture : IAsyncLifetime
    {
        public AuthorizedClientFixture()
        {
            Configuration = new WholesaleDomainConfiguration();
            UserAuthenticationClient = new B2CUserTokenAuthenticationClient(Configuration.UserTokenConfiguration);

            // Initially mock client, and set it later when 'InitializeAsync' is called.
            WholesaleClient = Mock.Of<IWholesaleClient>();
        }

        public WholesaleDomainConfiguration Configuration { get; }

        /// <summary>
        /// The actual client is not created until <see cref="IAsyncLifetime.InitializeAsync"/> has been called by xUnit.
        /// </summary>
        public IWholesaleClient WholesaleClient { get; private set; }

        private B2CUserTokenAuthenticationClient UserAuthenticationClient { get; }

        async Task IAsyncLifetime.InitializeAsync()
        {
            WholesaleClient = await CreateWholesaleClientAsync();
        }

        Task IAsyncLifetime.DisposeAsync()
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
        private async Task<IWholesaleClient> CreateWholesaleClientAsync()
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(m => m.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            var accessToken = await UserAuthenticationClient.AcquireAccessTokenAsync();

            return new WholesaleClient(
                new AuthorizedHttpClientFactory(
                    httpClientFactoryMock.Object,
                    () => $"Bearer {accessToken}"),
                Configuration.WebApiBaseAddress);
        }
    }
}
