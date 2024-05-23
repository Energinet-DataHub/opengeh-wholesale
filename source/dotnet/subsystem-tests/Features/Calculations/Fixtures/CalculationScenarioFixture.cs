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
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures;

public sealed class CalculationScenarioFixture : LazyFixtureBase
{
    private readonly string _subscriptionName = Guid.NewGuid().ToString();

    public CalculationScenarioFixture(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
        Configuration = new WholesaleSubsystemConfiguration();
        ServiceBusAdministrationClient = new ServiceBusAdministrationClient(Configuration.ServiceBus.FullyQualifiedNamespace, new DefaultAzureCredential());
        ServiceBusClient = new ServiceBusClient(Configuration.ServiceBus.ConnectionString);
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
    private WholesaleClient_V3 WholesaleWebApiClient { get; set; } = null!;

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private HttpClient WholesaleOrchestrationsApiClient { get; set; } = null!;

    /// <summary>
    /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
    /// </summary>
    private ServiceBusReceiver Receiver { get; set; } = null!;

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
                    calculation?.ExecutionState is CalculationState.Completed
                    or CalculationState.Failed;
            },
            waitTimeLimit,
            delay);

        DiagnosticMessageSink.WriteDiagnosticMessage($"Wait for calculation with id '{calculationId}' completed with '{nameof(calculation.ExecutionState)}={calculation?.ExecutionState}'.");

        return (isCompletedOrFailed, calculation);
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
            var messageOrNull = await Receiver.ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(1));
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
                await Receiver.CompleteMessageAsync(messageOrNull);
            }
        }

        stopwatch.Stop();
        DiagnosticMessageSink.WriteDiagnosticMessage($"""
            Message receiver loop for calculation with id '{calculationId}' took '{stopwatch.Elapsed}' to complete.
            It was listening for messages on entity path '{Receiver.EntityPath}', and collected '{collectedIntegrationEvents.Count}' messages spanning various event types.
            """);

        return collectedIntegrationEvents;
    }

    public async Task<Response<LogsQueryResult>> QueryLogAnalyticsAsync(string query, QueryTimeRange queryTimeRange)
    {
        return await LogsQueryClient.QueryWorkspaceAsync(Configuration.LogAnalyticsWorkspaceId, query, queryTimeRange);
    }

    /// <summary>
    /// Load CSV file and parse each data row into <see cref="AmountPerChargeResultProducedV1.Types.TimeSeriesPoint"/>.
    /// </summary>
    public async Task<IReadOnlyCollection<AmountPerChargeResultProducedV1.Types.TimeSeriesPoint>> ParseChargeResultTimeSeriesPointsFromCsvAsync(string testFileName)
    {
        return await ParseCsvAsync(
            testFileName,
            "grid_area_code,energy_supplier_id,quantity,time,price,amount,charge_code",
            ParseAmountPerChargeResultProducedV1TimeSeriesPoint);
    }

    /// <summary>
    /// Load CSV file and parse each data row into <see cref="EnergyResultProducedV2.Types.TimeSeriesPoint"/>.
    /// </summary>
    public async Task<IReadOnlyCollection<EnergyResultProducedV2.Types.TimeSeriesPoint>> ParseEnergyResultTimeSeriesPointsFromCsvAsync(string testFileName)
    {
        return await ParseCsvAsync(
            testFileName,
            "grid_area_code,energy_supplier_id,balance_responsible_id,quantity,quantity_qualities,time,aggregation_level,time_series_type,calculation_id,calculation_type,calculation_execution_time_start,out_grid_area_code,calculation_result_id,resolution",
            ParseEnergyResultProducedV2TimeSeriesPoint);
    }

    /// <summary>
    /// Load CSV file and parse each data line into a <see cref="GridLossResultProducedV1.Types.TimeSeriesPoint"/>.
    /// </summary>
    public async Task<IReadOnlyCollection<GridLossResultProducedV1.Types.TimeSeriesPoint>> ParseGridLossTimeSeriesPointsFromCsvAsync(string testFileName)
    {
        return await ParseCsvAsync(
            testFileName,
            "grid_area_code,energy_supplier_id,balance_responsible_id,quantity,quantity_qualities,time,aggregation_level,time_series_type,calculation_id,calculation_type,calculation_execution_time_start,out_grid_area_code,calculation_result_id,metering_point_id,resolution",
            ParseGridLossProducedV1TimeSeriesPoint);
    }

    public async Task<IReadOnlyList<(bool IsAccessible, string ErrorMessage)>> ArePublicDataModelsAccessibleAsync(
        IReadOnlyList<(string Name, string TableName)> modelsAndTables)
    {
        var results = new ConcurrentBag<(bool IsAccessible, string ErrorMessage)>();
        var tasks = modelsAndTables.Select(async model =>
        {
            try
            {
                var statement = DatabricksStatement.FromRawSql($"SELECT * FROM {model.Name}.{model.TableName} LIMIT 1");
                var result = DatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(statement.Build());
                var list = await result.ToListAsync();
                if (list.Count == 0)
                {
                    results.Add(new(
                        false,
                        $"Table '{model.TableName}' in model '{model.Name}' doesn't contain data."));
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
                    $"Table '{model.TableName}' in model '{model.Name}' is missing. Exception: {e.Message}"));
            }
        }).ToList();

        await Task.WhenAll(tasks);
        return results.ToList();
    }

    protected override async Task OnInitializeAsync()
    {
        await DatabricksClientExtensions.StartWarehouseAsync(Configuration.DatabricksWorkspace);
        WholesaleWebApiClient = await WholesaleClientFactory.CreateWebApiClientAsync(Configuration, useAuthentication: true);
        WholesaleOrchestrationsApiClient = await WholesaleClientFactory.CreateOrchestrationsApiClientAsync(Configuration, useAuthentication: true);
        await CreateTopicSubscriptionAsync();
        Receiver = ServiceBusClient.CreateReceiver(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName);
    }

    protected override async Task OnDisposeAsync()
    {
        await ServiceBusAdministrationClient.DeleteSubscriptionAsync(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName);
        await ServiceBusClient.DisposeAsync();
        WholesaleOrchestrationsApiClient.Dispose();
    }

    /// <summary>
    /// Load CSV file and parse each data row into T./>.
    /// Expects the first row to be a specific header to ensure we read data from the correct columns.
    /// </summary>
    /// <param name="testFileName">Filename of file located in 'TestData' folder.</param>
    /// <param name="expectedHeader">The expected headers in the CVS file.</param>
    /// <param name="createResult">Delegate function that create an specific result object.</param>
    private static async Task<IReadOnlyCollection<T>> ParseCsvAsync<T>(string testFileName, string expectedHeader, Func<string[], T> createResult)
    {
        var hasVerifiedHeader = false;
        await using var stream = EmbeddedResources.GetStream<Root>($"Features.Calculations.TestData.{testFileName}");
        using var reader = new StreamReader(stream);

        var resultList = new List<T>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (!hasVerifiedHeader)
            {
                if (line != expectedHeader)
                {
                    throw new Exception($"Cannot parse CSV file. Header is '{line}', expected '{expectedHeader}'.");
                }

                hasVerifiedHeader = true;
                continue;
            }

            var columns = line!.Split(',', ';');
            var result = createResult(columns);
            resultList.Add(result);
        }

        return resultList;
    }

    private static AmountPerChargeResultProducedV1.Types.TimeSeriesPoint ParseAmountPerChargeResultProducedV1TimeSeriesPoint(IReadOnlyList<string> columns)
    {
        return new AmountPerChargeResultProducedV1.Types.TimeSeriesPoint
        {
            Time = ParseTimestamp(columns[3]),
            Quantity = ParseDecimalValue(columns[2]),
            Price = ParseDecimalValue(columns[4]),
            Amount = ParseDecimalValue(columns[5]),
        };
    }

    private static GridLossResultProducedV1.Types.TimeSeriesPoint ParseGridLossProducedV1TimeSeriesPoint(string[] columns)
    {
        var result = new GridLossResultProducedV1.Types.TimeSeriesPoint
        {
            Time = ParseTimestamp(columns[5]),
            Quantity = ParseDecimalValue(columns[3]),
        };
        return result;
    }

    private static EnergyResultProducedV2.Types.TimeSeriesPoint ParseEnergyResultProducedV2TimeSeriesPoint(string[] columns)
    {
        var result = new EnergyResultProducedV2.Types.TimeSeriesPoint
        {
            Time = ParseTimestamp(columns[5]),
            Quantity = ParseDecimalValue(columns[3]),
        };
        result.QuantityQualities.AddRange(ParseEnumValueTo(columns[4]));
        return result;
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
                case EnergyResultProducedV2.EventName:
                    var energyResultProduced = EnergyResultProducedV2.Parser.ParseFrom(data);
                    if (energyResultProduced.CalculationId == calculationId.ToString())
                    {
                        eventMessage = energyResultProduced;
                        shouldCollect = true;
                    }

                    break;
                case GridLossResultProducedV1.EventName:
                    var gridLossResultProduced = GridLossResultProducedV1.Parser.ParseFrom(data);
                    if (gridLossResultProduced.CalculationId == calculationId.ToString())
                    {
                        eventMessage = gridLossResultProduced;
                        shouldCollect = true;
                    }

                    break;
                case AmountPerChargeResultProducedV1.EventName:
                    var amountPerChargeResultProduced = AmountPerChargeResultProducedV1.Parser.ParseFrom(data);
                    if (amountPerChargeResultProduced.CalculationId == calculationId.ToString())
                    {
                        DiagnosticMessageSink.WriteDiagnosticMessage($"""
                            {nameof(AmountPerChargeResultProducedV1)} received with values:
                                {nameof(amountPerChargeResultProduced.CalculationId)}={amountPerChargeResultProduced.CalculationId}
                                {nameof(amountPerChargeResultProduced.GridAreaCode)}={amountPerChargeResultProduced.GridAreaCode}
                                {nameof(amountPerChargeResultProduced.EnergySupplierId)}={amountPerChargeResultProduced.EnergySupplierId}
                                {nameof(amountPerChargeResultProduced.ChargeCode)}={amountPerChargeResultProduced.ChargeCode}
                                {nameof(amountPerChargeResultProduced.ChargeType)}={amountPerChargeResultProduced.ChargeType}
                                {nameof(amountPerChargeResultProduced.ChargeOwnerId)}={amountPerChargeResultProduced.ChargeOwnerId}
                                {nameof(amountPerChargeResultProduced.SettlementMethod)}={amountPerChargeResultProduced.SettlementMethod}
                            """);
                        eventMessage = amountPerChargeResultProduced;
                        shouldCollect = true;
                    }

                    break;
                case MonthlyAmountPerChargeResultProducedV1.EventName:
                    var monthlyAmountPerChargeResultProduced = MonthlyAmountPerChargeResultProducedV1.Parser.ParseFrom(data);
                    if (monthlyAmountPerChargeResultProduced.CalculationId == calculationId.ToString())
                    {
                        DiagnosticMessageSink.WriteDiagnosticMessage($"""
                            {nameof(MonthlyAmountPerChargeResultProducedV1)} received with values:
                                {nameof(monthlyAmountPerChargeResultProduced.CalculationId)}={monthlyAmountPerChargeResultProduced.CalculationId}
                                {nameof(monthlyAmountPerChargeResultProduced.GridAreaCode)}={monthlyAmountPerChargeResultProduced.GridAreaCode}
                                {nameof(monthlyAmountPerChargeResultProduced.EnergySupplierId)}={monthlyAmountPerChargeResultProduced.EnergySupplierId}
                                {nameof(monthlyAmountPerChargeResultProduced.ChargeCode)}={monthlyAmountPerChargeResultProduced.ChargeCode}
                                {nameof(monthlyAmountPerChargeResultProduced.ChargeType)}={monthlyAmountPerChargeResultProduced.ChargeType}
                                {nameof(monthlyAmountPerChargeResultProduced.ChargeOwnerId)}={monthlyAmountPerChargeResultProduced.ChargeOwnerId}
                                {nameof(monthlyAmountPerChargeResultProduced.Amount)}={monthlyAmountPerChargeResultProduced.Amount}
                            """);
                        eventMessage = monthlyAmountPerChargeResultProduced;
                        shouldCollect = true;
                    }

                    break;
                case TotalMonthlyAmountResultProducedV1.EventName:
                    var totalMonthlyAmountResultProduced = TotalMonthlyAmountResultProducedV1.Parser.ParseFrom(data);
                    if (totalMonthlyAmountResultProduced.CalculationId == calculationId.ToString())
                    {
                        DiagnosticMessageSink.WriteDiagnosticMessage($"""
                            {nameof(TotalMonthlyAmountResultProducedV1)} received with values:
                                {nameof(monthlyAmountPerChargeResultProduced.CalculationId)}={totalMonthlyAmountResultProduced.CalculationId}
                                {nameof(monthlyAmountPerChargeResultProduced.GridAreaCode)}={totalMonthlyAmountResultProduced.GridAreaCode}
                                {nameof(monthlyAmountPerChargeResultProduced.EnergySupplierId)}={totalMonthlyAmountResultProduced.EnergySupplierId}
                                {nameof(monthlyAmountPerChargeResultProduced.ChargeOwnerId)}={totalMonthlyAmountResultProduced.ChargeOwnerId}
                                {nameof(monthlyAmountPerChargeResultProduced.Amount)}={totalMonthlyAmountResultProduced.Amount}
                            """);
                        eventMessage = totalMonthlyAmountResultProduced;
                        shouldCollect = true;
                    }

                    break;
            }
        }

        return (shouldCollect, eventMessage);
    }

    private static Timestamp ParseTimestamp(string value)
    {
        return DateTimeOffset.Parse(value, null, DateTimeStyles.AssumeUniversal).ToTimestamp();
    }

    private static Contracts.IntegrationEvents.Common.DecimalValue? ParseDecimalValue(string value)
    {
        return string.IsNullOrEmpty(value) ? null : new Contracts.IntegrationEvents.Common.DecimalValue(decimal.Parse(value, CultureInfo.InvariantCulture));
    }

    private static List<EnergyResultProducedV2.Types.QuantityQuality> ParseEnumValueTo(string value)
    {
        value = value.Replace("[", string.Empty).Replace("]", string.Empty).Replace("'", string.Empty);
        var splits = value.Split(':');
        var result = new List<EnergyResultProducedV2.Types.QuantityQuality>();
        foreach (var split in splits)
        {
            switch (split)
            {
                case "measured":
                    result.Add(EnergyResultProducedV2.Types.QuantityQuality.Measured);
                    break;
                case "calculated":
                    result.Add(EnergyResultProducedV2.Types.QuantityQuality.Calculated);
                    break;
                case "missing":
                    result.Add(EnergyResultProducedV2.Types.QuantityQuality.Missing);
                    break;
            }
        }

        return result;
    }
}
