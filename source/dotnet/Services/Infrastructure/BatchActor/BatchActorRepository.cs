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

using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Wholesale.Domain.BatchActor;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;

namespace Energinet.DataHub.Wholesale.Infrastructure.BatchActor;

public class BatchActorRepository : DataLakeRepositoryBase, IBatchActorRepository
{
    private readonly IBatchActorFactory _batchActorFactory;

    public BatchActorRepository(
        DataLakeFileSystemClient dataLakeFileSystemClient,
        IBatchActorFactory batchActorFactory)
        : base(dataLakeFileSystemClient)
    {
        _batchActorFactory = batchActorFactory;
    }

    public async Task<Domain.BatchActor.BatchActor[]> GetAsync(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType,
        MarketRoleType marketRoleType)
    {
        var (directory, extension) = GetActorListFileSpecification(batchId, gridAreaCode, timeSeriesType, marketRoleType);
        var dataLakeFileClient = await GetDataLakeFileClientAsync(directory, extension).ConfigureAwait(false);

        var resultStream = await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
        var actors = await _batchActorFactory.GetBatchActorFromJsonStreamAsync(resultStream).ConfigureAwait(false);

        return MapToBatchActor(actors);
    }

    public static (string Directory, string Extension) GetActorListFileSpecification(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType,
        MarketRoleType marketRoleType)
    {
        return ($"calculation-output/batch_id={batchId}/actors/grid_area={gridAreaCode.Code}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/market_role={MarketRoleTypeMapper.Map(marketRoleType)}/", ".json");
    }

    private static Domain.BatchActor.BatchActor[] MapToBatchActor(IEnumerable<BatchActor> actors)
    {
        return actors.Select(actor => new Domain.BatchActor.BatchActor(actor.gln)).ToArray();
    }
}
