﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.Edi.Calculations;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Factories.AggregatedTimeSeries;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Events.Interfaces;
using Microsoft.Extensions.Logging;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;

namespace Energinet.DataHub.Wholesale.Edi;

/// <summary>
/// Handles AggregatedTimeSeriesRequest messages (typically received from the EDI subsystem through the WholesaleInbox service bus queue)
/// </summary>
public class AggregatedTimeSeriesRequestHandler : IWholesaleInboxRequestHandler
{
    private readonly IEdiClient _ediClient;
    private readonly IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> _validator;
    private readonly IAggregatedTimeSeriesQueries _aggregatedTimeSeriesQueries;
    private readonly ILogger<AggregatedTimeSeriesRequestHandler> _logger;
    private readonly CompletedCalculationRetriever _completedCalculationRetriever;
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");
    private static readonly ValidationError _noDataForRequestedGridArea = new("Forkert netområde / invalid grid area", "D46");

    public AggregatedTimeSeriesRequestHandler(
        IEdiClient ediClient,
        IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> validator,
        IAggregatedTimeSeriesQueries aggregatedTimeSeriesQueries,
        ILogger<AggregatedTimeSeriesRequestHandler> logger,
        CompletedCalculationRetriever completedCalculationRetriever)
    {
        _ediClient = ediClient;
        _validator = validator;
        _aggregatedTimeSeriesQueries = aggregatedTimeSeriesQueries;
        _logger = logger;
        _completedCalculationRetriever = completedCalculationRetriever;
    }

    public bool CanHandle(string requestSubject) => requestSubject.Equals(Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.Descriptor.Name);

    /// <summary>
    /// Handles the process of consuming the request for aggregated time series, then getting the required time series and creating and sending the response.
    /// </summary>
    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        var aggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest.Parser.ParseFrom(receivedMessage.Body);

        var validationErrors = await _validator.ValidateAsync(aggregatedTimeSeriesRequest).ConfigureAwait(false);

        if (validationErrors.Any())
        {
            _logger.LogWarning("Validation errors for AggregatedTimeSeriesRequest message with reference id {reference_id}", referenceId);
            await SendRejectedMessageAsync(validationErrors.ToList(), referenceId, cancellationToken).ConfigureAwait(false);
            return;
        }

        var aggregatedTimeSeriesRequestMessage = AggregatedTimeSeriesRequestFactory.Parse(aggregatedTimeSeriesRequest);
        var queryParameters =
            CreateAggregatedTimeSeriesQueryParametersWithoutCalculationType(aggregatedTimeSeriesRequestMessage);

        var calculationResults = await _aggregatedTimeSeriesQueries.GetAsync(
            queryParameters).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (!calculationResults.Any())
        {
            await SendNoDateRejectMessageAsync(
                referenceId,
                cancellationToken,
                aggregatedTimeSeriesRequest,
                aggregatedTimeSeriesRequestMessage)
                .ConfigureAwait(false);
            return;
        }

        _logger.LogInformation("Sending AggregatedTimeSeriesRequest accepted message with reference id {reference_id}", referenceId);
        await SendAcceptedMessageAsync(calculationResults, referenceId, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendNoDateRejectMessageAsync(
        string referenceId,
        CancellationToken cancellationToken,
        DataHub.Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest,
        AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage)
    {
        var error = new List<ValidationError> { _noDataAvailable };
        if (await EnergySupplierOrBalanceResponsibleHaveAggregatedTimeSeriesForAnotherGridAreasAsync(aggregatedTimeSeriesRequest, aggregatedTimeSeriesRequestMessage).ConfigureAwait(false))
        {
            error = [_noDataForRequestedGridArea];
        }

        await SendRejectedMessageAsync(error, referenceId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> EnergySupplierOrBalanceResponsibleHaveAggregatedTimeSeriesForAnotherGridAreasAsync(
        Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest,
        AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage)
    {
        if (!aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea.GridAreaCodes.Any())
            return false; // If grid area codes is empty, we already retrieved any data across all grid areas

        var actorRole = aggregatedTimeSeriesRequest.RequestedForActorRole;
        if (actorRole is DataHubNames.ActorRole.EnergySupplier or DataHubNames.ActorRole.BalanceResponsibleParty)
        {
            var newAggregationLevel = aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea with { GridAreaCodes = [] };
            var newRequest = aggregatedTimeSeriesRequestMessage with { AggregationPerRoleAndGridArea = newAggregationLevel };
            var parameters = CreateAggregatedTimeSeriesQueryParametersWithoutCalculationType(newRequest);

            var results = _aggregatedTimeSeriesQueries.GetAsync(
                    parameters)
                .ConfigureAwait(false);

            await foreach (var result in results)
            {
                return true;
            }
        }

        return false;
    }

    private AggregatedTimeSeriesQueryParameters CreateAggregatedTimeSeriesQueryParametersWithoutCalculationType(
        AggregatedTimeSeriesRequest request)
    {
        return new AggregatedTimeSeriesQueryParameters(
            request.TimeSeriesTypes.Select(CalculationTimeSeriesTypeMapper.MapTimeSeriesTypeFromEdi).ToList(),
            request.AggregationPerRoleAndGridArea.GridAreaCodes,
            request.AggregationPerRoleAndGridArea.EnergySupplierId,
            request.AggregationPerRoleAndGridArea.BalanceResponsibleId,
            new Period(request.Period.Start, request.Period.End));
    }

    private async Task SendRejectedMessageAsync(IReadOnlyCollection<ValidationError> validationErrors, string referenceId, CancellationToken cancellationToken)
    {
        var message = AggregatedTimeSeriesRequestRejectedMessageFactory.Create(validationErrors, referenceId);
        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendAcceptedMessageAsync(IReadOnlyCollection<AggregatedTimeSeries> results, string referenceId, CancellationToken cancellationToken)
    {
        var message = AggregatedTimeSeriesRequestAcceptedMessageFactory.Create(results, referenceId);
        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }
}
