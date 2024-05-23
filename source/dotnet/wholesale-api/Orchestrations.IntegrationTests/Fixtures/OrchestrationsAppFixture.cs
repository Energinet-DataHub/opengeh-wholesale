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
using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Test.Core.Fixture.Database;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using WireMock.Server;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;

/// <summary>
/// Support testing Orchestrations app.
/// </summary>
public class OrchestrationsAppFixture : IAsyncLifetime
{
    /// <summary>
    /// Durable Functions Task Hub Name
    /// See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
    /// </summary>
    private const string TaskHubName = "WholesaleTest01";

    public OrchestrationsAppFixture()
    {
        TestLogger = new TestDiagnosticsLogger();
        IntegrationTestConfiguration = new IntegrationTestConfiguration();

        AzuriteManager = new AzuriteManager(useOAuth: true);
        DatabaseManager = new WholesaleDatabaseManager<DatabaseContext>();

        DurableTaskManager = new DurableTaskManager(
            "AzureWebJobsStorage",
            AzuriteManager.FullConnectionString);

        ServiceBusResourceProvider = new ServiceBusResourceProvider(
            IntegrationTestConfiguration.ServiceBusConnectionString,
            TestLogger);

        ServiceBusListenerMock = new ServiceBusListenerMock(
            IntegrationTestConfiguration.ServiceBusConnectionString,
            TestLogger);

        HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();

        MockServer = WireMockServer.Start(port: 1024);
    }

    public ITestDiagnosticsLogger TestLogger { get; }

    public WireMockServer MockServer { get; }

    [NotNull]
    public FunctionAppHostManager? AppHostManager { get; private set; }

    [NotNull]
    public IDurableClient? DurableClient { get; private set; }

    public ServiceBusListenerMock ServiceBusListenerMock { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    private AzuriteManager AzuriteManager { get; }

    private WholesaleDatabaseManager<DatabaseContext> DatabaseManager { get; }

    private DurableTaskManager DurableTaskManager { get; }

    private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

    private FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

    public async Task InitializeAsync()
    {
        // Storage emulator
        AzuriteManager.StartAzurite();

        // Database
        await DatabaseManager.CreateDatabaseAsync();

        // Prepare host settings
        var port = 8000;
        var appHostSettings = CreateAppHostSettings("Orchestrations", ref port);

        // ServiceBus entities
        var topicResource = await ServiceBusResourceProvider
            .BuildTopic("integration-events")
            .Do(topic => appHostSettings.ProcessEnvironmentVariables
                .Add($"{IntegrationEventsOptions.SectionName}__{nameof(IntegrationEventsOptions.TopicName)}", topic.Name))
            .AddSubscription("subscription")
            .Do(subscription => appHostSettings.ProcessEnvironmentVariables
                .Add($"{IntegrationEventsOptions.SectionName}__{nameof(IntegrationEventsOptions.SubscriptionName)}", subscription.SubscriptionName))
            .CreateAsync();

        // => Receive messages on topic/subscription
        await ServiceBusListenerMock.AddTopicSubscriptionListenerAsync(
            topicResource.Name,
            topicResource.Subscriptions.Single().SubscriptionName);

        // Storage: DataLake + Blob Containers
        await EnsureCalculationStorageContainerExistsAsync();
        await EnsureSettlementReportStorageContainerExistsAsync();

        // Create and start host
        AppHostManager = new FunctionAppHostManager(appHostSettings, TestLogger);
        StartHost(AppHostManager);

        // Create durable client when TaskHub has been created
        DurableClient = DurableTaskManager.CreateClient(taskHubName: TaskHubName);
    }

    public async Task DisposeAsync()
    {
        AppHostManager.Dispose();
        await ServiceBusListenerMock.DisposeAsync();
        await ServiceBusResourceProvider.DisposeAsync();
        MockServer.Dispose();
        DurableTaskManager.Dispose();
        AzuriteManager.Dispose();
        await DatabaseManager.DeleteDatabaseAsync();
    }

    public void EnsureAppHostUsesActualDatabricksJobs()
    {
        AppHostManager.RestartHostIfChanges(new Dictionary<string, string>
        {
            { nameof(DatabricksJobsOptions.WorkspaceUrl), IntegrationTestConfiguration.DatabricksSettings.WorkspaceUrl },
        });
    }

    public void EnsureAppHostUsesMockedDatabricksJobs()
    {
        AppHostManager.RestartHostIfChanges(new Dictionary<string, string>
        {
            { nameof(DatabricksJobsOptions.WorkspaceUrl), MockServer.Url! },
        });
    }

    public BlobContainerClient CreateBlobContainerClient()
    {
        return new BlobContainerClient(AzuriteManager.FullConnectionString, "settlement-report-container");
    }

    /// <summary>
    /// Use this method to attach <paramref name="testOutputHelper"/> to the host logging pipeline.
    /// While attached, any entries written to host log pipeline will also be logged to xUnit test output.
    /// It is important that it is only attached while a test i active. Hence, it should be attached in
    /// the test class constructor; and detached in the test class Dispose method (using 'null').
    /// </summary>
    /// <param name="testOutputHelper">If a xUnit test is active, this should be the instance of xUnit's <see cref="ITestOutputHelper"/>;
    /// otherwise it should be 'null'.</param>
    public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        TestLogger.TestOutputHelper = testOutputHelper;
    }

