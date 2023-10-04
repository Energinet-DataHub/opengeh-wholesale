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
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using ProcessType = Energinet.DataHub.Wholesale.DomainTests.Clients.v3.ProcessType;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

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
                // Act
                using var actualResponse = await UnauthorizedHttpClient.GetAsync("monitor/ready");

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var actualContent = await actualResponse.Content.ReadAsStringAsync();
                actualContent.Should().StartWith("{\"status\":\"Healthy\"");
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

            private static List<CalculationResultCompleted> _energyCalculationResults = null!;
            private static List<CalculationResultCompleted> _wholesaleCalculationResults = null!;

            public Given_Authorized(AuthorizedClientFixture fixture)
            {
                Fixture = fixture;
                _energyCalculationResults = Fixture.Output.EnergyCalculationResults;
                _wholesaleCalculationResults = Fixture.Output.WholesaleCalculationResults;
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
            public void When_CreatingEnergyCalculationBatch_Then_BatchIsEventuallyCompleted()
            {
                Fixture.Output.EnergyCalculationIsComplete.Should().BeTrue();
            }

            [DomainFact]
            public void When_CreatingWholesaleCalculationBatch_Then_BatchIsEventuallyCompleted()
            {
                Fixture.Output.WholesaleCalculationIsComplete.Should().BeTrue();
            }

            [DomainFact]
            public void When_EnergyCalculationBatchIsCompleted_Then_BatchIsReceivedOnTopicSubscription()
            {
                _energyCalculationResults.Count.Should().Be(112);
            }

            [DomainFact]
            public void When_WholesaleCalculationBatchIsCompleted_Then_BatchIsReceivedOnTopicSubscription()
            {
                _wholesaleCalculationResults.Count.Should().Be(137);
            }

            [DomainFact]
            public void When_EnergyCalculationBatchIsComplete_Then_MessagesReceivedContainAllTimeSeriesTypes()
            {
                var actualTimeSeriesTypes = GetTimeSeriesTypes(_energyCalculationResults);
                foreach (var expectedTimeSeriesType in EnergyCalculationTimeSeriesTypes)
                {
                    actualTimeSeriesTypes.Should().Contain(expectedTimeSeriesType);
                }
            }

            [DomainFact]
            public void When_WholesaleCalculationBatchIsComplete_Then_MessagesReceivedContainAllTimeSeriesTypes()
            {
                var actualTimeSeriesTypes = GetTimeSeriesTypes(_wholesaleCalculationResults);
                foreach (var expectedTimeSeriesType in WholesaleCalculationTimeSeriesTypes)
                {
                    actualTimeSeriesTypes.Should().Contain(expectedTimeSeriesType);
                }
            }

            [DomainFact]
            public void When_EnergyCalculationBatchIsReceivedOnTopicSubscription_Then_MessagesReceivedContainAllTypesOfCalculations()
            {
                using (new AssertionScope())
                {
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "NonProfiledConsumption", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "NonProfiledConsumption", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "NonProfiledConsumption", "AggregationPerBalanceresponsiblepartyPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "NonProfiledConsumption", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "Production", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "Production", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "Production", "AggregationPerBalanceresponsiblepartyPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "Production", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "FlexConsumption", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "FlexConsumption", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "FlexConsumption", "AggregationPerBalanceresponsiblepartyPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "FlexConsumption", "AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "NetExchangePerGa", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "NetExchangePerNeighboringGa", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "GridLoss", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "NegativeGridLoss", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "PositiveGridLoss", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "TotalConsumption", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "TempFlexConsumption", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_energyCalculationResults, "TempProduction", "AggregationPerGridarea").Should().BeTrue();
                }
            }

            [DomainFact]
            public void When_WholesaleCalculationBatchIsReceivedOnTopicSubscription_Then_MessagesReceivedContainAllTypesOfCalculations()
            {
                using (new AssertionScope())
                {
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "NonProfiledConsumption", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "NonProfiledConsumption", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "Production", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "Production", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "FlexConsumption", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "FlexConsumption", "AggregationPerEnergysupplierPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "NetExchangePerGa", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "GridLoss", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "NegativeGridLoss", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "PositiveGridLoss", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "TotalConsumption", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "TempFlexConsumption", "AggregationPerGridarea").Should().BeTrue();
                    CheckIfExistsInCalculationResults(_wholesaleCalculationResults, "TempProduction", "AggregationPerGridarea").Should().BeTrue();
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

            private List<string> EnergyCalculationTimeSeriesTypes { get; } = Enum.GetNames(typeof(TimeSeriesType)).ToList();

            private List<string> WholesaleCalculationTimeSeriesTypes
                => EnergyCalculationTimeSeriesTypes.Where(s => s != nameof(TimeSeriesType.NetExchangePerNeighboringGa)).ToList();

            private List<string?> GetTimeSeriesTypes(List<CalculationResultCompleted> calculationResults)
            {
                return calculationResults.Select(o => Enum.GetName(o.TimeSeriesType)).Distinct().ToList();
            }

            private bool CheckIfExistsInCalculationResults(
                List<CalculationResultCompleted>? calculationResults,
                string timeSeriesType,
                string aggregationLevel)
            {
                ArgumentNullException.ThrowIfNull(calculationResults);

                return calculationResults.Any(
                    obj => Enum.GetName(obj.TimeSeriesType) == timeSeriesType
                           && Enum.GetName(obj.AggregationLevelCase) == aggregationLevel);
            }
        }
    }
}
