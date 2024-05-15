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
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed class GetSettlementReportsHandler : IGetSettlementReportsHandler
{
    private readonly IClock _clock;
    private readonly ISettlementReportRepository _settlementReportRepository;
    private readonly ISettlementReportFileRepository _settlementReportFileRepository;

    public GetSettlementReportsHandler(
        IClock clock,
        ISettlementReportRepository settlementReportRepository,
        ISettlementReportFileRepository settlementReportFileRepository)
    {
        _clock = clock;
        _settlementReportRepository = settlementReportRepository;
        _settlementReportFileRepository = settlementReportFileRepository;
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetAsync()
    {
        var settlementReports = await _settlementReportRepository.GetAsync().ConfigureAwait(false);
        return await FilterOutExpiredReportsAsync(settlementReports).ConfigureAwait(false);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetAsync(Guid userId, Guid actorId)
    {
        var settlementReports = await _settlementReportRepository.GetAsync(userId, actorId).ConfigureAwait(false);
        return await FilterOutExpiredReportsAsync(settlementReports).ConfigureAwait(false);
    }

    private async Task<IEnumerable<RequestedSettlementReportDto>> FilterOutExpiredReportsAsync(IEnumerable<SettlementReport> settlementReports)
    {
        var currentReports = new List<RequestedSettlementReportDto>();
        var cutOffPeriod = _clock
            .GetCurrentInstant()
            .Minus(TimeSpan.FromDays(7).ToDuration());

        foreach (var settlementReport in settlementReports)
        {
            if (settlementReport.Status == SettlementReportStatus.InProgress ||
                settlementReport.CreatedDateTime > cutOffPeriod)
            {
                currentReports.Add(Map(settlementReport));
            }
            else
            {
                if (settlementReport.BlobFileName != null)
                {
                    await _settlementReportFileRepository
                        .DeleteAsync(new SettlementReportRequestId(settlementReport.RequestId), settlementReport.BlobFileName)
                        .ConfigureAwait(false);
                }

                await _settlementReportRepository
                    .DeleteAsync(settlementReport)
                    .ConfigureAwait(false);
            }
        }

        return currentReports;
    }

    private static RequestedSettlementReportDto Map(SettlementReport report)
    {
        return new RequestedSettlementReportDto(
            new SettlementReportRequestId(report.RequestId),
            report.CalculationType,
            report.PeriodStart.ToDateTimeOffset(),
            report.PeriodEnd.ToDateTimeOffset(),
            report.Status,
            report.GridAreaCount,
            0,
            report.ActorId,
            report.ContainsBasisData);
    }
}
