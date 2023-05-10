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

public class DatabricksSqlResponse
{
    private JObject _jsonObject = new();

    public void DeserializeFromJson(string jsonResponse)
    {
        _jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse) ?? throw new InvalidOperationException();
    }

    public string GetState()
    {
        return _jsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException();
    }

    public IEnumerable<string[]> GetDataArray()
    {
        return _jsonObject["result"]?["data_array"]?.ToObject<List<string[]>>() ?? throw new InvalidOperationException();
    }
}
