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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Application.InboxEvents;
using CalculationTimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;
using TimeSeriesType = Energinet.DataHub.Wholesale.Events.Application.InboxEvents.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class AggregatedTimeSeriesRequestHandler : IAggregatedTimeSeriesRequestHandler
{
    private readonly ICalculationResultQueries _calculationResultQueries;
    private readonly IEdiClient _ediClient;
    private readonly IAggregatedTimeSeriesMessageFactory _aggregatedTimeSeriesMessageFactory;
    private readonly IAggregatedTimeSeriesRequestMessageParser _aggregatedTimeSeriesRequestMessageParser;

    public AggregatedTimeSeriesRequestHandler(
        ICalculationResultQueries calculationResultQueries,
        IEdiClient ediClient,
        IAggregatedTimeSeriesRequestMessageParser aggregatedTimeSeriesRequestMessageParser,
        IAggregatedTimeSeriesMessageFactory aggregatedTimeSeriesMessageFactory)
    {
        _calculationResultQueries = calculationResultQueries;
        _ediClient = ediClient;
        _aggregatedTimeSeriesRequestMessageParser = aggregatedTimeSeriesRequestMessageParser;
        _aggregatedTimeSeriesMessageFactory = aggregatedTimeSeriesMessageFactory;
    }

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        // create the request from the protobuf message
        var aggregatedTimeSeriesRequestMessage = _aggregatedTimeSeriesRequestMessageParser.Parse(receivedMessage);

        // call the query service
        var result = await GetCalculationResultsAsync(
            aggregatedTimeSeriesRequestMessage,
            cancellationToken).ConfigureAwait(false);

        // create the response
        var message = _aggregatedTimeSeriesMessageFactory.Create(
            result,
            referenceId,
            isRejected: aggregatedTimeSeriesRequestMessage.TimeSeriesType != TimeSeriesType.Production);

        // send the response to EDI inbox.
        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<CalculationResult>> GetCalculationResultsAsync(
        AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage,
        CancellationToken cancellationToken)
    {
        var query = new CalculationResultQuery(
            nameof(aggregatedTimeSeriesRequestMessage.TimeSeriesType),
            aggregatedTimeSeriesRequestMessage.Period.Start,
            aggregatedTimeSeriesRequestMessage.Period.Start,
            MapGridAreaCode(aggregatedTimeSeriesRequestMessage),
            MapAggregationLevel(aggregatedTimeSeriesRequestMessage));

        return await _calculationResultQueries.GetAsync(query)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private AggregationLevel MapAggregationLevel(AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage)
    {
        if (aggregatedTimeSeriesRequestMessage.AggregationPerGridArea != null)
            return AggregationLevel.GridArea;
        throw new InvalidOperationException($"Unknown aggregation level: {aggregatedTimeSeriesRequestMessage}");
    }

    private string MapGridAreaCode(AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage)
    {
        var gridAreaCode = aggregatedTimeSeriesRequestMessage.AggregationPerGridArea?.GridAreaCode
                              ?? aggregatedTimeSeriesRequestMessage.AggregationPerBalanceResponsiblePartyPerGridArea?.GridAreaCode
                              ?? aggregatedTimeSeriesRequestMessage.AggregationPerEnergySupplierPerGridArea?.GridAreaCode
                              ?? aggregatedTimeSeriesRequestMessage.AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea?
                                  .GridAreaCode;
        if (gridAreaCode is null)
            throw new InvalidOperationException($"Unknown grid area code: {gridAreaCode}.");

        return gridAreaCode;
    }
}
