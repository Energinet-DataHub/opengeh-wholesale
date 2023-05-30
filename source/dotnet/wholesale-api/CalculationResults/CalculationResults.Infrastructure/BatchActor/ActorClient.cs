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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.JsonNewlineSerializer;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.Actors;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.Actors.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.BatchActor;

public class ActorClient : IActorClient
{
    private readonly IDataLakeClient _dataLakeClient;
    private readonly IJsonNewlineSerializer _jsonNewlineSerializer;

    public ActorClient(
        IDataLakeClient dataLakeClient,
        IJsonNewlineSerializer jsonNewlineSerializer)
    {
        _dataLakeClient = dataLakeClient;
        _jsonNewlineSerializer = jsonNewlineSerializer;
    }

    public async Task<Actor[]> GetEnergySuppliersAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType)
    {
        var actorRelations = await GetActorRelationsAsync(batchId, gridAreaCode, timeSeriesType).ConfigureAwait(false);
        return actorRelations.Select(relation => new Actor(relation.energy_supplier_gln)).Distinct().ToArray();
    }

    public async Task<Actor[]> GetBalanceResponsiblePartiesAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType)
    {
        var actorRelations = await GetActorRelationsAsync(batchId, gridAreaCode, timeSeriesType).ConfigureAwait(false);
        return actorRelations.Select(relation => new Actor(relation.balance_responsible_party_gln)).Distinct()
            .ToArray();
    }

    public async Task<Actor[]> GetEnergySuppliersByBalanceResponsiblePartyAsync(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsibleGln)
    {
        var actorRelations = await GetActorRelationsAsync(batchId, gridAreaCode, timeSeriesType).ConfigureAwait(false);
        return actorRelations.Where(relation => relation.balance_responsible_party_gln == balanceResponsibleGln).Select(relation => new Actor(relation.energy_supplier_gln)).Distinct()
            .ToArray();
    }

    public static (string Directory, string Extension) GetActorListFileSpecification(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType)
    {
        return (
            $"calculation-output/batch_id={batchId}/actors/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode}/",
            ".json");
    }

    private async Task<List<ActorRelation>> GetActorRelationsAsync(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType)
    {
        var (directory, extension) = GetActorListFileSpecification(batchId, gridAreaCode, timeSeriesType);

        var filepath =
            await _dataLakeClient.FindFileAsync(directory, extension).ConfigureAwait(false);

        var resultStream =
            await _dataLakeClient.GetReadableFileStreamAsync(filepath).ConfigureAwait(false);

        var actorRelations = await _jsonNewlineSerializer.DeserializeAsync<ActorRelation>(resultStream).ConfigureAwait(false);

        // We have a json newline without data in it in the actor lists, this is a temporary workaround.
        return actorRelations.Where(x => x.balance_responsible_party_gln != null && x.energy_supplier_gln != null).ToList();
    }
}
