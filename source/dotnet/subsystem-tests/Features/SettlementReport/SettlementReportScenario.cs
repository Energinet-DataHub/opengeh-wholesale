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

using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReport.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReport;

[TestCaseOrderer(
    "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    "Energinet.DataHub.Wholesale.SubsystemTests")]
public class SettlementReportScenario : SubsystemTestsBase<SettlementReportScenarioFixture>
{
    public SettlementReportScenario(LazyFixtureFactory<SettlementReportScenarioFixture> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_SettlementDownloadInput()
    {
        // The values below are taken from BalanceFixingCalculationScenario. The tests below expects that the balance
        // fixing calculation has been executed. This means that the tests will fail the very first time they are run,
        // when there is no data in the delta table.
        Fixture.ScenarioState.SettlementDownloadInput.GridAreaCodes.Add("543");
        Fixture.ScenarioState.SettlementDownloadInput.ProcessType = ProcessType.BalanceFixing;
        Fixture.ScenarioState.SettlementDownloadInput.CalculationPeriodStart = DateTimeOffset.Parse("2022-01-11T23:00:00");
        Fixture.ScenarioState.SettlementDownloadInput.CalculationPeriodEnd = DateTimeOffset.Parse("2022-01-12T23:00:00");
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public async Task When_SettlementReportDownloadedIsStarted()
    {
        Fixture.ScenarioState.CompressedSettlementReport =
            await Fixture.StartDownloadingAsync(Fixture.ScenarioState.SettlementDownloadInput);
    }

    [ScenarioStep(2)]
    [SubsystemFact]
    public void Then_SettlementReportEntriesShouldNotBeEmpty()
    {
        // Assert
        Fixture.ScenarioState.CompressedSettlementReport.Entries.Should().NotBeEmpty();
    }

    [ScenarioStep(3)]
    [SubsystemFact]
    public void AndThen_SingleEntryNameShouldBeResultCsv()
    {
        var expected = "Result.csv";
        Fixture.ScenarioState.Entry = Fixture.ScenarioState.CompressedSettlementReport.Entries.Single();

        // Assert
        Fixture.ScenarioState.Entry.Name.Should().Be(expected);
    }

    [ScenarioStep(4)]
    [SubsystemFact]
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
    [SubsystemFact]
    public void AndThen_SingleEntryShouldContainCorrectGridAreaCodesAndProcessType()
    {
        var expected = "543,D04,";

        // Assert
        using var assertionScope = new AssertionScope();
        foreach (var line in Fixture.ScenarioState.EntryDataLines)
            line.Should().StartWith(expected);
    }

    [ScenarioStep(6)]
    [SubsystemFact]
    public void AndThen_TheUtcDateOfTheFirstLineShouldBeCorrect()
    {
        // Assert
        Fixture.GetUtcDate(Fixture.ScenarioState.EntryDataLines.First())
            .Should()
            .Be("2022-01-11T23:00:00Z");
    }

    [ScenarioStep(7)]
    [SubsystemFact]
    public void AndThen_TheUtcDateOfTheLastLineShouldBeCorrect()
    {
        // Assert
        Fixture.GetUtcDate(Fixture.ScenarioState.EntryDataLines.Last())
            .Should()
            .Be("2022-01-12T22:45:00Z");
    }
}
