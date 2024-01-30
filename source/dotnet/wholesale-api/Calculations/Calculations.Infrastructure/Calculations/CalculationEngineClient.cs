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

    public async Task<CalculationId> StartAsync(Calculation calculation)
    {
        var runParameters = _calculationParametersFactory.CreateParameters(calculation);

        var calculatorJob = await _databricksCalculatorJobSelector
            .GetAsync()
            .ConfigureAwait(false);

        var runId = await _client
            .Jobs
            .RunNow(calculatorJob.JobId, runParameters)
            .ConfigureAwait(false);

        return new CalculationId(runId);
    }

    public async Task<Application.Model.CalculationState> GetStatusAsync(CalculationId calculationId)
    {
        var runState = await _client
            .Jobs
            .RunsGet(calculationId.Id)
            .ConfigureAwait(false);

        return runState.Item1.State.LifeCycleState switch
        {
            RunLifeCycleState.PENDING => Application.Model.CalculationState.Pending,
            RunLifeCycleState.RUNNING => Application.Model.CalculationState.Running,
            RunLifeCycleState.TERMINATING => Application.Model.CalculationState.Running,
            RunLifeCycleState.SKIPPED => Application.Model.CalculationState.Canceled,
            RunLifeCycleState.INTERNAL_ERROR => Application.Model.CalculationState.Failed,
            RunLifeCycleState.TERMINATED => runState.Item1.State.ResultState switch
            {
                RunResultState.SUCCESS => Application.Model.CalculationState.Completed,
                RunResultState.FAILED => Application.Model.CalculationState.Failed,
                RunResultState.CANCELED => Application.Model.CalculationState.Canceled,
                RunResultState.TIMEDOUT => Application.Model.CalculationState.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(runState.Item1.State)),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(runState.Item1.State)),
        };
    }
}
