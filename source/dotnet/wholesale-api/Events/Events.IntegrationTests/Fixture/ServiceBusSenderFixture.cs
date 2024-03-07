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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Fixture;

public class ServiceBusSenderFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly ServiceBusResourceProvider _serviceBusResourceProvider;
    private ServiceBusSender? _sender;

    public ServiceBusSenderFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        _serviceBusResourceProvider = new ServiceBusResourceProvider(
            integrationTestConfiguration.ServiceBusConnectionString,
            new TestDiagnosticsLogger());

        ServiceBusClient = new ServiceBusClient(integrationTestConfiguration.ServiceBusConnectionString);
        WholesaleInboxQueueOptions = Options.Create(new WholesaleInboxQueueOptions());
    }

    public ServiceBusClient ServiceBusClient { get; }

    public IOptions<WholesaleInboxQueueOptions> WholesaleInboxQueueOptions { get; }

    public async Task InitializeAsync()
    {
        await _serviceBusResourceProvider
            .BuildQueue("sbq-wholesale-inbox")
                .Do(queueProperties => WholesaleInboxQueueOptions.Value.QueueName = queueProperties.Name)
            .CreateAsync();

        _sender = ServiceBusClient.CreateSender(WholesaleInboxQueueOptions.Value.QueueName);
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceBusResourceProvider.DisposeAsync();
        if (_sender != null)
            await _sender.DisposeAsync();
        await ServiceBusClient.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await Task.CompletedTask.ConfigureAwait(false);
    }

    internal async Task PublishAsync(string message, string? referenceId = null)
    {
        if (_sender == null)
            return;

        if (referenceId != null)
        {
            await _sender.SendMessageAsync(CreateAggregatedTimeSeriesRequestMessage(message, referenceId));
            return;
        }

        await _sender.SendMessageAsync(new ServiceBusMessage(message));
    }

    private ServiceBusMessage CreateAggregatedTimeSeriesRequestMessage(string body, string referenceId)
    {
        var message = new ServiceBusMessage(body)
        {
            Subject = "subject",
            ApplicationProperties =
            {
                { "ReferenceId", referenceId },
            },
        };

        return message;
    }
}
