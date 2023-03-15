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
using Energinet.DataHub.Wholesale.Application;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.ProcessManager.Endpoints;

public class PublishIntegrationEventsEndpoint
{
    private readonly IIntegrationEventDispatcher _integrationEventDispatcher;
    private readonly ICorrelationContext _correlationContext;

    public PublishIntegrationEventsEndpoint(
        IIntegrationEventDispatcher integrationEventDispatcher,
        ICorrelationContext correlationContext)
    {
        _integrationEventDispatcher = integrationEventDispatcher;
        _correlationContext = correlationContext;
    }

    // Executes every 120 seconds (see the [TimerTrigger] below)
    [Function(nameof(PublishIntegrationEventsEndpoint))]
    public async Task RunAsync([TimerTrigger("*/120 * * * * *")] TimerInfo timerInfo, CancellationToken token)
    {
        // CorrelationIdMiddleware does not currently support timer triggered functions,
        // so we need to add a correlation ID ourselves
        _correlationContext.SetId(Guid.NewGuid().ToString());

        await _integrationEventDispatcher.PublishIntegrationEventsAsync(token).ConfigureAwait(false);
    }
}
