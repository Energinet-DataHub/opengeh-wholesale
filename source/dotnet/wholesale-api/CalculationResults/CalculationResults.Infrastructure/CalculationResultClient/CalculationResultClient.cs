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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReport;
// TODO: Should we avoid referencing the DatabricksClient project "just" to get access to the DatabricksOptions?
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0
// https://learn.microsoft.com/en-gb/azure/databricks/sql/api/sql-execution-tutorial
public class CalculationResultClient : ICalculationResultClient
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly IOptions<DatabricksOptions> _options;
    private readonly IDatabricksSqlResponseParser _databricksSqlResponseParser;

    public CalculationResultClient(HttpClient httpClient, IOptions<DatabricksOptions> options, IDatabricksSqlResponseParser databricksSqlResponseParser)
    {
        _httpClient = httpClient;
        _options = options;
        _databricksSqlResponseParser = databricksSqlResponseParser;
        ConfigureHttpClient(_httpClient, _options);
    }

    public async Task<IEnumerable<SettlementReportResultRow>> GetSettlementReportResultAsync(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        var sql = SqlForSettlementReport.CreateSqlStatement(gridAreaCodes, processType, periodStart, periodEnd, energySupplier);

        var databricksSqlResponse = await SendSqlStatementAsync(sql).ConfigureAwait(false);

        return SqlForSettlementReport.CreateSettlementReportData(databricksSqlResponse);
    }

    public async Task<ProcessStepResult> GetAsync(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierGln,
        string? balanceResponsiblePartyGln)
    {
        await Task.Delay(1000).ConfigureAwait(false);

        throw new NotImplementedException("GetAsync is not implemented yet");
    }

    private async Task<DatabricksSqlResponse> SendSqlStatementAsync(string sqlStatement)
    {
        const int timeOutPerAttemptSeconds = 30;
        const int maxAttempts = 16; // 8 minutes in total (16 * 30 seconds). The warehouse takes around 5 minutes to start if it has been stopped.

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"{timeOutPerAttemptSeconds}s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = _options.Value.DATABRICKS_WAREHOUSE_ID,
        };
        // TODO (JMG): Should we use Polly for retrying?
        // TODO (JMG): Unit test this method
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Unable to get calculation result from Databricks. Status code: {response.StatusCode}");

            var jsonResponse = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            var databricksSqlResponse = _databricksSqlResponseParser.Parse(jsonResponse);

            if (databricksSqlResponse.State == "SUCCEEDED")
                return databricksSqlResponse;

            if (databricksSqlResponse.State != "PENDING")
                throw new Exception($"Unable to get calculation result from Databricks. State: {databricksSqlResponse.State}");
        }

        throw new Exception($"Unable to get calculation result from Databricks. Max attempts reached ({maxAttempts}) and the state is still not SUCCEEDED.");
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
        var pointsDto = databricksSqlResponse.Rows.Select(
                res => new TimeSeriesPoint(
                    DateTimeOffset.Parse(res[0]),
                    decimal.Parse(res[1], CultureInfo.InvariantCulture),
                    QuantityQualityMapper.MapQuality(res[2])))
            .ToArray();

        return new ProcessStepResult(timeSeriesType, pointsDto);
    }
}
