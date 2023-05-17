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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.ProcessManager.Monitor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.ProcessManager;

public static class Program
{
    public static async Task Main()
    {
        using var host = CreateHostBuilder().Build();
        await host.RunAsync().ConfigureAwait(false);
    }

    public static IHostBuilder CreateHostBuilder()
    {
        return new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(Infrastructure)
            .ConfigureServices(HealthCheck);
    }

    private static void Infrastructure(IServiceCollection serviceCollection)
    {
        serviceCollection.AddApplicationInsights();
    }

    private static void HealthCheck(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
        serviceCollection.AddScoped<HealthCheckEndpoint>();

        serviceCollection
            .AddHealthChecks()
            .AddLiveCheck();
    }
}
