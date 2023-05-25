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

using Energinet.DataHub.Wholesale.Common.Workers;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.UseCases;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Workers;

/// <summary>
/// Worker invoking fetching completed batches from the batches module and registering them in the module.
/// </summary>
public class RegisterCompletedBatchesWorker : RepeatingWorker
{
    private const int DelayInSecondsBeforeNextExecution = 10;
    private readonly IRegisterCompletedBatchesHandler _handler;

    public RegisterCompletedBatchesWorker(
        IServiceProvider serviceProvider,
        ILogger<RegisterCompletedBatchesWorker> logger,
        IRegisterCompletedBatchesHandler handler)
        : base(serviceProvider, logger, TimeSpan.FromSeconds(DelayInSecondsBeforeNextExecution))
    {
        _handler = handler;
    }

    protected override async Task ExecuteAsync()
    {
        await _handler.RegisterCompletedBatchesAsync().ConfigureAwait(false);
    }
}
