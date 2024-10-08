﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;

public sealed record SettlementReportEnergyResultRow
{
    public SettlementReportEnergyResultRow(
        Instant time,
        decimal quantity,
        string gridAreaCode,
        Resolution resolution,
        CalculationType calculationType,
        MeteringPointType? meteringPointType,
        SettlementMethod? settlementMethod,
        string? energySupplierId)
    {
        Time = time;
        Quantity = quantity;
        GridAreaCode = gridAreaCode;
        Resolution = resolution;
        MeteringPointType = meteringPointType;
        SettlementMethod = settlementMethod;
        EnergySupplierId = energySupplierId;
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
    }

    public string EnergyBusinessProcess { get; }

    public Instant Time { get; }

    public decimal Quantity { get; }

    public string GridAreaCode { get; }

    public Resolution Resolution { get; }

    public MeteringPointType? MeteringPointType { get; }

    public SettlementMethod? SettlementMethod { get; }

    public string? EnergySupplierId { get; }
}
