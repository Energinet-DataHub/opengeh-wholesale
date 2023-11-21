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

using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.DomainTests.Features.Authorization.Fixtures
{
    public sealed class UnauthorizedClientFixture : LazyFixtureBase
    {
        public UnauthorizedClientFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            var configuration = new WholesaleDomainConfiguration();
            UnauthorizedHttpClient = new HttpClient
            {
                BaseAddress = configuration.WebApiBaseAddress,
            };
        }

        public HttpClient UnauthorizedHttpClient { get; }

        protected override Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected override Task OnDisposeAsync()
        {
            UnauthorizedHttpClient.Dispose();

            return Task.CompletedTask;
        }
    }
}
