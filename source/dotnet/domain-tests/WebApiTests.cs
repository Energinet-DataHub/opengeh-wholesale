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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using Xunit;
using ProcessType = Energinet.DataHub.Wholesale.DomainTests.Clients.v3.ProcessType;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;

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
        /// </summary>'
        public class Given_Authorized : IClassFixture<AuthorizedClientFixture>
        {
            private static readonly Guid _existingBatchId = new("ed39dbc5-bdc5-41b9-922a-08d3b12d4538");
            private static readonly DateTimeOffset _existingBatchPeriodStart = DateTimeOffset.Parse("2020-01-28T23:00:00Z");
            private static readonly DateTimeOffset _existingBatchPeriodEnd = DateTimeOffset.Parse("2020-01-29T23:00:00Z");
            private static readonly string ExistingGridAreaCode = "543";

            private static List<CalculationResultCompleted>? _calculationResults;

            public Given_Authorized(AuthorizedClientFixture fixture)
            {
                Fixture = fixture;
                _calculationResults = Fixture.Output.CalculationResults;
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
            public void When_CreatingBatch_Then_BatchIsEventuallyCompleted()
            {
                Fixture.Output.CalculationIsComplete.Should().BeTrue();
            }

            [DomainFact]
            public void When_BatchIsCompleted_Then_BatchIsReceivedOnTopicSubscription()
            {
                _calculationResults?.Count.Should().Be(109);
            }

            [DomainFact]
            public void When_BatchIsReceivedOnTopicSubscription_Then_MessagesReceivedContainAllTimeSeriesTypes()
            {
                GetActualAndExpectedTimeSeriesTypes(out var actualTimeSeriesTypes, out var expectedTimeSeriesTypes);
                foreach (var expectedTimeSeriesType in expectedTimeSeriesTypes)
                {
                    actualTimeSeriesTypes.Should().Contain(expectedTimeSeriesType);
                }
            }

            [DomainFact]
            public void When_BatchIsReceivedOnTopicSubscription_Then_MessagesReceivedContainAllTypesOfCalculations()
            {
                using (new AssertionScope())
                {
                    CheckIfExistsInCaluclationResults("NonProfiledConsumption", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain NonProfiledConsumption for AggregationPerGridarea");
                    CheckIfExistsInCaluclationResults("NonProfiledConsumption", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue("because the calculation result should contain NonProfiledConsumption for AggregationPerEnergysupplierPerGridarea");
                    CheckIfExistsInCaluclationResults("NonProfiledConsumption", "AggregationPerBalanceresponsiblepartyPerGridarea").Should().BeTrue("because the calculation result should contain NonProfiledConsumption for AggregationPerBalanceresponsiblepartyPerGridarea");
                    CheckIfExistsInCaluclationResults("NonProfiledConsumption", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea").Should().BeTrue("because the calculation result should contain NonProfiledConsumption for AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea");
                    CheckIfExistsInCaluclationResults("Production", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain Production for AggregationPerGridarea");
                    CheckIfExistsInCaluclationResults("Production", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue("because the calculation result should contain Production for AggregationPerEnergysupplierPerGridarea");
                    CheckIfExistsInCaluclationResults("Production", "AggregationPerBalanceresponsiblepartyPerGridarea").Should().BeTrue("because the calculation result should contain Production for AggregationPerBalanceresponsiblepartyPerGridarea");
                    CheckIfExistsInCaluclationResults("Production", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea").Should().BeTrue("because the calculation result should contain Production for AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea");
                    // flex is not in current test data
                    // CheckIfExistsInCaluclationResults("FlexConsumption", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain FlexConsumption for AggregationPerGridarea");
                    // CheckIfExistsInCaluclationResults("FlexConsumption", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue("because the calculation result should contain FlexConsumption for AggregationPerEnergysupplierPerGridarea");
                    // CheckIfExistsInCaluclationResults("FlexConsumption", "AggregationPerBalanceresponsiblepartyPerGridarea").Should().BeTrue("because the calculation result should contain FlexConsumption for AggregationPerBalanceresponsiblepartyPerGridarea");
                    // CheckIfExistsInCaluclationResults("FlexConsumption", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea").Should().BeTrue("because the calculation result should contain FlexConsumption for AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea");
                    CheckIfExistsInCaluclationResults("NetExchangePerGa", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain NetExchangePerGa for AggregationPerGridarea");
                    CheckIfExistsInCaluclationResults("NetExchangePerNeighboringGa", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain NetExchangePerNeighboringGa for AggregationPerGridarea");
                    CheckIfExistsInCaluclationResults("GridLoss", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain GridLoss for AggregationPerGridarea");
                    CheckIfExistsInCaluclationResults("NegativeGridLoss", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain NegativeGridLoss for AggregationPerGridarea");
                    CheckIfExistsInCaluclationResults("PositiveGridLoss", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain PositiveGridLoss for AggregationPerGridarea");
                    CheckIfExistsInCaluclationResults("TotalConsumption", "AggregationPerGridarea").Should().BeTrue("because the calculation result should contain TotalConsumption for AggregationPerGridarea");
                }
            }

            [DomainFact(Skip = "Test fails on cold runs with a timeout error - expected to be fixed when switching to Databricks Serverless warehouse")]
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

            private void GetActualAndExpectedTimeSeriesTypes(out List<string?> actual, out List<string> expected)
            {
                actual = (_calculationResults ?? throw new InvalidOperationException("calculationResults in null")).Select(o => Enum.GetName(o.TimeSeriesType)).Distinct().ToList();
                expected = Enum.GetNames(typeof(TimeSeriesType)).ToList();
                expected.Remove("FlexConsumption"); // FlexConsumption is not in the current test data
            }

            private bool CheckIfExistsInCaluclationResults(string timeSeriesType, string aggregationLevel)
            {
                return (_calculationResults ?? throw new Exception("calculation results is null")).Any(obj => Enum.GetName(obj.TimeSeriesType) == timeSeriesType && Enum.GetName(obj.AggregationLevelCase) == aggregationLevel);
            }
        }
    }
}
