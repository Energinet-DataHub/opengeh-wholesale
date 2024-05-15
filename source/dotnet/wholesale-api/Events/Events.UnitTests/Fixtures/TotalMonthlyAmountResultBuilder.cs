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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TotalMonthlyAmountResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;

public sealed class TotalMonthlyAmountResultBuilder
{
    private readonly Guid _calculationId = Guid.NewGuid();
    private readonly Guid _resultId = Guid.NewGuid();
    private readonly Instant _periodStart = SystemClock.Instance.GetCurrentInstant();
    private readonly Instant _periodEnd = SystemClock.Instance.GetCurrentInstant();
    private readonly string _gridArea = "543";
    private readonly string _energySupplierId = "es_id";
    private readonly long _version = 1;
    private readonly decimal _amount = 1;

    private string _chargeOwnerId = "charge_owner_id";
    private CalculationType _calculationType = CalculationType.WholesaleFixing;

    public TotalMonthlyAmountResultBuilder WithCalculationType(CalculationType calculationType)
    {
        _calculationType = calculationType;
        return this;
    }

    public TotalMonthlyAmountResultBuilder WithChargeOwner(string chargeOwnerId)
    {
        _chargeOwnerId = chargeOwnerId;
        return this;
    }

    public TotalMonthlyAmountResult Build()
    {
        return new TotalMonthlyAmountResult(
            _resultId,
            _calculationId,
            _calculationType,
            _periodStart,
            _periodEnd,
            _gridArea,
            _energySupplierId,
            _chargeOwnerId,
            _amount,
            _version);
    }
}
