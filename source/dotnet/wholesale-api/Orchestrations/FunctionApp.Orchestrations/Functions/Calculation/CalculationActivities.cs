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

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp.Orchestrations.Functions.Calculation
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
    internal static class CalculationActivities
    {
        [Function(nameof(CreateCalculationMetaActivity))]
        public static string CreateCalculationMetaActivity(
            [ActivityTrigger] string cityName,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(CreateCalculationMetaActivity));
            logger.LogInformation("Saying hello to {name}", cityName);
            return $"Hello, {cityName}!";
        }

        [Function(nameof(StartCalculationActivity))]
        public static string StartCalculationActivity(
            [ActivityTrigger] string cityName,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(StartCalculationActivity));
            logger.LogInformation("Saying hello to {name}", cityName);
            return $"Hello, {cityName}!";
        }

        [Function(nameof(UpdateCalculationMetaActivity))]
        public static string UpdateCalculationMetaActivity(
            [ActivityTrigger] string cityName,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(UpdateCalculationMetaActivity));
            logger.LogInformation("Saying hello to {name}", cityName);
            return $"Hello, {cityName}!";
        }
    }
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
}
