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
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.EDI.Client;
using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.EDI.Mappers;
using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Microsoft.Extensions.Logging;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.EDI;

public class AggregatedTimeSeriesRequestHandler : IAggregatedTimeSeriesRequestHandler
{
    private readonly IEdiClient _ediClient;
    private readonly IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> _validator;
    private readonly IAggregatedTimeSeriesQueries _aggregatedTimeSeriesQueries;
    private readonly ILogger<AggregatedTimeSeriesRequestHandler> _logger;
    private readonly ICalculationsClient _calculationsClient;
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");
    private static readonly ValidationError _noDataForRequestedGridArea = new("Forkert netområde / invalid grid area", "D46");

    public AggregatedTimeSeriesRequestHandler(
        IEdiClient ediClient,
        IValidator<Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest> validator,
        IAggregatedTimeSeriesQueries aggregatedTimeSeriesQueries,
        ILogger<AggregatedTimeSeriesRequestHandler> logger,
        ICalculationsClient calculationsClient)
    {
        _ediClient = ediClient;
        _validator = validator;
        _aggregatedTimeSeriesQueries = aggregatedTimeSeriesQueries;
        _logger = logger;
        _calculationsClient = calculationsClient;
    }

    public async Task ProcessAsync(ServiceBusReceivedMessage receivedMessage, string referenceId, CancellationToken cancellationToken)
    {
        var aggregatedTimeSeriesRequest = Edi.Requests.AggregatedTimeSeriesRequest.Parser.ParseFrom(receivedMessage.Body);

        var validationErrors = await _validator.ValidateAsync(aggregatedTimeSeriesRequest).ConfigureAwait(false);

        if (validationErrors.Any())
        {
            _logger.LogWarning("Validation errors for message with reference id {reference_id}", referenceId);
            await SendRejectedMessageAsync(validationErrors.ToList(), referenceId, cancellationToken).ConfigureAwait(false);
            return;
        }

        var newestBatchCalculationIdsForPeriod = await _calculationsClient
            .GetNewestCalculationIdsForPeriodAsync(
                filterByGridAreaCodes: aggregatedTimeSeriesRequest.HasGridAreaCode ? new[] { aggregatedTimeSeriesRequest.GridAreaCode } : new string[] { },
                filterByExecutionState: CalculationState.Completed,
                periodStart: InstantPattern.General.Parse(aggregatedTimeSeriesRequest.Period.Start).Value,
                periodEnd: InstantPattern.General.Parse(aggregatedTimeSeriesRequest.Period.End).Value)
            .ConfigureAwait(false);

        var aggregatedTimeSeriesRequestMessage = AggregatedTimeSeriesRequestFactory.Parse(aggregatedTimeSeriesRequest);

        var results = await GetAggregatedTimeSeriesAsync(
            aggregatedTimeSeriesRequestMessage,
            cancellationToken).ConfigureAwait(false);

        if (!results.Any())
        {
            var error = new List<ValidationError> { _noDataAvailable };
            if (await EnergySupplierOrBalanceResponsibleHaveAggregatedTimeSeriesForAnotherGridAreasAsync(aggregatedTimeSeriesRequest, aggregatedTimeSeriesRequestMessage).ConfigureAwait(false))
            {
                error = new List<ValidationError> { _noDataForRequestedGridArea };
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
        var parameters = CreateAggregatedTimeSeriesQueryParametersWithoutProcessType(request);

        if (request.RequestedProcessType == RequestedProcessType.LatestCorrection)
        {
            return await _aggregatedTimeSeriesQueries.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        return await _aggregatedTimeSeriesQueries.GetAsync(
            parameters with
            {
                ProcessType = ProcessTypeMapper.FromRequestedProcessType(request.RequestedProcessType),
            }).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> EnergySupplierOrBalanceResponsibleHaveAggregatedTimeSeriesForAnotherGridAreasAsync(
        Edi.Requests.AggregatedTimeSeriesRequest aggregatedTimeSeriesRequest,
        AggregatedTimeSeriesRequest aggregatedTimeSeriesRequestMessage)
    {
        if (aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea.GridAreaCode == null)
            return false;

        var actorRole = aggregatedTimeSeriesRequest.RequestedByActorRole;
        if (actorRole == ActorRoleCode.EnergySupplier || actorRole == ActorRoleCode.BalanceResponsibleParty)
        {
            var newAggregationLevel = aggregatedTimeSeriesRequestMessage.AggregationPerRoleAndGridArea with { GridAreaCode = null };
            var newRequest = aggregatedTimeSeriesRequestMessage with { AggregationPerRoleAndGridArea = newAggregationLevel };
            var parameters = CreateAggregatedTimeSeriesQueryParametersWithoutProcessType(newRequest);

            var results = _aggregatedTimeSeriesQueries.GetAsync(
                    parameters with { ProcessType = ProcessTypeMapper.FromRequestedProcessType(newRequest.RequestedProcessType), })
                .ConfigureAwait(false);

            await foreach (var result in results)
            {
                return true;
            }
        }

        return false;
    }

    private static AggregatedTimeSeriesQueryParameters CreateAggregatedTimeSeriesQueryParametersWithoutProcessType(
        AggregatedTimeSeriesRequest request)
    {
        var parameters = new AggregatedTimeSeriesQueryParameters(
            CalculationTimeSeriesTypeMapper.MapTimeSeriesTypeFromEdi(request.TimeSeriesType),
            request.Period.Start,
            request.Period.End,
            request.AggregationPerRoleAndGridArea.GridAreaCode,
            request.AggregationPerRoleAndGridArea.EnergySupplierId,
            request.AggregationPerRoleAndGridArea.BalanceResponsibleId);
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
