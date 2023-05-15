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

using System.Linq.Expressions;
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
        var columnNames = GetColumnNames(jsonObject);
        var dataArray = GetDataArray(jsonObject);

        return new DatabricksSqlResponse(state, new TableData(columnNames, dataArray));
    }

    private string GetState(JObject responseJsonObject)
    {
        return responseJsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException();
    }

    private IEnumerable<string> GetColumnNames(JObject responseJsonObject)
    {
        var columnNames = responseJsonObject["manifest"]?["columns"]?.Select(x => x["name"]?.ToString()) ??
                          throw new InvalidOperationException();
        return columnNames!;
    }

    private IEnumerable<string[]> GetDataArray(JObject responseJsonObject)
    {
        var dataArray = responseJsonObject["result"]?["data_array"]?.ToObject<List<string[]>>() ??
                        throw new InvalidOperationException();
        return dataArray;
    }
}
