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

[Collection(nameof(SettlementReportJobCollectionDefinition))]
[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
public class SettlementReportJobConcurrentRunsScenario : SubsystemTestsBase<SettlementReportJobScenarioFixture<ConcurrentRunsScenarioState>>
{
    public SettlementReportJobConcurrentRunsScenario(LazyFixtureFactory<SettlementReportJobScenarioFixture<ConcurrentRunsScenarioState>> lazyFixtureFactory)
        : base(lazyFixtureFactory)
    {
    }

    [ScenarioStep(0)]
    [SubsystemFact]
    public void Given_ScenarioSetup()
    {
        // Input
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
        Fixture.ScenarioState.ExpectedMaxConcurrentRuns = 40;
    }

    [ScenarioStep(1)]
    [SubsystemFact]
    public async Task When_MaxConcurrentRunsAreStarted()
    {
        Fixture.ScenarioState.JobRuns = await Fixture.StartSettlementReportJobRunsAsync(
            Fixture.ScenarioState.ExpectedMaxConcurrentRuns,
            Fixture.ScenarioState.JobParametersTemplate);

        // Assert
        Fixture.ScenarioState.JobRuns.Should()
            .HaveCount(Fixture.ScenarioState.ExpectedMaxConcurrentRuns)
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
        // XDAST - TODO: Wait for running state
        await Task.Delay(100);
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
