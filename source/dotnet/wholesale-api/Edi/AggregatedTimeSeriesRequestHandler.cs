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
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.EDI.Client;
using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.EDI;

public class AggregatedTimeSeriesRequestHandler : IAggregatedTimeSeriesRequestHandler
{
    private readonly IRequestCalculationResultRetriever _requestCalculationResultRetriever;
    private readonly IEdiClient _ediClient;
    private readonly IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> _validator;
    private readonly ILogger<AggregatedTimeSeriesRequestHandler> _logger;
    private readonly IAggregatedTimeSeriesRequestFactory _aggregatedTimeSeriesRequestFactory;
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");

    public AggregatedTimeSeriesRequestHandler(
        IRequestCalculationResultRetriever requestCalculationResultRetriever,
        IEdiClient ediClient,
        IAggregatedTimeSeriesRequestFactory aggregatedTimeSeriesRequestFactory,
        IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> validator,
        ILogger<AggregatedTimeSeriesRequestHandler> logger)
    {
        _requestCalculationResultRetriever = requestCalculationResultRetriever;
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
            var results = await GetCalculationResultsAsync(aggregatedTimeSeriesRequestMessage)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            if (!results.Any())
            {
                await SendRejectedMessageAsync(new List<ValidationError> { _noDataAvailable }, referenceId, cancellationToken).ConfigureAwait(false);
                return;
            }

            message = AggregatedTimeSeriesRequestAcceptedMessageFactory.Create(results, referenceId);
        }
        else
        {
            message = AggregatedTimeSeriesRequestRejectedMessageFactory.Create(validationErrors.ToList(), referenceId);
        }

        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private IAsyncEnumerable<EnergyResult> GetCalculationResultsAsync(
        AggregatedTimeSeriesRequest request)
    {
        var filter = new EnergyResultFilter(
            CalculationTimeSeriesTypeMapper.MapTimeSeriesTypeFromEdi(request.TimeSeriesType),
            request.Period.Start,
            request.Period.End,
            request.RequestedProcessType,
            request.AggregationPerRoleAndGridArea.GridAreaCode,
            request.AggregationPerRoleAndGridArea.EnergySupplierId,
            request.AggregationPerRoleAndGridArea.BalanceResponsibleId);

        return _requestCalculationResultRetriever.GetRequestCalculationResultAsync(filter);
    }

    private async Task SendRejectedMessageAsync(List<ValidationError> validationErrors, string referenceId, CancellationToken cancellationToken)
    {
        var message = AggregatedTimeSeriesRequestRejectedMessageFactory.Create(validationErrors, referenceId);
        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }
}
