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

    public async Task<Domain.ActorAggregate.Actor[]> GetAsync(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType,
        MarketRole marketRole)
    {
        var (directory, extension) = GetActorListFileSpecification(batchId, gridAreaCode, timeSeriesType, marketRole);
        var dataLakeFileClient = await _dataLakeClient.GetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);

        var resultStream = await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
        var actors = await _jsonNewlineSerializer.DeserializeAsync<Actor>(resultStream).ConfigureAwait(false);

        return MapToBatchActor(actors);
    }

    public static (string Directory, string Extension) GetActorListFileSpecification(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType,
        MarketRole marketRole)
    {
        return ($"calculation-output/batch_id={batchId}/actors/grid_area={gridAreaCode.Code}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/market_role={MarketRoleMapper.Map(marketRole)}/", ".json");
    }

    private static Domain.ActorAggregate.Actor[] MapToBatchActor(IEnumerable<Actor> actors)
    {
        return actors.Select(actor => new Domain.ActorAggregate.Actor(actor.gln)).ToArray();
    }
}
