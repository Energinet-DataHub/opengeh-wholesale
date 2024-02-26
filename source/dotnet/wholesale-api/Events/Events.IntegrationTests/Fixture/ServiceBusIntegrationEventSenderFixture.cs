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

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class ServiceBusIntegrationEventSenderFixture : IAsyncLifetime
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private const string TopicNamePrefix = "sbt-integration-event-topic";
    private const string SubscriptionName = "sbs-integration-event-subscription";

    private readonly ServiceBusResourceProvider _serviceBusResourceProvider;
    private ServiceBusSender? _sender;

    public ServiceBusIntegrationEventSenderFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        _serviceBusResourceProvider = new ServiceBusResourceProvider(
            integrationTestConfiguration.ServiceBusConnectionString,
            new TestDiagnosticsLogger());

        ServiceBusClient = new ServiceBusClient(integrationTestConfiguration.ServiceBusConnectionString);

        ServiceBusOptions = Options.Create(
            new ServiceBusOptions
            {
                SERVICE_BUS_TRANCEIVER_CONNECTION_STRING = integrationTestConfiguration.ServiceBusConnectionString,
                INTEGRATIONEVENTS_SUBSCRIPTION_NAME = SubscriptionName,
            });
    }

    public IOptions<ServiceBusOptions> ServiceBusOptions { get; }

    public ServiceBusClient ServiceBusClient { get; }

    public async Task InitializeAsync()
    {
        await _serviceBusResourceProvider
            .BuildTopic(TopicNamePrefix)
            .Do(topicProperties =>
            {
                ServiceBusOptions.Value.INTEGRATIONEVENTS_TOPIC_NAME = topicProperties.Name;
            })
            .AddSubscription(ServiceBusOptions.Value.INTEGRATIONEVENTS_SUBSCRIPTION_NAME)
            .CreateAsync();

        _sender = ServiceBusClient.CreateSender(ServiceBusOptions.Value.INTEGRATIONEVENTS_TOPIC_NAME);
    }

    public async Task DisposeAsync()
    {
        await ServiceBusClient.DisposeAsync();
        await _serviceBusResourceProvider.DisposeAsync();
    }

    public async Task PublishAsync(string message, string messageId, string subject)
    {
        if (_sender == null)
            throw new InvalidOperationException($"Call '{nameof(InitializeAsync)}' before calling this method.");

        await _sender.SendMessageAsync(CreateReceivedIntegrationEvent(message, messageId, subject));
    }

    private static ServiceBusMessage CreateReceivedIntegrationEvent(string body, string messageId, string subject)
    {
        var message = new ServiceBusMessage(body);
        message.MessageId = messageId;
        message.Subject = subject;
        return message;
    }
}
