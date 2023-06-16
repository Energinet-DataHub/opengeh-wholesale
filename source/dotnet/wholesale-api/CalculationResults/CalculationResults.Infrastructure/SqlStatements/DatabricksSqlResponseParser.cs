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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

public class DatabricksSqlResponseParser : IDatabricksSqlResponseParser
{
    public DatabricksSqlResponse Parse(string jsonResponse)
    {
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();

        var statementId = GetStatementId(jsonObject);
        var state = GetState(jsonObject);
        switch (state)
        {
            case "PENDING":
                return DatabricksSqlResponse.CreateAsPending(statementId);
            case "CANCELED":
                return DatabricksSqlResponse.CreateAsCancelled(statementId);
            case "SUCCEEDED":
                var columnNames = GetColumnNames(jsonObject);
                var hasData = GetRowCount(jsonObject) > 0;
                var dataArray = hasData ? GetDataArray(jsonObject) : new List<string[]>();
                var tableChunk = new TableChunk(columnNames, dataArray);
                var nextChunkInternalLink = GetNextChunkInternalLink(jsonObject);
                return DatabricksSqlResponse.CreateAsSucceeded(statementId, tableChunk, nextChunkInternalLink);
            default:
                throw new DatabricksSqlException($@"Databricks SQL statement execution failed. State: {state}");
        }
    }

    private static Guid GetStatementId(JObject responseJsonObject)
    {
        return responseJsonObject["statement_id"]!.ToObject<Guid>();
    }

    private static string GetState(JObject responseJsonObject)
    {
        return responseJsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");
    }

    private static int GetRowCount(JObject responseJsonObject)
    {
        var rowCount = responseJsonObject["result"]?["row_count"]?.ToObject<int>() ?? throw new DatabricksSqlException("Unable to retrieve 'row_count' from the responseJsonObject.");
        return rowCount!;
    }

    private static string[] GetColumnNames(JObject responseJsonObject)
    {
        var columnNames = responseJsonObject["manifest"]?["schema"]?["columns"]?.Select(x => x["name"]?.ToString()).ToArray() ??
                          throw new DatabricksSqlException("Unable to retrieve 'columns' from the responseJsonObject.");
        return columnNames!;
    }

    private static List<string[]> GetDataArray(JObject responseJsonObject)
    {
        var dataArray = responseJsonObject["result"]?["data_array"]?.ToObject<List<string[]>>() ??
                        throw new DatabricksSqlException("Unable to retrieve 'data_array' from the responseJsonObject");
        return dataArray;
    }

    private static string? GetNextChunkInternalLink(JObject responseJsonObject)
    {
        return responseJsonObject["result"]?["next_chunk_internal_link"]?.ToObject<string>();
    }
}
