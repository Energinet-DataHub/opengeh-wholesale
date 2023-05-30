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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepEnergySupplier;

/// <summary>
/// Energy suppliers for which batch results have been calculated.
/// </summary>
[Route("/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{timeSeriesType}/energy-suppliers")]
public class ProcessStepEnergySupplierController : V3ControllerBase
{
    private readonly IActorRepository _actorRepository;

    public ProcessStepEnergySupplierController(IActorRepository actorRepository)
    {
        _actorRepository = actorRepository;
    }

    /// <summary>
    /// Returns a list of Energy suppliers. If balance responsible party is specified by the <paramref name="balanceResponsibleParty"/> parameter only the energy suppliers associated with that balance responsible party is returned
    /// </summary>
    [HttpGet(Name = "GetListOfEnergySuppliers")]
    [Produces("application/json", Type = typeof(List<ActorDto>))]
    public async Task<List<ActorDto>> GetAsync([FromRoute] Guid batchId, [FromRoute] string gridAreaCode, [FromRoute] TimeSeriesType timeSeriesType, [FromQuery] string? balanceResponsibleParty)
    {
        if (balanceResponsibleParty != null)
            return await GetByBalanceResponsiblePartyAsync(batchId, gridAreaCode, timeSeriesType, balanceResponsibleParty).ConfigureAwait(false);

        return await GetAllAsync(batchId, gridAreaCode, timeSeriesType).ConfigureAwait(false);
    }

    /// <summary>
    /// All energy suppliers.
    /// </summary>
    private async Task<List<ActorDto>> GetAllAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType)
    {
        var actors = await _actorRepository.GetEnergySuppliersAsync(batchId, gridAreaCode, timeSeriesType).ConfigureAwait(false);

        return actors
            .Select(a => new ActorDto(a.Gln))
            .ToList();
    }

    /// <summary>
    /// Energy suppliers associated with the balance responsible party specified by the <paramref name="balanceResponsibleParty"/>.
    /// </summary>
    private async Task<List<ActorDto>> GetByBalanceResponsiblePartyAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsibleParty)
    {
        var actors = await _actorRepository.GetEnergySuppliersByBalanceResponsiblePartyAsync(batchId, gridAreaCode, timeSeriesType, balanceResponsibleParty).ConfigureAwait(false);

        return actors
            .Select(a => new ActorDto(a.Gln))
            .ToList();
    }
}
