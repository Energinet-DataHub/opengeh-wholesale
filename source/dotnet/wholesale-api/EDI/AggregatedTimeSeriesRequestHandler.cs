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
using EDI.InboxEvents;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Logging;
using Microsoft.Extensions.Logging;

namespace EDI;

public class AggregatedTimeSeriesRequestHandler : IAggregatedTimeSeriesRequestHandler
{
    private readonly IRequestCalculationResultQueries _requestCalculationResultQueries;
    private readonly IEdiClient _ediClient;
    private readonly IAggregatedTimeSeriesMessageFactory _aggregatedTimeSeriesMessageFactory;
    private readonly ILogger<AggregatedTimeSeriesRequestHandler> _logger;
    private readonly IAggregatedTimeSeriesRequestMessageParser _aggregatedTimeSeriesRequestMessageParser;

    public AggregatedTimeSeriesRequestHandler(
        IRequestCalculationResultQueries requestCalculationResultQueries,
        IEdiClient ediClient,
        IAggregatedTimeSeriesRequestMessageParser aggregatedTimeSeriesRequestMessageParser,
        IAggregatedTimeSeriesMessageFactory aggregatedTimeSeriesMessageFactory,
        ILogger<AggregatedTimeSeriesRequestHandler> logger)
    {
        _requestCalculationResultQueries = requestCalculationResultQueries;
        _ediClient = ediClient;
        _aggregatedTimeSeriesRequestMessageParser = aggregatedTimeSeriesRequestMessageParser;
        _aggregatedTimeSeriesMessageFactory = aggregatedTimeSeriesMessageFactory;
        _logger = logger;
    }

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        var aggregatedTimeSeriesRequestMessage = _aggregatedTimeSeriesRequestMessageParser.Parse(receivedMessage);

        var result = await GetCalculationResultsAsync(
            aggregatedTimeSeriesRequestMessage,
            cancellationToken).ConfigureAwait(false);

        var message = _aggregatedTimeSeriesMessageFactory.Create(
            result,
            referenceId);

        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task<EnergyResult?> GetCalculationResultsAsync(
        AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage,
        CancellationToken cancellationToken)
    {
        var query = new CalculationResultQuery(
            TimeSeriesTypeMapper.MapTimeSerieType(aggregatedTimeSeriesRequestMessage.TimeSeriesType),
            aggregatedTimeSeriesRequestMessage.Period.Start,
            aggregatedTimeSeriesRequestMessage.Period.End,
            aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea.GridAreaCode,
            aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea.EnergySupplierId,
            aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea.BalanceResponsibleId);

        var calculationResult = await _requestCalculationResultQueries.GetAsync(query)
            .ConfigureAwait(false);

        _logger.LogDebug("Found {CalculationResult} calculation results based on {Query} query.", calculationResult?.ToJsonString(), query.ToJsonString());
        return calculationResult;
    }
}
