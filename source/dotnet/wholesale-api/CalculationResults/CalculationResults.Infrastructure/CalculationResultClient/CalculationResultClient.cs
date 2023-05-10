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

    public CalculationResultClient(HttpClient httpClient, IOptions<DatabricksOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
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

        var databricksSqlResponse = await SendSqlStatementAsync(sql).ConfigureAwait(false);

        return CreateProcessStepResult(timeSeriesType, databricksSqlResponse);
    }

    private async Task<DatabricksSqlResponse> SendSqlStatementAsync(string sqlStatement)
    {
        const int timeOutPerAttemptSeconds = 30;
        const int maxAttempts = 16; // 8 minutes in total

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"{timeOutPerAttemptSeconds}s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = _options.Value.DATABRICKS_WAREHOUSE_ID,
        };

        var databricksSqlResponse = new DatabricksSqlResponse();

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Unable to get calculation result from Databricks. Status code: {response.StatusCode}");

            var jsonResponse = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            databricksSqlResponse.DeserializeFromJson(jsonResponse);

            var statementState = databricksSqlResponse.GetState();

            if (statementState == "SUCCEEDED")
                return databricksSqlResponse;

            if (statementState != "PENDING")
                throw new Exception($"Unable to get calculation result from Databricks. State: {statementState}");
        }

        throw new Exception($"Unable to get calculation result from Databricks.");
    }

    private static void ConfigureHttpClient(HttpClient httpClient, IOptions<DatabricksOptions> options)
    {
        httpClient.BaseAddress = new Uri(options.Value.DATABRICKS_WORKSPACE_URL);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Value.DATABRICKS_WORKSPACE_TOKEN);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        httpClient.BaseAddress = new Uri(options.Value.DATABRICKS_WORKSPACE_URL);
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
            case TimeSeriesType.Production:
            case TimeSeriesType.FlexConsumption:
                if (energySupplierGln != null && balanceResponsiblePartyGln != null)
                    return "es_brp_ga";
                if (energySupplierGln != null)
                    return "es_ga";
                if (balanceResponsiblePartyGln != null)
                    return "brp_ga";
                return "total_ga";
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

    private static ProcessStepResult CreateProcessStepResult(
        TimeSeriesType timeSeriesType,
        DatabricksSqlResponse databricksSqlResponse)
    {
        var pointsDto = databricksSqlResponse.GetDataArray().Select(
                res => new TimeSeriesPoint(
                    DateTimeOffset.Parse(res[0]),
                    decimal.Parse(res[1], CultureInfo.InvariantCulture),
                    QuantityQualityMapper.MapQuality(res[2])))
            .ToArray();

        return new ProcessStepResult(timeSeriesType, pointsDto);
    }
}
