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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Fixture;

public class ServiceBusIntegrationEventSenderFixture : IAsyncLifetime, IAsyncDisposable
{
    public IOptions<ServiceBusOptions> ServiceBusOptions { get; }

    public ServiceBusClient ServiceBusClient { get; }

    private readonly ServiceBusResourceProvider _serviceBusResourceProvider;
    private readonly string _topicName = "sbt-integration-event-topic";
    private ServiceBusSender? _sender;

    public ServiceBusIntegrationEventSenderFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        ServiceBusOptions = Options.Create(
            new ServiceBusOptions
            {
                SERVICE_BUS_MANAGE_CONNECTION_STRING = integrationTestConfiguration.ServiceBusConnectionString,
            });

        _serviceBusResourceProvider = new ServiceBusResourceProvider(
            ServiceBusOptions.Value.SERVICE_BUS_MANAGE_CONNECTION_STRING,
            new TestDiagnosticsLogger());

        ServiceBusClient = new ServiceBusClient(ServiceBusOptions.Value.SERVICE_BUS_MANAGE_CONNECTION_STRING);
    }

    public async Task InitializeAsync()
    {
        var builder = _serviceBusResourceProvider
            .BuildTopic(_topicName);
        builder
            .Do(topicProperties =>
            {
                ServiceBusOptions.Value.INTEGRATIONEVENTS_TOPIC_NAME = topicProperties.Name;
                ServiceBusOptions.Value.INTEGRATIONEVENTS_SUBSCRIPTION_NAME = "sbs-integration-event-subscription";
            });
        await builder
            .CreateAsync();
        _sender = ServiceBusClient.CreateSender(ServiceBusOptions.Value.INTEGRATIONEVENTS_TOPIC_NAME);
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

    internal async Task PublishAsync(string message, string messageId, string subject)
    {
        if (_sender == null)
            return;

        await _sender.SendMessageAsync(CreateReceivedIntegrationEvent(message, messageId, subject));
    }

    private ServiceBusMessage CreateReceivedIntegrationEvent(string body, string messageId, string subject)
    {
        var message = new ServiceBusMessage(body);
        message.MessageId = messageId;
        message.Subject = subject;
        return message;
    }
}
