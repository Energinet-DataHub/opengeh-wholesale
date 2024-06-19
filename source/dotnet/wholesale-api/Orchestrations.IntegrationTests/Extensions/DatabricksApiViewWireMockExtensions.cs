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

using System.Net;
using System.Text;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Microsoft.Net.Http.Headers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

/// <summary>
/// A collection of WireMock extensions for easy mock configuration of
/// Databricks REST API endpoints.
///
/// IMPORTANT developer tips:
///  - It's possible to start the WireMock server in Proxy mode, this means
///    that all requests are proxied to the real URL. And the mappings can be recorded and saved.
///    See https://github.com/WireMock-Net/WireMock.Net/wiki/Proxying
///  - WireMockInspector: https://github.com/WireMock-Net/WireMockInspector/blob/main/README.md
///  - WireMock.Net examples: https://github.com/WireMock-Net/WireMock.Net-examples
/// </summary>
public static class DatabricksApiViewWireMockExtensions
{
    public static WireMockServer MockEnergySqlStatementsView(this WireMockServer server, string statementId, int chunkIndex)
    {
        var request = Request
            .Create()
            .WithPath("/api/2.0/sql/statements")
            .UsingPost();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(DatabricksEnergyStatementViewResponseMock(statementId, chunkIndex));

        server
            .Given(request)
            .RespondWith(response);

        return server;
    }

    public static WireMockServer MockEnergySqlStatementsResultViewStream(this WireMockServer server, string path, Guid calculationId)
    {
        var request = Request
            .Create()
            .WithPath($"/{path}")
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithBody(Encoding.UTF8.GetBytes(DatabricksEnergyStatementRowMock(calculationId)));

        server
            .Given(request)
            .RespondWith(response);
        return server;
    }

    private static string DatabricksEnergyStatementViewResponseMock(string statementId, int chunkIndex)
    {
        var json = """
               {
                 "statement_id": "{statementId}",
                 "status": {
                   "state": "SUCCEEDED"
                 },
                 "manifest": {
                   "format": "CSV",
                   "schema": {
                     "column_count": 1,
                     "columns": [
                       {columnArray}
                     ]
                   },
                   "total_chunk_count": 1,
                   "chunks": [
                     {
                       "chunk_index": {chunkIndex},
                       "row_offset": 0,
                       "row_count": 1
                     }
                   ],
                   "total_row_count": 1,
                   "total_byte_count": 293
                 },
                 "result": {
                   "external_links": [
                     {
                       "chunk_index": {chunkIndex},
                       "row_offset": 0,
                       "row_count": 100,
                       "byte_count": 293,
                       "external_link": "https://someplace.cloud-provider.com/very/long/path/...",
                       "expiration": "2023-01-30T22:23:23.140Z"
                     }
                   ]
                 }
               }
               """;

        // Make sure that the order of the names matches the order of the data defined in 'DatabricksEnergyStatementRowMock'
        var columns = string.Join(
            ",",
            SettlementReportEnergyResultViewColumns
                .AllNames
                .Concat(SettlementReportEnergyResultCountColumns.AllNames)
                .Select(name => $" {{\"name\": \"{name}\" }}"));

        return json.Replace("{statementId}", statementId)
            .Replace("{chunkIndex}", chunkIndex.ToString())
            .Replace(
                "{columnArray}",
                columns);
    }

    private static string DatabricksEnergyStatementRowMock(Guid calculationId)
    {
        // Make sure that the order of the data matches the order of the columns defined in 'DatabricksEnergyStatementResponseMock'
        var data = SettlementReportEnergyResultViewColumns.AllNames.Concat(SettlementReportEnergyResultCountColumns.AllNames).Select(columnName => columnName switch
        {
            SettlementReportEnergyResultViewColumns.CalculationId => $"\"{calculationId}\"",
            SettlementReportEnergyResultViewColumns.CalculationType => $"\"{DeltaTableCalculationType.BalanceFixing}\"",
            SettlementReportEnergyResultViewColumns.CalculationVersion => "\"1\"",
            SettlementReportEnergyResultViewColumns.ResultId => "\"aaaaaaaa-1111-1111-1c1c-08d3b12d4511\"",
            SettlementReportEnergyResultViewColumns.GridArea => "\"805\"",
            SettlementReportEnergyResultViewColumns.MeteringPointType => "\"Consumption\"",
            SettlementReportEnergyResultViewColumns.SettlementMethod => "\"non_profiled\"",
            SettlementReportEnergyResultViewColumns.Resolution => "\"PT15M\"",
            SettlementReportEnergyResultViewColumns.Time => "\"2022-05-16T03:00:00.000Z\"",
            SettlementReportEnergyResultViewColumns.Quantity => "\"1.123\"",
            SettlementReportEnergyResultCountColumns.Count => "\"1\"",
            _ => throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null),
        }).ToArray();
        var temp = $"""[[{string.Join(",", data)}]]""";
        return temp;
    }
}
