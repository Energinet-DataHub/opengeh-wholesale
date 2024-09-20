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

using Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports;

[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
public class SettlementReportJobGeneratesZipScenario : SubsystemTestsBase<SettlementReportJobScenarioFixture>
{
    public SettlementReportJobGeneratesZipScenario(LazyFixtureFactory<SettlementReportJobScenarioFixture> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_ScenarioSetup()
    {
        // Input
        Fixture.ScenarioState.ReportId = Guid.NewGuid();
        Fixture.ScenarioState.JobParameters =
        [
            $"--report-id={Fixture.ScenarioState.ReportId}",
            "--calculation-type=wholesale_fixing",
            "--calculation-id-by-grid-area={\"804\": \"4084c118-735b-44a5-827b-8314e0c02544\"}",
            "--period-start=2023-01-31T23:00:00Z",
            "--period-end=2023-02-28T23:00:00Z",
            "--market-role=datahub_administrator",
            "--split-report-by-grid-area",
            "--prevent-large-text-files",
        ];

        // Expectations
        Fixture.ScenarioState.ExpectedJobTimeLimit = TimeSpan.FromMinutes(25);
        Fixture.ScenarioState.ExpectedRelativeOutputFilePath =
            $"wholesale_settlement_report_output/settlement_reports/{Fixture.ScenarioState.ReportId}/{Fixture.ScenarioState.ReportId}.zip";
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public async Task When_JobIsStarted()
    {
        Fixture.ScenarioState.JobId = await Fixture.StartSettlementReportJobAsync(
            Fixture.ScenarioState.ReportId,
            Fixture.ScenarioState.JobParameters.AsReadOnly());

        // Assert
        Fixture.ScenarioState.JobId.Should().BePositive();
    }

    /// <summary>
    /// In this step we focus on completing the job with a certain 'wait time'.
    /// This is not an exact time for how long it took to perform the job,
    /// but the time it took for our retry loop to determine that the job has completed.
    /// </summary>
    [ScenarioStep(2)]
    [SubsystemFact]
    public async Task Then_JobIsCompletedWithinWaitTime()
    {
        var (isCompleted, run) = await Fixture.WaitForSettlementReportJobCompletedAsync(
            Fixture.ScenarioState.JobId,
            waitTimeLimit: Fixture.ScenarioState.ExpectedJobTimeLimit.Add(TimeSpan.FromMinutes(10)));

        Fixture.ScenarioState.Run = run;

        // Assert
        using var assertionScope = new AssertionScope();
        isCompleted.Should().BeTrue();
        run.Should().NotBeNull();
    }

    [ScenarioStep(3)]
    [SubsystemFact]
    public async Task AndThen_OutputFileIsGeneratedAtExpectedLocation()
    {
        var outputFileExists = await Fixture.FileExistsAsync(Fixture.ScenarioState.ExpectedRelativeOutputFilePath);

        // Assert
        outputFileExists.Should().BeTrue();
    }

    /// <summary>
    /// In this step we verify the 'duration' of the job is within our 'performance goal'.
    /// </summary>
    [ScenarioStep(4)]
    [SubsystemFact]
    public void AndThen_JobDurationIsLessThanOrEqualToTimeLimit()
    {
        var actualCalculationJobDuration =
            Fixture.ScenarioState.Run.EndTime - Fixture.ScenarioState.Run.StartTime;

        // Assert
        actualCalculationJobDuration.Should().BeGreaterThan(TimeSpan.Zero);
        actualCalculationJobDuration.Should().BeLessThanOrEqualTo(Fixture.ScenarioState.ExpectedJobTimeLimit);
    }
}
