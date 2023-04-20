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

using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.WebApi.Configuration.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.Hosts;

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
        const string anyServiceBusConnectionString = "Endpoint=sb://foo.servicebus.windows.net/;SharedAccessKeyName=someKeyName;SharedAccessKey=someKeyValue";
        const string anyBlobServiceUri = "https://localhost:10000/anyaccount";

        Environment.SetEnvironmentVariable(nameof(AppInsightOptions.APPINSIGHTS_INSTRUMENTATIONKEY), anyValue);
        Environment.SetEnvironmentVariable(nameof(JwtOptions.BACKEND_BFF_APP_ID), anyValue);
        Environment.SetEnvironmentVariable(nameof(JwtOptions.EXTERNAL_OPEN_ID_URL), anyValue);
        Environment.SetEnvironmentVariable(nameof(JwtOptions.INTERNAL_OPEN_ID_URL), anyValue);
        Environment.SetEnvironmentVariable($"{ConnectionStringsOptions.ConnectionStrings}__{nameof(ConnectionStringsOptions.DB_CONNECTION_STRING)}", anyValue);
        Environment.SetEnvironmentVariable(nameof(DataLakeOptions.STORAGE_CONNECTION_STRING), "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable(nameof(DataLakeOptions.STORAGE_CONTAINER_NAME), anyValue);
        Environment.SetEnvironmentVariable(ConfigurationSettingNames.CalculationStorageAccountUri, anyBlobServiceUri);
        Environment.SetEnvironmentVariable(ConfigurationSettingNames.CalculationStorageContainerName, anyValue);
        Environment.SetEnvironmentVariable(nameof(ServiceBusOptions.SERVICE_BUS_MANAGE_CONNECTION_STRING), anyServiceBusConnectionString);
        Environment.SetEnvironmentVariable(nameof(ServiceBusOptions.SERVICE_BUS_SEND_CONNECTION_STRING), anyServiceBusConnectionString);
        Environment.SetEnvironmentVariable(nameof(ServiceBusOptions.BATCH_CREATED_EVENT_NAME), "batch-created");
        Environment.SetEnvironmentVariable(nameof(ServiceBusOptions.DOMAIN_EVENTS_TOPIC_NAME), anyValue);
        Environment.SetEnvironmentVariable(nameof(DateTimeOptions.TIME_ZONE), "Europe/Copenhagen");
        Environment.SetEnvironmentVariable(nameof(DatabricksOptions.DATABRICKS_WORKSPACE_URL), "http://localhost/");
        Environment.SetEnvironmentVariable(nameof(DatabricksOptions.DATABRICKS_WORKSPACE_TOKEN), "no_token");
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
    }
}
