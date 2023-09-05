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
using Energinet.DataHub.Wholesale.Events.Application.InboxEvents;
using Energinet.DataHub.Wholesale.Events.Application.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents;

public class EdiInboxSender : IEdiInboxSender, IAsyncDisposable
{
    private readonly EdiInboxOptions _options;
    private readonly ServiceBusClient _serviceBusClient;

    public EdiInboxSender(IOptions<EdiInboxOptions> options)
    {
        _options = options.Value;
        _serviceBusClient = new ServiceBusClient(_options.EDI_INBOX_CONNECTION_STRING);
    }

    public async Task SendAsync(ServiceBusMessage message, CancellationToken cancellationToken)
    {
        var sender = _serviceBusClient.CreateSender(_options.EDI_INBOX_QUEUE_NAME);
        await sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
