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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.JsonNewlineSerializer;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class FileBasedCalculationResultClient : ICalculationResultClient
{
    private const string EnergySupplierBalanceResponsiblePartyGridArea = "es_brp_ga";
    private const string EnergySupplierGridArea = "es_ga";
    private const string BalanceResponsiblePartyGridArea = "brp_ga";
    private const string TotalGridArea = "total_ga";
    private readonly IDataLakeClient _dataLakeClient;
    private readonly IJsonNewlineSerializer _jsonNewlineSerializer;

    public FileBasedCalculationResultClient(
        IDataLakeClient dataLakeClient,
        IJsonNewlineSerializer jsonNewlineSerializer)
    {
        _dataLakeClient = dataLakeClient;
        _jsonNewlineSerializer = jsonNewlineSerializer;
    }

    public async Task<CalculationResult> GetAsync(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierId,
        string? balanceResponsiblePartyId)
    {
        if (balanceResponsiblePartyId != null && energySupplierId == null)
            return await GetResultAsync(batchId, gridAreaCode, energySupplierId, balanceResponsiblePartyId, GetDirectoryForBrpGridArea(batchId, gridAreaCode, timeSeriesType, balanceResponsiblePartyId), timeSeriesType).ConfigureAwait(false);

        if (energySupplierId != null && balanceResponsiblePartyId == null)
            return await GetResultAsync(batchId, gridAreaCode, energySupplierId, balanceResponsiblePartyId, GetDirectoryForEsGridArea(batchId, gridAreaCode, timeSeriesType, energySupplierId), timeSeriesType).ConfigureAwait(false);

        if (energySupplierId != null && balanceResponsiblePartyId != null)
            return await GetResultAsync(batchId, gridAreaCode, energySupplierId, balanceResponsiblePartyId, GetDirectoryForEsBrpGridArea(batchId, gridAreaCode, timeSeriesType, balanceResponsiblePartyId, energySupplierId), timeSeriesType).ConfigureAwait(false);

        return await GetResultAsync(batchId, gridAreaCode, energySupplierId, balanceResponsiblePartyId, GetDirectoryForTotalGridArea(batchId, gridAreaCode, timeSeriesType), timeSeriesType).ConfigureAwait(false);
    }

    public static string GetDirectoryForBrpGridArea(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsiblePartyGln)
        => $"calculation-output/batch_id={batchId}/result/grouping={BalanceResponsiblePartyGridArea}/time_series_type={TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)}/grid_area={gridAreaCode}/gln={balanceResponsiblePartyGln}/";

    public static string GetDirectoryForEsGridArea(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string energySupplierGln)
        => $"calculation-output/batch_id={batchId}/result/grouping={EnergySupplierGridArea}/time_series_type={TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)}/grid_area={gridAreaCode}/gln={energySupplierGln}/";

    public static string GetDirectoryForTotalGridArea(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType)
        => $"calculation-output/batch_id={batchId}/result/grouping={TotalGridArea}/time_series_type={TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)}/grid_area={gridAreaCode}/";

    public static string GetDirectoryForEsBrpGridArea(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsiblePartyGln, string energySupplierGln)
        => $"calculation-output/batch_id={batchId}/result/grouping={EnergySupplierBalanceResponsiblePartyGridArea}/time_series_type={TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)}/grid_area={gridAreaCode}/balance_responsible_party_gln={balanceResponsiblePartyGln}/energy_supplier_gln={energySupplierGln}/";

    private async Task<CalculationResult> GetResultAsync(Guid batchId, string gridArea, string? energySupplierId, string? balanceResponsibleId, string directory, TimeSeriesType timeSeriesType)
    {
        var filepath = await _dataLakeClient.FindFileAsync(directory, ".json").ConfigureAwait(false);
        var stream = await _dataLakeClient.GetReadableFileStreamAsync(filepath).ConfigureAwait(false);
        var points = await _jsonNewlineSerializer.DeserializeAsync<ProcessResultPoint>(stream).ConfigureAwait(false);
        return MapToCalculationResult(batchId, gridArea, energySupplierId, balanceResponsibleId, timeSeriesType, points);
    }

    private static CalculationResult MapToCalculationResult(
        Guid batchId,
        string gridArea,
        string? energySupplierId,
        string? balanceResponsibleId,
        TimeSeriesType timeSeriesType,
        IEnumerable<ProcessResultPoint> points)
    {
        var pointsDto = points.Select(
                point => new TimeSeriesPoint(
                    DateTimeOffset.Parse(point.quarter_time),
                    decimal.Parse(point.quantity, CultureInfo.InvariantCulture),
                    QuantityQualityMapper.FromDeltaTableValue(point.quality)))
            .ToList();

        return new CalculationResult(batchId, gridArea, timeSeriesType, energySupplierId, balanceResponsibleId, pointsDto.ToArray());
    }
}
