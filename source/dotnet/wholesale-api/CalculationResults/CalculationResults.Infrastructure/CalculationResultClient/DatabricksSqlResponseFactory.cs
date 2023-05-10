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

public class DatabricksSqlResponseFactory : IDatabricksSqlResponseFactory
{
    public DatabricksSqlResponse Create(string jsonResponse)
    {
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();
        var state = jsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException();
        var dataArray = jsonObject["result"]?["data_array"]?.ToObject<List<string[]>>() ??
                        throw new InvalidOperationException();

        return new DatabricksSqlResponse(state, dataArray);
    }
}
