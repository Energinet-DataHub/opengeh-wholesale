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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed record SettlementReportChargePriceRow
{
    public SettlementReportChargePriceRow(
        ChargeType chargeType,
        string chargeCode,
        string chargeOwnerId,
        Resolution resolution,
        bool taxIndicator,
        Instant startDateTime,
        IReadOnlyCollection<decimal> energyPrices)
    {
        ChargeType = chargeType;
        ChargeCode = chargeCode;
        ChargeOwnerId = chargeOwnerId;
        TaxIndicator = taxIndicator;
        StartDateTime = startDateTime;
        Resolution = resolution;
        EnergyPrices = energyPrices;
    }

    public ChargeType ChargeType { get; }

    public string? ChargeCode { get; }

    public string ChargeOwnerId { get; }

    public bool TaxIndicator { get; }

    public Instant StartDateTime { get; }

    public Resolution Resolution { get; }

    public IReadOnlyCollection<decimal> EnergyPrices { get; }
}
