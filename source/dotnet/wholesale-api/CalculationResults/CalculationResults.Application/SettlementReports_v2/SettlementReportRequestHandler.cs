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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed class SettlementReportRequestHandler : ISettlementReportRequestHandler
{
    //TODO: move to config?
    public const long ThresholdToSplitFiles = 10000;

    public Task<IEnumerable<SettlementReportFileRequestDto>> RequestReportAsync(
        SettlementReportRequestId requestId,
        SettlementReportRequestDto reportRequest)
    {
        IEnumerable<SettlementReportFileRequestDto> filesToRequest;

        switch (reportRequest.CalculationType)
        {
            case CalculationType.BalanceFixing:
                filesToRequest = RequestFilesForAggregatedEnergyResults(requestId, reportRequest, SettlementReportFileContent.BalanceFixingResult);
                break;

            case CalculationType.WholesaleFixing:
                filesToRequest = RequestFilesForAggregatedEnergyResults(requestId, reportRequest, SettlementReportFileContent.WholesaleFixingResult);
                break;
            default:
                throw new InvalidOperationException($"Cannot generate report for calculation type {reportRequest.CalculationType}.");
        }

        return Task.FromResult(filesToRequest);
    }

    private static IEnumerable<SettlementReportFileRequestDto> RequestFilesForAggregatedEnergyResults(
       SettlementReportRequestId requestId,
       SettlementReportRequestDto reportRequest,
       SettlementReportFileContent settlementReportFileContent)
    {
        var filesToGenerate = new List<SettlementReportFileRequestDto>();

        if (reportRequest is { SplitReportPerGridArea: true, Filter.Calculations.Count: > 1 })
        {
            //TODO: check the logic here, when repository is available
            foreach (var calculation in reportRequest.Filter.Calculations)
            {
                double randomNumber = new Random().Next(5000, 100000); //TODO: replace randomNumber with a call to repository when available
                var parts = Math.Ceiling(randomNumber / ThresholdToSplitFiles);
                for (var index = 0; index < parts; index++)
                {
                    var partialInfo = parts > 1 ? new SettlementReportRequestPartialInfo(index) : null;
                    filesToGenerate.Add(new SettlementReportFileRequestDto(
                        settlementReportFileContent,
                        $"Result Energy ({calculation.GridAreaCode})",
                        requestId,
                        reportRequest.Filter with { Calculations = [calculation], PartialInfo = partialInfo }));
                }
            }
        }
        else
        {
            double randomNumber = new Random().Next(5000, 100000); //TODO: replace randomNumber with a call to repository when available
            var parts = Math.Ceiling(randomNumber / ThresholdToSplitFiles);
            for (var index = 0; index < parts; index++)
            {
                var partialInfo = parts > 1 ? new SettlementReportRequestPartialInfo(index) : null;
                filesToGenerate.Add(new SettlementReportFileRequestDto(
                    settlementReportFileContent,
                    "Result Energy",
                    requestId,
                    reportRequest.Filter with { PartialInfo = partialInfo }));
            }
        }

        return filesToGenerate;
    }
}
