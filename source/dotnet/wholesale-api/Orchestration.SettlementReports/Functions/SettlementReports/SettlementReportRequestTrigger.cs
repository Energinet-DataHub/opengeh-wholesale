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

using System.Net;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
using Energinet.DataHub.Wholesale.Orchestration.SettlementReports.Functions.SettlementReports.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace Energinet.DataHub.Wholesale.Orchestration.SettlementReports.Functions.SettlementReports;

internal sealed class SettlementReportRequestTrigger
{
    private readonly IUserContext<FrontendUser> _userContext;
    private readonly IGridAreaOwnershipClient _gridAreaOwnershipClient;
    private readonly ISettlementReportInitializeHandler _settlementReportInitializeHandler;

    public SettlementReportRequestTrigger(
        IUserContext<FrontendUser> userContext,
        IGridAreaOwnershipClient gridAreaOwnershipClient,
        ISettlementReportInitializeHandler settlementReportInitializeHandler)
    {
        _userContext = userContext;
        _gridAreaOwnershipClient = gridAreaOwnershipClient;
        _settlementReportInitializeHandler = settlementReportInitializeHandler;
    }

    [Function(nameof(RequestSettlementReport))]
    public async Task<HttpResponseData> RequestSettlementReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        [FromBody] SettlementReportRequestDto settlementReportRequest,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        if (_userContext.CurrentUser.Actor.MarketRole == FrontendActorMarketRole.EnergySupplier && string.IsNullOrWhiteSpace(settlementReportRequest.Filter.EnergySupplier))
        {
            settlementReportRequest = settlementReportRequest with
            {
                Filter = settlementReportRequest.Filter with
                {
                    EnergySupplier = _userContext.CurrentUser.Actor.ActorNumber,
                },
            };
        }

        if (!await IsValidAsync(settlementReportRequest).ConfigureAwait(false))
        {
            return req.CreateResponse(HttpStatusCode.Forbidden);
        }

        if (settlementReportRequest.Filter.CalculationType != CalculationType.BalanceFixing)
        {
            if (settlementReportRequest.Filter.GridAreas.Any(kv => kv.Value is null))
                return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var marketRole = _userContext.CurrentUser.Actor.MarketRole switch
        {
            FrontendActorMarketRole.Other => MarketRole.Other,
            FrontendActorMarketRole.GridAccessProvider => MarketRole.GridAccessProvider,
            FrontendActorMarketRole.EnergySupplier => MarketRole.EnergySupplier,
            FrontendActorMarketRole.SystemOperator => MarketRole.SystemOperator,
            FrontendActorMarketRole.DataHubAdministrator => MarketRole.DataHubAdministrator,
            _ => throw new ArgumentOutOfRangeException(nameof(_userContext.CurrentUser.Actor.MarketRole)),
        };

        var chargeOwnerId = marketRole is MarketRole.GridAccessProvider or MarketRole.SystemOperator
            ? _userContext.CurrentUser.Actor.ActorNumber
            : null;

        var actorInfo = new SettlementReportRequestedByActor(marketRole, chargeOwnerId);

        var instanceId = await client
            .ScheduleNewOrchestrationInstanceAsync(nameof(SettlementReportOrchestration.OrchestrateSettlementReport), new SettlementReportRequestInput(settlementReportRequest, actorInfo))
            .ConfigureAwait(false);

        var requestId = new SettlementReportRequestId(instanceId);

        await _settlementReportInitializeHandler
            .InitializeAsync(
                _userContext.CurrentUser.UserId,
                _userContext.CurrentUser.Actor.ActorId,
                _userContext.CurrentUser.MultiTenancy,
                requestId,
                settlementReportRequest)
            .ConfigureAwait(false);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response
            .WriteAsJsonAsync(new SettlementReportHttpResponse(requestId))
            .ConfigureAwait(false);

        return response;
    }

    private async Task<bool> IsValidAsync(SettlementReportRequestDto req)
    {
        if (_userContext.CurrentUser.MultiTenancy)
        {
            return true;
        }

        var marketRole = _userContext.CurrentUser.Actor.MarketRole;

        if (marketRole == FrontendActorMarketRole.GridAccessProvider)
        {
            if (!string.IsNullOrWhiteSpace(req.Filter.EnergySupplier))
            {
                return false;
            }

            var actorsGridAreas = await _gridAreaOwnershipClient
                .GetOwnedByAsync(_userContext.CurrentUser.Actor.ActorNumber)
                .ConfigureAwait(false);

            return req.Filter.GridAreas.All(x => actorsGridAreas.Contains(x.Key));
        }

        if (marketRole == FrontendActorMarketRole.EnergySupplier)
        {
            return req.Filter.EnergySupplier == _userContext.CurrentUser.Actor.ActorNumber;
        }

        if (marketRole == FrontendActorMarketRole.SystemOperator &&
            req.Filter.CalculationType != CalculationType.BalanceFixing &&
            req.Filter.CalculationType != CalculationType.Aggregation)
        {
            return true;
        }

        return false;
    }
}
