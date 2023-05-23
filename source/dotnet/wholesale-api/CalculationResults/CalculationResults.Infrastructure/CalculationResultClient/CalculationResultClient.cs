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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
// TODO BJM: Should we avoid referencing the DatabricksClient project "just" to get access to the DatabricksOptions?
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Microsoft.Extensions.Options;
using NodaTime;
using ProcessType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient.ProcessType;

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
        var sql = SqlStatementFactory.CreateForSettlementReport(gridAreaCodes, processType, periodStart, periodEnd, energySupplier);

        var databricksSqlResponse = await SendSqlStatementAsync(sql).ConfigureAwait(false);

        return SettlementReportDataFactory.Create(databricksSqlResponse.Table!);
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
                throw new DatabricksSqlException($"Unable to get calculation result from Databricks. Status code: {response.StatusCode}");

            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var databricksSqlResponse = _databricksSqlResponseParser.Parse(jsonResponse);

            if (databricksSqlResponse.State == "SUCCEEDED")
                return databricksSqlResponse;

            if (databricksSqlResponse.State != "PENDING")
                throw new DatabricksSqlException($"Unable to get calculation result from Databricks. State: {databricksSqlResponse.State}");
        }

        throw new DatabricksSqlException($"Unable to get calculation result from Databricks. Max attempts reached ({maxAttempts}) and the state is still not SUCCEEDED.");
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

    private static ProcessStepResult CreateProcessStepResult(
        TimeSeriesType timeSeriesType,
        Table resultTable)
    {
        var pointsDto = Enumerable.Range(0, resultTable.RowCount)
            .Select(row => new TimeSeriesPoint(
                DateTimeOffset.Parse(resultTable[row, ResultColumnNames.Time]),
                decimal.Parse(resultTable[row, ResultColumnNames.Quantity], CultureInfo.InvariantCulture),
                QuantityQualityMapper.FromDeltaTableValue(resultTable[row, ResultColumnNames.QuantityQuality])))
            .ToList();

        return new ProcessStepResult(timeSeriesType, pointsDto.ToArray());
    }
}
