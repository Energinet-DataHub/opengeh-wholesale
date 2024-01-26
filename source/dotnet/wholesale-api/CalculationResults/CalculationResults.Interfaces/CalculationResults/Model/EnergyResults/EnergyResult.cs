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

using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

public sealed class EnergyResult : AggregatedTimeSeries
{
    public EnergyResult(
        Guid id,
        Guid batchId,
        string gridArea,
        TimeSeriesType timeSeriesType,
        string? energySupplierId,
        string? balanceResponsibleId,
        EnergyTimeSeriesPoint[] timeSeriesPoints,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? fromGridArea,
        long version)
    : base(gridArea, timeSeriesPoints, timeSeriesType, processType, batchId)
    {
        Id = id;
        EnergySupplierId = energySupplierId;
        BalanceResponsibleId = balanceResponsibleId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        FromGridArea = fromGridArea;
        MeteringPointId = string.Empty; // TODO: use constructor parameter instead - waiting the columns to be included in the delta table
        Version = version;
    }

    public Guid Id { get; }

    public string? FromGridArea { get; }

    public string? EnergySupplierId { get; private set; }

    public string? BalanceResponsibleId { get; private set; }

    public Instant PeriodStart { get; }

    public Instant PeriodEnd { get; }

    public string? MeteringPointId { get; private set; }

    public long Version { get; }
}
