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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Infrastructure.Core;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.FunctionApp;
using Energinet.DataHub.Wholesale.ProcessManager;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.ProcessManager;

public class PublishProcessCompletedIntegrationEventEndpointTests
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
            var processCompletedMessage = CreateProcessCompletedEventDtoMessage();
            using var eventualProcessCompletedIntegrationEvent = await Fixture
                .ProcessCompletedIntegrationEventListener
                .ListenForMessageAsync<ProcessCompletedEventDto>(_ => true);
                //.ListenForMessageByCorrelationIdAsync(processCompletedMessage.CorrelationId);

            // Act
            await Fixture.DomainEventsTopic.SenderClient.SendMessageAsync(processCompletedMessage);

            // Assert
            var isProcessCompletedIntegrationEventPublished = eventualProcessCompletedIntegrationEvent
                .MessageAwaiter!
                .Wait(TimeSpan.FromSeconds(20));
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
            var processCompleted = new ProcessCompletedEventDto(CreateGridAreaCode(), Guid.NewGuid());
            var someCorrelationContextId = Guid.NewGuid().ToString();

            return ServiceBusMessageFactory.CreateServiceBusMessage(processCompleted, messageType, someCorrelationContextId);
        }
    }
}
