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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Factories;
using Energinet.DataHub.Wholesale.Edi.Factories.WholesaleServices;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Events.Interfaces;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;

namespace Energinet.DataHub.Wholesale.Edi;

/// <summary>
/// Handles WholesaleServicesRequest messages (typically received from the EDI subsystem through the WholesaleInbox service bus queue)
/// </summary>
public class WholesaleServicesRequestHandler(
    IEdiClient ediClient,
    IValidator<Energinet.DataHub.Edi.Requests.WholesaleServicesRequest> validator,
    IWholesaleServicesQueries wholesaleServicesQueries,
    WholesaleServicesRequestMapper wholesaleServicesRequestMapper,
    ILogger<WholesaleServicesRequestHandler> logger)
    : IWholesaleInboxRequestHandler
{
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");
    private static readonly ValidationError _noDataForRequestedGridArea = new("Forkert netområde / invalid grid area", "D46");

    private readonly IEdiClient _ediClient = ediClient;
    private readonly IValidator<Energinet.DataHub.Edi.Requests.WholesaleServicesRequest> _validator = validator;
    private readonly ILogger<WholesaleServicesRequestHandler> _logger = logger;
    private readonly IWholesaleServicesQueries _wholesaleServicesQueries = wholesaleServicesQueries;
    private readonly WholesaleServicesRequestMapper _wholesaleServicesRequestMapper = wholesaleServicesRequestMapper;

    public bool CanHandle(string requestSubject) => requestSubject.Equals(Energinet.DataHub.Edi.Requests.WholesaleServicesRequest.Descriptor.Name);

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        var incomingRequest = Energinet.DataHub.Edi.Requests.WholesaleServicesRequest.Parser.ParseFrom(receivedMessage.Body);

        var validationErrors = await _validator.ValidateAsync(incomingRequest).ConfigureAwait(false);

        if (validationErrors.Any())
        {
            _logger.LogWarning("Validation errors for WholesaleServicesRequest message with reference id {reference_id}", referenceId);
            await SendRejectedMessageAsync(validationErrors.ToList(), referenceId, cancellationToken).ConfigureAwait(false);
            return;
        }

        var request = _wholesaleServicesRequestMapper.Map(incomingRequest);
        var queryParameters = GetWholesaleResultQueryParameters(request);

        var calculationResults = await _wholesaleServicesQueries.GetAsync(queryParameters).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (!calculationResults.Any())
        {
            await SendNoDateRejectMessageAsync(referenceId, cancellationToken, incomingRequest, queryParameters)
                .ConfigureAwait(false);
            return;
        }

        _logger.LogInformation("Sending WholesaleServicesRequest accepted message with reference id {reference_id}", referenceId);
        await SendAcceptedMessageAsync(calculationResults, referenceId, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendNoDateRejectMessageAsync(
        string referenceId,
        CancellationToken cancellationToken,
        DataHub.Edi.Requests.WholesaleServicesRequest incomingRequest,
        WholesaleServicesQueryParameters queryParameters)
    {
        var errors = new List<ValidationError>
        {
            await HasDataInAnotherGridAreaAsync(incomingRequest.RequestedForActorRole, queryParameters).ConfigureAwait(false)
                ? _noDataForRequestedGridArea
                : _noDataAvailable,
        };

        _logger.LogInformation("No data available for WholesaleServicesRequest message with reference id {reference_id}", referenceId);
        await SendRejectedMessageAsync(errors, referenceId, cancellationToken).ConfigureAwait(false);
    }

    private WholesaleServicesQueryParameters GetWholesaleResultQueryParameters(WholesaleServicesRequest request)
    {
        return new WholesaleServicesQueryParameters(
            request.AmountType,
            request.GridAreaCodes,
            request.EnergySupplierId,
            request.ChargeOwnerId,
            request.ChargeTypes.Select(c => (c.ChargeCode, c.ChargeType)).ToList(),
            CalculationTypeMapper.FromRequestedCalculationType(request.RequestedCalculationType),
            new Period(request.Period.Start, request.Period.End));
    }

    private async Task<bool> HasDataInAnotherGridAreaAsync(
        string? requestedByActorRole,
        WholesaleServicesQueryParameters queryParameters)
    {
        if (queryParameters.GridAreaCodes.Count == 0) // If grid area codes is empty, we already retrieved any data across all grid areas
            return false;

        if (requestedByActorRole is DataHubNames.ActorRole.EnergySupplier or DataHubNames.ActorRole.SystemOperator)
        {
            var queryParametersWithoutGridArea = queryParameters with
            {
                GridAreaCodes = Array.Empty<string>(),
            };

            var anyResultsExists = await _wholesaleServicesQueries.AnyAsync(queryParametersWithoutGridArea).ConfigureAwait(false);

            return anyResultsExists;
        }

        return false;
    }

    private async Task SendRejectedMessageAsync(IReadOnlyCollection<ValidationError> validationErrors, string referenceId, CancellationToken cancellationToken)
    {
        var message = WholesaleServicesRequestRejectedMessageFactory.Create(validationErrors, referenceId);
        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendAcceptedMessageAsync(IReadOnlyCollection<WholesaleServices> results, string referenceId, CancellationToken cancellationToken)
    {
        var message = WholesaleServiceRequestAcceptedMessageFactory.Create(results, referenceId);
        await _ediClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
    }
}
