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

using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.ProcessManager;

public static class Program
{
    public static async Task Main()
    {
        var startup = new Startup();

        using var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.UseMiddleware<CorrelationIdMiddleware>();
                builder.UseMiddleware<FunctionTelemetryScopeMiddleware>();
                builder.UseMiddleware<IntegrationEventMetadataMiddleware>();
            })
            .ConfigureServices((context, services) => startup.Initialize(context.Configuration, services))
            .Build();

        await host.RunAsync().ConfigureAwait(false);
    }
}
