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

using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.IntegrationEventListener;

public class IntegrationEventReceiveLogger : IFunctionsWorkerMiddleware
{
    private readonly ILogger _logger;
    private readonly IntegrationEventContext _integrationEventContext;

    public IntegrationEventReceiveLogger(ILoggerFactory loggerFactory, IntegrationEventContext integrationEventContext)
    {
        _integrationEventContext = integrationEventContext;
        _logger = loggerFactory.CreateLogger(nameof(IntegrationEventReceiveLogger));
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        if (!_integrationEventContext.TryReadMetadata(out var metadata))
        {
            _logger.LogError(
                "Function '{FunctionName}' received integration event with missing meta data according to ADR-008",
                context.FunctionDefinition.Name);
            return;
        }

        _logger.LogInformation(
            "Function '{FunctionName}' received integration event of type '{MessageType}'",
            context.FunctionDefinition.Name,
            metadata.MessageType);

        await next(context).ConfigureAwait(false);
    }
}
