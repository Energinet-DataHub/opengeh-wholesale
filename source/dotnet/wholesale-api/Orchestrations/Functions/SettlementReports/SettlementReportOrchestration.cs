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

using Azure;
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

        var dataSourceExceptionHandler = TaskOptions.FromRetryHandler(retryContext => HandleDataSourceExceptions(
                retryContext,
                executionContext.GetLogger<SettlementReportOrchestration>()));

        var scatterResults = await context
            .CallActivityAsync<IEnumerable<SettlementReportFileRequestDto>>(
                nameof(ScatterSettlementReportFilesActivity),
                scatterInput,
                dataSourceExceptionHandler);

        var fileRequestTasks = scatterResults.Select(fileRequest => context
            .CallActivityAsync<GeneratedSettlementReportFileDto>(
                nameof(GenerateSettlementReportFileActivity),
                fileRequest,
                dataSourceExceptionHandler));

        var generatedFiles = await Task.WhenAll(fileRequestTasks);

        var generatedSettlementReport = await context.CallActivityAsync<GeneratedSettlementReportDto>(
            nameof(GatherSettlementReportFilesActivity),
            generatedFiles);

        await context.CallActivityAsync(
            nameof(FinalizeSettlementReportActivity),
            generatedSettlementReport);

        return "Success";
    }

    private static bool HandleDataSourceExceptions(RetryContext retryContext, ILogger<SettlementReportOrchestration> logger)
    {
        // When running ScatterSettlementReportFilesActivity or GenerateSettlementReportFile, the call to the data source may time out for several reasons:
        // 1) The server is stopped, but requesting the data has triggered a startup. It should come online within 3 retries.
        // 2) The query for getting the data timed out. It is not known if query will succeed, but we are trying 3 to 6 times.
        if (retryContext.LastFailure.ErrorMessage == ISettlementReportDataRepository.DataSourceUnavailableExceptionMessage)
        {
            logger.LogError("Databricks data source failed. Inner exception message: {innerException}.", retryContext.LastFailure.InnerFailure?.ToString());
            return retryContext.LastAttemptNumber <= 6;
        }

        // In case the error is not related to the data source, we are retrying thrice to take care of transient failures:
        // 1) From SQL.
        // 2) From BlobStorage.
        if (retryContext.LastFailure.ErrorType.Contains("SqlException") ||
            retryContext.LastFailure.ErrorType == typeof(RequestFailedException).FullName)
        {
            return retryContext.LastAttemptNumber <= 3;
        }

        return false;
    }
}
