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

using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Orchestration.Functions.Calculation;

public class RegisterCompletedCalculationsTrigger
{
    private readonly ILogger _logger;
    private readonly IRegisterCompletedCalculationsHandler _handler;

    public RegisterCompletedCalculationsTrigger(
        ILogger<RegisterCompletedCalculationsTrigger> logger,
        IRegisterCompletedCalculationsHandler handler)
    {
        _logger = logger;
        _handler = handler;
    }

    [Function(nameof(RegisterCompletedCalculationsTrigger))]
    public async Task Run(
        [TimerTrigger("00:00:10")]
        TimerInfo timerInfo)
    {
        await _handler.RegisterCompletedCalculationsAsync().ConfigureAwait(false);
    }
}
