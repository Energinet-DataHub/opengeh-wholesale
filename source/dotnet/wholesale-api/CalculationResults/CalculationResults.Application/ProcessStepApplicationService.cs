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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep.Model;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep.Model.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application;

/// <summary>
/// This class provides the ability to retrieve a calculated result for a given step for a batch.
/// </summary>
public class ProcessStepApplicationService : IProcessStepApplicationService
{
    private readonly IProcessStepResultRepository _processStepResultRepository;
    private readonly IProcessStepResultMapper _processStepResultMapper;
    private readonly IActorRepository _actorRepository;

    public ProcessStepApplicationService(
        IProcessStepResultRepository processStepResultRepository,
        IProcessStepResultMapper processStepResultMapper,
        IActorRepository actorRepository)
    {
        _processStepResultRepository = processStepResultRepository;
        _processStepResultMapper = processStepResultMapper;
        _actorRepository = actorRepository;
    }

    public async Task<WholesaleActorDto[]> GetBalanceResponsiblePartiesAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType)
    {
        var balanceResponsibleParties = await _actorRepository.GetBalanceResponsiblePartiesAsync(batchId, gridAreaCode, TimeSeriesTypeMapper.Map(timeSeriesType)).ConfigureAwait(false);
        return Map(balanceResponsibleParties);
    }

    public async Task<WholesaleActorDto[]> GetEnergySuppliersAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType)
    {
        var energySuppliers = await _actorRepository.GetEnergySuppliersAsync(batchId, gridAreaCode, TimeSeriesTypeMapper.Map(timeSeriesType)).ConfigureAwait(false);
        return Map(energySuppliers);
    }

    public async Task<WholesaleActorDto[]> GetEnergySuppliersByBalanceResponsiblePartyAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsiblePartyGln)
    {
        var energySuppliers = await _actorRepository.GetEnergySuppliersByBalanceResponsiblePartyAsync(batchId, gridAreaCode, TimeSeriesTypeMapper.Map(timeSeriesType), balanceResponsiblePartyGln).ConfigureAwait(false);
        return Map(energySuppliers);
    }

    public async Task<ProcessStepResultDto> GetResultAsync(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierGln,
        string? balanceResponsibleParty)
    {
        var processStepResult = await _processStepResultRepository.GetAsync(
                batchId,
                gridAreaCode,
                TimeSeriesTypeMapper.Map(timeSeriesType),
                energySupplierGln,
                balanceResponsibleParty)
            .ConfigureAwait(false);

        return _processStepResultMapper.MapToDto(processStepResult);
    }

    private static WholesaleActorDto[] Map(Actor[] actors)
    {
        return actors.Select(batchActor => new WholesaleActorDto(batchActor.Gln)).ToArray();
    }
}
