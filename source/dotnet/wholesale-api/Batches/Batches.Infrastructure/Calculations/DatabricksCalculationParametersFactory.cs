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

using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.Calculations;

public class DatabricksCalculationParametersFactory : ICalculationParametersFactory
{
    public RunParameters CreateParameters(Calculation calculation)
    {
        var gridAreas = string.Join(", ", calculation.GridAreaCodes.Select(c => c.Code));

        var jobParameters = new List<string>
        {
            $"--batch-id={calculation.Id}",
            $"--batch-grid-areas=[{gridAreas}]",
            $"--batch-period-start-datetime={calculation.PeriodStart}",
            $"--batch-period-end-datetime={calculation.PeriodEnd}",
            $"--batch-process-type={calculation.ProcessType}",
            $"--batch-execution-time-start={calculation.ExecutionTimeStart}",
        };

        return RunParameters.CreatePythonParams(jobParameters);
    }
}
