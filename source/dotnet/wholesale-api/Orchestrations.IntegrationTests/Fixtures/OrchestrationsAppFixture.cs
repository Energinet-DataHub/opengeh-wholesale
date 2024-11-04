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
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.RevisionLog.Integration.Options;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Outbox;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Energinet.DataHub.Wholesale.Test.Core.Fixture.Database;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using NodaTime;
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
            "DURABLETASK_STORAGE_CONNECTION_STRING",
            AzuriteManager.FullConnectionString);

        ServiceBusResourceProvider = new ServiceBusResourceProvider(
            TestLogger,
            IntegrationTestConfiguration.ServiceBusFullyQualifiedNamespace,
            IntegrationTestConfiguration.Credential);

        ServiceBusListenerMock = new ServiceBusListenerMock(
            TestLogger,
            IntegrationTestConfiguration.ServiceBusFullyQualifiedNamespace,
            IntegrationTestConfiguration.Credential);

        HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();

        MockServer = WireMockServer.Start(port: 1024);
        OpenIdJwtManager = new OpenIdJwtManager(IntegrationTestConfiguration.B2CSettings);
    }

    public TestUserInformation UserInformation { get; } = new TestUserInformation(
        UserId: Guid.NewGuid(),
        ActorId: Guid.NewGuid(),
        ActorNumber: "0000000000000",
        ActorRole: "EnergySupplier",
        Permissions: ["calculations:manage"]);

    public ITestDiagnosticsLogger TestLogger { get; }

    public WireMockServer MockServer { get; }

    [NotNull]
    public QueueResource? WholesaleInboxQueue { get; set; }

    [NotNull]
    public FunctionAppHostManager? AppHostManager { get; private set; }

    [NotNull]
    public IDurableClient? DurableClient { get; private set; }

    public ServiceBusListenerMock ServiceBusListenerMock { get; }

    public WholesaleDatabaseManager<DatabaseContext> DatabaseManager { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    private AzuriteManager AzuriteManager { get; }

    private DurableTaskManager DurableTaskManager { get; }

    private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

    private FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

    private OpenIdJwtManager OpenIdJwtManager { get; }

    public async Task InitializeAsync()
    {
        // Clean up old Azurite storage
        CleanupAzuriteStorage();

        // Storage emulator
        AzuriteManager.StartAzurite();

        // Database
        await DatabaseManager.CreateDatabaseAsync();

        // Authentication
        OpenIdJwtManager.StartServer();

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

        // Create Wholesale Inbox service bus queue used by WholesaleInboxTrigger service bus trigger
        WholesaleInboxQueue = await ServiceBusResourceProvider
            .BuildQueue("wholesale-inbox")
            .Do(queue => appHostSettings.ProcessEnvironmentVariables
                .Add($"{WholesaleInboxQueueOptions.SectionName}__{nameof(WholesaleInboxQueueOptions.QueueName)}", queue.Name))
            .CreateAsync();

        // Create EDI Inbox service bus queue (used by WholesaleInboxTrigger to deliver messages back to EDI)
        var ediInboxResource = await ServiceBusResourceProvider
            .BuildQueue("edi-inbox")
            .Do(queue => appHostSettings.ProcessEnvironmentVariables
                .Add($"{EdiInboxQueueOptions.SectionName}__{nameof(EdiInboxQueueOptions.QueueName)}", queue.Name))
            .CreateAsync();

        // => Receive messages on EDI Inbox queue
        await ServiceBusListenerMock.AddQueueListenerAsync(ediInboxResource.Name);

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
        OpenIdJwtManager.Dispose();
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

    /// <summary>
    /// Calls the <see cref="AppHostManager"/> to send a post request to start the
    /// calculation.
    /// </summary>
    /// <returns>Calculation id of the started calculation.</returns>
    public async Task<Guid> StartCalculationAsync(bool isInternalCalculation = false)
    {
        var dateTimeZone = DateTimeZoneProviders.Tzdb["Europe/Copenhagen"];
        var dateAtMidnight = new LocalDate(2024, 5, 17)
            .AtMidnight()
            .InZoneStrictly(dateTimeZone)
            .ToDateTimeOffset();

        // Input parameters
        var requestDto = new StartCalculationRequestDto(
            CalculationType.Aggregation,
            GridAreaCodes: ["256", "512"],
            StartDate: dateAtMidnight,
            EndDate: dateAtMidnight.AddDays(2),
            ScheduledAt: DateTimeOffset.UtcNow,
            IsInternalCalculation: isInternalCalculation);

        return await StartCalculationAsync(requestDto);
    }

    public async Task<Guid> StartCalculationAsync(StartCalculationRequestDto requestDto)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/StartCalculation");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(requestDto),
            Encoding.UTF8,
            "application/json");

        request.Headers.Authorization = await CreateInternalTokenAuthenticationHeaderForEnergySupplierAsync();

        using var startCalculationResponse = await AppHostManager.HttpClient.SendAsync(request);
        startCalculationResponse.EnsureSuccessStatusCode();

        return await startCalculationResponse.Content.ReadFromJsonAsync<Guid>();
    }

    /// <summary>
    /// Calls the <see cref="OpenIdJwtManager"/> on to create an "internal token"
    /// and returns a 'Bearer' authentication header.
    /// </summary>
    private Task<AuthenticationHeaderValue> CreateInternalTokenAuthenticationHeaderForEnergySupplierAsync()
    {
        // Fake claims
        var actorNumberClaim = new Claim("actornumber", UserInformation.ActorNumber);
        var actorRoleClaim = new Claim("marketroles", UserInformation.ActorRole);

        return OpenIdJwtManager.JwtProvider.CreateInternalTokenAuthenticationHeaderAsync(
            userId: UserInformation.UserId.ToString(),
            actorId: UserInformation.ActorId.ToString(),
            roles: UserInformation.Permissions,
            extraClaims: [actorNumberClaim, actorRoleClaim]);
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
        appHostSettings.ProcessEnvironmentVariables.Add(
            "DURABLETASK_STORAGE_CONNECTION_STRING",
            AzuriteManager.FullConnectionString);

        // => Authentication
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}__{nameof(UserAuthenticationOptions.MitIdExternalMetadataAddress)}",
            OpenIdJwtManager.ExternalMetadataAddress);
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}__{nameof(UserAuthenticationOptions.ExternalMetadataAddress)}",
            OpenIdJwtManager.ExternalMetadataAddress);
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}__{nameof(UserAuthenticationOptions.BackendBffAppId)}",
            OpenIdJwtManager.TestBffAppId);
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}__{nameof(UserAuthenticationOptions.InternalMetadataAddress)}",
            OpenIdJwtManager.InternalMetadataAddress);

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
        // => Only SQL Statement needs Warehouse
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DatabricksSqlStatementOptions.WarehouseId),
            IntegrationTestConfiguration.DatabricksSettings.WarehouseId);
        // => Use hive_metastore (instead of unity catalog) for tests, since the tests need to create actual tables
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DeltaTableOptions.DatabricksCatalogName),
            "hive_metastore");

        // DataLake
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DataLakeOptions.STORAGE_ACCOUNT_URI),
            AzuriteManager.BlobStorageServiceUri.ToString());
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DataLakeOptions.STORAGE_CONTAINER_NAME),
            "wholesale");

        // Dead-letter logging
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{BlobDeadLetterLoggerOptions.SectionName}__{nameof(BlobDeadLetterLoggerOptions.StorageAccountUrl)}",
            AzuriteManager.BlobStorageServiceUri.OriginalString);
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{BlobDeadLetterLoggerOptions.SectionName}__{nameof(BlobDeadLetterLoggerOptions.ContainerName)}",
            "wholesale-orchestrations");

        // ServiceBus connection strings
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{ServiceBusNamespaceOptions.SectionName}__{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}",
            ServiceBusResourceProvider.FullyQualifiedNamespace);

        // Override default CalculationJob status monitor configuration
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{CalculationOrchestrationMonitorOptions.SectionName}__{nameof(CalculationOrchestrationMonitorOptions.CalculationJobStatusPollingIntervalInSeconds)}",
            "3");
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{CalculationOrchestrationMonitorOptions.SectionName}__{nameof(CalculationOrchestrationMonitorOptions.CalculationJobStatusExpiryTimeInSeconds)}",
            "20");
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{CalculationOrchestrationMonitorOptions.SectionName}__{nameof(CalculationOrchestrationMonitorOptions.MessagesEnqueuingExpiryTimeInSeconds)}",
            "20");

        // Audit log
        var auditLogUrl = $"{MockServer.Url}{AuditLogWireMockExtensions.AuditLogUrlSuffix}";
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{nameof(RevisionLogOptions)}__{nameof(RevisionLogOptions.ApiAddress)}",
            auditLogUrl);

        // Disable timer trigger (should be manually triggered in tests)
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"AzureWebJobs.{OutboxPublisher.FunctionName}.Disabled",
            "true");

        return appHostSettings;
    }

    /// <summary>
    /// Create storage container.
    /// Note: Azurite is based on the Blob Storage API, but sinceData Lake Storage Gen2 is built on top of it,
    /// we can still create the container like this.
    /// </summary>
    private async Task EnsureCalculationStorageContainerExistsAsync()
    {
        // Uses BlobStorageConnectionString instead of Uri and DefaultAzureCredential for faster test execution
        // (new DefaultAzureCredential() takes >30 seconds to check credentials)
        var dataLakeServiceClient = new DataLakeServiceClient(AzuriteManager.BlobStorageConnectionString);
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient("wholesale");
        if (!await fileSystemClient.ExistsAsync())
            await fileSystemClient.CreateAsync();
    }

    private async Task EnsureSettlementReportStorageContainerExistsAsync()
    {
        // Uses BlobStorageConnectionString instead of Uri and DefaultAzureCredential for faster test execution
        // (new DefaultAzureCredential() takes >30 seconds to check credentials)
        var blobClient = new BlobServiceClient(AzuriteManager.BlobStorageConnectionString);
        var blobContainerClient = blobClient.GetBlobContainerClient("settlement-report-container");
        var containerExists = await blobContainerClient.ExistsAsync();
        if (!containerExists)
            await blobContainerClient.CreateAsync();
    }

    /// <summary>
    /// Cleanup Azurite storage to avoid situations where Durable Functions
    /// would otherwise continue working on old orchestrations that e.g. failed in
    /// previous runs.
    /// </summary>
    private void CleanupAzuriteStorage()
    {
        if (Directory.Exists("__blobstorage__"))
            Directory.Delete("__blobstorage__", true);

        if (Directory.Exists("__queuestorage__"))
            Directory.Delete("__queuestorage__", true);

        if (Directory.Exists("__tablestorage__"))
            Directory.Delete("__tablestorage__", true);

        if (File.Exists("__azurite_db_blob__.json"))
            File.Delete("__azurite_db_blob__.json");

        if (File.Exists("__azurite_db_blob_extent__.json"))
            File.Delete("__azurite_db_blob_extent__.json");

        if (File.Exists("__azurite_db_queue__.json"))
            File.Delete("__azurite_db_queue__.json");

        if (File.Exists("__azurite_db_queue_extent__.json"))
            File.Delete("__azurite_db_queue_extent__.json");

        if (File.Exists("__azurite_db_table__.json"))
            File.Delete("__azurite_db_table__.json");

        if (File.Exists("__azurite_db_table_extent__.json"))
            File.Delete("__azurite_db_table_extent__.json");
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
