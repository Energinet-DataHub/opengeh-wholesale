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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Logging;
using Energinet.DataHub.Wholesale.EDI.Client;
using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.EDI;

public class AggregatedTimeSeriesRequestHandler : IAggregatedTimeSeriesRequestHandler
{
    private readonly IRequestCalculationResultQueries _requestCalculationResultQueries;
    private readonly IEdiClient _ediClient;
    private readonly IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> _validator;
    private readonly ILogger<AggregatedTimeSeriesRequestHandler> _logger;
    private readonly IAggregatedTimeSeriesRequestFactory _aggregatedTimeSeriesRequestFactory;

    public AggregatedTimeSeriesRequestHandler(
        IRequestCalculationResultQueries requestCalculationResultQueries,
        IEdiClient ediClient,
        IAggregatedTimeSeriesRequestFactory aggregatedTimeSeriesRequestFactory,
        IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> validator,
        ILogger<AggregatedTimeSeriesRequestHandler> logger)
    {
        _requestCalculationResultQueries = requestCalculationResultQueries;
        _ediClient = ediClient;
        _aggregatedTimeSeriesRequestFactory = aggregatedTimeSeriesRequestFactory;
        _validator = validator;
        _logger = logger;
    }

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        var aggregatedTimeSeriesRequest = Edi.Requests.AggregatedTimeSeriesRequest.Parser.ParseFrom(receivedMessage.Body);

        var validationErrors = _validator.Validate(aggregatedTimeSeriesRequest);

        ServiceBusMessage message;
        if (!validationErrors.Any())
        {
            var aggregatedTimeSeriesRequestMessage = _aggregatedTimeSeriesRequestFactory.Parse(aggregatedTimeSeriesRequest);
            var result = await GetCalculationResultsAsync(
                aggregatedTimeSeriesRequestMessage,
                cancellationToken).ConfigureAwait(false);

            if (result is not null)
            {
                message = AggregatedTimeSeriesRequestAcceptedMessageFactory.Create(
                result,
                referenceId);
            }
            else
            {
                message = AggregatedTimeSeriesRequestRejectedMessageFactory.Create(new[] { ValidationError.NoDataFound }, referenceId);
            }
        }
        else
        {
            message = AggregatedTimeSeriesRequestRejectedMessageFactory.Create(validationErrors.ToList(), referenceId);
        }

        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task<EnergyResult?> GetCalculationResultsAsync(
        AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage,
        CancellationToken cancellationToken)
    {
        var query = new EnergyResultQuery(
            TimeSeriesTypeMapper.MapTimeSeriesTypeFromEdi(aggregatedTimeSeriesRequestMessage.TimeSeriesType),
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
