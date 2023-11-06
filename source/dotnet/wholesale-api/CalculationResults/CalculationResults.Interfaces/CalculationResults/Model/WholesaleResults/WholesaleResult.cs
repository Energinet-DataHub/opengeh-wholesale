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

namespace Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

public sealed class WholesaleResult
{
    public WholesaleResult(
        Guid id,
        Guid calculationId,
        ProcessType calculationType,
        Instant periodStart,
        Instant periodEnd,
        string gridArea,
        string energySupplierId,
        AmountType amountType,
        string chargeCode,
        ChargeType chargeType,
        string chargeOwnerId,
        bool isTax,
        QuantityUnit quantityUnit,
        ChargeResolution chargeResolution,
        MeteringPointType? meteringPointType,
        SettlementMethod? settlementMethod,
        IReadOnlyCollection<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        if (timeSeriesPoints.Count == 0)
            throw new ArgumentException("Time series points empty");

        Id = id;
        CalculationId = calculationId;
        CalculationType = calculationType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        GridArea = gridArea;
        EnergySupplierId = energySupplierId;
        AmountType = amountType;
        IsTax = isTax;
        ChargeCode = chargeCode;
        ChargeType = chargeType;
        ChargeOwnerId = chargeOwnerId;
        QuantityUnit = quantityUnit;
        ChargeResolution = chargeResolution;
        MeteringPointType = meteringPointType;
        SettlementMethod = settlementMethod;

        TimeSeriesPoints = timeSeriesPoints;
    }

    public Guid Id { get; }

    public Guid CalculationId { get; }

    public ProcessType CalculationType { get; }

    public string GridArea { get; }

    public Instant PeriodStart { get; }

    public Instant PeriodEnd { get; }

    public string? EnergySupplierId { get; }

    public AmountType AmountType { get; }

    public bool IsTax { get; }

    public string ChargeCode { get; }

    public ChargeType ChargeType { get; }

    public string ChargeOwnerId { get; }

    public QuantityUnit QuantityUnit { get; }

    public ChargeResolution ChargeResolution { get; }

    public MeteringPointType? MeteringPointType { get; }

    public SettlementMethod? SettlementMethod { get; }

    public IReadOnlyCollection<WholesaleTimeSeriesPoint> TimeSeriesPoints { get; }
}
