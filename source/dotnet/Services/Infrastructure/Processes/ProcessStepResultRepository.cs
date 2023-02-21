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

using System.Globalization;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

public class ProcessStepResultRepository : IProcessStepResultRepository
{
    private const string EnergySupplierGridArea = "es_ga";
    private const string BalanceResponsiblePartyGridArea = "brp_ga";
    private const string TotalGridArea = "total_ga";
    private readonly IDataLakeClient _dataLakeClient;
    private readonly IJsonNewlineSerializer _jsonNewlineSerializer;

    public ProcessStepResultRepository(
        IDataLakeClient dataLakeClient,
        IJsonNewlineSerializer jsonNewlineSerializer)
    {
        _dataLakeClient = dataLakeClient;
        _jsonNewlineSerializer = jsonNewlineSerializer;
    }

    public async Task<ProcessStepResult> GetAsync(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierGln,
        string? balanceResponsiblePartyGln)
    {
        if (balanceResponsiblePartyGln != null && energySupplierGln == null)
            return await GetResultAsync(batchId, GetDirectoryForBrpGridArea(batchId, gridAreaCode, timeSeriesType, balanceResponsiblePartyGln)).ConfigureAwait(false);

        if (energySupplierGln != null && balanceResponsiblePartyGln == null)
            return await GetResultAsync(batchId, GetDirectoryForEsGridArea(batchId, gridAreaCode, timeSeriesType, energySupplierGln)).ConfigureAwait(false);

        return await GetResultAsync(batchId, GetDirectoryForTotalGridArea(batchId, gridAreaCode, timeSeriesType)).ConfigureAwait(false);
    }

    public static string GetDirectoryForBrpGridArea(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsiblePartyGln)
        => $"calculation-output/batch_id={batchId}/result/grouping={BalanceResponsiblePartyGridArea}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode.Code}/gln={balanceResponsiblePartyGln}/";

    public static string GetDirectoryForEsGridArea(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType, string energySupplierGln)
        => $"calculation-output/batch_id={batchId}/result/grouping={EnergySupplierGridArea}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode.Code}/gln={energySupplierGln}/";

    public static string GetDirectoryForTotalGridArea(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType)
        => $"calculation-output/batch_id={batchId}/result/grouping={TotalGridArea}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode.Code}/";

    private async Task<ProcessStepResult> GetResultAsync(Guid batchId, string directory)
    {
        var dataLakeFileClient =
            await _dataLakeClient.GetDataLakeFileClientAsync(directory, ".json").ConfigureAwait(false);
        if (dataLakeFileClient == null)
        {
            throw new InvalidOperationException($"Blob for batch with id={batchId} was not found.");
        }

        var resultStream = await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
        var points = await _jsonNewlineSerializer.DeserializeAsync<ProcessResultPoint>(resultStream).ConfigureAwait(false);

        return MapToProcessStepResultDto(points);
    }

    private static ProcessStepResult MapToProcessStepResultDto(IEnumerable<ProcessResultPoint> points)
    {
        var pointsDto = points.Select(
                point => new TimeSeriesPoint(
                    DateTimeOffset.Parse(point.quarter_time),
                    decimal.Parse(point.quantity, CultureInfo.InvariantCulture),
                    point.quality))
            .ToList();

        return new ProcessStepResult(pointsDto.ToArray());
    }
}
