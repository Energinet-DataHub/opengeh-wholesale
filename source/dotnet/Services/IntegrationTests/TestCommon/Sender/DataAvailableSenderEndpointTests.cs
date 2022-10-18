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
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.FunctionApp;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Sender;

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
            Fixture.MessageHubMock.Clear();
            return Task.CompletedTask;
        }

        [Theory(Skip = "There is currently no way to test DataLake containers.")]
        [InlineAutoMoqData]
        public async Task Given_ProcessCompleted_When_MeteredDataResponsiblePeeks_Then_MessageHubReceivesReply(
            Guid batchId,
            Guid correlationId,
            DateTime operationTimestamp,
            string messageType)
        {
            // Arrange
            var gridAreaCode = "805";

            var completedProcess = new ProcessCompletedEventDto(gridAreaCode, batchId);
            var message = ServiceBusTestMessage.Create(
                completedProcess,
                operationTimestamp.AsUtc(),
                correlationId.ToString(),
                messageType);

            // Act -> Publish process completed event, which will transitively invoke
            await Fixture.CompletedProcessTopic.SenderClient.SendMessageAsync(message);

            // Assert
            await Fixture.MessageHubMock.WaitForNotificationsInDataAvailableQueueAsync(correlationId.ToString());

            var response = await Fixture.MessageHubMock.PeekAsync();
            response.Content.Should().NotBeNull();
        }

        [Fact]
        public async Task ServiceCollection_CanResolveDataAvailableEndpoint()
        {
            // Arrange
            using var host = await SenderIntegrationTestHost
                .CreateAsync(collection => collection.AddScoped<DataAvailableSenderEndpoint>());

            await using var scope = host.BeginScope();

            // Act & Assert that the container can resolve the endpoints dependencies
            scope.ServiceProvider.GetRequiredService<DataAvailableSenderEndpoint>();
        }
    }
}
