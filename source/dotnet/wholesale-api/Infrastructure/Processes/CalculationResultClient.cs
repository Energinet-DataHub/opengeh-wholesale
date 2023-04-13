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
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

public class CalculationResultClient : ICalculationResultClient
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly IOptions<CalculationResultClientOptions> _options;
    private readonly IJsonSerializer _jsonSerializer;

    public CalculationResultClient(HttpClient httpClient, IOptions<CalculationResultClientOptions> options, IJsonSerializer jsonSerializer)
    {
        _httpClient = httpClient;
        _options = options;
        _jsonSerializer = jsonSerializer;
        ConfigureHttpClient(httpClient, options);
    }

    public async Task<ProcessStepResult> GetAsync(
        Guid batchId,
        GridAreaCode gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierGln,
        string? balanceResponsiblePartyGln)
    {
        var sql = CreateSqlStatement(batchId, gridAreaCode, timeSeriesType, energySupplierGln, balanceResponsiblePartyGln);

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = "30s", // Make the operation synchronous
            statement = sql,
            warehouse_id = _options.Value.DataBricksSqlWarehouseId,
        };
        var requestString = _jsonSerializer.Serialize(requestObject);

        // TODO: Use databricks workspace url from config
        // TODO: Enable SQL statement execution API in terraform: https://registry.terraform.io/providers/databricks/databricks/latest/docs/data-sources/sql_warehouse#attribute-reference
        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, new StringContent(requestString)).ConfigureAwait(false);
        // TODO: Unit test
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Unable to get calculation result from Databricks. Status code: {response.StatusCode}");

        return await GetResultAsync(response.Content).ConfigureAwait(false);
    }

    private static void ConfigureHttpClient(HttpClient httpClient, IOptions<CalculationResultClientOptions> options)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Value.DatabricksAccessToken);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
    }

    private async Task<ProcessStepResult> GetResultAsync(HttpContent content)
    {
        var body = await content.ReadAsStringAsync().ConfigureAwait(false);
        var result = _jsonSerializer.Deserialize<IEnumerable<CalculationResult>>(body);
        throw new NotImplementedException();
    }

    // TODO: Unit test the SQL (ensure it works as expected)
    private string CreateSqlStatement(Guid batchId, GridAreaCode gridAreaCode, TimeSeriesType timeSeriesType, string? energySupplierGln, string? balanceResponsiblePartyGln)
    {
        return $@"select time, quantity, quantity_quality
from wholesale_output.result
where batch_id = '{batchId}'
  and grid_area = '{gridAreaCode.Code}'
  and time_series_type = '{ToDeltaValue(timeSeriesType)}'
  and aggregation_level = '{GetAggregationLevelDeltaValue(timeSeriesType, energySupplierGln, balanceResponsiblePartyGln)}'
order by time
";
    }

    // TODO: Unit test and move to mapper
    private string GetAggregationLevelDeltaValue(TimeSeriesType timeSeriesType, string? energySupplierGln, string? balanceResponsiblePartyGln)
    {
        switch (timeSeriesType)
        {
            case TimeSeriesType.NonProfiledConsumption:
                if (energySupplierGln != null && balanceResponsiblePartyGln != null)
                    return "energy_supplier_and_balance_responsible_party";
                if (energySupplierGln != null)
                    return "energy_supplier";
                if (balanceResponsiblePartyGln != null)
                    return "balance_responsible_party";
                return "grid_area";
            default:
                throw new NotImplementedException($"Mapping of '{timeSeriesType}' not implemented.");
        }
    }

    // TODO: Unit test
    private string ToDeltaValue(TimeSeriesType timeSeriesType)
    {
        switch (timeSeriesType)
        {
            case TimeSeriesType.NonProfiledConsumption:
                return "non_profiled_consumption";
            default:
                throw new NotImplementedException($"Mapping of '{timeSeriesType}' not implemented.");
        }
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
