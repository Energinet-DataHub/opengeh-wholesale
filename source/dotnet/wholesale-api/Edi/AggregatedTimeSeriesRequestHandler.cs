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
using Energinet.DataHub.Wholesale.Edi.Factories;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Microsoft.Extensions.Logging;

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
            _logger.LogWarning("Validation errors for message with reference id {reference_id}", referenceId);
            await SendRejectedMessageAsync(validationErrors.ToList(), referenceId, cancellationToken).ConfigureAwait(false);
            return;
        }

        var aggregatedTimeSeriesRequestMessage = AggregatedTimeSeriesRequestFactory.Parse(aggregatedTimeSeriesRequest);
        var results = await GetAggregatedTimeSeriesAsync(
            aggregatedTimeSeriesRequestMessage,
            cancellationToken).ConfigureAwait(false);

        if (!results.Any())
        {
            var error = new List<ValidationError> { _noDataAvailable };
            if (await EnergySupplierOrBalanceResponsibleHaveAggregatedTimeSeriesForAnotherGridAreasAsync(aggregatedTimeSeriesRequest, aggregatedTimeSeriesRequestMessage).ConfigureAwait(false))
            {
                error = [_noDataForRequestedGridArea];
            }

            _logger.LogInformation("No data available for message with reference id {reference_id}", referenceId);
            await SendRejectedMessageAsync(error, referenceId, cancellationToken).ConfigureAwait(false);
            return;
        }

        _logger.LogInformation("Sending message with reference id {reference_id}", referenceId);
        await SendAcceptedMessageAsync(results, referenceId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyCollection<AggregatedTimeSeries>> GetAggregatedTimeSeriesAsync(
        AggregatedTimeSeriesRequest request,
        CancellationToken cancellationToken)
    {
        var parameters = await CreateAggregatedTimeSeriesQueryParametersForLatestCalculationsAsync(request).ConfigureAwait(false);

        return await _aggregatedTimeSeriesQueries.GetAsync(
            parameters).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> EnergySupplierOrBalanceResponsibleHaveAggregatedTimeSeriesForAnotherGridAreasAsync(
        Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest,
        AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage)
    {
        if (aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea.GridAreaCode == null)
            return false;

        var actorRole = aggregatedTimeSeriesRequest.RequestedByActorRole;
        if (actorRole is ActorRoleCode.EnergySupplier or ActorRoleCode.BalanceResponsibleParty)
        {
            var newAggregationLevel = aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea with { GridAreaCode = null };
            var newRequest = aggregatedTimeSeriesRequestMessage with { AggregationPerRoleAndGridArea = newAggregationLevel };
            var parameters = await CreateAggregatedTimeSeriesQueryParametersForLatestCalculationsAsync(newRequest).ConfigureAwait(false);

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

    private async Task<AggregatedTimeSeriesQueryParameters> CreateAggregatedTimeSeriesQueryParametersForLatestCalculationsAsync(
        AggregatedTimeSeriesRequest request)
    {
        var latestCalculationsForRequest = await _completedCalculationRetriever.GetLatestCompletedCalculationForRequestAsync(
                request.AggregationPerRoleAndGridArea.GridAreaCode,
                request.Period,
                request.RequestedCalculationType)
            .ConfigureAwait(true);

        var parameters = new AggregatedTimeSeriesQueryParameters(
            request.TimeSeriesTypes.Select(CalculationTimeSeriesTypeMapper.MapTimeSeriesTypeFromEdi).ToList(),
            request.AggregationPerRoleAndGridArea.GridAreaCode,
            request.AggregationPerRoleAndGridArea.EnergySupplierId,
            request.AggregationPerRoleAndGridArea.BalanceResponsibleId,
            latestCalculationsForRequest);
        return parameters;
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
