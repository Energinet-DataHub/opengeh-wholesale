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

using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.Fixtures.Databricks;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using Energinet.DataHub.Wholesale.SubsystemTests.Performance.Fixtures;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Azure.Databricks.Client.Models;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.Fixtures;

public sealed class SettlementReportJobScenarioFixture<TScenarioState> : LazyFixtureBase
    where TScenarioState : new()
{
    public SettlementReportJobScenarioFixture(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        Configuration = new SettlementReportJobScenarioConfiguration();
        ScenarioState = new TScenarioState();
    }

    public TScenarioState ScenarioState { get; }

    public SettlementReportJobScenarioConfiguration Configuration { get; }

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private DatabricksClient DatabricksClient { get; set; } = null!;

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private FilesDatabricksClient FilesDatabricksClient { get; set; } = null!;

    public async Task<long> StartSettlementReportJobRunAsync(Guid reportId, IReadOnlyCollection<string> jobParameters)
    {
        var settlementReportJobId = await DatabricksClient.GetSettlementReportJobIdAsync();
        var runParameters = RunParameters.CreatePythonParams(jobParameters);

        var runId = await DatabricksClient
            .Jobs
            .RunNow(settlementReportJobId, runParameters);

        DiagnosticMessageSink.WriteDiagnosticMessage($"'SettlementReportJob' for '{reportId}' with run id '{runId}' started.");

        return runId;
    }

    public async Task<(bool IsCompleted, Run? Run)> WaitForSettlementReportJobRunCompletedAsync(
        long runId,
        TimeSpan waitTimeLimit)
    {
        var delay = TimeSpan.FromMinutes(1);

        (Run, RepairHistory) runState = default;
        SettlementReportJobState? settlementReportJobState = SettlementReportJobState.Pending;
        var isCondition = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                runState = await DatabricksClient.Jobs.RunsGet(runId);
                settlementReportJobState = ConvertToSettlementReportJobState(runState.Item1);

                return
                    settlementReportJobState is SettlementReportJobState.Completed
                    or SettlementReportJobState.Failed
                    or SettlementReportJobState.Canceled;
            },
            waitTimeLimit,
            delay);

        DiagnosticMessageSink.WriteDiagnosticMessage($"Wait for 'SettlementReportJob' with run id '{runId}' completed with '{nameof(isCondition)}={isCondition}' and '{nameof(settlementReportJobState)}={settlementReportJobState}'.");

        return (settlementReportJobState == SettlementReportJobState.Completed, runState.Item1);
    }

    /// <summary>
    /// Determine if a file exists in the Databricks Catalogue.
    /// </summary>
    /// <param name="relativeFilePath">File path relative to the Databricks Catalogue root configured per environment.</param>
    public async Task<bool> FileExistsAsync(string relativeFilePath)
    {
        try
        {
            var absoluteFilePath = $"{Configuration.DatabricksCatalogRoot}{relativeFilePath}";
            await FilesDatabricksClient.Files.GetFileInfoAsync(absoluteFilePath);
            return true;
        }
        catch (Exception ex)
        {
            DiagnosticMessageSink.WriteDiagnosticMessage($"File exists failed with exception: {ex}.");
            return false;
        }
    }

    protected override Task OnInitializeAsync()
    {
        DatabricksClient = DatabricksClient.CreateClient(Configuration.DatabricksWorkspace.BaseUrl, Configuration.DatabricksWorkspace.Token);
        FilesDatabricksClient = new FilesDatabricksClient(Configuration.DatabricksWorkspace.BaseUrl, Configuration.DatabricksWorkspace.Token);

        return Task.CompletedTask;
    }

    protected override Task OnDisposeAsync()
    {
        DatabricksClient.Dispose();
        FilesDatabricksClient.Dispose();

        return Task.CompletedTask;
    }

    private static SettlementReportJobState ConvertToSettlementReportJobState(Run run)
    {
        return run.State.LifeCycleState switch
        {
            RunLifeCycleState.PENDING => SettlementReportJobState.Pending,
            RunLifeCycleState.QUEUED => SettlementReportJobState.Queued,
            RunLifeCycleState.RUNNING => SettlementReportJobState.Running,
            RunLifeCycleState.TERMINATING => SettlementReportJobState.Running,
            RunLifeCycleState.SKIPPED => SettlementReportJobState.Canceled,
            RunLifeCycleState.INTERNAL_ERROR => SettlementReportJobState.Failed,
            RunLifeCycleState.TERMINATED => run.State.ResultState switch
            {
                RunResultState.SUCCESS => SettlementReportJobState.Completed,
                RunResultState.FAILED => SettlementReportJobState.Failed,
                RunResultState.CANCELED => SettlementReportJobState.Canceled,
                RunResultState.TIMEDOUT => SettlementReportJobState.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(run.State)),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(run.State)),
        };
    }
}
