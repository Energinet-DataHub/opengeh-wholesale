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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.Wholesale.Events.Application.Options;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Fixture;

public class ServiceBusSenderFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly string _connectionString;
    private readonly ServiceBusResourceProvider _serviceBusResourceProvider;
    private readonly string _queueName = "sbq-wholesale-inbox";

    public ServiceBusSenderFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        _connectionString = integrationTestConfiguration.ServiceBusConnectionString;

        _client = new ServiceBusClient(_connectionString);
        _sender = _client.CreateSender(_queueName);
        _serviceBusResourceProvider = new ServiceBusResourceProvider(
            _connectionString,
            new TestDiagnosticsLogger());
    }

    public async Task InitializeAsync()
    {
        await _serviceBusResourceProvider
            .BuildQueue(_queueName)
            .SetEnvironmentVariableToQueueName(nameof(ServiceBusOptions.SERVICE_BUS_INBOX_QUEUE_NAME))
            .CreateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeCoreAsync().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }

    internal Task PublishAsync(string eventName, byte[] eventPayload)
    {
        return _sender.SendMessageAsync(CreateAggregatedTimeSeriesRequestMessage(eventName, eventPayload));
    }

    private ServiceBusMessage CreateAggregatedTimeSeriesRequestMessage(string eventName, byte[] eventPayload)
    {
        var message = new ServiceBusMessage(
            "hello world");
        return message;
    }

    private async ValueTask DisposeCoreAsync()
    {
        await _client.DisposeAsync().ConfigureAwait(false);
        await _sender.DisposeAsync().ConfigureAwait(false);
        await _serviceBusResourceProvider.DisposeAsync().ConfigureAwait(false);
    }
}
