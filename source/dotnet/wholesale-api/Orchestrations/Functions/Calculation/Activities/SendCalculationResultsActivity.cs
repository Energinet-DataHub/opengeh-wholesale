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

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Activities;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
internal class SendCalculationResultsActivity(
    IPublisher integrationEventsPublisher)
{
    private readonly IPublisher _integrationEventsPublisher = integrationEventsPublisher;

    /// <summary>
    /// Retrieve calculation results from Databricks and send them as events using ServiceBus.
    /// </summary>
    [Function(nameof(SendCalculationResultsActivity))]
    public async Task Run(
        [ActivityTrigger] Guid calculationId)
    {
        await _integrationEventsPublisher.PublishAsync(CancellationToken.None);
    }
}
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
