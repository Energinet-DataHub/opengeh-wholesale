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

using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Logging;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

public abstract class ServiceBusWorker<TWorkerType> : BackgroundService, IAsyncDisposable
{
    private readonly ServiceBusProcessor _serviceBusProcessor;
    private readonly ILogger<TWorkerType> _logger;
    private readonly string _serviceName;
    private readonly LoggingScope _loggingScope;

    protected ServiceBusWorker(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> options,
        ILogger<TWorkerType> logger)
    {
        _serviceBusProcessor = CreateServiceBusProcessor(serviceBusClient, options);

        _logger = logger;
        _serviceName = typeof(TWorkerType).Name;
        _loggingScope = new LoggingScope { ["HostedService"] = _serviceName };
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceBusProcessor.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_serviceBusProcessor == null) throw new ArgumentNullException();

        _logger.LogInformation("{Worker} started", _serviceName);
        _serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
        _serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

        await _serviceBusProcessor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
    }

    protected abstract Task ProcessAsync(ProcessMessageEventArgs arg);

    private static ServiceBusProcessor CreateServiceBusProcessor(ServiceBusClient serviceBusClient, IOptions<ServiceBusOptions> options)
    {
        var serviceBusProcessorOptions = new ServiceBusProcessorOptions()
        {
            AutoCompleteMessages = false, // Default is true
        };
        if (!string.IsNullOrWhiteSpace(options.Value.WHOLESALE_INBOX_MESSAGE_QUEUE_NAME))
        {
            return serviceBusClient.CreateProcessor(options.Value.WHOLESALE_INBOX_MESSAGE_QUEUE_NAME, serviceBusProcessorOptions);
        }

        return serviceBusClient.CreateProcessor(
            options.Value.INTEGRATIONEVENTS_TOPIC_NAME,
            options.Value.INTEGRATIONEVENTS_SUBSCRIPTION_NAME,
            serviceBusProcessorOptions);
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

        var stopWatch = new Stopwatch();
        using (_logger.BeginScope(loggingScope))
        {
            stopWatch.Start();
            await ProcessAsync(arg).ConfigureAwait(false);
            stopWatch.Stop();
            _logger.LogInformation("Processed message with id {MessageId} in {ElapsedMilliseconds} ms", arg.Message.MessageId, stopWatch.ElapsedMilliseconds);
        }

        await arg.CompleteMessageAsync(arg.Message).ConfigureAwait(false);
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
}
