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
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Events.Application.Workers;

public abstract class ServiceBusQueueWorker<TWorkerType> : ServiceBusWorker<TWorkerType>
{
    private readonly ILogger<TWorkerType> _logger;
    private readonly LoggingScope _loggingScope;

    protected ServiceBusQueueWorker(ILogger<TWorkerType> logger, ServiceBusProcessor serviceBusProcessor)
        : base(logger, serviceBusProcessor)
    {
        _logger = logger;
        _loggingScope = new LoggingScope { ["HostedService"] = typeof(TWorkerType).Name };
    }

    protected override async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
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
            if (arg.Message.ApplicationProperties.TryGetValue("ReferenceId", out var referenceIdPropertyValue)
                && referenceIdPropertyValue is string referenceId)
            {
                await ProcessAsync(arg, referenceId).ConfigureAwait(false);
            }
            else
            {
                _logger.LogError("Missing reference id for Service Bus Message");
            }
        }

        await arg.CompleteMessageAsync(arg.Message).ConfigureAwait(false);
    }
}
