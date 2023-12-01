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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.EDI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

/// <summary>
/// Responsible for establishing the service bus connection on a background thread.
/// </summary>
public class AggregatedTimeSeriesServiceBusWorker : ServiceBusWorker<AggregatedTimeSeriesServiceBusWorker>
{
    private readonly IServiceProvider _serviceProvider;

    public AggregatedTimeSeriesServiceBusWorker(
        IServiceProvider serviceProvider,
        ILogger<AggregatedTimeSeriesServiceBusWorker> logger,
        IOptions<ServiceBusOptions> options,
        ServiceBusClient serviceBusClient)
    : base(logger, serviceBusClient.CreateProcessor(options.Value.WHOLESALE_INBOX_MESSAGE_QUEUE_NAME), isQueueListener: true)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ProcessAsync(ProcessMessageEventArgs arg, string referenceId)
    {
        var scope = _serviceProvider.CreateScope();
        var requestHandler = scope.ServiceProvider.GetRequiredService<IAggregatedTimeSeriesRequestHandler>();
        return requestHandler.ProcessAsync(arg.Message, referenceId, arg.CancellationToken);
    }
}
