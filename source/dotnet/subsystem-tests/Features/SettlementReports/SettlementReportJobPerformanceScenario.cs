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

[ExecutionEnvironment(AzureEnvironment.Dev003)]
[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
public class SettlementReportJobPerformanceScenario : SubsystemTestsBase<SettlementReportJobScenarioFixture<PerformanceScenarioState>>
{
    public SettlementReportJobPerformanceScenario(LazyFixtureFactory<SettlementReportJobScenarioFixture<PerformanceScenarioState>> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_ScenarioSetup()
    {
        // Input
        Fixture.ScenarioState.ReportId = Guid.NewGuid();
        Fixture.ScenarioState.JobParameters = new[]
        {
            $"--report-id={Fixture.ScenarioState.ReportId}",
            "--period-start=2024-06-30T22:00:00Z",
            "--period-end=2024-07-31T22:00:00Z",
            "--calculation-type=wholesale_fixing",
            "--requesting-actor-market-role=datahub_administrator",
            "--requesting-actor-id=1234567890123",
            "--calculation-id-by-grid-area=" +
                "{" +
                    "\"003\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"007\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"016\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"031\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"042\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"051\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"084\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"085\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"131\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"141\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"151\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"154\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"233\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"244\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"245\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"331\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"341\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"342\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"344\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"347\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"348\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"351\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"357\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"370\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"371\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"381\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"384\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"385\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"396\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"531\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"532\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"533\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"543\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"584\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"740\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"757\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"791\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"853\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"854\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"860\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"911\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"950\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"951\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"952\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"953\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"954\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"960\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"962\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"," +
                    "\"990\": \"32e49805-20ef-4db2-ac84-c4455de7a373\"" +
                "}",
            "--split-report-by-grid-area",
            "--prevent-large-text-files",
        };

        // Expectations
        Fixture.ScenarioState.ExpectedJobTimeLimit = TimeSpan.FromHours(2);
        Fixture.ScenarioState.ExpectedRelativeOutputFilePath =
            $"/wholesale_settlement_report_output/settlement_reports/{Fixture.ScenarioState.ReportId}.zip";
        Fixture.ScenarioState.ExpectedMinimumOutputFileSizeInBytes = 4000000000;
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public async Task When_JobIsStarted()
    {
        Fixture.ScenarioState.JobRunId = await Fixture.StartSettlementReportJobRunAsync(
            Fixture.ScenarioState.ReportId,
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
        Fixture.ScenarioState.OutputFileInfo = await Fixture.GetFileInfoAsync(Fixture.ScenarioState.ExpectedRelativeOutputFilePath);

        // Assert
        Fixture.ScenarioState.OutputFileInfo.Should().NotBeNull($"because we expected the file (relative path) '{Fixture.ScenarioState.ExpectedRelativeOutputFilePath}' to exists.");
    }

    [ScenarioStep(4)]
    [SubsystemFact]
    public void AndThen_OutputFileSizeIsGreatherThan()
    {
        // Assert
        Fixture.ScenarioState.OutputFileInfo.ContentLength.Should().BeGreaterThan(Fixture.ScenarioState.ExpectedMinimumOutputFileSizeInBytes);
    }

    /// <summary>
    /// In this step we verify the 'duration' of the job is within our 'performance goal'.
    /// </summary>
    [ScenarioStep(5)]
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
