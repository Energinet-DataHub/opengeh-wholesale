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
using System.Text.Unicode;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon;
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
            Fixture.MessageHubMock.Clear();
            return Task.CompletedTask;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task Given_ProcessCompleted_When_MeteredDataResponsiblePeeks_Then_MessageHubReceivesReply(
            Guid correlationId,
            DateTime operationTimestamp)
        {
            // Arrange
            var jsonLinesstuff = "{\"position\":1}\n{\"position\":2}\n{\"position\":3}";
            var newGuid = Guid.NewGuid();
            var bcc = new BlobContainerClient("UseDevelopmentStorage=true", "processes");
            var bc = bcc.GetBlobClient(
                $"results/batch_id={newGuid}/grid_area=805/part-00000-tid-8731683461868483703-9c3b4be4-b8b3-4ab8-a7e8-6399ba0f3f70-0-1.c000.json");
            await bc.UploadAsync(new BinaryData(jsonLinesstuff));
            var completedProcess = new ProcessCompletedEventDto("805", newGuid);
            var message = ServiceBusTestMessage.Create(completedProcess, operationTimestamp.AsUtc(), correlationId.ToString());

            // Act -> Publish process completed event, which will transitively invoke
            await Fixture.CompletedProcessTopic.SenderClient.SendMessageAsync(message);

            // Assert
            await Fixture.MessageHubMock.WaitForNotificationsInDataAvailableQueueAsync(correlationId.ToString());
            var response = await Fixture.MessageHubMock.PeekAsync();
            response.Content.Should().NotBeNull();
        }
    }
}
