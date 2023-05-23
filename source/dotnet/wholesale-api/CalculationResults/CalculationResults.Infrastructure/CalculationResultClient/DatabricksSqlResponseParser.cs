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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public class DatabricksSqlResponseParser : IDatabricksSqlResponseParser
{
    public DatabricksSqlResponse Parse(string jsonResponse)
    {
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();

        var state = GetState(jsonObject);
        switch (state)
        {
            case "PENDING":
                return new DatabricksSqlResponse(state, null);
            case "RUNNING":
                return new DatabricksSqlResponse("PENDING", null); // We currently don't distinguish between PENDING and RUNNING
            case "SUCCEEDED":
                var columnNames = GetColumnNames(jsonObject);
                var dataArray = GetDataArray(jsonObject);
                return new DatabricksSqlResponse(state, new Table(columnNames, dataArray));
            default:
                throw new Exception($@"Databricks SQL statement execution failed. State: {state}");
        }
    }

    private static string GetState(JObject responseJsonObject)
    {
        return responseJsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");
    }

    private static IEnumerable<string> GetColumnNames(JObject responseJsonObject)
    {
        var columnNames = responseJsonObject["manifest"]?["schema"]?["columns"]?.Select(x => x["name"]?.ToString()) ??
                          throw new InvalidOperationException("Unable to retrieve 'columns' from the responseJsonObject.");
        return columnNames!;
    }

    private static IEnumerable<string[]> GetDataArray(JObject responseJsonObject)
    {
        var dataArray = responseJsonObject["result"]?["data_array"]?.ToObject<List<string[]>>() ??
                        throw new InvalidOperationException("Unable to retrieve 'data_array' from the responseJsonObject");
        return dataArray;
    }
}
