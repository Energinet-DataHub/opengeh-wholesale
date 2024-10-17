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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;

public sealed class CalculationEngineClient : ICalculationEngineClient
{
    private readonly IDatabricksCalculatorJobSelector _databricksCalculatorJobSelector;
    private readonly IJobsApiClient _client;
    private readonly ICalculationParametersFactory _calculationParametersFactory;

    public CalculationEngineClient(
        IDatabricksCalculatorJobSelector databricksCalculatorJobSelector,
        IJobsApiClient client,
        ICalculationParametersFactory calculationParametersFactory)
    {
        _databricksCalculatorJobSelector = databricksCalculatorJobSelector;
        _client = client;
        _calculationParametersFactory = calculationParametersFactory;
    }

    public async Task<CalculationJobId> StartAsync(Calculation calculation)
    {
        var runParameters = _calculationParametersFactory.CreateParameters(calculation);

        var calculatorJob = await _databricksCalculatorJobSelector
            .GetAsync()
            .ConfigureAwait(false);

        var runId = await _client
            .Jobs
            .RunNow(calculatorJob.JobId, runParameters)
            .ConfigureAwait(false);

        return new CalculationJobId(runId);
    }

    public async Task<CalculationState> GetStatusAsync(CalculationJobId calculationJobId)
    {
        var runState = await _client
            .Jobs
            .RunsGet(calculationJobId.Id)
            .ConfigureAwait(false);

        return runState.Item1.State.LifeCycleState switch
        {
            RunLifeCycleState.PENDING => CalculationState.Pending,
            RunLifeCycleState.RUNNING => CalculationState.Running,
            RunLifeCycleState.TERMINATING => CalculationState.Running,
            RunLifeCycleState.SKIPPED => CalculationState.Canceled,
            RunLifeCycleState.INTERNAL_ERROR => CalculationState.Failed,
            RunLifeCycleState.TERMINATED => runState.Item1.State.ResultState switch
            {
                RunResultState.SUCCESS => CalculationState.Completed,
                RunResultState.FAILED => CalculationState.Failed,
                RunResultState.CANCELED => CalculationState.Canceled,
                RunResultState.TIMEDOUT => CalculationState.Failed,
                RunResultState.MAXIMUM_CONCURRENT_RUNS_REACHED => CalculationState.Failed,
                RunResultState.EXCLUDED => CalculationState.Failed,
                RunResultState.SUCCESS_WITH_FAILURES => CalculationState.Failed,
                RunResultState.UPSTREAM_FAILED => CalculationState.Failed,
                RunResultState.UPSTREAM_CANCELED => CalculationState.Canceled,
                null => CalculationState.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(runState.Item1.State)),
            },
            RunLifeCycleState.BLOCKED => CalculationState.Failed,
            RunLifeCycleState.WAITING_FOR_RETRY => CalculationState.Pending,
            RunLifeCycleState.QUEUED => CalculationState.Pending,
            _ => throw new ArgumentOutOfRangeException(nameof(runState.Item1.State)),
        };
    }
}
