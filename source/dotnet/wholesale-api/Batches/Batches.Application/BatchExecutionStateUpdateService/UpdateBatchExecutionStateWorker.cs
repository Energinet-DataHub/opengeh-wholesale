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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Batches.Application.BatchExecutionStateUpdateService;

/// <summary>
/// Timer triggered hosted service to invoke the service for updating batch execution states.
/// </summary>
public class UpdateBatchExecutionStateWorker : BackgroundService
{
    private const int DelayInSecondsBeforeNextExecution = 20;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateBatchExecutionStateWorker> _logger;

    public UpdateBatchExecutionStateWorker(IServiceProvider serviceProvider, ILogger<UpdateBatchExecutionStateWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("{Worker} running at: {Time}", nameof(UpdateBatchExecutionStateWorker), DateTimeOffset.Now);

            await ExecuteInScopeAsync().ConfigureAwait(false);

            await Task.Delay(DelayInSecondsBeforeNextExecution * 1000, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ExecuteInScopeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var updateService = scope.ServiceProvider.GetRequiredService<IBatchExecutionStateUpdateService>();

        await updateService.UpdateBatchExecutionStatesAsync().ConfigureAwait(false);
    }
}
