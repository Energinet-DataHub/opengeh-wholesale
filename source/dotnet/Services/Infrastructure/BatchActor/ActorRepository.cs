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

using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;

namespace Energinet.DataHub.Wholesale.Infrastructure.BatchActor;

public class ActorRepository : IActorRepository
{
    private readonly IDataLakeClient _dataLakeClient;
    private readonly IJsonNewlineSerializer _jsonNewlineSerializer;

    public ActorRepository(
        IDataLakeClient dataLakeClient,
        IJsonNewlineSerializer jsonNewlineSerializer)
    {
        _dataLakeClient = dataLakeClient;
        _jsonNewlineSerializer = jsonNewlineSerializer;
    }

    public async Task<Actor[]> GetEnergySuppliersAsync(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType)
    {
        var actorRelations = await GetActorRelationsAsync(batchId, gridAreaCode, timeSeriesType).ConfigureAwait(false);
        return actorRelations.Select(relation => new Actor(relation.energy_supplier_gln)).Distinct().ToArray();
    }

    public async Task<Actor[]> GetBalanceResponsiblePartiesAsync(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType)
    {
        var actorRelations = await GetActorRelationsAsync(batchId, gridAreaCode, timeSeriesType).ConfigureAwait(false);
        return actorRelations.Select(relation => new Actor(relation.balance_responsible_party_gln)).Distinct()
            .ToArray();
    }

    public static (string Directory, string Extension) GetActorListFileSpecification(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType)
    {
        return (
            $"calculation-output/batch_id={batchId}/actors/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode.Code}/",
            ".json");
    }

    private async Task<List<ActorRelation>> GetActorRelationsAsync(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType)
    {
        var (directory, extension) = GetActorListFileSpecification(batchId, gridAreaCode, timeSeriesType);
        var dataLakeFileClient =
            await _dataLakeClient.GetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);

        var resultStream = await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
        return await _jsonNewlineSerializer.DeserializeAsync<ActorRelation>(resultStream).ConfigureAwait(false);
    }
}
