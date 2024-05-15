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

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed class GetSettlementReportsHandler : IGetSettlementReportsHandler
{
    private readonly ISettlementReportRepository _settlementReportRepository;
    private readonly IRemoveExpiredSettlementReports _removeExpiredSettlementReports;

    public GetSettlementReportsHandler(
        ISettlementReportRepository settlementReportRepository,
        IRemoveExpiredSettlementReports removeExpiredSettlementReports)
    {
        _settlementReportRepository = settlementReportRepository;
        _removeExpiredSettlementReports = removeExpiredSettlementReports;
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetAsync()
    {
        var settlementReports = (await _settlementReportRepository
                .GetAsync()
                .ConfigureAwait(false))
            .ToList();

        await _removeExpiredSettlementReports.RemoveExpiredAsync(settlementReports).ConfigureAwait(false);
        return settlementReports.Select(Map);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetAsync(Guid userId, Guid actorId)
    {
        var settlementReports = (await _settlementReportRepository
                .GetAsync(userId, actorId)
                .ConfigureAwait(false))
            .ToList();

        await _removeExpiredSettlementReports.RemoveExpiredAsync(settlementReports).ConfigureAwait(false);
        return settlementReports.Select(Map);
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
