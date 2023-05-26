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

using System.IO.Compression;
using System.Net;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
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

            private static readonly Guid _existingBatchId = new("ed39dbc5-bdc5-41b9-922a-08d3b12d4538");
            private static readonly DateTimeOffset _existingBatchPeriodStart = DateTimeOffset.Parse("2020-01-28T23:00:00Z");
            private static readonly DateTimeOffset _existingBatchPeriodEnd = DateTimeOffset.Parse("2020-01-29T23:00:00Z");
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
            public async Task When_CreatingBatch_Then_BatchIsEventuallyCompletedAndReceivedOnTopicSubscription()
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
                var messageHasValue = true;
                var match = false;

                while (messageHasValue)
                {
                    var message = await Fixture.Receiver.ReceiveMessageAsync();
                    if (message != null)
                    {
                        match = message.Body.ToString().Contains(batchId.ToString());
                        if (match)
                        {
                            messageHasValue = false;
                        }
                    }
                    else
                    {
                        messageHasValue = false;
                    }
                }

                isCompleted.Should().BeTrue();
                match.Should().BeTrue();
            }

            [DomainFact(Skip = "Test fails on cold runs with a timeout error.")]
            public async Task When_DownloadingSettlementReport_Then_ResponseIsCompressedFileWithData()
            {
                // Arrange + Act
                var fileResponse = await Fixture.WholesaleClient.DownloadAsync(
                    new[] { ExistingGridAreaCode },
                    ProcessType.BalanceFixing,
                    _existingBatchPeriodStart,
                    _existingBatchPeriodEnd);

                // Assert
                using var compressedSettlementReport = new ZipArchive(fileResponse.Stream, ZipArchiveMode.Read);
                compressedSettlementReport.Entries.Should().NotBeEmpty();

                var resultEntry = compressedSettlementReport.Entries.Single();
                resultEntry.Name.Should().Be("Result.csv");

                using var stringReader = new StreamReader(resultEntry.Open());
                var content = await stringReader.ReadToEndAsync();

                var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines[1..])
                {
                    // Check that the line contains the expected grid area code and process type.
                    Assert.StartsWith("543,D04,", line);
                }
            }
        }
    }
}
