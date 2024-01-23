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
using System.Globalization;
using Azure;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents;
using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
using Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.States;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Extensions;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.LazyFixture;
using Google.Protobuf.WellKnownTypes;
using Test.Core;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Calculations.Fixtures
{
    public sealed class CalculationScenarioFixture : LazyFixtureBase
    {
        private readonly string _subscriptionName = Guid.NewGuid().ToString();

        public CalculationScenarioFixture(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            Configuration = new WholesaleSubsystemConfiguration();
            ServiceBusAdministrationClient = new ServiceBusAdministrationClient(Configuration.ServiceBus.FullyQualifiedNamespace, new DefaultAzureCredential());
            ServiceBusClient = CreateServiceBusClient(Configuration.ServiceBus.ConnectionString);
            ScenarioState = new CalculationScenarioState();
            LogsQueryClient = new LogsQueryClient(new DefaultAzureCredential());
        }

        public CalculationScenarioState ScenarioState { get; }

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        private WholesaleClient_V3 WholesaleClient { get; set; } = null!;

        /// <summary>
        /// The actual client is not created until <see cref="OnInitializeAsync"/> has been called by the base class.
        /// </summary>
        private ServiceBusReceiver Receiver { get; set; } = null!;

        private WholesaleSubsystemConfiguration Configuration { get; }

        private ServiceBusAdministrationClient ServiceBusAdministrationClient { get; }

        private ServiceBusClient ServiceBusClient { get; }

        private LogsQueryClient LogsQueryClient { get; }

        public async Task<Guid> StartCalculationAsync(BatchRequestDto calculationInput)
        {
            var calculationId = await WholesaleClient.CreateBatchAsync(calculationInput);
            DiagnosticMessageSink.WriteDiagnosticMessage($"Calculation for {calculationInput.ProcessType} with id '{calculationId}' started.");

            return calculationId;
        }

        public async Task<(bool IsState, BatchDto? Batch)> WaitForCalculationStateAsync(
            Guid calculationId,
            BatchState waitForState,
            TimeSpan waitTimeLimit)
        {
            var delay = TimeSpan.FromSeconds(30);

            BatchDto? batch = null;
            var isState = await Awaiter.TryWaitUntilConditionAsync(
                async () =>
                {
                    batch = await WholesaleClient.GetBatchAsync(calculationId);
                    return batch?.ExecutionState == waitForState;
                },
                waitTimeLimit,
                delay);

            DiagnosticMessageSink.WriteDiagnosticMessage($"Wait for calculation with id '{calculationId}' completed with '{nameof(isState)}={isState}'.");

            return (isState, batch);
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
                    var result = ShouldCollectMessage(messageOrNull, calculationId, integrationEventNames);
                    if (result.ShouldCollect)
                    {
                        collectedIntegrationEvents.Add(result.EventMessage!);
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
        /// Expects the first row to be a specific header to ensure we read data from the correct columns.
        /// </summary>
        /// <param name="testFileName">Filename of file located in 'TestData' folder.</param>
        public async Task<IReadOnlyCollection<AmountPerChargeResultProducedV1.Types.TimeSeriesPoint>> ParseTimeSeriesPointsFromCsvAsync(string testFileName)
        {
            const string ExpectedHeader = "grid_area;energy_supplier_id;quantity;time;price;amount;charge_code;";

            using var stream = EmbeddedResources.GetStream<Root>("Features.Calculations.TestData.amount_for_es_for_hourly_tarif_40000_for_e17_e02.csv");
            using var reader = new StreamReader(stream);

            var hasVerifiedHeader = false;
            var parsedTimeSeriesPoints = new List<AmountPerChargeResultProducedV1.Types.TimeSeriesPoint>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!hasVerifiedHeader)
                {
                    if (line != ExpectedHeader)
                    {
                        throw new Exception($"Cannot parse CSV file. Header is '{line}', expected '{ExpectedHeader}'.");
                    }

                    hasVerifiedHeader = true;
                    continue;
                }

                var columns = line!.Split(';');
                parsedTimeSeriesPoints.Add(new()
                {
                    Time = ParseTimestamp(columns[3]),
                    Quantity = ParseDecimalValue(columns[2]),
                    Price = ParseDecimalValue(columns[4]),
                    Amount = ParseDecimalValue(columns[5]),
                });
            }

            return parsedTimeSeriesPoints;
        }

        public async Task<IReadOnlyCollection<EnergyResultProducedV2.Types.TimeSeriesPoint>> ParseTimeSeriesPointsFromEnergyResultProducedCsvAsync(string testFileName)
        {
            const string expectedHeader = "grid_area,energy_supplier_id,balance_responsible_id,quantity,quantity_qualities,time,aggregation_level,time_series_type,calculation_id,calculation_type,calculation_execution_time_start,out_grid_area,calculation_result_id";
            var hasVerifiedHeader = false;
            await using var stream = EmbeddedResources.GetStream<Root>($"Features.Calculations.TestData.{testFileName}");
            using var reader = new StreamReader(stream);
            var parsedTimeSeriesPoints = new List<EnergyResultProducedV2.Types.TimeSeriesPoint>();

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

                var columns = line!.Split(',');
                var result = new EnergyResultProducedV2.Types.TimeSeriesPoint
                {
                    Time = ParseTimestamp(columns[5]),
                    Quantity = ParseDecimalValue(columns[3]),
                };
                result.QuantityQualities.AddRange(ParseEnumValueTo(columns[4]));
                parsedTimeSeriesPoints.Add(result);
            }

            return parsedTimeSeriesPoints;
        }

        public async Task<IReadOnlyCollection<T>> ParseCsvAsync<T>(string testFileName, string expectedHeader, Func<string[], T> createResult)
        {
            var hasVerifiedHeader = false;

            using var stream = EmbeddedResources.GetStream<Root>($"Features.Calculations.TestData.{testFileName}");
            using var reader = new StreamReader(stream);

            var parsedTimeSeriesPoints = new List<T>();

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

                var columns = line!.Split(',');

                // Use the provided function to create the result object
                var result = createResult(columns);

                parsedTimeSeriesPoints.Add(result);
            }

            return parsedTimeSeriesPoints;
        }

        protected override async Task OnInitializeAsync()
        {
            await DatabricksClientExtensions.StartWarehouseAsync(Configuration.DatabricksWorkspace);
            WholesaleClient = await WholesaleClientFactory.CreateAsync(Configuration, useAuthentication: true);
            await CreateTopicSubscriptionAsync();
            Receiver = ServiceBusClient.CreateReceiver(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName);
        }

        protected override async Task OnDisposeAsync()
        {
            await ServiceBusAdministrationClient.DeleteSubscriptionAsync(Configuration.ServiceBus.SubsystemRelayTopicName, _subscriptionName);
            await ServiceBusClient.DisposeAsync();
        }

        /// <summary>
        /// We configure the client to use <see cref="ServiceBusTransportType.AmqpWebSockets"/> to be able to
        /// pass through the firewall, as it blocks the AMQP ports.
        /// See "https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-faq#what-ports-do-i-need-to-open-on-the-firewall--"
        /// </summary>
        private static ServiceBusClient CreateServiceBusClient(string connectionString)
        {
            return new ServiceBusClient(
                connectionString,
                new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets,
                });
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
                    case CalculationResultCompleted.EventName:
                        var calculationResultCompleted = CalculationResultCompleted.Parser.ParseFrom(data);
                        if (calculationResultCompleted.BatchId == calculationId.ToString())
                        {
                            eventMessage = calculationResultCompleted;
                            shouldCollect = true;
                        }

                        break;
                    case EnergyResultProducedV2.EventName:
                        var energyResultProduced = EnergyResultProducedV2.Parser.ParseFrom(data);
                        if (energyResultProduced.CalculationId == calculationId.ToString())
                        {
                            eventMessage = energyResultProduced;
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

        private static IEnumerable<Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2.Types.QuantityQuality> ParseEnumValueTo(string value)
        {
            value = value.Replace("[", string.Empty).Replace("]", string.Empty).Replace("'", string.Empty);
            var splits = value.Split(',');
            var result = new List<Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2.Types.QuantityQuality>();
            foreach (var split in splits)
            {
                if (split == "measured")
                {
                    result.Add(EnergyResultProducedV2.Types.QuantityQuality.Measured);
                }
            }

            return result;
        }
    }
}
