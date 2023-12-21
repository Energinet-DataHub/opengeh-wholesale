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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

public abstract class ServiceBusWorker<TWorkerType> : BackgroundService, IAsyncDisposable
{
    private readonly ServiceBusProcessor _serviceBusProcessor;
    private readonly string _serviceName;

    protected ServiceBusWorker(
        ServiceBusProcessor serviceBusProcessor,
        ILogger<TWorkerType> logger)
    {
        _serviceBusProcessor = serviceBusProcessor;
        Logger = logger;
        _serviceName = typeof(TWorkerType).Name;
    }

    protected ILogger<TWorkerType> Logger { get; }

    public async ValueTask DisposeAsync()
    {
        await _serviceBusProcessor.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _serviceBusProcessor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
        Logger.LogWarning("{worker} has stopped", _serviceName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{worker} started", _serviceName);
        _serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
        _serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

        await _serviceBusProcessor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
    }

    protected abstract Task ProcessAsync(ProcessMessageEventArgs arg);

    private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        var stopWatch = Stopwatch.StartNew();
        await ProcessAsync(arg).ConfigureAwait(false);
        stopWatch.Stop();
        Logger.LogInformation("Processed message with id {message_id} in {elapsed_milliseconds} ms", arg.Message.MessageId, stopWatch.ElapsedMilliseconds);

        await arg.CompleteMessageAsync(arg.Message).ConfigureAwait(false);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        Logger.LogError(
            arg.Exception,
            "Process message encountered an exception. ErrorSource: {error_source}, Entity Path: {entity_path}",
            arg.ErrorSource, // Source of the error. For example, a Message completion operation failed.
            arg.EntityPath); // The entity path for which the exception occurred. For example, the entity path of the queue.

        return Task.CompletedTask;
    }
}
