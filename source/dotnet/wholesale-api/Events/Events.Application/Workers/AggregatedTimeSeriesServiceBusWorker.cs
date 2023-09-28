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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Logging;
using Energinet.DataHub.Wholesale.Events.Application.Options;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

/// <summary>
/// Responsible for establishing the service bus connection on a background thread.
/// </summary>
public class AggregatedTimeSeriesServiceBusWorker : BackgroundService, IAsyncDisposable
{
    private readonly IAggregatedTimeSeriesRequestHandler _aggregatedTimeSeriesRequestHandler;
    private readonly ILogger<AggregatedTimeSeriesServiceBusWorker> _logger;
    private readonly ServiceBusProcessor _serviceBusProcessor;
    private readonly string _serviceName;
    private readonly LoggingScope _loggingScope;

    public AggregatedTimeSeriesServiceBusWorker(
        IAggregatedTimeSeriesRequestHandler aggregatedTimeSeriesRequestHandler,
        ILogger<AggregatedTimeSeriesServiceBusWorker> logger,
        IOptions<ServiceBusOptions> options,
        ServiceBusClient serviceBusClient)
    {
        _serviceBusProcessor = serviceBusClient.CreateProcessor(options.Value.WHOLESALE_INBOX_MESSAGE_QUEUE_NAME);
        _aggregatedTimeSeriesRequestHandler = aggregatedTimeSeriesRequestHandler;
        _logger = logger;

        _serviceName = GetType().Name;
        _loggingScope = new LoggingScope { ["HostedService"] = _serviceName };
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using (_logger.BeginScope(_loggingScope))
        {
            await _serviceBusProcessor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("{Worker} has stopped", _serviceName);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceBusProcessor.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_serviceBusProcessor == null) throw new ArgumentNullException();
        using (_logger.BeginScope(_loggingScope))
        {
            _logger.LogInformation("{Worker} started", _serviceName);
            _serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
            _serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

            await _serviceBusProcessor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        using (_logger.BeginScope(_loggingScope))
        {
            _logger.LogError(
                arg.Exception,
                "Process message encountered an exception. ErrorSource: {ErrorSource}, Entity Path: {EntityPath}",
                arg.ErrorSource, // Source of the error. For example, a Message completion operation failed.
                arg.EntityPath); // The entity path for which the exception occurred. For example, the entity path of the queue.
        }

        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        var loggingScope = new LoggingScope
        {
            ["MessageId"] = arg.Message.MessageId,
            ["Subject"] = arg.Message.Subject,
        };

        foreach (var (key, value) in _loggingScope)
        {
            loggingScope[key] = value;
        }

        using (_logger.BeginScope(loggingScope))
        {
            if (
                arg.Message.ApplicationProperties.TryGetValue("ReferenceId", out var referenceIdPropertyValue)
                && referenceIdPropertyValue is string referenceId)
            {
                await _aggregatedTimeSeriesRequestHandler.ProcessAsync(arg.Message, referenceId, arg.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogError("Missing reference id for Service Bus Message");
            }
        }

        await arg.CompleteMessageAsync(arg.Message).ConfigureAwait(false);
    }
}
