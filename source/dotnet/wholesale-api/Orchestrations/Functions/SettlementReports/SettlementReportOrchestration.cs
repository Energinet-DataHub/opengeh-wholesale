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

using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Activities;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using RetryContext = Microsoft.DurableTask.RetryContext;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports;

internal sealed class SettlementReportOrchestration
{
    [Function(nameof(OrchestrateSettlementReport))]
    public async Task<string> OrchestrateSettlementReport(
         [OrchestrationTrigger] TaskOrchestrationContext context,
         FunctionContext executionContext)
    {
        var settlementReportRequest = context.GetInput<SettlementReportRequestDto>();
        if (settlementReportRequest == null)
        {
            return "Error: No input specified.";
        }

        var scatterInput = new ScatterSettlementReportFilesInput(
            context.InstanceId,
            settlementReportRequest);

        var scatterResults = await context
            .CallActivityAsync<SettlementReportFileRequestDto[]>(nameof(ScatterSettlementReportFilesActivity), scatterInput);

        if (string.IsNullOrWhiteSpace(scatterResults.First().SuggestedName))
        {
            throw new Exception("WTF!");
        }

        var coldRetryHandler = TaskOptions.FromRetryHandler(retryContext => HandleColdDataSource(
                retryContext,
                executionContext.GetLogger<SettlementReportOrchestration>()));

        var fileRequestTasks = scatterResults.Select(fileRequest => context
            .CallActivityAsync<GeneratedSettlementReportFileDto>(
                nameof(GenerateSettlementReportFileActivity),
                fileRequest,
                coldRetryHandler));

        var generatedFiles = await Task.WhenAll(fileRequestTasks);

        var generatedSettlementReport = await context.CallActivityAsync<GeneratedSettlementReportDto>(
            nameof(GatherSettlementReportFilesActivity),
            generatedFiles);

        await context.CallActivityAsync(
            nameof(FinalizeSettlementReportActivity),
            generatedSettlementReport);

        return "Success";
    }

    private static bool HandleColdDataSource(RetryContext retryContext, ILogger<SettlementReportOrchestration> logger)
    {
        if (retryContext.LastFailure.ErrorMessage == ISettlementReportDataRepository.DataSourceUnavailableExceptionMessage)
        {
            logger.LogError("GenerateSettlementReportFile databricks failed. Inner exception message: {innerException}.", retryContext.LastFailure.InnerFailure?.ToString());
        }

        return retryContext.LastFailure.ErrorMessage == ISettlementReportDataRepository.DataSourceUnavailableExceptionMessage &&
               retryContext.LastAttemptNumber <= 3;
    }
}
