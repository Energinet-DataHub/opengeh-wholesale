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
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

/// <summary>
/// Copied from the Messaging package and refactored to allow us to send events immediately
/// and for a certain calculation only.
/// </summary>
public class CalculationIntegrationEventPublisher : ICalculationIntegrationEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private readonly ICalculationCompletedEventProvider _calculationCompletedEventProvider;
    private readonly ILogger _logger;

    public CalculationIntegrationEventPublisher(
        IOptions<IntegrationEventsOptions> integrationEventsOptions,
        IAzureClientFactory<ServiceBusSender> senderFactory,
        IServiceBusMessageFactory serviceBusMessageFactory,
        ICalculationCompletedEventProvider calculationCompletedEventProvider,
        ILogger<CalculationIntegrationEventPublisher> logger)
    {
        _sender = senderFactory.CreateClient(integrationEventsOptions.Value.TopicName);
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _calculationCompletedEventProvider = calculationCompletedEventProvider;
        _logger = logger;
    }

    public async Task PublishAsync(CalculationDto completedCalculation, string orchestrationInstanceId, CancellationToken cancellationToken)
    {
        var messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

        foreach (var @event in GetCalculationCompletedEvent(completedCalculation, orchestrationInstanceId))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var serviceBusMessage = _serviceBusMessageFactory.Create(@event);
            if (!messageBatch.TryAddMessage(serviceBusMessage))
            {
                await SendBatchAsync(messageBatch).ConfigureAwait(false);
                messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

                if (!messageBatch.TryAddMessage(serviceBusMessage))
                {
                    await SendMessageThatExceedsBatchLimitAsync(serviceBusMessage).ConfigureAwait(false);
                }
            }
        }

        try
        {
            await _sender.SendMessagesAsync(messageBatch, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish messages");
        }
    }

    private IReadOnlyCollection<IntegrationEvent> GetCalculationCompletedEvent(CalculationDto completedCalculation, string orchestrationInstanceId)
    {
        // Publish integration events for calculation completed
        IntegrationEvent? calculationCompletedEvent = default;
        var hasFailed = false;

        try
        {
            calculationCompletedEvent = _calculationCompletedEventProvider.Get(completedCalculation, orchestrationInstanceId);
        }
        catch (Exception ex)
        {
            hasFailed = true;
            _logger.LogError(ex, "Failed calculation completed event publishing for completed calculation {calculation_id}.", completedCalculation.CalculationId);
        }

        if (calculationCompletedEvent != null)
        {
            _logger.LogInformation("Published results for succeeded energy calculation {calculation_id} to the service bus.", completedCalculation.CalculationId);
            return [calculationCompletedEvent];
        }

        if (hasFailed)
        {
            throw new Exception($"Publish failed for completed calculation (id: {completedCalculation.CalculationId})");
        }

        _logger.LogInformation("Published no events for calculation {calculation_id} to the service bus.", completedCalculation.CalculationId);
        return [];
    }

    private async Task SendBatchAsync(ServiceBusMessageBatch batch)
    {
        await _sender.SendMessagesAsync(batch).ConfigureAwait(false);
    }

    private async Task SendMessageThatExceedsBatchLimitAsync(ServiceBusMessage serviceBusMessage)
    {
        await _sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
    }
}
