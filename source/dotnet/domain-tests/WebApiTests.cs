﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.WebApi.Clients.Wholesale.v3;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests
{
    /// <summary>
    /// Contains tests where we operate at the level of a "domain", so basically what in some context has been named "domain tests".
    /// However, with the technique displayed here we perform these tests in a live environment.
    /// </summary>
    public class WebApiTests
    {
        /// <summary>
        /// These tests uses an unauthorized http client to perform requests.
        /// </summary>
        public class Given_Unauthorized : IClassFixture<WholesaleDomainConfiguration>
        {
            public Given_Unauthorized(WholesaleDomainConfiguration configuration)
            {
                Configuration = configuration;
                UnauthorizedHttpClient = new HttpClient
                {
                    BaseAddress = configuration.WebApiBaseAddress,
                };
            }

            private WholesaleDomainConfiguration Configuration { get; }

            private HttpClient UnauthorizedHttpClient { get; }

            /// <summary>
            /// This is just to be able to verify everything works with regards to settings and executing the tests after deployment.
            /// If needed, this test can be removed when the actual domain test has been implemented.
            /// </summary>
            [DomainFact]
            public async Task When_RequestReadinessStatus_Then_ResponseIsOkAndHealthy()
            {
                // Arrange

                // Act
                using var actualResponse = await UnauthorizedHttpClient.GetAsync("monitor/ready");

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await actualResponse.Content.ReadAsStringAsync();
                content.Should().Contain("Healthy");
            }

            /// <summary>
            /// This shows our request will fail if we call Web API without a valid access token.
            /// </summary>
            [DomainFact]
            public async Task When_RequestBatchId_Then_ResponseIsUnauthorized()
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, "v3/batches?batchId=1");

                // Act
                using var actualResponse = await UnauthorizedHttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// These tests uses an authorized Wholesale client to perform requests.
        /// </summary>
        public class Given_Authorized : IClassFixture<AuthorizedClientFixture>
        {
            private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(15);
            private static readonly TimeSpan _defaultDelay = TimeSpan.FromSeconds(30);

            private static readonly Guid _existingBatchId = new("a68d4452-943b-4f45-b32f-5f84607c6b6b");
            private static readonly string ExistingGridAreaCode = "543";

            public Given_Authorized(AuthorizedClientFixture fixture)
            {
                Fixture = fixture;
            }

            private AuthorizedClientFixture Fixture { get; }

            [DomainFact]
            public async Task When_RequestingExistingBatchId_Then_ResponseIsOk()
            {
                // Arrange

                // Act
                var batchResult = await Fixture.WholesaleClient.GetBatchAsync(_existingBatchId);

                // Assert
                batchResult.Should().NotBeNull();
                batchResult!.BatchId.Should().Be(_existingBatchId);
            }

            [DomainFact]
            public async Task When_CreatingBatch_Then_BatchIsEventuallyCompleted()
            {
                // Arrange
                var startDate = new DateTimeOffset(2020, 1, 28, 23, 0, 0, TimeSpan.Zero);
                var endDate = new DateTimeOffset(2020, 1, 29, 23, 0, 0, TimeSpan.Zero);
                var batchRequestDto = new BatchRequestDto
                {
                    ProcessType = ProcessType.BalanceFixing,
                    GridAreaCodes = new List<string> { ExistingGridAreaCode },
                    StartDate = startDate,
                    EndDate = endDate,
                };

                // Act
                var batchId = await Fixture.WholesaleClient.CreateBatchAsync(batchRequestDto).ConfigureAwait(false);

                // Assert
                var isCompleted = await Awaiter.TryWaitUntilConditionAsync(
                    async () =>
                    {
                        var batchResult = await Fixture.WholesaleClient.GetBatchAsync(batchId);
                        return batchResult?.ExecutionState == BatchState.Completed;
                    },
                    _defaultTimeout,
                    _defaultDelay);

                isCompleted.Should().BeTrue();
            }
        }
    }
}
