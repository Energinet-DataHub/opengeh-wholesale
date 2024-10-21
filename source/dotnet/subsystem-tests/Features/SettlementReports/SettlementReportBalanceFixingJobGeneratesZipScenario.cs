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
using Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports;

[Collection(nameof(SettlementReportJobCollectionDefinition))]
[ExecutionContext(AzureEnvironment.AllDev)]
[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
public class SettlementReportBalanceFixingJobGeneratesZipScenario : SubsystemTestsBase<SettlementReportJobScenarioFixture<GeneratesZipScenarioState>>
{
    public SettlementReportBalanceFixingJobGeneratesZipScenario(LazyFixtureFactory<SettlementReportJobScenarioFixture<GeneratesZipScenarioState>> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_ScenarioSetup()
    {
        // Input
        Fixture.ScenarioState.ReportId = Guid.NewGuid();
        Fixture.ScenarioState.JobName = SettlementReportJobName.SettlementReportBalanceFixing;
        Fixture.ScenarioState.JobParameters = new[]
        {
            $"--report-id={Fixture.ScenarioState.ReportId}",
            "--period-start=2023-01-31T23:00:00Z",
            "--period-end=2023-02-28T23:00:00Z",
            "--calculation-type=balance_fixing",
            "--requesting-actor-market-role=datahub_administrator",
            "--requesting-actor-id=1234567890123",
            "--include-basis-data",
            $"--grid_area_codes=[804]",
        };

        // Expectations
        Fixture.ScenarioState.ExpectedJobTimeLimit = TimeSpan.FromMinutes(15);
        Fixture.ScenarioState.ExpectedRelativeOutputFilePath =
            $"/wholesale_settlement_report_output/settlement_reports/{Fixture.ScenarioState.ReportId}.zip";
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public async Task When_JobIsStarted()
    {
        Fixture.ScenarioState.JobRunId = await Fixture.StartSettlementReportJobRunAsync(
            Fixture.ScenarioState.ReportId,
            Fixture.ScenarioState.JobName,
            Fixture.ScenarioState.JobParameters);

        // Assert
        Fixture.ScenarioState.JobRunId.Should().BePositive();
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
        var (isCompleted, run) = await Fixture.WaitForSettlementReportJobRunCompletedAsync(
            Fixture.ScenarioState.JobRunId,
            waitTimeLimit: Fixture.ScenarioState.ExpectedJobTimeLimit.Add(TimeSpan.FromMinutes(5)));

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
        var outputFileInfo = await Fixture.GetFileInfoAsync(Fixture.ScenarioState.ExpectedRelativeOutputFilePath);

        // Assert
        outputFileInfo.Should().NotBeNull($"because we expected the file (relative path) '{Fixture.ScenarioState.ExpectedRelativeOutputFilePath}' to exists.");
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
        using var assertionScope = new AssertionScope();
        actualCalculationJobDuration.Should().BeGreaterThan(TimeSpan.Zero);
        actualCalculationJobDuration.Should().BeLessThanOrEqualTo(Fixture.ScenarioState.ExpectedJobTimeLimit);
    }
}
