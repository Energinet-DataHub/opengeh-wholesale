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

using System.Text;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Energinet.DataHub.Wholesale.ProcessManager.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestHelpers;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Xunit.Abstractions;
using ProcessType = Energinet.DataHub.Wholesale.Contracts.ProcessType;

namespace Energinet.DataHub.Wholesale.ProcessManager.IntegrationTests;

public class PublishIntegrationEventsEndpointTests
{
    [Collection(nameof(ProcessManagerFunctionAppCollectionFixture))]
    public class RunAsync : FunctionAppTestBase<ProcessManagerFunctionAppFixture>, IAsyncLifetime
    {
        public RunAsync(ProcessManagerFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        public Task InitializeAsync()
        {
            Fixture.BatchCompletedListener.Reset();
            return Task.CompletedTask;
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task When_ProcessCompletedDomainEventPublished_Then_ProcessCompletedIntegrationEventPublished()
        {
            // Arrange
            var dbc = Fixture.DatabaseManager.CreateDbContext();
            var calculationResultCompleted = new CalculationResultCompleted()
            {
                Resolution = Resolution.Quarter,
                BatchId = Guid.NewGuid().ToString(),
                ProcessType = Contracts.Events.ProcessType.BalanceFixing,
                QuantityUnit = QuantityUnit.Kwh,
                AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea = new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = "543",
                    EnergySupplierGlnOrEic = "1234567890123456",
                    BalanceResponsiblePartyGlnOrEic = "1234567890123456",
                },
                PeriodStartUtc = Timestamp.FromDateTime(DateTime.UtcNow),
                PeriodEndUtc = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(1)),
                TimeSeriesType = TimeSeriesType.Production,
            };
            var sre = new JsonSerializer();
            var body = sre.Serialize(calculationResultCompleted);
            var bytes = Encoding.UTF8.GetBytes(body);

            dbc.OutboxMessages.Add(new OutboxMessage(new IntegrationEventDto(CalculationResultCompleted.BalanceFixingEventName, System.Text.Encoding.UTF8.GetString(bytes), DateTime.UtcNow.ToInstant())));
            await dbc.SaveChangesAsync().ConfigureAwait(false);

            using var eventualProcessCompletedIntegrationEvent = await Fixture
                .ProcessCompletedIntegrationEventListener.ListenForMessageAsync(CalculationResultCompleted.BalanceFixingEventName);

            // Act & Assert
            var isProcessCompletedIntegrationEventPublished = eventualProcessCompletedIntegrationEvent
                .MessageAwaiter!
                .Wait(TimeSpan.FromSeconds(40));
            isProcessCompletedIntegrationEventPublished.Should().BeTrue();
        }

        private static readonly Random _generator = new();

        /// <summary>
        /// Create a grid area code with valid format.
        /// </summary>
        private static string CreateGridAreaCode() => _generator.Next(100, 1000).ToString();

        private static ServiceBusMessage CreateProcessCompletedEventDtoMessage()
        {
            var messageType = EnvironmentVariableHelper.GetEnvVariable(EnvironmentSettingNames.ProcessCompletedEventName);
            var processCompleted = new ProcessCompletedEventDto(
                CreateGridAreaCode(),
                Guid.NewGuid(),
                ProcessType.BalanceFixing,
                Periods.January_EuropeCopenhagen_Instant.PeriodStart,
                Periods.January_EuropeCopenhagen_Instant.PeriodEnd);
            var someCorrelationContextId = Guid.NewGuid().ToString();

            var body = new JsonSerializer().Serialize(processCompleted);
            var bytes = Encoding.UTF8.GetBytes(body);
            return ServiceBusMessageFactory.CreateServiceBusMessage(bytes, messageType, someCorrelationContextId);
        }
    }
}
