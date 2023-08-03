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

using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Common.Workers;

/// <summary>
/// Hosted service worker repeating a task indefinitely.
/// </summary>
public abstract class RepeatingWorker<TService> : BackgroundService
    where TService : notnull
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly TimeSpan _delayBetweenExecutions;
    private readonly string _serviceName;
    private readonly Dictionary<string, object> _loggingScope;

    protected RepeatingWorker(
        IServiceProvider serviceProvider,
        ILogger logger,
        TimeSpan delayBetweenExecutions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _delayBetweenExecutions = delayBetweenExecutions;

        _serviceName = GetType().Name;
        _loggingScope = new Dictionary<string, object> { ["HostedService"] = _serviceName };
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using (_logger.BeginScope(_loggingScope))
        {
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("{Worker} has stopped", _serviceName);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (_logger.BeginScope(_loggingScope))
        {
            _logger.LogInformation("{Worker} started", _serviceName);

            while (!stoppingToken.IsCancellationRequested)
            {
                await InvokeAsync(stoppingToken).ConfigureAwait(false);

                await Task.Delay(_delayBetweenExecutions, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Method to be implemented by the inheriting class.
    /// The method is invoked repeatedly with a delay between each invocation.
    /// </summary>
    protected abstract Task ExecuteAsync(TService instance, CancellationToken cancellationToken);

    private async Task InvokeAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        // CorrelationIdMiddleware does not support IHostedService, so we need to add a correlation ID ourselves
        var correlationContext = scope.ServiceProvider.GetRequiredService<ICorrelationContext>();
        correlationContext.SetId(Guid.NewGuid().ToString());

        var service = scope.ServiceProvider.GetRequiredService<TService>();
        try
        {
            await ExecuteAsync(service, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception in {Worker}", _serviceName);
        }
    }
}
