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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.Events.Application.InboxEvents;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class AggregatedTimeSeriesRequestHandler : IAggregatedTimeSeriesRequestHandler
{
    private readonly ICalculationResultQueries _calculationResultQueries;
    private readonly IEdiClient _ediClient;
    private readonly IAggregatedTimeSeriesMessageFactory _aggregatedTimeSeriesMessageFactory;

    public AggregatedTimeSeriesRequestHandler(
        ICalculationResultQueries calculationResultQueries,
        IEdiClient ediClient,
        IAggregatedTimeSeriesMessageFactory aggregatedTimeSeriesMessageFactory)
    {
        _calculationResultQueries = calculationResultQueries;
        _ediClient = ediClient;
        _aggregatedTimeSeriesMessageFactory = aggregatedTimeSeriesMessageFactory;
    }

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, CancellationToken cancellationToken)
    {
        // create the request from the protobuf message
        // call the query service
        var result = new List<object>();
        // create the response
        var message = _aggregatedTimeSeriesMessageFactory.Create(result, receivedMessage.MessageId);

        // send the response to EDI inbox.
        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }
}
