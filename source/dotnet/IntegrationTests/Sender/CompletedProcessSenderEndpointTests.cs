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
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.IntegrationTests.Core;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon.Function;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture;
using Energinet.DataHub.Wholesale.Sender.Endpoints;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Sender;

public class CompletedProcessSenderEndpointTests
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

        [Fact]
        public async Task CompletedProcessTestAsync()
        {
            // Arrange
            using var whenAllEvent = await Fixture.ServiceBusListener
                .WhenAny()
                .VerifyCountAsync(1)
                .ConfigureAwait(false);

            var completedProcess = new ProcessCompletedEventDto("123");
            var operationTimestamp = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var byteArray = ConvertObjectToByteArray(completedProcess);
            var message = ServiceBusTestMessage.Create(byteArray, operationTimestamp, Guid.NewGuid().ToString());

            // Act
            await Fixture.CompletedProcessTopic.SenderClient.SendMessageAsync(message);

            // Assert
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(DataAvailableSenderEndpoint))
                .ConfigureAwait(false);

            var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
            allReceived.Should().BeTrue();
        }

        private byte[] ConvertObjectToByteArray(ProcessCompletedEventDto completedProcess)
        {
            using var m = new MemoryStream();
            using var writer = new BinaryWriter(m);
            writer.Write(completedProcess.GridAreaCode);
            return m.ToArray();
        }
    }
}
