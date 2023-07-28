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
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal;

internal class ServiceBusChannel : IAsyncDisposable
{
    private const int DefaultBatchSize = 1500;
    private readonly int _batchLimitSize;
    private readonly ILogger<ServiceBusChannel> _logger;
    private readonly ServiceBusSender _sender;

    protected ServiceBusChannel(ServiceBusClient client, string topicName, ILogger<ServiceBusChannel> logger, int batchLimitSize = DefaultBatchSize)
    {
        _batchLimitSize = batchLimitSize;
        _logger = logger;
        _sender = client.CreateSender(topicName);
    }

    /// <summary>
    /// Sends a message to the service bus.
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="cancellationToken">CancellationToken if the operation should be cancelled</param>
    public async Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Enter method {MethodName}", nameof(SendMessageAsync));
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogDebug("Sending message {MessageId}", message.MessageId);

        await _sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Sent message {MessageId}", message.MessageId);
        _logger.LogTrace("Exit method {MethodName}", nameof(SendMessageAsync));
    }

    /// <summary>
    /// Dispose of service bus resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a batch of messages to be sent on the service bus.
    /// Handles the complexity of creating a batch and sending it when it is full.
    /// </summary>
    /// <returns><see cref="MessageBatch"/></returns>
    internal MessageBatch CreateBatch() => new(_sender, _batchLimitSize);

    internal class MessageBatch
    {
        private readonly ServiceBusSender _sender;
        private readonly int _batchLimitSize;
        private ServiceBusMessageBatch? _batch;

        internal MessageBatch(ServiceBusSender sender, int batchLimitSize)
        {
            _sender = sender;
            _batchLimitSize = batchLimitSize;
        }

        /// <summary>
        /// Adds a message to the batch.
        /// </summary>
        /// <remarks>Remember to call <see cref="FinalizeBatchAsync"/> when last message has been added</remarks>
        /// <param name="message">Message to add</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <exception cref="InvalidOperationException">Raised if we are unable to add a message to a batch</exception>
        internal async Task AddMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
        {
            _batch ??= await _sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false); // Create batch if null
            if (_batch.Count + 1 <= _batchLimitSize && _batch.TryAddMessage(message)) return; // Add message if possible

            await _sender.SendMessagesAsync(_batch, cancellationToken).ConfigureAwait(false); // Send batch if full
            _batch = await _sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false); // Create new batch
            if (!_batch.TryAddMessage(message)) throw new InvalidOperationException("Unable to add message to empty batch"); // Add message to new batch
        }

        /// <summary>
        /// Finalizes the batch and sends it to the service bus.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        internal async Task FinalizeBatchAsync(CancellationToken cancellationToken = default)
        {
            if (_batch?.Count > 0)
                await _sender.SendMessagesAsync(_batch, cancellationToken).ConfigureAwait(false);
        }
    }
}
