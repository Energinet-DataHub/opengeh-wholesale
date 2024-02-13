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

namespace Energinet.DataHub.Wholesale.Orchestration.Functions.Calculation
{
    public class StartCalculationTrigger
    {
        private readonly ILogger _logger;

        public StartCalculationTrigger(ILogger<StartCalculationTrigger> logger)
        {
            _logger = logger;
        }

        [Function("StartCalculationTrigger")]
        public void Run(
            [TimerTrigger("00:00:10")]
            TimerInfo timerInfo)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (timerInfo.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {timerInfo.ScheduleStatus.Next}");
            }
        }
    }
}