    private FunctionAppHostSettings CreateAppHostSettings(string csprojName, ref int port)
    {
        var buildConfiguration = GetBuildConfiguration();

        var appHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
        appHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\{csprojName}\\bin\\{buildConfiguration}\\net8.0";
        appHostSettings.Port = ++port;

        // It seems the host + worker is not ready if we use the default startup log message, so we override it here
        appHostSettings.HostStartedEvent = "Host lock lease acquired";

        appHostSettings.ProcessEnvironmentVariables.Add(
            "FUNCTIONS_WORKER_RUNTIME",
            "dotnet-isolated");
        appHostSettings.ProcessEnvironmentVariables.Add(
            "AzureWebJobsStorage",
            AzuriteManager.FullConnectionString);
        appHostSettings.ProcessEnvironmentVariables.Add(
            "APPLICATIONINSIGHTS_CONNECTION_STRING",
            IntegrationTestConfiguration.ApplicationInsightsConnectionString);

        // Durable Functions Task Hub Name
        appHostSettings.ProcessEnvironmentVariables.Add(
            "OrchestrationsTaskHubName",
            TaskHubName);

        // Database
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{ConnectionStringsOptions.ConnectionStrings}__{nameof(ConnectionStringsOptions.DB_CONNECTION_STRING)}",
            DatabaseManager.ConnectionString);

        // Databricks
        // => Notice we reconfigure this setting in "EnsureAppHostUsesActualDatabricksJobs" and "EnsureAppHostUsesMockedDatabricksJobs"
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DatabricksJobsOptions.WorkspaceUrl),
            MockServer.Url!);
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DatabricksJobsOptions.WorkspaceToken),
            IntegrationTestConfiguration.DatabricksSettings.WorkspaceAccessToken);
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DatabricksJobsOptions.WarehouseId),
            IntegrationTestConfiguration.DatabricksSettings.WarehouseId);

        // DataLake
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DataLakeOptions.STORAGE_ACCOUNT_URI),
            AzuriteManager.BlobStorageServiceUri.ToString());
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DataLakeOptions.STORAGE_CONTAINER_NAME),
            "wholesale");

        // ServiceBus connection strings
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{ServiceBusNamespaceOptions.SectionName}__{nameof(ServiceBusNamespaceOptions.ConnectionString)}",
            ServiceBusResourceProvider.ConnectionString);

        // Settlement Report blob storage configuration
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{SettlementReportStorageOptions.SectionName}__{nameof(SettlementReportStorageOptions.StorageContainerName)}",
            "settlement-report-container");
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{SettlementReportStorageOptions.SectionName}__{nameof(SettlementReportStorageOptions.StorageAccountUri)}",
            AzuriteManager.BlobStorageServiceUri + "/");

        // Override default CalculationJob status monitor configuration
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{CalculationJobStatusMonitorOptions.SectionName}__{nameof(CalculationJobStatusMonitorOptions.PollingIntervalInSeconds)}",
            "3");
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{CalculationJobStatusMonitorOptions.SectionName}__{nameof(CalculationJobStatusMonitorOptions.ExpiryTimeInSeconds)}",
            "20");

        return appHostSettings;
    }

    /// <summary>
    /// Create storage container.
    /// Note: Azurite is based on the Blob Storage API, but sinceData Lake Storage Gen2 is built on top of it,
    /// we can still create the container like this.
    /// </summary>
    private async Task EnsureCalculationStorageContainerExistsAsync()
    {
        var dataLakeServiceClient = new DataLakeServiceClient(
            serviceUri: AzuriteManager.BlobStorageServiceUri,
            credential: new DefaultAzureCredential());

        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient("wholesale");

        await fileSystemClient.CreateIfNotExistsAsync();
    }

    private async Task EnsureSettlementReportStorageContainerExistsAsync()
    {
        var blobContainerUri = new Uri(AzuriteManager.BlobStorageServiceUri + "/settlement-report-container");
        var blobContainerClient = new BlobContainerClient(blobContainerUri, new DefaultAzureCredential());

        await blobContainerClient.CreateIfNotExistsAsync();
    }

    private static void StartHost(FunctionAppHostManager hostManager)
    {
        IEnumerable<string> hostStartupLog;

        try
        {
            hostManager.StartHost();
        }
        catch (Exception)
        {
            // Function App Host failed during startup.
            // Exception has already been logged by host manager.
            hostStartupLog = hostManager.GetHostLogSnapshot();

            if (Debugger.IsAttached)
                Debugger.Break();

            // Rethrow
            throw;
        }

        // Function App Host started.
        hostStartupLog = hostManager.GetHostLogSnapshot();
    }

    private static string GetBuildConfiguration()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }
}
