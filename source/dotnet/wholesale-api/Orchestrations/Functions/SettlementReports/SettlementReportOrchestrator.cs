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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Activities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports;

internal sealed class SettlementReportOrchestrator
{
    [Function(nameof(OrchestrateSettlementReportAsync))]
    public async Task<string> OrchestrateSettlementReportAsync(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        FunctionContext executionContext)
    {
        var settlementReportRequest = context.GetInput<SettlementReportRequestDto>();
        if (settlementReportRequest == null)
        {
            // TODO: What do we return here?
            return "Error: No input specified.";
        }

        var scatterResults = await context
            .CallActivityAsync<IEnumerable<SettlementReportFileRequestDto>>(nameof(ScatterSettlementReportFiles), settlementReportRequest)
            .ConfigureAwait(false);

        var fileRequests = scatterResults.Select(settlementReportFileRequest =>
            context.CallActivityAsync<GeneratedSettlementReportFileDto>(nameof(GenerateSettlementReportFile), settlementReportFileRequest));

        var generatedFiles = await Task
            .WhenAll(fileRequests)
            .ConfigureAwait(false);

        var generatedSettlementReport = await context.CallActivityAsync<GeneratedSettlementReportDto>(
            nameof(GatherSettlementReportFiles),
            generatedFiles).ConfigureAwait(false);

        await context.CallActivityAsync(
            nameof(FinalizeSettlementReport),
            generatedSettlementReport).ConfigureAwait(false);

        // calculationMetadata.Progress = "??";
        // context.SetCustomStatus(calculationMetadata);
        return "Success";
    }
}
