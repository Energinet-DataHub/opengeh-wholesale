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
using FileInfo = Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.Fixtures.Databricks.FileInfo;

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

    public async Task CancelSettlementReportJobRunsAsync(IReadOnlyCollection<long> jobRunIds)
    {
        foreach (var jobRunId in jobRunIds)
        {
            try
            {
                await DatabricksClient.Jobs.RunsCancel(jobRunId);
            }
            catch
            {
            }
        }
    }

    public async Task<IReadOnlyDictionary<long, SettlementReportJobState>> StartSettlementReportJobRunsAsync(int concurrentRuns, SettlementReportJobName jobName, IReadOnlyCollection<string> jobParametersTemplate)
    {
        var jobRuns = new Dictionary<long, SettlementReportJobState>();
        for (var index = 0; index < concurrentRuns; index++)
        {
            var reportId = Guid.NewGuid();
            var jobParameters = jobParametersTemplate
                .Select(parameter => parameter.Replace("Guid", reportId.ToString()))
                .ToList();

            var runId = await StartSettlementReportJobRunAsync(reportId, jobName, jobParameters.AsReadOnly());
            var runState = await DatabricksClient.Jobs.RunsGet(runId);
            var settlementReportJobState = ConvertToSettlementReportJobState(runState.Item1);

            jobRuns.Add(runId, settlementReportJobState);
        }

        return jobRuns.AsReadOnly();
    }

    public async Task<bool> WaitForSettlementReportJobRunsAreRunningAsync(IReadOnlyDictionary<long, SettlementReportJobState> jobRuns, TimeSpan waitTimeLimit)
    {
        var delay = TimeSpan.FromMinutes(1);

        var allAreRunning = false;
        var runIds = jobRuns
            .Select(kv => kv.Key)
            .ToList();
        await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                // We can only verify clusters and quotas if we have all jobs running at the same time,
                // so we want to verify them all whenever we check the awaiter condition.
                foreach (var runId in runIds)
                {
                    var runState = await DatabricksClient.Jobs.RunsGet(runId);
                    var settlementReportJobState = ConvertToSettlementReportJobState(runState.Item1);
                    switch (settlementReportJobState)
                    {
                        case SettlementReportJobState.Running:
                            // Not pending anymore: Continue 'for' loop and verify the next run
                            continue;
                        case SettlementReportJobState.Pending:
                            // Not running yet: Skip verifying other runs for the moment
                            return false;
                        case SettlementReportJobState.Completed:
                        case SettlementReportJobState.Queued:
                        case SettlementReportJobState.Canceled:
                        case SettlementReportJobState.Failed:
                            // Failure: We can only verify clusters and quotas if we have all jobs running at the same time
                            // => Break out of 'awaiter' loop
                            return true;
                        default:
                            throw new InvalidOperationException($"Unexpected state '{settlementReportJobState}'.");
                    }
                }

                // All runs are now running
                allAreRunning = true;
                return allAreRunning;
            },
            waitTimeLimit,
            delay);

        return allAreRunning;
    }

    public async Task<long> StartSettlementReportJobRunAsync(Guid reportId, SettlementReportJobName jobName, IReadOnlyCollection<string> jobParameters)
    {
        var jobNameAsString = jobName.ToString();
        var settlementReportJobId = await DatabricksClient.GetJobIdAsync(jobNameAsString);
        var runParameters = RunParameters.CreatePythonParams(jobParameters);

        var runId = await DatabricksClient
            .Jobs
            .RunNow(settlementReportJobId, runParameters);

        DiagnosticMessageSink.WriteDiagnosticMessage($"'{jobNameAsString}' for '{reportId}' with run id '{runId}' started.");

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

    public string GetAbsolutePath(string relativeFilePath)
    {
        return $"{Configuration.DatabricksCatalogRoot}{relativeFilePath}";
    }

    /// <summary>
    /// Get file information for at file in the Databricks Catalogue.
    /// </summary>
    /// <param name="relativeFilePath">File path relative to the Databricks Catalogue root configured per environment.</param>
    /// <returns>File information if file exists; otherwise null.</returns>
    public async Task<FileInfo?> GetFileInfoAsync(string relativeFilePath)
    {
        try
        {
            return await FilesDatabricksClient.Files.GetFileInfoAsync(GetAbsolutePath(relativeFilePath));
        }
        catch (Exception ex)
        {
            DiagnosticMessageSink.WriteDiagnosticMessage($"File exists failed with exception: {ex}.");
            return null;
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
