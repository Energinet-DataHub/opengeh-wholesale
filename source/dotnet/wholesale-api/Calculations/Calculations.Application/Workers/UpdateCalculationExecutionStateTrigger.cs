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

using Energinet.DataHub.Core.App.WebApp.Hosting;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Calculations.Application.Workers;

/// <summary>
/// Worker invoking updating calculation execution states.
/// </summary>
public class UpdateCalculationExecutionStateTrigger : RepeatingTrigger<IUpdateCalculationExecutionStateHandler>
{
    private const int DelayInSecondsBeforeNextExecution = 20;

    public UpdateCalculationExecutionStateTrigger(
        IServiceProvider serviceProvider,
        ILogger<UpdateCalculationExecutionStateTrigger> logger)
        : base(serviceProvider, logger, TimeSpan.FromSeconds(DelayInSecondsBeforeNextExecution))
    {
    }

    protected override async Task ExecuteAsync(
        IUpdateCalculationExecutionStateHandler scopedService,
        CancellationToken cancellationToken,
        Action isAliveCallback)
    {
        await scopedService.UpdateAsync().ConfigureAwait(false);
    }
}
