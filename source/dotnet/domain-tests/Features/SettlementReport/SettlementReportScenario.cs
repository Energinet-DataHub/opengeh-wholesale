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

using Energinet.DataHub.Wholesale.DomainTests.Clients.v3;
using Energinet.DataHub.Wholesale.DomainTests.Features.SettlementReport.Fixtures;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.DomainTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.Features.SettlementReport;

[TestCaseOrderer(
    "Energinet.DataHub.Wholesale.DomainTests.Fixtures.Orderers.ScenarioStepOrderer",
    "Energinet.DataHub.Wholesale.DomainTests")]
public class SettlementReportScenario : DomainTestsBase<SettlementReportScenarioFixture>
{
    public SettlementReportScenario(LazyFixtureFactory<SettlementReportScenarioFixture> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [DomainFact]
    public void Given_SettlementDownloadInput()
    {
        Fixture.ScenarioState.SettlementDownloadInput.GridAreaCodes.Add("804");
        Fixture.ScenarioState.SettlementDownloadInput.ProcessType = ProcessType.BalanceFixing;
        Fixture.ScenarioState.SettlementDownloadInput.CalculationPeriodStart = DateTimeOffset.Parse("2023-01-31T23:00:00");
        Fixture.ScenarioState.SettlementDownloadInput.CalculationPeriodEnd = DateTimeOffset.Parse("2023-02-01T23:00:00");
    }

    [ScenarioStep(1)]
    [DomainFact]
    public async Task When_SettlementReportDownloadedIsStarted()
    {
        Fixture.ScenarioState.CompressedSettlementReport =
            await Fixture.StartDownloadingAsync(Fixture.ScenarioState.SettlementDownloadInput);
    }

    [ScenarioStep(2)]
    [DomainFact]
    public void Then_SettlementReportEntriesShouldNotBeEmpty()
    {
        // Assert
        Fixture.ScenarioState.CompressedSettlementReport.Entries.Should().NotBeEmpty();
    }

    [ScenarioStep(3)]
    [DomainFact]
    public void AndThen_SingleEntryNameShouldBeResultCsv()
    {
        var expected = "Result.csv";
        Fixture.ScenarioState.Entry = Fixture.ScenarioState.CompressedSettlementReport.Entries.Single();

        // Assert
        Fixture.ScenarioState.Entry.Name.Should().Be(expected);
    }

    [ScenarioStep(4)]
    [DomainFact]
    public async Task AndThen_NumberOfLinesPrTimeSeriesTypesShouldBeCorrect()
    {
        Fixture.ScenarioState.EntryDataLines = await Fixture.SplitEntryIntoDataLinesAsync(Fixture.ScenarioState.Entry);
        var typeSeriesTypeLines = Fixture.CountLinesPerTimeSeriesTypes(Fixture.ScenarioState.EntryDataLines);

        // Assert
        typeSeriesTypeLines.ProductionLines.Should().Be(96); //// 4 x 15 minutes x 24 hours = 96
        typeSeriesTypeLines.ExchangeLines.Should().Be(96);
        typeSeriesTypeLines.ConsumptionLines.Should().Be(288); //// 4 x 15 minutes x 24 hours x 3 types of consumption = 288
    }

    [ScenarioStep(5)]
    [DomainFact]
    public void AndThen_SingleEntryShouldContainCorrectGridAreaCodesAndProcessType()
    {
        var expected = "804,D04,";

        // Assert
        using var assertionScope = new AssertionScope();
        foreach (var line in Fixture.ScenarioState.EntryDataLines)
            line.Should().StartWith(expected);
    }

    [ScenarioStep(6)]
    [DomainFact]
    public void AndThen_TheUtcDateOfTheFirstLineShouldBeCorrect()
    {
        // Assert
        Fixture.GetUtcDate(Fixture.ScenarioState.EntryDataLines.First())
            .Should()
            .Be("2023-01-31T23:00:00Z");
    }

    [ScenarioStep(7)]
    [DomainFact]
    public void AndThen_TheUtcDateOfTheLastLineShouldBeCorrect()
    {
        // Assert
        Fixture.GetUtcDate(Fixture.ScenarioState.EntryDataLines.Last())
            .Should()
            .Be("2023-02-01T22:45:00Z");
    }
}
