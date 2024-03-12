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
using Energinet.DataHub.Wholesale.Edi.Calculations;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Factories;
using Energinet.DataHub.Wholesale.Edi.Mappers;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Microsoft.Extensions.Logging;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.Edi;

/// <summary>
/// Handles WholesaleServicesRequest messages (typically received from the EDI subsystem through the WholesaleInbox service bus queue)
/// </summary>
public class WholesaleServicesRequestHandler : IWholesaleInboxRequestHandler
{
    // TODO: Is this the correct error code?
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");
    private static readonly ValidationError _noDataForRequestedGridArea = new("Forkert netområde / invalid grid area", "D46");

    private readonly IEdiClient _ediClient;
    private readonly IValidator<Energinet.DataHub.Edi.Requests.WholesaleServicesRequest> _validator;
    private readonly ILogger<WholesaleServicesRequestHandler> _logger;
    private readonly CompletedCalculationRetriever _completedCalculationRetriever;
    private readonly IWholesaleResultQueries _wholesaleResultQueries;
    private readonly WholesaleServicesRequestMapper _wholesaleServicesRequestMapper;

    public WholesaleServicesRequestHandler(
        IEdiClient ediClient,
        IValidator<Energinet.DataHub.Edi.Requests.WholesaleServicesRequest> validator,
        CompletedCalculationRetriever completedCalculationRetriever,
        IWholesaleResultQueries wholesaleResultQueries,
        WholesaleServicesRequestMapper wholesaleServicesRequestMapper,
        ILogger<WholesaleServicesRequestHandler> logger)
    {
        _ediClient = ediClient;
        _validator = validator;
        _completedCalculationRetriever = completedCalculationRetriever;
        _wholesaleResultQueries = wholesaleResultQueries;
        _wholesaleServicesRequestMapper = wholesaleServicesRequestMapper;
        _logger = logger;
    }

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
        var data = await GetWholesaleServicesDataAsync(request, cancellationToken).ConfigureAwait(false);

        if (!data.Any())
        {
          var errors = new List<ValidationError>
          {
              await HasDataInAnotherGridAreaAsync(incomingRequest, request, cancellationToken).ConfigureAwait(false)
                  ? _noDataForRequestedGridArea
                  : _noDataAvailable,
          };

          _logger.LogInformation("No data available for WholesaleServicesRequest message with reference id {reference_id}", referenceId);
          await SendRejectedMessageAsync(errors, referenceId, cancellationToken).ConfigureAwait(false);
          return;
        }

        _logger.LogInformation("Sending WholesaleServicesRequest accepted message with reference id {reference_id}", referenceId);
        await SendAcceptedMessageAsync(data, referenceId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<object>> GetWholesaleServicesDataAsync(
        WholesaleServicesRequest request,
        CancellationToken cancellationToken)
    {
        var latestCalculationsForRequest = await _completedCalculationRetriever.GetLatestCompletedCalculationsForPeriodAsync(
                request.GridArea,
                new Period(request.Period.Start, request.Period.End),
                request.RequestedCalculationType)
            .ConfigureAwait(true);

        // TODO: Implement data lookup
        // var queryParameters = CreateQueryParameters(request, latestCalculationsForRequest);
        // var queryResult = await _wholesaleResultQueries.GetAsync(queryParameters).ToListAsync(cancellationToken).ConfigureAwait(false);
        // return queryResult;
        return
        [
            new()
        ];
    }

    private async Task<bool> HasDataInAnotherGridAreaAsync(
        Energinet.DataHub.Edi.Requests.WholesaleServicesRequest incomingRequest,
        WholesaleServicesRequest request,
        CancellationToken cancellationToken)
    {
        if (request.GridArea == null) // If grid area is null, we already retrieved all data across multiple grid areas
            return false;

        var actorRole = incomingRequest.RequestedByActorRole;
        if (actorRole is ActorRoleCode.EnergySupplier or ActorRoleCode.BalanceResponsibleParty)
        {
            var requestWithoutGridArea = request with
            {
                GridArea = null,
            };

            var results = await GetWholesaleServicesDataAsync(request, cancellationToken).ConfigureAwait(false);

            if (results.Any())
                return true;
            // await foreach (var result in results)
            // {
            //     return true;
            // }
        }

        return false;
    }

    private Task SendRejectedMessageAsync(IReadOnlyCollection<ValidationError> validationErrors, string referenceId, CancellationToken cancellationToken)
    {
        // TODO: Implement rejected message
        throw new NotImplementedException();
    }

    private Task SendAcceptedMessageAsync(IReadOnlyCollection<object> results, string referenceId, CancellationToken cancellationToken)
    {
        // TODO: Implement accepted message
        return Task.CompletedTask;
    }
}
