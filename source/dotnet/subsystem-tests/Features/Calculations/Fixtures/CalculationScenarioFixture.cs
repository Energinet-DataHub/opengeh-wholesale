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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using Azure;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.EnergySupplying.RequestResponse.InboxEvents;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using Energinet.DataHub.Wholesale.Test.Core;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures;

public sealed class CalculationScenarioFixture : LazyFixtureBase
{
    private readonly string _subscriptionName = Guid.NewGuid().ToString();
    private long? _latestCalculationVersion = null;

    public CalculationScenarioFixture(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        Configuration = new WholesaleSubsystemConfiguration();
        var credential = new DefaultAzureCredential();
        ServiceBusAdministrationClient = new ServiceBusAdministrationClient(Configuration.ServiceBus.FullyQualifiedNamespace, credential);
        ServiceBusClient = new ServiceBusClient(Configuration.ServiceBus.FullyQualifiedNamespace, credential);
        ScenarioState = new CalculationScenarioState();
        LogsQueryClient = new LogsQueryClient(new DefaultAzureCredential());
        DatabricksSqlWarehouseQueryExecutor = GetDatabricksSqlWarehouseQueryExecutor();
    }

    private DatabricksSqlWarehouseQueryExecutor GetDatabricksSqlWarehouseQueryExecutor()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "WorkspaceUrl", Configuration.DatabricksWorkspace.BaseUrl },
            { "WorkspaceToken", Configuration.DatabricksWorkspace.Token },
            { "WarehouseId", Configuration.DatabricksWorkspace.WarehouseId },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDatabricksSqlStatementExecution(configuration);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider.GetRequiredService<DatabricksSqlWarehouseQueryExecutor>();
    }

    public CalculationScenarioState ScenarioState { get; }

    public DatabricksSqlWarehouseQueryExecutor DatabricksSqlWarehouseQueryExecutor { get; set; }

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private ServiceBusSender WholesaleInboxSender { get; set; } = null!;

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private WholesaleClient_V3 WholesaleWebApiClient { get; set; } = null!;

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private HttpClient WholesaleOrchestrationsApiClient { get; set; } = null!;

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private ServiceBusReceiver IntegrationEventReceiver { get; set; } = null!;

    private WholesaleSubsystemConfiguration Configuration { get; }

    private ServiceBusAdministrationClient ServiceBusAdministrationClient { get; }

    private ServiceBusClient ServiceBusClient { get; }

    private LogsQueryClient LogsQueryClient { get; }

    public async Task<Guid> StartCalculationAsync(StartCalculationRequestDto calculationInput)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/StartCalculation");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(calculationInput),
            Encoding.UTF8,
            "application/json");

        using var actualResponse = await WholesaleOrchestrationsApiClient.SendAsync(request);
        actualResponse.EnsureSuccessStatusCode();
        var calculationId = await actualResponse.Content.ReadFromJsonAsync<Guid>();

        DiagnosticMessageSink.WriteDiagnosticMessage($"Calculation for {calculationInput.CalculationType} with id '{calculationId}' started.");

        return calculationId;
    }

    /// <summary>
    /// Wait for the calculation to complete or fail.
    /// </summary>
    /// <returns>IsCompletedOrFailed: True if the calculation completed or failed; otherwise false.</returns>
    public async Task<(bool IsCompletedOrFailed, CalculationDto? Calculation)> WaitForCalculationCompletedOrFailedAsync(
        Guid calculationId,
        TimeSpan waitTimeLimit)
    {
        var delay = TimeSpan.FromSeconds(30);

        CalculationDto? calculation = null;
        var isCompletedOrFailed = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                calculation = await WholesaleWebApiClient.GetCalculationAsync(calculationId);
                return
                    calculation != null && (
                        calculation.OrchestrationState.IsCalculationJobCompleted() ||
                        calculation.OrchestrationState is CalculationOrchestrationState.CalculationFailed);
            },
            waitTimeLimit,
            delay);

        DiagnosticMessageSink.WriteDiagnosticMessage(
            $"Wait for calculation with id '{calculationId}' to be completed/failed finished with '{nameof(isCompletedOrFailed)}={isCompletedOrFailed}', '{nameof(calculation.OrchestrationState)}={calculation?.OrchestrationState}'.");

        return (isCompletedOrFailed, calculation);
    }

    /// <summary>
    /// Wait for the calculation to be started
    /// </summary>
    /// <returns>isStarted: True if the calculation was started within the time limit; otherwise false.</returns>
    public async Task<(bool IsStarted, CalculationDto? Calculation)> WaitForScheduledCalculationToStartAsync(
        Guid calculationId,
        TimeSpan waitTimeLimit,
        TimeSpan checkInterval)
    {
        CalculationDto? calculation = null;
        var isStarted = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                calculation = await WholesaleWebApiClient.GetCalculationAsync(calculationId);
                return calculation is
                {
                    // We cannot check for CalculationOrchestrationState.Started since the calculation might already be later in the orchestration
                    OrchestrationState: not CalculationOrchestrationState.Scheduled and
                                        not CalculationOrchestrationState.Canceled
                };
            },
            waitTimeLimit,
            checkInterval);

        DiagnosticMessageSink.WriteDiagnosticMessage(
            $"Wait for calculation with id '{calculationId}' to be started finished with '{nameof(isStarted)}={isStarted}', '{nameof(calculation.OrchestrationState)}={calculation?.OrchestrationState}'.");

        return (isStarted, calculation);
    }

    /// <summary>
    /// Wait for the calculation orchestration state to be one of the given states
    /// </summary>
    /// <returns>IsSuccess: true if the calculation is in one of the given states; otherwise false.</returns>
    public async Task<(bool IsSuccess, CalculationDto? Calculation)> WaitForOneOfCalculationStatesAsync(
        Guid calculationId,
        CalculationOrchestrationState[] states,
        TimeSpan waitTimeLimit)
    {
        var delay = TimeSpan.FromSeconds(30);

        CalculationDto? calculation = null;
        var isSuccess = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                calculation = await WholesaleWebApiClient.GetCalculationAsync(calculationId);
                return calculation != null && states.Contains(calculation.OrchestrationState);
            },
            waitTimeLimit,
            delay);

        var stateNames = string.Join(",", states);
        DiagnosticMessageSink.WriteDiagnosticMessage(
            $"Wait for calculation with id '{calculationId}' state to be one of [{stateNames}] finished with '{nameof(isSuccess)}={isSuccess}', '{nameof(calculation.OrchestrationState)}={calculation?.OrchestrationState}'.");

        return (isSuccess, calculation);
    }

    public async Task<IReadOnlyCollection<IEventMessage>> WaitForIntegrationEventsAsync(
        Guid calculationId,
        IReadOnlyCollection<string> integrationEventNames,
        TimeSpan waitTimeLimit)
    {
        using var cts = new CancellationTokenSource(waitTimeLimit);
        var stopwatch = Stopwatch.StartNew();

        var collectedIntegrationEvents = new List<IEventMessage>();
        while (!cts.Token.IsCancellationRequested)
        {
            var messageOrNull = await IntegrationEventReceiver.ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(1));
            if (messageOrNull?.Body == null)
            {
                if (collectedIntegrationEvents.Count > 0)
                    break;
            }
            else
            {
                var (shouldCollect, eventMessage) = ShouldCollectMessage(messageOrNull, calculationId, integrationEventNames);
                if (shouldCollect)
                {
                    collectedIntegrationEvents.Add(eventMessage!);
                }

                // We should always complete (delete) messages since we use a subscription
                // and no other receiver is using the same, so we will never by mistake
                // interfere with other scenarios or message receivers in the live environment.
                await IntegrationEventReceiver.CompleteMessageAsync(messageOrNull);
            }
        }

        stopwatch.Stop();
        DiagnosticMessageSink.WriteDiagnosticMessage($"""
            Message receiver loop for calculation with id '{calculationId}' took '{stopwatch.Elapsed}' to complete.
            It was listening for messages on entity path '{IntegrationEventReceiver.EntityPath}', and collected '{collectedIntegrationEvents.Count}' messages spanning various event types.
            """);

        return collectedIntegrationEvents;
    }

    public async Task SendActorMessagesEnqueuedMessageAsync(Guid calculationId, string orchestrationInstanceId)
    {
        var actorMessagesEnqueuedMessage = new ActorMessagesEnqueuedV1
        {
            CalculationId = calculationId.ToString(),
            OrchestrationInstanceId = orchestrationInstanceId,
            Success = true,
        };

        var serviceBusMessage = new ServiceBusMessage(actorMessagesEnqueuedMessage.ToByteArray())
        {
            Subject = ActorMessagesEnqueuedV1.EventName,
            ApplicationProperties =
            {
                { "ReferenceId", Guid.Parse("00000000-0000-0000-0000-000000000001").ToString() },
            },
        };

        // Act
        await WholesaleInboxSender.SendMessageAsync(serviceBusMessage);
    }

    public async Task<Response<LogsQueryResult>> QueryLogAnalyticsAsync(string query, QueryTimeRange queryTimeRange)
    {
        return await LogsQueryClient.QueryWorkspaceAsync(Configuration.LogAnalyticsWorkspaceId, query, queryTimeRange);
    }

    public async Task<IReadOnlyList<(bool IsAccessible, string ErrorMessage)>> ArePublicDataModelsAccessibleAsync(
        IReadOnlyList<(string ModelName, string TableName)> modelsAndTables)
    {
        var results = new ConcurrentBag<(bool IsAccessible, string ErrorMessage)>();
        var tasks = modelsAndTables.Select(async item =>
        {
            try
            {
                var statement = DatabricksStatement.FromRawSql($"SELECT * FROM {Configuration.DatabricksCatalogName}.{item.ModelName}.{item.TableName} LIMIT 1");
                var queryResult = DatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement.Build());
                var list = await queryResult.ToListAsync();
                if (list.Count == 0)
                {
                    results.Add(new(
                        false,
                        $"Table '{item.TableName}' in model '{item.ModelName}' doesn't contain data."));
                }
                else
                {
                    results.Add(new(true, string.Empty));
                }
            }
            catch (Exception e)
            {
                results.Add(new(
                    false,
                    $"Table '{item.TableName}' in model '{item.ModelName}' is missing. Exception: {e.Message}"));
            }
        }).ToList();

        await Task.WhenAll(tasks);
        return results.ToList();
    }

    public async Task<(long? CalculationVersion, string Message)> GetCalculationVersionOfCalculationIdFromCalculationsAsync(
        Guid calculationId)
    {
        try
        {
            var statement = DatabricksStatement.FromRawSql(
                $"SELECT calculation_version FROM {Configuration.DatabricksCatalogName}.wholesale_internal.calculations WHERE calculation_id = '{calculationId}'");
            var queryResult = DatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement.Build());
            var item = await queryResult.FirstAsync();

            if (item.calculation_version != null)
            {
                return (item.calculation_version, "Calculation ID retrieved successfully");
            }

            return (null, "No data found in the table");
        }
        catch (Exception e)
        {
            return (null, $"An error occurred: {e.Message}");
        }
    }

    public async Task<(long? CalculationVersion, string Message)> GetLatestCalculationVersionFromCalculationsAsync()
    {
        try
        {
            var statement = DatabricksStatement.FromRawSql(
                $"SELECT calculation_version FROM {Configuration.DatabricksCatalogName}.wholesale_internal.calculations ORDER BY calculation_version DESC LIMIT 1");
            var queryResult = DatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement.Build());
            var item = await queryResult.FirstAsync();

            if (item.calculation_version != null)
            {
                _latestCalculationVersion = item.calculation_version;
                return (item.calculation_version, "Calculation version retrieved successfully");
            }

            return (null, "No data found in the table");
        }
        catch (Exception e)
        {
            return (null, $"An error occurred: {e.Message}");
        }
    }

    public long? GetLatestCalculationVersion()
    {
        return _latestCalculationVersion;
    }

    protected override async Task OnInitializeAsync()
    {
        await DatabricksClientExtensions.StartWarehouseAsync(Configuration.DatabricksWorkspace);
        WholesaleWebApiClient = await WholesaleClientFactory.CreateWebApiClientAsync(Configuration, useAuthentication: true);
        WholesaleOrchestrationsApiClient = await WholesaleClientFactory.CreateOrchestrationsApiClientAsync(Configuration, useAuthentication: true);
        await CreateTopicSubscriptionAsync();
        IntegrationEventReceiver = ServiceBusClient.CreateReceiver(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName);
        WholesaleInboxSender = ServiceBusClient.CreateSender(Configuration.ServiceBus.WholesaleInboxQueueName);
    }

    protected override async Task OnDisposeAsync()
    {
        await ServiceBusAdministrationClient.DeleteSubscriptionAsync(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName);
        await ServiceBusClient.DisposeAsync();
        WholesaleOrchestrationsApiClient.Dispose();
    }

    private async Task CreateTopicSubscriptionAsync()
    {
        if (await ServiceBusAdministrationClient.SubscriptionExistsAsync(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName))
        {
            await ServiceBusAdministrationClient.DeleteSubscriptionAsync(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName);
        }

        var options = new CreateSubscriptionOptions(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName)
        {
            AutoDeleteOnIdle = TimeSpan.FromHours(1),
        };

        await ServiceBusAdministrationClient.CreateSubscriptionAsync(options);
        DiagnosticMessageSink.WriteDiagnosticMessage($"ServiceBus subscription '{options.SubscriptionName}' created for topic '{options.TopicName}'.");
    }

    /// <summary>
    /// Returns <see langword="true"/> if we should collect the message type; otherwise <see langword="false"/> .
    /// </summary>
    private (bool ShouldCollect, IEventMessage? EventMessage) ShouldCollectMessage(ServiceBusReceivedMessage message, Guid calculationId, IReadOnlyCollection<string> integrationEventNames)
    {
        var shouldCollect = false;
        IEventMessage? eventMessage = null;

        if (integrationEventNames.Contains(message.Subject))
        {
            var data = message.Body.ToArray();

            switch (message.Subject)
            {
                case CalculationCompletedV1.EventName:
                    var calculationCompleted = CalculationCompletedV1.Parser.ParseFrom(data);
                    if (calculationCompleted.CalculationId == calculationId.ToString())
                    {
                        DiagnosticMessageSink.WriteDiagnosticMessage($"""
                            {nameof(CalculationCompletedV1)} received with values:
                                {nameof(calculationCompleted.CalculationId)}={calculationCompleted.CalculationId}
                                {nameof(calculationCompleted.CalculationType)}={calculationCompleted.CalculationType}
                                {nameof(calculationCompleted.InstanceId)}={calculationCompleted.InstanceId}
                                {nameof(calculationCompleted.CalculationVersion)}={calculationCompleted.CalculationVersion}
                            """);
                        eventMessage = calculationCompleted;
                        shouldCollect = true;
                    }

                    break;
            }
        }

        return (shouldCollect, eventMessage);
    }
}
