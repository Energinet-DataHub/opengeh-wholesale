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
using Azure;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports;

internal sealed class SettlementReportDownloadTrigger
{
    private readonly IUserContext<FrontendUser> _userContext;
    private readonly ISettlementReportDownloadHandler _settlementReportDownloadHandler;

    public SettlementReportDownloadTrigger(IUserContext<FrontendUser> userContext, ISettlementReportDownloadHandler settlementReportDownloadHandler)
    {
        _userContext = userContext;
        _settlementReportDownloadHandler = settlementReportDownloadHandler;
    }

    [Function(nameof(SettlementReportDownload))]
    public async Task<HttpResponseData> SettlementReportDownload(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        [FromBody] SettlementReportRequestId settlementReportRequestId,
        FunctionContext executionContext)
    {
        try
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/octet-stream");
            await _settlementReportDownloadHandler
                .DownloadReportAsync(settlementReportRequestId, response.Body, _userContext.CurrentUser.UserId, _userContext.CurrentUser.ActorId)
                .ConfigureAwait(false);
            return response;
        }
        catch (Exception ex) when (ex is InvalidOperationException or RequestFailedException)
        {
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            return response;
        }
    }
}
