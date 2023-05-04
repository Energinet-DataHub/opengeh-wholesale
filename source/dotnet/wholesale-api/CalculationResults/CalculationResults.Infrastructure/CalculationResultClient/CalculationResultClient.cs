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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
// TODO: Should we avoid referencing the DatabricksClient project "just" to get access to the DatabricksOptions?
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0
// https://learn.microsoft.com/en-gb/azure/databricks/sql/api/sql-execution-tutorial
public class CalculationResultClient : ICalculationResultClient
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly IOptions<DatabricksOptions> _options;
    private readonly IProcessResultPointFactory _processResultPointFactory;

    public CalculationResultClient(HttpClient httpClient, IOptions<DatabricksOptions> options, IProcessResultPointFactory processResultPointFactory)
    {
        _httpClient = httpClient;
        _options = options;
        _processResultPointFactory = processResultPointFactory;
    }

    public async Task<ProcessStepResult> GetAsync(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierGln,
        string? balanceResponsiblePartyGln)
    {
        ConfigureHttpClient(_httpClient, _options);

        var sql = CreateSqlStatement(batchId, gridAreaCode, timeSeriesType, energySupplierGln, balanceResponsiblePartyGln);

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = "30s", // Make the operation synchronous
            statement = sql,
            warehouse_id = _options.Value.DATABRICKS_WAREHOUSE_ID,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        // TODO: Unit test
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Unable to get calculation result from Databricks. Status code: {response.StatusCode}");

        var jsonResponse = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        var list = _processResultPointFactory.Create(jsonResponse);

        return MapToProcessStepResultDto(timeSeriesType, list);
    }

    private static void ConfigureHttpClient(HttpClient httpClient, IOptions<DatabricksOptions> options)
    {
        httpClient.BaseAddress = new Uri(options.Value.DATABRICKS_WORKSPACE_URL);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Value.DATABRICKS_WORKSPACE_TOKEN);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
    }

    // TODO: Unit test the SQL (ensure it works as expected)
    private string CreateSqlStatement(Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string? energySupplierGln, string? balanceResponsiblePartyGln)
    {
        return $@"select time, quantity, quantity_quality
from wholesale_output.result
where batch_id = '{batchId}'
  and grid_area = '{gridAreaCode}'
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
            case TimeSeriesType.Production:
                return "production";
            case TimeSeriesType.FlexConsumption:
                return "flex_consumption";
            default:
                throw new NotImplementedException($"Mapping of '{timeSeriesType}' not implemented.");
        }
    }

    // TODO: Why do we have both ProcessResultPoint and TimeSeriesPoint?
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
