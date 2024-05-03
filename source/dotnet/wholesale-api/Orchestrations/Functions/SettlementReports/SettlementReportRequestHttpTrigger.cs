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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports;

internal sealed class SettlementReportRequestHttpTrigger
{
    [Function(nameof(RequestSettlementReport))]
    public async Task<HttpResponseData> RequestSettlementReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [FromBody] SettlementReportRequestDto settlementReportRequest,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var instanceId = await client
            .ScheduleNewOrchestrationInstanceAsync(nameof(SettlementReportOrchestrator.OrchestrateSettlementReportAsync), settlementReportRequest)
            .ConfigureAwait(false);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response
            .WriteAsJsonAsync(new SettlementReportHttpResponse(new SettlementReportRequestId(instanceId)))
            .ConfigureAwait(false);

        return response;
    }
}
