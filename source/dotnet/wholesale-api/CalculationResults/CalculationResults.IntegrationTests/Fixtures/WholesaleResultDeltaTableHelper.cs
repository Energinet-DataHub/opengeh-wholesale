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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

public static class WholesaleResultDeltaTableHelper
{
    public static string?[] CreateRowValues(
        string calculationId = "ed39dbc5-bdc5-41b9-922a-08d3b12d4538",
        string calculationExecutionTimeStart = "2022-03-11T03:00:00.000Z",
        string calculationType = DeltaTableCalculationType.WholesaleFixing,
        string calculationResultId = "aaaaaaaa-1111-1111-1c1c-08d3b12d4511",
        string gridArea = "805",
        string energySupplierId = "2236552000028",
        string amountType = "amount_per_charge",
        string? meteringPointType = null,
        string? settlementMethod = null,
        string quantityUnit = "kWh",
        string resolution = "PT1H",
        string chargeCode = "4000",
        string chargeType = "tariff",
        string chargeOwnerId = "9876543210000",
        string time = "2022-05-16T03:00:00.000Z",
        string? quantity = "1.123",
        IReadOnlyCollection<string>? quantityQualities = null,
        string? price = "9.876543",
        string? amount = "2.345678",
        string isTax = "false")
    {
        quantityQualities ??= new List<string> { "'missing'", "'measured'" };

        if (amountType == "amount_per_charge" && meteringPointType == null)
            meteringPointType = "production";

        return WholesaleResultColumnNames.GetAllNames().Select(columnName => columnName switch
        {
            WholesaleResultColumnNames.CalculationId => $@"'{calculationId}'",
            WholesaleResultColumnNames.CalculationExecutionTimeStart => $@"'{calculationExecutionTimeStart}'",
            WholesaleResultColumnNames.CalculationType => $@"'{calculationType}'",
            WholesaleResultColumnNames.CalculationResultId => $@"'{calculationResultId}'",
            WholesaleResultColumnNames.GridArea => $@"'{gridArea}'",
            WholesaleResultColumnNames.EnergySupplierId => $@"'{energySupplierId}'",
            WholesaleResultColumnNames.AmountType => $@"'{amountType}'",
            WholesaleResultColumnNames.MeteringPointType => meteringPointType != null ? $@"'{meteringPointType}'" : null,
            WholesaleResultColumnNames.SettlementMethod => settlementMethod != null ? $@"'{settlementMethod}'" : null,
            WholesaleResultColumnNames.QuantityUnit => $@"'{quantityUnit}'",
            WholesaleResultColumnNames.Resolution => $@"'{resolution}'",
            WholesaleResultColumnNames.ChargeCode => $@"'{chargeCode}'",
            WholesaleResultColumnNames.ChargeType => $@"'{chargeType}'",
            WholesaleResultColumnNames.ChargeOwnerId => $@"'{chargeOwnerId}'",
            WholesaleResultColumnNames.IsTax => $@"{isTax}",
            WholesaleResultColumnNames.Time => $@"'{time}'",
            WholesaleResultColumnNames.Quantity => $@"{quantity}",
            WholesaleResultColumnNames.QuantityQualities => @"array(" + string.Join(",", quantityQualities) + ")",
            WholesaleResultColumnNames.Price => $@"{price}",
            WholesaleResultColumnNames.Amount => $@"{amount}",
            _ => throw new ArgumentException($"Unexpected column name: {columnName}."),
        }).ToArray();
    }
}
