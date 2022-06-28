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

using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.IntegrationTests.Core;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Sender;

public class DataAvailableSenderEndpointTests
{
    [Collection(nameof(SenderFunctionAppCollectionFixture))]
    public class RunAsync : FunctionAppTestBase<SenderFunctionAppFixture>, IAsyncLifetime
    {
        public RunAsync(SenderFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Fixture.HostManager.ClearHostLog();
            return Task.CompletedTask;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task When_ProcessCompleted_Then_DataAvailableIsReceivedInMessageHub(
            Guid correlationId,
            DateTime operationTimestamp)
        {
            // Arrange
            var completedProcess = new ProcessCompletedEventDto("805");
            using var eventualDataAvailableEvent = await Fixture
                .DataAvailableListener
                .ListenForMessageAsync(correlationId.ToString())
                .ConfigureAwait(false);

            var message = ServiceBusTestMessage.Create(completedProcess, operationTimestamp.AsUtc(), correlationId.ToString());

            // Act
            await Fixture.CompletedProcessTopic.SenderClient.SendMessageAsync(message);

            // Assert
            var isDataAvailableEventReceived = eventualDataAvailableEvent
                .MessageAwaiter!
                .Wait(TimeSpan.FromSeconds(10));
            isDataAvailableEventReceived.Should().BeTrue();
        }
    }
}
