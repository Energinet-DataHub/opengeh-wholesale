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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed record SettlementReportMonthlyAmountRow
{
    public SettlementReportMonthlyAmountRow(
        CalculationType calculationType,
        string gridArea,
        string energySupplierId,
        Instant startDateTime,
        QuantityUnit quantityUnit,
        decimal? amount,
        ChargeType? chargeType,
        string? chargeCode,
        string? chargeOwnerId)
    {
        GridArea = gridArea;
        EnergySupplierId = energySupplierId;
        StartDateTime = startDateTime;
        QuantityUnit = quantityUnit;
        Amount = amount;
        ChargeType = chargeType;
        ChargeCode = chargeCode;
        ChargeOwnerId = chargeOwnerId;
        EnergyBusinessProcess = calculationType switch
        {
            CalculationType.Aggregation => "D03",
            CalculationType.BalanceFixing => "D04",
            CalculationType.WholesaleFixing => "D05",
            CalculationType.FirstCorrectionSettlement => "D32",
            CalculationType.SecondCorrectionSettlement => "D32",
            CalculationType.ThirdCorrectionSettlement => "D32",
            _ => throw new ArgumentOutOfRangeException(nameof(calculationType)),
        };
        ProcessVariant = calculationType switch
        {
            CalculationType.Aggregation => null,
            CalculationType.BalanceFixing => null,
            CalculationType.WholesaleFixing => null,
            CalculationType.FirstCorrectionSettlement => "1ST",
            CalculationType.SecondCorrectionSettlement => "2ND",
            CalculationType.ThirdCorrectionSettlement => "3RD",
            _ => throw new ArgumentOutOfRangeException(nameof(calculationType)),
        };
    }

    public string GridArea { get; }

    public string EnergySupplierId { get; }

    public Instant StartDateTime { get; }

    public Resolution Resolution => Resolution.Month;

    public QuantityUnit QuantityUnit { get; }

    public Currency Currency => Currency.DKK;

    public decimal? Amount { get; }

    public ChargeType? ChargeType { get; }

    public string? ChargeCode { get; }

    public string? ChargeOwnerId { get; }

    public string EnergyBusinessProcess { get; }

    public string? ProcessVariant { get; }
}
