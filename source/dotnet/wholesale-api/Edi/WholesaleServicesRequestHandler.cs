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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Edi;

/// <summary>
/// Handles WholesaleServicesRequest messages (typically received from EDI through the WholesaleInbox service bus queue)
/// </summary>
public class WholesaleServicesRequestHandler : IWholesaleInboxRequestHandler
{
    private readonly IEdiClient _ediClient;
    private readonly IValidator<Energinet.DataHub.Edi.Requests.WholesaleServicesRequest> _validator;
    private readonly ILogger<WholesaleServicesRequestHandler> _logger;

    // TODO: Is this the correct error code?
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");

    public WholesaleServicesRequestHandler(
        IEdiClient ediClient,
        IValidator<Energinet.DataHub.Edi.Requests.WholesaleServicesRequest> validator,
        ILogger<WholesaleServicesRequestHandler> logger)
    {
        _ediClient = ediClient;
        _validator = validator;
        _logger = logger;
    }

    public bool CanHandle(string requestSubject) => requestSubject.Equals(Energinet.DataHub.Edi.Requests.WholesaleServicesRequest.Descriptor.Name);

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        var request = Energinet.DataHub.Edi.Requests.WholesaleServicesRequest.Parser.ParseFrom(receivedMessage.Body);

        var validationErrors = await _validator.ValidateAsync(request).ConfigureAwait(false);

        if (validationErrors.Any())
        {
            _logger.LogWarning("Validation errors for message with reference id {reference_id}", referenceId);
            await SendRejectedMessageAsync(validationErrors.ToList(), referenceId, cancellationToken).ConfigureAwait(false);
            return;
        }

        var data = await GetWholesaleServicesDataAsync(request, cancellationToken).ConfigureAwait(false);
        /* TODO: Check if data exists, else return rejected
        if (!data.Any())
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
        */

        _logger.LogInformation("Sending message with reference id {reference_id}", referenceId);
        await SendAcceptedMessageAsync(data, referenceId, cancellationToken).ConfigureAwait(false);
    }

    private Task<List<object>> GetWholesaleServicesDataAsync(
        Energinet.DataHub.Edi.Requests.WholesaleServicesRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Implement data lookup
        return Task.FromResult(new List<object>());
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
