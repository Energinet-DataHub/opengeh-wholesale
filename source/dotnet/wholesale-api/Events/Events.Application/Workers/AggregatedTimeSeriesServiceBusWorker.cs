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
using Energinet.DataHub.Wholesale.Events.Application.Options;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

/// <summary>
/// Responsible for establishing the service bus connection on a background thread.
/// </summary>
public class AggregatedTimeSeriesServiceBusWorker : BackgroundService, IAsyncDisposable
{
    private readonly IAggregatedTimeSeriesRequestHandler _aggregatedTimeSeriesRequestHandler;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<AggregatedTimeSeriesRequestHandler> _logger;
    private readonly ServiceBusOptions _serviceBusOptions;
    private ServiceBusProcessor? _serviceBusProcessor;

    public AggregatedTimeSeriesServiceBusWorker(
        IAggregatedTimeSeriesRequestHandler aggregatedTimeSeriesRequestHandler,
        ILogger<AggregatedTimeSeriesRequestHandler> logger,
        ServiceBusOptions serviceBusOptions)
    {
        _serviceBusClient = new ServiceBusClient(serviceBusOptions.SERVICE_BUS_LISTEN_CONNECTION_STRING);
        _aggregatedTimeSeriesRequestHandler = aggregatedTimeSeriesRequestHandler;
        _logger = logger;
        _serviceBusOptions = serviceBusOptions;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping the service bus subscription");

        if (_serviceBusProcessor != null)
            await _serviceBusProcessor.CloseAsync(cancellationToken).ConfigureAwait(false);
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        if (_serviceBusProcessor != null) await _serviceBusProcessor.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _serviceBusProcessor = _serviceBusClient.CreateProcessor(_serviceBusOptions.SERVICE_BUS_INBOX_QUEUE_NAME);

        _serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
        _serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

        await _serviceBusProcessor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(
            arg.Exception,
            "Message handler encountered an exception. ErrorSource: {ErrorSource}, Entity Path: {EntityPath}",
            arg.ErrorSource,
            arg.EntityPath);
        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        await _aggregatedTimeSeriesRequestHandler.ProcessAsync(arg.CancellationToken).ConfigureAwait(false);
        await arg.CompleteMessageAsync(arg.Message).ConfigureAwait(false);
    }
}
