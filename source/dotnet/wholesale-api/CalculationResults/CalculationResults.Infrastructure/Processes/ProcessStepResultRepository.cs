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
using Energinet.DataHub.Wholesale.CalculationResults.Application;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.JsonNewlineSerializer;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Processes;

public class ProcessStepResultRepository : IProcessStepResultRepository
{
    private const string EnergySupplierBalanceResponsiblePartyGridArea = "es_brp_ga";
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
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierGln,
        string? balanceResponsiblePartyGln)
    {
        if (balanceResponsiblePartyGln != null && energySupplierGln == null)
            return await GetResultAsync(GetDirectoryForBrpGridArea(batchId, gridAreaCode, timeSeriesType, balanceResponsiblePartyGln), timeSeriesType).ConfigureAwait(false);

        if (energySupplierGln != null && balanceResponsiblePartyGln == null)
            return await GetResultAsync(GetDirectoryForEsGridArea(batchId, gridAreaCode, timeSeriesType, energySupplierGln), timeSeriesType).ConfigureAwait(false);

        if (energySupplierGln != null && balanceResponsiblePartyGln != null)
            return await GetResultAsync(GetDirectoryForEsBrpGridArea(batchId, gridAreaCode, timeSeriesType, balanceResponsiblePartyGln, energySupplierGln), timeSeriesType).ConfigureAwait(false);

        return await GetResultAsync(GetDirectoryForTotalGridArea(batchId, gridAreaCode, timeSeriesType), timeSeriesType).ConfigureAwait(false);
    }

    public static string GetDirectoryForBrpGridArea(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsiblePartyGln)
        => $"calculation-output/batch_id={batchId}/result/grouping={BalanceResponsiblePartyGridArea}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode}/gln={balanceResponsiblePartyGln}/";

    public static string GetDirectoryForEsGridArea(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string energySupplierGln)
        => $"calculation-output/batch_id={batchId}/result/grouping={EnergySupplierGridArea}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode}/gln={energySupplierGln}/";

    public static string GetDirectoryForTotalGridArea(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType)
        => $"calculation-output/batch_id={batchId}/result/grouping={TotalGridArea}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode}/";

    public static string GetDirectoryForEsBrpGridArea(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsiblePartyGln, string energySupplierGln)
        => $"calculation-output/batch_id={batchId}/result/grouping={EnergySupplierBalanceResponsiblePartyGridArea}/time_series_type={TimeSeriesTypeMapper.Map(timeSeriesType)}/grid_area={gridAreaCode}/balance_responsible_party_gln={balanceResponsiblePartyGln}/energy_supplier_gln={energySupplierGln}/";

    private async Task<ProcessStepResult> GetResultAsync(string directory, TimeSeriesType timeSeriesType)
    {
        var filepath = await _dataLakeClient.FindFileAsync(directory, ".json").ConfigureAwait(false);
        var stream = await _dataLakeClient.GetReadableFileStreamAsync(filepath).ConfigureAwait(false);
        var points = await _jsonNewlineSerializer.DeserializeAsync<ProcessResultPoint>(stream).ConfigureAwait(false);
        return MapToProcessStepResultDto(timeSeriesType, points);
    }

    private static ProcessStepResult MapToProcessStepResultDto(
        TimeSeriesType timeSeriesType,
        IEnumerable<ProcessResultPoint> points)
    {
        var pointsDto = points.Select(
                point => new TimeSeriesPoint(
                    DateTimeOffset.Parse(point.quarter_time),
                    decimal.Parse(point.quantity, CultureInfo.InvariantCulture),
                    QuantityQualityMapper.MapQuality(point.quality)))
            .ToList();

        return new ProcessStepResult(timeSeriesType, pointsDto.ToArray());
    }
}
