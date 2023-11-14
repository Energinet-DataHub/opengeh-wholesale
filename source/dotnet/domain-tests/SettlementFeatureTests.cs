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
using Energinet.DataHub.Wholesale.DomainTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests
{
    /// <summary>
    /// Contains tests with focus on verifying settlement report functionality using the Web API running in a live environment.
    /// </summary>
    public class SettlementFeatureTests
    {
        /// <summary>
        /// These tests uses an authorized Wholesale client to perform requests.
        /// </summary>
        public class Given_Calculated : DomainTestsBase<AuthorizedClientFixture>
        {
            private static readonly DateTimeOffset _existingBatchPeriodStart = DateTimeOffset.Parse("2020-01-28T23:00:00Z");
            private static readonly DateTimeOffset _existingBatchPeriodEnd = DateTimeOffset.Parse("2020-01-29T23:00:00Z");
            private static readonly string _existingGridAreaCode = "543";

            public Given_Calculated(LazyFixtureFactory<AuthorizedClientFixture> lazyFixtureFactory)
                : base(lazyFixtureFactory)
            {
            }

            [DomainFact(Skip = "Test fails on cold runs with a timeout error - expected to be fixed when switching to Databricks Serverless warehouse")]
            public async Task WhenDownloadingSettlementReport_ResponseIsCompressedFileWithData()
            {
                // Arrange + Act
                var fileResponse = await Fixture.WholesaleClient.DownloadAsync(
                    new[] { _existingGridAreaCode },
                    Clients.v3.ProcessType.BalanceFixing,
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
