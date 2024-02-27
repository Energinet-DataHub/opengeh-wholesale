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

using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Orchestration.Functions.IntegrationEvents;

public class PublishIntegrationEventsTrigger
{
    private readonly ILogger _logger;
    private readonly IPublisher _handler;

    public PublishIntegrationEventsTrigger(
        ILogger<PublishIntegrationEventsTrigger> logger,
        IPublisher handler)
    {
        _logger = logger;
        _handler = handler;
    }

    [Function(nameof(PublishIntegrationEventsTrigger))]
    public async Task Run(
        [TimerTrigger("00:00:10")]
        TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        await _handler.PublishAsync(cancellationToken).ConfigureAwait(false);
    }
}
