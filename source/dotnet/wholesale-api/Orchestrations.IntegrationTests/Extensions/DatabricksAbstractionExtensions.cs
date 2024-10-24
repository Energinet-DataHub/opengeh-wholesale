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

using Microsoft.Azure.Databricks.Client.Models;
using WireMock.Server;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

/// <summary>
/// A collection of extensions methods that provides abstractions on top of
/// the more technical databricks api extensions in <see cref="DatabricksApiWireMockExtensions"/>
/// </summary>
public static class DatabricksAbstractionExtensions
{
    /// <summary>
    /// Setup databrick api response mocks to be able to respond with energy results for the calculation id
    /// provided by <paramref name="calculationId"/>
    /// </summary>
    /// <param name="server"></param>
    /// <param name="calculationId">The calculation id for the energy results, defaults
    /// to <see cref="Guid"/>.<see cref="Guid.NewGuid()"/> if not provided</param>
    public static WireMockServer MockEnergyResultsResponse(
        this WireMockServer server,
        Guid? calculationId = null)
    {
        // => Databricks SQL Statement API
        var chunkIndex = 0;
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        server
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockEnergySqlStatementsResultStream(path, calculationId ?? Guid.NewGuid());

        return server;
    }

    /// <summary>
    /// Setup databrick api response mocks to be able to respond with energy results for the calculation id
    /// provided by <paramref name="getCalculationIdCallback"/>
    /// </summary>
    public static WireMockServer MockEnergyResultsResponse(
        this WireMockServer server,
        Func<Guid?> getCalculationIdCallback)
    {
        // => Databricks SQL Statement API
        var chunkIndex = 0;
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        server
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockEnergySqlStatementsResultStream(path, getCalculationIdCallback);

        return server;
    }

    /// <summary>
    /// Setup databrick api response mocks to be able to respond with the job state provided by <paramref name="state"/>
    /// Supports PENDING, RUNNING and TERMINATED (which resolves to SUCCESS resultState)
    /// </summary>
    public static WireMockServer MockCalculationJobStatusResponse(
        this WireMockServer server,
        RunLifeCycleState state,
        int? runId = null)
    {
        // => Databricks Jobs API
        var jobId = Random.Shared.Next(1, 1000);
        runId ??= Random.Shared.Next(1000, 2000);

        var (jobRunState, resultState) = state switch
        {
            RunLifeCycleState.PENDING => ("PENDING", "EXCLUDED"),
            RunLifeCycleState.RUNNING => ("RUNNING", "EXCLUDED"),
            RunLifeCycleState.TERMINATED => ("TERMINATED", "SUCCESS"),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The given state is not implemented"),
        };

        // => Mock job run status to respond according to the jobRunState and resultState
        server
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId.Value)
            .MockJobsRunsGet(runId.Value, jobRunState, resultState);

        return server;
    }

    /// <summary>
    /// Setup databrick api response mocks to be able to respond with the job state provided by <paramref name="jobRunStateCallback"/>
    /// Supports PENDING, RUNNING and TERMINATED (which resolves to SUCCESS resultState)
    /// </summary>
    public static WireMockServer MockCalculationJobStatusResponse(
        this WireMockServer server,
        Func<RunLifeCycleState?> jobRunStateCallback,
        int? runId = null)
    {
        // => Databricks Jobs API
        var jobId = Random.Shared.Next(1, 1000);
        runId ??= Random.Shared.Next(1000, 2000);

        string? JobRunStateStringCallback()
        {
            var state = jobRunStateCallback();

            return state switch
            {
                null => null,
                RunLifeCycleState.PENDING => "PENDING",
                RunLifeCycleState.RUNNING => "RUNNING",
                RunLifeCycleState.TERMINATED => "TERMINATED",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The given state is not implemented"),
            };
        }

        // => Mock job run status to respond according to the JobRunStateStringCallback
        server
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId.Value)
            .MockJobsRunsGet(runId.Value, JobRunStateStringCallback);

        return server;
    }

    /// <summary>
    /// Setup databrick api response mocks to be able to respond with job run statuses that
    /// goes from "Pending" -> "Running" -> "Completed".
    /// </summary>
    public static WireMockServer MockCalculationJobStatusResponsesWithLifecycle(this WireMockServer server, int? runId = null)
    {
        var jobId = Random.Shared.Next(1, 1000);
        runId ??= Random.Shared.Next(1000, 2000);

        server
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId.Value)
            .MockJobsRunsGetLifeCycleScenario(runId.Value);

        return server;
    }
}
