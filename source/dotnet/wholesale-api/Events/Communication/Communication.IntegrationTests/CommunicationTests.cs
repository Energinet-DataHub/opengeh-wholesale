// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Communication.IntegrationTests;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Objects are disposed in IAsyncLifetime.DisposeAsync method")]
public class CommunicationTests : IAsyncLifetime
{
    private readonly ServiceBusResourceProvider _provider;
    private readonly IntegrationTestConfiguration _config;

    public CommunicationTests()
    {
        _config = new IntegrationTestConfiguration();
        _provider = new ServiceBusResourceProvider(_config.ServiceBusConnectionString, new TestDiagnosticsLogger());
    }

    [Theory]
    [MemberData(nameof(GetBatchSizeAndEventsToProduce))]
    public async Task Verify_That_All_Messages_Are_Delivered_Based_On_ActiveMessageCount(int eventsToProduce, int batchSize)
    {
        const string queueName = "my-input-queue";

        var queueResource = await _provider.BuildQueue(queueName).CreateAsync();

        var services = new ServiceCollection()
            .Configure<MockIntegrationEventProviderOptions>(instance => instance.IntegrationEventsToProduce = eventsToProduce)
            .AddLogging()
            .AddCommunication<MockIntegrationEventProvider>(
                _config.ServiceBusConnectionString,
                queueResource.Name,
                useNewChannelObject: true,
                batchSize: batchSize)
            .BuildServiceProvider();

        // Act
        var sender = services.GetRequiredService<IOutboxSender>();
        await sender.SendAsync();

        // Assert
        var admin = new ServiceBusAdministrationClient(_config.ServiceBusConnectionString);
        var queueInfo = await admin.GetQueueRuntimePropertiesAsync(queueResource.Name);
        queueInfo.Value.ActiveMessageCount.Should().Be(eventsToProduce);
    }

    public static IEnumerable<object[]> GetBatchSizeAndEventsToProduce()
    {
        yield return new object[] { 10, 100 }; // 10 events, batch size 100
        yield return new object[] { 99, 7 }; // 99 events, batch size 7
        yield return new object[] { 100, 100 }; // 100 events, batch size 100
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }
}
