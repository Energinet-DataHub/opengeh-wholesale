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

using Energinet.DataHub.Wholesale.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.Hosts;

public sealed class WebApiIntegrationTestHost : IDisposable
{
    private readonly IHost _processManagerHost;

    private WebApiIntegrationTestHost(IHost processManagerHost)
    {
        _processManagerHost = processManagerHost;
    }

    public static Task<WebApiIntegrationTestHost> CreateAsync(
        Action<IServiceCollection>? serviceConfiguration = default)
    {
        ConfigureEnvironmentVars();

        var hostBuilder = Program
            .CreateWebHostBuilder(new[] { string.Empty })
            .ConfigureServices(ConfigureServices);

        if (serviceConfiguration != null)
        {
            hostBuilder = hostBuilder.ConfigureServices(serviceConfiguration);
        }

        return Task.FromResult(new WebApiIntegrationTestHost(hostBuilder.Build()));
    }

    public AsyncServiceScope BeginScope()
    {
        return _processManagerHost.Services.CreateAsyncScope();
    }

    public void Dispose()
    {
        _processManagerHost.Dispose();
    }

    private static void ConfigureEnvironmentVars()
    {
        const string anyValue = "fake_value";
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.BackendAppId, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ExternalOpenIdUrl, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.InternalOpenIdUrl, anyValue);
        Environment.SetEnvironmentVariable($"CONNECTIONSTRINGS:{EnvironmentSettingNames.DbConnectionString}", "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DateTimeZoneId, "Europe/Copenhagen");
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
    }
}
