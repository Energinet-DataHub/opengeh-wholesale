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

using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    public sealed class UnauthorizedClientFixture : IAsyncLifetime
    {
        public UnauthorizedClientFixture()
        {
            var configuration = new WholesaleDomainConfiguration();
            UnauthorizedHttpClient = new HttpClient
            {
                BaseAddress = configuration.WebApiBaseAddress,
            };
        }

        public HttpClient UnauthorizedHttpClient { get; }

        Task IAsyncLifetime.InitializeAsync()
        {
            return Task.CompletedTask;
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            UnauthorizedHttpClient.Dispose();

            return Task.CompletedTask;
        }
    }
}
