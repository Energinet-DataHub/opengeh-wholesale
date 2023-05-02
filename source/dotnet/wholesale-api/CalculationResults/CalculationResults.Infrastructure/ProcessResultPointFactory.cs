// Copyright 2020 Energinet DataHub A/S
//
//  Licensed under the Apache License, Version 2.0 (the "License2");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Newtonsoft.Json;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure;

public class ProcessResultPointFactory : IProcessResultPointFactory
{
 public IEnumerable<ProcessResultPoint> Create(string input)
 {
     dynamic jsonObject = JsonConvert.DeserializeObject(input) ?? throw new InvalidOperationException();
     var result = jsonObject.result.data_array;

     // TODO: Why is the list not used?
     var list = new List<ProcessResultPoint>();
     foreach (var res in result)
     {
         // the sql statement dictates order of the columns
         list.Add(new ProcessResultPoint(res[1], res[2], res[0]));
     }

     return result;
 }
}
