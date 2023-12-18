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

using System.Net;
using Energinet.DataHub.Wholesale.DomainTests.Features.Authorization.Fixtures;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.Features.Authorization
{
    /// <summary>
    /// Contains tests with focus on verifying authorization in the Web API running in a live environment.
    /// </summary>
    public class AuthorizationFeatureTests
    {
        /// <summary>
        /// These tests uses an unauthorized http client to perform requests.
        /// </summary>
        public class Given_Unauthorized : DomainTestsBase<UnauthorizedClientFixture>
        {
            public Given_Unauthorized(LazyFixtureFactory<UnauthorizedClientFixture> lazyFixtureFactory)
                : base(lazyFixtureFactory)
            {
            }

            /// <summary>
            /// Perform a request that doesn't require authorization.
            /// </summary>
            [Trait("Category", "Functional")]
            [DomainFact]
            public async Task WhenRequestReadinessStatus_ResponseIsOkAndHealthy()
            {
                // Act
                using var actualResponse = await Fixture.UnauthorizedHttpClient.GetAsync("monitor/ready");

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var actualContent = await actualResponse.Content.ReadAsStringAsync();
                actualContent.Should().StartWith("{\"status\":\"Healthy\"");
            }

            /// <summary>
            /// Perform a request that do require authorization.
            /// </summary>
            [Trait("Category", "Functional")]
            [DomainFact]
            public async Task WhenRequestBatchId_ResponseIsUnauthorized()
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, "v3/batches?batchId=1");

                // Act
                using var actualResponse = await Fixture.UnauthorizedHttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// These tests uses an authorized Wholesale client to perform requests.
        /// </summary>
        public class Given_Authorized : DomainTestsBase<AuthorizedClientFixture>
        {
            private static readonly Guid _existingBatchId = new("ed39dbc5-bdc5-41b9-922a-08d3b12d4538");

            public Given_Authorized(LazyFixtureFactory<AuthorizedClientFixture> lazyFixtureFactory)
                : base(lazyFixtureFactory)
            {
            }

            /// <summary>
            /// Perform a request that do require authorization.
            /// </summary>
            [Trait("Category", "Functional")]
            [DomainFact]
            public async Task WhenRequestingExistingBatchId_ResponseIsOk()
            {
                // Arrange

                // Act
                var batchResult = await Fixture.WholesaleClient.GetBatchAsync(_existingBatchId);

                // Assert
                batchResult.Should().NotBeNull();
                batchResult!.BatchId.Should().Be(_existingBatchId);
            }
        }
    }
}
