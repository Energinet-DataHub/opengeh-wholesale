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
using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using Energinet.DataHub.Wholesale.DomainTests.SettlementReportFeatures.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.SettlementReportFeatures;

[TestCaseOrderer(
    "Energinet.DataHub.Wholesale.DomainTests.Fixtures.Orderers.PriorityOrderer",
    "Energinet.DataHub.Wholesale.DomainTests")]
public class SettlementReportLinesScenario : DomainTestsBase<SettlementReportScenarioFixture>
{
    public SettlementReportLinesScenario(LazyFixtureFactory<SettlementReportScenarioFixture> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [Priority(0)]
    [DomainFact(Skip = "AJW: The Web Api must be updated first.")]
    public void Given_SettlementDownloadInput()
    {
        Fixture.ScenarioState.SettlementDownloadInput.GridAreaCodes.Add("804");
        Fixture.ScenarioState.SettlementDownloadInput.ProcessType = ProcessType.BalanceFixing;
        Fixture.ScenarioState.SettlementDownloadInput.CalculationPeriodStart = DateTimeOffset.Parse("2023-01-31T23:00:00");
        Fixture.ScenarioState.SettlementDownloadInput.CalculationPeriodEnd = DateTimeOffset.Parse("2023-02-01T23:00:00");
    }

    [Priority(1)]
    [DomainFact(Skip = "AJW: The Web Api must be updated first.")]
    public async Task When_SettlementReportDownloadedIsStarted()
    {
        Fixture.ScenarioState.SettlementReportFile =
            await Fixture.StartDownloadingAsync(Fixture.ScenarioState.SettlementDownloadInput);
    }

    [Priority(2)]
    [DomainFact(Skip = "AJW: The Web Api must be updated first.")]
    public void Then_SettlementReportEntriesShouldNotBeEmpty()
    {
        Fixture.ScenarioState.CompressedSettlementReport =
            new ZipArchive(Fixture.ScenarioState.SettlementReportFile.Stream, ZipArchiveMode.Read);

        // Assert
        Fixture.ScenarioState.CompressedSettlementReport.Entries.Should().NotBeEmpty();
    }

    [Priority(3)]
    [DomainFact(Skip = "AJW: The Web Api must be updated first.")]
    public async Task AndThen_NumberOfEntriesShouldBeCorrect()
    {
        Fixture.ScenarioState.Entry = Fixture.ScenarioState.CompressedSettlementReport.Entries.Single();
        var lines = await Fixture.SplitEntryIntoLinesAsync(Fixture.ScenarioState.Entry);
        var (consumptionLines, productionLines, exchangeLines) = Fixture.CountTimeSeriesTypes(lines);

        // Assert
        productionLines.Should().Be(96);
        exchangeLines.Should().Be(96);
        consumptionLines.Should().Be(288);

        var firstEntryTime = lines[1].Split(",")[2];
        var lastEntryTime = lines.Last().Split(",")[2];
        firstEntryTime.Should().Be("2023-01-31T23:00:00Z");
        lastEntryTime.Should().Be("2023-02-01T22:45:00Z");
    }
}
