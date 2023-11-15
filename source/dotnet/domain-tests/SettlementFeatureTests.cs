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
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests
{
    /// <summary>
    /// Contains tests with focus on verifying settlement report functionality using the Web API running in a live environment.
    /// </summary>
    public class SettlementFeatureTests
    {
        [TestCaseOrderer(
            ordererTypeName: "Energinet.DataHub.Wholesale.DomainTests.Fixtures.Orderers.PriorityOrderer",
            ordererAssemblyName: "Energinet.DataHub.Wholesale.DomainTests")]
        public class SettlementReportDownloadScenario : DomainTestsBase<SettlementReportScenarioFixture>
        {
            public SettlementReportDownloadScenario(LazyFixtureFactory<SettlementReportScenarioFixture> lazyFixtureFactory)
                : base(lazyFixtureFactory)
            {
            }

            [Priority(0)]
            [DomainFact]
            public void Given_SettlementDownloadInput()
            {
                Fixture.ScenarioState.SettlementDownloadInput.GridAreaCodes.Add("543");
                Fixture.ScenarioState.SettlementDownloadInput.ProcessType = Clients.v3.ProcessType.BalanceFixing;
                Fixture.ScenarioState.SettlementDownloadInput.CalculationPeriodStart = DateTimeOffset.Parse("2020-01-28T23:00:00Z");
                Fixture.ScenarioState.SettlementDownloadInput.CalculationPeriodEnd = DateTimeOffset.Parse("2020-01-29T23:00:00Z");
            }

            [Priority(1)]
            [DomainFact]
            public async Task When_SettlementReportDownloadedIsStarted()
            {
                Fixture.ScenarioState.SettlementReportFile = await Fixture.StartDownloadingAsync(Fixture.ScenarioState.SettlementDownloadInput);
            }

            [Priority(2)]
            [DomainFact]
            public void Then_SettlementReportEntriesShouldNotBeEmpty()
            {
                Fixture.ScenarioState.CompressedSettlementReport = new ZipArchive(Fixture.ScenarioState.SettlementReportFile.Stream, ZipArchiveMode.Read);

                // Assert
                Fixture.ScenarioState.CompressedSettlementReport.Entries.Should().NotBeEmpty();
            }

            [Priority(3)]
            [DomainFact]
            public void AndThen_SingleEntryNameShouldBeResultCsv()
            {
                var expected = "Result.csv";
                Fixture.ScenarioState.Entry = Fixture.ScenarioState.CompressedSettlementReport.Entries.Single();

                // Assert
                Fixture.ScenarioState.Entry.Name.Should().Be(expected);
            }

            [Priority(4)]
            [DomainFact]
            public async Task AndThen_SingleEntryShouldContainCorrectGridAreaCodesAndProcessType()
            {
                var expected = "543,D04,";
                var lines = await Fixture.SplitEntryIntoLinesAsync(Fixture.ScenarioState.Entry);
                foreach (var line in lines[1..]) //// The first line is the header.
                {
                    // Assert
                    line.Should().StartWith(expected);
                }
            }
        }
    }
}
