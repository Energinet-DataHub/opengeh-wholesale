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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;

public sealed class WholesaleResultBuilder
{
    private readonly Guid _calculationId = Guid.NewGuid();
    private readonly Guid _resultId = Guid.NewGuid();
    private readonly Instant _periodStart = SystemClock.Instance.GetCurrentInstant();
    private readonly Instant _periodEnd = SystemClock.Instance.GetCurrentInstant();
    private readonly string _gridArea = "543";
    private readonly string _energySupplierId = "es_id";
    private readonly string _chargeCode = "charge_code";
    private readonly string _chargeOwnerId = "charge_owner_id";
    private readonly ChargeType _chargeType = ChargeType.Tariff;
    private readonly bool _isTax = true;
    private IReadOnlyCollection<WholesaleTimeSeriesPoint> _wholesaleTimeSeriesPoint
        = new List<WholesaleTimeSeriesPoint> { new(new DateTime(2021, 1, 1), 1, new[] { QuantityQuality.Measured }, 2, 3) };

    private ProcessType _calculationType = ProcessType.WholesaleFixing;
    private AmountType _amountType = AmountType.AmountPerCharge;
    private ChargeResolution _resolution = ChargeResolution.Hour;

    public WholesaleResultBuilder WithCalculationType(ProcessType calculationType)
    {
        _calculationType = calculationType;
        return this;
    }

    public WholesaleResultBuilder WithAmountType(AmountType amountType)
    {
        _amountType = amountType;
        return this;
    }

    public WholesaleResultBuilder WithResolution(ChargeResolution resolution)
    {
        _resolution = resolution;
        return this;
    }

    public WholesaleResultBuilder WithTimeSeriesPoints(IReadOnlyCollection<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        _wholesaleTimeSeriesPoint = timeSeriesPoints;
        return this;
    }

    public WholesaleResult Build()
    {
        return new WholesaleResult(
            _resultId,
            _calculationId,
            _calculationType,
            _periodStart,
            _periodEnd,
            _gridArea,
            _energySupplierId,
            _amountType,
            _chargeCode,
            _chargeType,
            _chargeOwnerId,
            _isTax,
            QuantityUnit.Kwh,
            _resolution,
            MeteringPointType.Production,
            null,
            _wholesaleTimeSeriesPoint);
    }
}
