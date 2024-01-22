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

using System.Text;

namespace FunctionApp.Orchestrations.Functions.Calculation.Model
{
    public class CalculationMeta
    {
        /// <summary>
        /// Calculation id.
        /// </summary>
        public Guid Id { get; set; }
            = Guid.Empty;

        /// <summary>
        /// Calculation input given as parameters when starting job in Databricks.
        /// </summary>
        public BatchRequestDto? Input { get; set; }

        /// <summary>
        /// Id of started calculation job in Databricks.
        /// </summary>
        public Guid JobId { get; set; }
            = Guid.Empty;

        /// <summary>
        /// Status of calculation job in Databricks.
        /// </summary>
        public string JobStatus { get; set; }
            = string.Empty;

        public override string ToString()
        {
            return GetType()
                .GetProperties()
                .Select(p => (p.Name, Value: p.GetValue(this, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
        }
    }
}
