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

using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Calculations;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class StartCalculationActivity
{
    private readonly IUnitOfWork _calculationUnitOfWork;
    private readonly ICalculationRepository _calculationRepository;
    private readonly ICalculationEngineClient _calculationEngineClient;

    public StartCalculationActivity(
        IUnitOfWork calculationUnitOfWork,
        ICalculationRepository calculationRepository,
        ICalculationEngineClient calculationEngineClient)
    {
        _calculationUnitOfWork = calculationUnitOfWork;
        _calculationRepository = calculationRepository;
        _calculationEngineClient = calculationEngineClient;
    }

    /// <summary>
    /// Start calculation in Databricks.
    /// </summary>
    [Function(nameof(StartCalculationActivity))]
    public async Task<CalculationJobId> Run(
        [ActivityTrigger] Guid calculationdId)
    {
        var calculation = await _calculationRepository.GetAsync(calculationdId);
        var jobId = await _calculationEngineClient.StartAsync(calculation);
        calculation.MarkAsSubmitted(jobId);
        await _calculationUnitOfWork.CommitAsync();

        return jobId;
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
