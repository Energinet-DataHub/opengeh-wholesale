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

using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Application.Communication.Messaging;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

// TODO - XDAST: Currently refactoring, so this is a step on the way. Some code was copied from the Messaging package.
public class CalculationIntegrationEventPublisher : ICalculationIntegrationEventPublisher
{
    private readonly IServiceBusSenderProvider _senderProvider;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private readonly ICalculationCompletedEventProvider _calculationCompletedEventProvider;
    private readonly IEnergyResultEventProvider _energyResultEventProvider;
    private readonly ILogger _logger;

    public CalculationIntegrationEventPublisher(
        IServiceBusSenderProvider senderProvider,
        IServiceBusMessageFactory serviceBusMessageFactory,
        ICalculationCompletedEventProvider calculationCompletedEventProvider,
        IEnergyResultEventProvider energyResultEventProvider,
        ILogger<CalculationIntegrationEventPublisher> logger)
    {
        _senderProvider = senderProvider;
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _calculationCompletedEventProvider = calculationCompletedEventProvider;
        _energyResultEventProvider = energyResultEventProvider;
        _logger = logger;
    }

    public async Task PublishAsync(CalculationDto completedCalculation, string orchestrationInstanceId, CancellationToken cancellationToken)
    {
        // TODO - XDAST: Currently refactoring. This code was copied from the Messaging package.
        var stopwatch = Stopwatch.StartNew();
        var eventCount = 0;
        var messageBatch = await _senderProvider.Instance.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

        await foreach (var @event in GetAsync(completedCalculation, orchestrationInstanceId).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            eventCount++;
            var serviceBusMessage = _serviceBusMessageFactory.Create(@event);
            if (!messageBatch.TryAddMessage(serviceBusMessage))
            {
                await SendBatchAsync(messageBatch).ConfigureAwait(false);
                messageBatch = await _senderProvider.Instance.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

                if (!messageBatch.TryAddMessage(serviceBusMessage))
                {
                    await SendMessageThatExceedsBatchLimitAsync(serviceBusMessage).ConfigureAwait(false);
                }
            }
        }

        try
        {
            await _senderProvider.Instance.SendMessagesAsync(messageBatch, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish messages");
        }

        if (eventCount > 0)
        {
            _logger.LogDebug("Sent {EventCount} integration events in {Time} ms", eventCount, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    private async IAsyncEnumerable<IntegrationEvent> GetAsync(CalculationDto completedCalculation, string orchestrationInstanceId)
    {
        var hasFailed = false;

        // Publish integration events for energy results
        var energyResultCount = 0;
        var energyResultEventProviderEnumerator = _energyResultEventProvider.GetAsync(completedCalculation.CalculationId).GetAsyncEnumerator();
        try
        {
            var hasResult = true;
            while (hasResult)
            {
                try
                {
                    hasResult = await energyResultEventProviderEnumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    hasResult = false;
                    hasFailed = true;
                    _logger.LogError(ex, "Failed energy result event publishing for completed calculation {calculation_id}. Handled '{energy_result_count}' energy results before failing.", completedCalculation.CalculationId, energyResultCount);
                }

                if (hasResult)
                {
                    energyResultCount++;
                    yield return energyResultEventProviderEnumerator.Current;
                }
            }
        }
        finally
        {
            if (energyResultEventProviderEnumerator != null)
            {
                await energyResultEventProviderEnumerator.DisposeAsync().ConfigureAwait(false);
            }
        }

        // Publish integration events for calculation completed
        IntegrationEvent? calculationCompletedEvent = default;
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
            yield return calculationCompletedEvent;
        }

        if (hasFailed)
        {
            throw new Exception($"Publish failed for completed calculation (id: {completedCalculation.CalculationId})");
        }

        _logger.LogInformation("Published results for succeeded energy calculation {calculation_id} to the service bus ({energy_result_count} integration events).", completedCalculation.CalculationId, energyResultCount);
    }

    private async Task SendBatchAsync(ServiceBusMessageBatch batch)
    {
        await _senderProvider.Instance.SendMessagesAsync(batch).ConfigureAwait(false);
    }

    private async Task SendMessageThatExceedsBatchLimitAsync(ServiceBusMessage serviceBusMessage)
    {
        await _senderProvider.Instance.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
    }
}
