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

using FunctionApp.Orchestrations.Functions.Calculation.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp.Orchestrations.Functions.Calculation
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
    internal class CalculationActivities
    {
        private readonly ILogger<CalculationActivities> _logger;

        public CalculationActivities(ILogger<CalculationActivities> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Create calculation status record in SQL database.
        /// </summary>
        [Function(nameof(CreateCalculationMetaActivity))]
        public async Task<CalculationMeta> CreateCalculationMetaActivity(
            [ActivityTrigger] BatchRequestDto batchRequestDto)
        {
            _logger.LogInformation($"{nameof(batchRequestDto)}: {batchRequestDto}");

            // TODO: Create new calculation id and create calculation tracking record in SQL database.
            await Task.Delay(Random.Shared.Next(1, 3) * 1000);

            // TODO: Return calculation status record.
            return new CalculationMeta
            {
                Id = Guid.NewGuid(),
                Input = batchRequestDto,
            };
        }

        /// <summary>
        /// Update calculation status record in SQL database.
        /// </summary>
        [Function(nameof(UpdateCalculationMetaActivity))]
        public async Task UpdateCalculationMetaActivity(
            [ActivityTrigger] CalculationMeta calculationMeta)
        {
            _logger.LogInformation($"{nameof(calculationMeta)}: {calculationMeta}");

            // TODO: Update calculation tracking record in SQL database.
            await Task.Delay(Random.Shared.Next(1, 3) * 1000);
        }

        /// <summary>
        /// Start calculation in Databricks.
        /// </summary>
        [Function(nameof(StartCalculationActivity))]
        public async Task<Guid> StartCalculationActivity(
            [ActivityTrigger] Guid calculationId)
        {
            _logger.LogInformation($"{nameof(calculationId)}: {calculationId}");

            // TODO: Start calculation job with parameters in databricks.
            await Task.Delay(Random.Shared.Next(1, 5) * 1000);

            // TODO: Return calculation job id.
            var jobId = Guid.NewGuid();
            return jobId;
        }

        /// <summary>
        /// Request calculation job status in Databricks.
        /// </summary>
        [Function(nameof(GetJobStatusActivity))]
        public async Task<string> GetJobStatusActivity(
            [ActivityTrigger] Guid jobId)
        {
            _logger.LogInformation($"{nameof(jobId)} : {jobId}");

            // TODO: Request calculation job status in databricks.
            var rnd = Random.Shared.Next(1, 5);
            await Task.Delay(rnd * 1000);

            // TODO: Return calculation job status.
            var status = "Running";
            if (rnd > 3)
            {
                status = "Completed";
            }

            return status;
        }

        /// <summary>
        /// Retrieve calculation results from Databricks and send them as events using ServiceBus.
        /// </summary>
        [Function(nameof(SendCalculationResultsActivity))]
        public async Task SendCalculationResultsActivity(
            [ActivityTrigger] Guid calculationId)
        {
            _logger.LogInformation($"{nameof(calculationId)} : {calculationId}");

            // TODO: Retrieve calculation results from Databricks and send them as events using ServiceBus.
            await Task.Delay(Random.Shared.Next(1, 5) * 1000);
        }
    }
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
}
