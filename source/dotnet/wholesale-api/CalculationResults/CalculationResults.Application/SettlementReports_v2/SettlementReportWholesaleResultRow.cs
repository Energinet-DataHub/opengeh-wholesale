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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

/// <summary>
/// SettlementReportWholesaleResultRow
/// </summary>
// ENERGYBUSINESSPROCESS,
// PROCESSVARIANT,
// METERINGGRIDAREAID,
// ENERGYSUPPLIERID,
// STARTDATETIME,
// RESOLUTIONDURATION,
// TYPEOFMP,
// SETTLEMENTMETHOD,
// MEASUREUNIT,
// ENERGYCURRENCY,
// ENERGYQUANTITY,
// PRICE,
// AMOUNT,
// CHARGETYPE,
// CHARGETYPEID,
// CHARGETYPEOWNERID
public sealed record SettlementReportWholesaleResultRow
{
    /// <summary>
    /// SettlementReportWholesaleResultRow
    /// </summary>
    public SettlementReportWholesaleResultRow(
        Guid id,
        Guid calculationId,
        CalculationType calculationType,
        string gridArea,
        string energySupplierId,
        Instant startDateTime,
        Resolution resolution,
        MeteringPointType? meteringPointType,
        SettlementMethod? settlementMethod,
        QuantityUnit quantityUnit,
        Currency currency,
        decimal? quantity,
        decimal? price,
        decimal? amount,
        ChargeType chargeType,
        string? chargeCode,
        string chargeOwnerId,
        long version)
    {
        Id = id;
        CalculationId = calculationId;
        CalculationType = calculationType;
        GridArea = gridArea;
        EnergySupplierId = energySupplierId;
        StartDateTime = startDateTime;
        Resolution = resolution;
        MeteringPointType = meteringPointType;
        SettlementMethod = settlementMethod;
        QuantityUnit = quantityUnit;
        Currency = currency;
        Quantity = quantity;
        Price = price;
        Amount = amount;
        ChargeType = chargeType;
        ChargeCode = chargeCode;
        ChargeOwnerId = chargeOwnerId;
        Version = version;
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

    public Guid Id { get; }

    public Guid CalculationId { get; }

    public CalculationType CalculationType { get; }

    public string GridArea { get; }

    public string EnergySupplierId { get; }

    public Instant StartDateTime { get; }

    public Resolution Resolution { get; }

    public MeteringPointType? MeteringPointType { get; }

    public SettlementMethod? SettlementMethod { get; }

    public QuantityUnit QuantityUnit { get; }

    public Currency Currency { get; }

    public decimal? Quantity { get; }

    public decimal? Price { get; }

    public decimal? Amount { get; }

    public ChargeType ChargeType { get; }

    public string? ChargeCode { get; }

    public string ChargeOwnerId { get; }

    public long Version { get; }

    public string EnergyBusinessProcess { get; }

    public string? ProcessVariant { get; }
}
