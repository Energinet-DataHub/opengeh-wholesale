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

using Energinet.DataHub.Wholesale.Common.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;

public sealed class CalculationResult
{
    public CalculationResult(
        Guid batchId,
        string gridArea,
        TimeSeriesType timeSeriesType,
        string? energySupplierId,
        string? balanceResponsibleId,
        TimeSeriesPoint[] timeSeriesPoints,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd)
    {
        if (timeSeriesPoints.Length == 0)
            throw new ArgumentException("Time series points empty");

        BatchId = batchId;
        GridArea = gridArea;
        TimeSeriesType = timeSeriesType;
        EnergySupplierId = energySupplierId;
        BalanceResponsibleId = balanceResponsibleId;
        TimeSeriesPoints = timeSeriesPoints;
        ProcessType = processType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
    }

    public Guid BatchId { get; }

    public ProcessType ProcessType { get; }

    public string GridArea { get; }

    public TimeSeriesType TimeSeriesType { get; private set; }

    public string? EnergySupplierId { get; private set; }

    public string? BalanceResponsibleId { get; private set; }

    public Instant PeriodStart { get; }

    public Instant PeriodEnd { get; }

    public TimeSeriesPoint[] TimeSeriesPoints { get; private set; }
}
