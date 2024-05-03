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

public sealed class EnergyResult(
    Guid id,
    Guid calculationId,
    string gridArea,
    TimeSeriesType timeSeriesType,
    string? energySupplierId,
    string? balanceResponsibleId,
    EnergyTimeSeriesPoint[] timeSeriesPoints,
    CalculationType calculationType,
    Instant periodStart,
    Instant periodEnd,
    string? fromGridArea,
    string? meteringPointId,
    Resolution resolution,
    long version)
    : AggregatedTimeSeries(gridArea,
        timeSeriesPoints,
        timeSeriesType,
        calculationType,
        periodStart,
        periodEnd,
        resolution,
        version)
{
    public Guid Id { get; } = id;

    public Guid CalculationId { get; } = calculationId;

    public string? FromGridArea { get; } = fromGridArea;

    public string? EnergySupplierId { get; private set; } = energySupplierId;

    public string? BalanceResponsibleId { get; private set; } = balanceResponsibleId;

    public string? MeteringPointId { get; } = meteringPointId;
}
