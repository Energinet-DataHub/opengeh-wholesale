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

public class DatabricksSqlChunkDataResponseParser : IDatabricksSqlChunkDataResponseParser
{
    public TableChunk Parse(string jsonResponse, string[] columnNames)
    {
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonArray = JsonConvert.DeserializeObject<JArray>(jsonResponse, settings) ??
                         throw new InvalidOperationException();

        var rows = GetDataArray(jsonArray);
        return new TableChunk(columnNames, rows);
    }

    private static List<string[]> GetDataArray(JArray jsonArray)
    {
        var dataArray = jsonArray.ToObject<List<string[]>>() ??
                        throw new DatabricksSqlException("Unable to retrieve 'data_array' from the response");
        return dataArray;
    }
}
