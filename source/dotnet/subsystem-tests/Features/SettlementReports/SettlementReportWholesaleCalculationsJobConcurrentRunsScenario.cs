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
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports;

[Collection(nameof(SettlementReportWholesaleCalculationsJobCollectionDefinition))]
[ExecutionContext(AzureEnvironment.Dev001)]
[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
public class SettlementReportWholesaleCalculationsJobConcurrentRunsScenario : SubsystemTestsBase<SettlementReportJobScenarioFixture<ConcurrentRunsScenarioState>>
{
    public SettlementReportWholesaleCalculationsJobConcurrentRunsScenario(LazyFixtureFactory<SettlementReportJobScenarioFixture<ConcurrentRunsScenarioState>> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_ScenarioSetup()
    {
        // Input
        Fixture.ScenarioState.JobName = SettlementReportJobName.SettlementReportWholesaleCalculations;
        Fixture.ScenarioState.JobParametersTemplate = new[]
        {
          "--report-id=Guid",
          "--calculation-type=wholesale_fixing",
          $"--calculation-id-by-grid-area={{\"804\": \"{Fixture.Configuration.InputCalculationId}\"}}",
          "--period-start=2023-01-31T23:00:00Z",
          "--period-end=2023-02-28T23:00:00Z",
          "--market-role=datahub_administrator",
        };

        // Expectations
        Fixture.ScenarioState.ExpectedConcurrentRuns = 40;
        Fixture.ScenarioState.ExpectedClusterWarmupTimeLimit = TimeSpan.FromMinutes(10);
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public async Task When_MaxConcurrentRunsAreStarted()
    {
        Fixture.ScenarioState.JobRuns = await Fixture.StartSettlementReportJobRunsAsync(
            Fixture.ScenarioState.ExpectedConcurrentRuns,
            Fixture.ScenarioState.JobName,
            Fixture.ScenarioState.JobParametersTemplate);

        // Assert
        Fixture.ScenarioState.JobRuns.Should()
            .HaveCount(Fixture.ScenarioState.ExpectedConcurrentRuns)
            .And.OnlyContain(kv =>
                kv.Value == SettlementReportJobState.Pending
                || kv.Value == SettlementReportJobState.Running);
    }

    [ScenarioStep(2)]
    [SubsystemFact]
    public async Task Then_NextJobWeStartIsQueued()
    {
        Fixture.ScenarioState.ExceedingJobRuns = await Fixture.StartSettlementReportJobRunsAsync(
            concurrentRuns: 1,
            Fixture.ScenarioState.JobName,
            Fixture.ScenarioState.JobParametersTemplate);

        // Assert
        Fixture.ScenarioState.ExceedingJobRuns.Should()
            .HaveCount(1)
            .And.OnlyContain(kv => kv.Value == SettlementReportJobState.Queued);
    }

    /// <summary>
    /// It is not until a job run is actually running that the cluster is created and we
    /// know if we are allowed to create the VM's based on the configured quotas.
    /// </summary>
    [ScenarioStep(3)]
    [SubsystemFact]
    public async Task AndThen_AllJobRunsAreRunningWithinWaitTime()
    {
        var allAreRunning = await Fixture.WaitForSettlementReportJobRunsAreRunningAsync(
            Fixture.ScenarioState.JobRuns,
            waitTimeLimit: Fixture.ScenarioState.ExpectedClusterWarmupTimeLimit);

        // Assert
        allAreRunning.Should().BeTrue("because we expect all job runs to be running at the same time");
    }

    [ScenarioStep(4)]
    [SubsystemFact]
    public async Task AndThen_WeCleanup()
    {
        var allJobRunIds =
            Fixture.ScenarioState.JobRuns.Select(kv => kv.Key)
            .Union(
                Fixture.ScenarioState.ExceedingJobRuns.Select(kv => kv.Key));

        await Fixture.CancelSettlementReportJobRunsAsync(allJobRunIds.ToList().AsReadOnly());
    }
}
