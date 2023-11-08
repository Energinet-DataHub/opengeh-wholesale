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
using Energinet.DataHub.Wholesale.EDI.Client;
using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.Validation;

namespace Energinet.DataHub.Wholesale.EDI;

public class AggregatedTimeSeriesRequestHandler : IAggregatedTimeSeriesRequestHandler
{
    private readonly IEdiClient _ediClient;
    private readonly IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> _validator;
    private readonly IAggregatedTimeSeriesQueries _aggregatedTimeSeriesQueries;
    private readonly IAggregatedTimeSeriesRequestFactory _aggregatedTimeSeriesRequestFactory;
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");

    public AggregatedTimeSeriesRequestHandler(
        IEdiClient ediClient,
        IAggregatedTimeSeriesRequestFactory aggregatedTimeSeriesRequestFactory,
        IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> validator,
        IAggregatedTimeSeriesQueries aggregatedTimeSeriesQueries)
    {
        _ediClient = ediClient;
        _aggregatedTimeSeriesRequestFactory = aggregatedTimeSeriesRequestFactory;
        _validator = validator;
        _aggregatedTimeSeriesQueries = aggregatedTimeSeriesQueries;
    }

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        var aggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.Parser.ParseFrom(receivedMessage.Body);

        var validationErrors = _validator.Validate(aggregatedTimeSeriesRequest);

        ServiceBusMessage message;
        if (!validationErrors.Any())
        {
            var aggregatedTimeSeriesRequestMessage = _aggregatedTimeSeriesRequestFactory.Parse(aggregatedTimeSeriesRequest);
            var result = await GetCalculationResultsAsync(
                aggregatedTimeSeriesRequestMessage).ConfigureAwait(false);

            if (result is not null)
            {
                message = AggregatedTimeSeriesRequestAcceptedMessageFactory.Create(
                    result,
                    referenceId);
            }
            else
            {
                message = AggregatedTimeSeriesRequestRejectedMessageFactory.Create(new[] { _noDataAvailable }, referenceId);
            }
        }
        else
        {
            message = AggregatedTimeSeriesRequestRejectedMessageFactory.Create(validationErrors.ToList(), referenceId);
        }

        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private Task<EnergyResult?> GetCalculationResultsAsync(
        AggregatedTimeSeriesRequest request)
    {
        var parameters = new EnergyResultQueryParameters(
            CalculationTimeSeriesTypeMapper.MapTimeSeriesTypeFromEdi(request.TimeSeriesType),
            request.Period.Start,
            request.Period.End,
            request.AggregationPerRoleAndGridArea.GridAreaCode,
            request.AggregationPerRoleAndGridArea.EnergySupplierId,
            request.AggregationPerRoleAndGridArea.BalanceResponsibleId);

        if (request.RequestedProcessType == RequestedProcessType.LatestCorrection)
        {
            return _aggregatedTimeSeriesQueries.GetLatestCorrectionAsync(parameters);
        }

        var query = new EnergyResultQueryParameters(parameters, ProcessTypeMapper.FromRequestedProcessType(request.RequestedProcessType));

        return _aggregatedTimeSeriesQueries.GetAsync(query);
    }
}
