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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed class SettlementReport
{
    public int Id { get; init; }

    public string RequestId { get; init; } = null!;

    public Guid UserId { get; init; }

    public Guid ActorId { get; init; }

    public Instant CreatedDateTime { get; init; }

    public CalculationType CalculationType { get; init; }

    public bool ContainsBasisData { get; init; }

    public Instant PeriodStart { get; init; }

    public Instant PeriodEnd { get; init; }

    public int GridAreaCount { get; init; }

    public SettlementReportStatus Status { get; private set; }

    public string? BlobFileName { get; private set; }

    public SettlementReport(
        IClock clock,
        Guid userId,
        Guid actorId,
        SettlementReportRequestId requestId,
        SettlementReportRequestDto request)
    {
        RequestId = requestId.Id;
        UserId = userId;
        ActorId = actorId;
        CreatedDateTime = clock.GetCurrentInstant();
        Status = SettlementReportStatus.InProgress;
        CalculationType = request.CalculationType;
        ContainsBasisData = false;
        PeriodStart = request.Filter.PeriodStart.ToInstant();
        PeriodEnd = request.Filter.PeriodEnd.ToInstant();
        GridAreaCount = request.Filter.Calculations.Count;
    }

    // EF Core Constructor.
    // ReSharper disable once UnusedMember.Local
    private SettlementReport()
    {
    }

    public void MarkAsCompleted(GeneratedSettlementReportDto generatedSettlementReport)
    {
        Status = SettlementReportStatus.Completed;
        BlobFileName = generatedSettlementReport.ReportFileName;
    }

    public void MarkAsFailed()
    {
        Status = SettlementReportStatus.Failed;
    }
}
