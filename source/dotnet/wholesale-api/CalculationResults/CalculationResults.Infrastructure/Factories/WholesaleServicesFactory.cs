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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public static class WholesaleServicesFactory
{
    public static WholesaleServices Create(
        DatabricksSqlRow databricksSqlRow,
        AmountType amountType,
        IReadOnlyCollection<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        return amountType switch
        {
            AmountType.AmountPerCharge => GetWholesaleServicesForAmountsPerCharge(databricksSqlRow, timeSeriesPoints),
            AmountType.MonthlyAmountPerCharge => GetWholesaleServicesForMonthlyAmountsPerCharge(databricksSqlRow, timeSeriesPoints),
            AmountType.TotalMonthlyAmount => GetWholesaleServicesForTotalMonthlyAmount(databricksSqlRow, timeSeriesPoints),
            _ => throw new ArgumentOutOfRangeException(nameof(amountType), amountType, null),
        };
    }

    private static WholesaleServices GetWholesaleServicesForAmountsPerCharge(
        DatabricksSqlRow databricksSqlRow,
        IReadOnlyCollection<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        var resolution = ResolutionMapper.FromDeltaTableValue(databricksSqlRow[WholesaleResultColumnNames.Resolution]!);
        var period = PeriodHelper.GetPeriod(timeSeriesPoints, resolution);

        return new WholesaleServices(
            period,
            databricksSqlRow[AmountsPerChargeViewColumnNames.GridAreaCode]!,
            databricksSqlRow[AmountsPerChargeViewColumnNames.EnergySupplierId]!,
            databricksSqlRow[AmountsPerChargeViewColumnNames.ChargeCode]!,
            ChargeTypeMapper.FromDeltaTableValue(databricksSqlRow[AmountsPerChargeViewColumnNames.ChargeType]!),
            databricksSqlRow[AmountsPerChargeViewColumnNames.ChargeOwnerId]!,
            AmountType.AmountPerCharge,
            resolution,
            QuantityUnitMapper.FromDeltaTableValue(databricksSqlRow[AmountsPerChargeViewColumnNames.QuantityUnit]!),
            MeteringPointTypeMapper.FromDeltaTableValue(databricksSqlRow[AmountsPerChargeViewColumnNames.MeteringPointType]),
            SettlementMethodMapper.FromDeltaTableValue(databricksSqlRow[AmountsPerChargeViewColumnNames.SettlementMethod]),
            Currency.DKK,
            CalculationTypeMapper.FromDeltaTableValue(databricksSqlRow[AmountsPerChargeViewColumnNames.CalculationType]!),
            timeSeriesPoints,
            SqlResultValueConverters.ToInt(databricksSqlRow[AmountsPerChargeViewColumnNames.CalculationVersion]!)!.Value);
    }

    private static WholesaleServices GetWholesaleServicesForMonthlyAmountsPerCharge(
        DatabricksSqlRow databricksSqlRow,
        IReadOnlyCollection<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        var period = PeriodHelper.GetPeriod(timeSeriesPoints, Resolution.Month);

        return new WholesaleServices(
            period,
            databricksSqlRow[MonthlyAmountsPerChargeViewColumnNames.GridAreaCode]!,
            databricksSqlRow[MonthlyAmountsPerChargeViewColumnNames.EnergySupplierId]!,
            databricksSqlRow[MonthlyAmountsPerChargeViewColumnNames.ChargeCode]!,
            ChargeTypeMapper.FromDeltaTableValue(databricksSqlRow[MonthlyAmountsPerChargeViewColumnNames.ChargeType]!),
            databricksSqlRow[MonthlyAmountsPerChargeViewColumnNames.ChargeOwnerId]!,
            AmountType.MonthlyAmountPerCharge,
            Resolution.Month,
            QuantityUnitMapper.FromDeltaTableValue(databricksSqlRow[MonthlyAmountsPerChargeViewColumnNames.QuantityUnit]!),
            null,
            null,
            Currency.DKK,
            CalculationTypeMapper.FromDeltaTableValue(databricksSqlRow[MonthlyAmountsPerChargeViewColumnNames.CalculationType]!),
            timeSeriesPoints,
            SqlResultValueConverters.ToInt(databricksSqlRow[MonthlyAmountsPerChargeViewColumnNames.CalculationVersion]!)!.Value);
    }

    private static WholesaleServices GetWholesaleServicesForTotalMonthlyAmount(
        DatabricksSqlRow databricksSqlRow,
        IReadOnlyCollection<WholesaleTimeSeriesPoint> timeSeriesPoints)
    {
        var period = PeriodHelper.GetPeriod(timeSeriesPoints, Resolution.Month);

        return new WholesaleServices(
            period,
            databricksSqlRow[TotalMonthlyAmountsViewColumnNames.GridAreaCode]!,
            databricksSqlRow[TotalMonthlyAmountsViewColumnNames.EnergySupplierId]!,
            null,
            null,
            databricksSqlRow[TotalMonthlyAmountsViewColumnNames.ChargeOwnerId],
            AmountType.TotalMonthlyAmount,
            Resolution.Month,
            null,
            null,
            null,
            Currency.DKK,
            CalculationTypeMapper.FromDeltaTableValue(databricksSqlRow[TotalMonthlyAmountsViewColumnNames.CalculationType]!),
            timeSeriesPoints,
            SqlResultValueConverters.ToInt(databricksSqlRow[TotalMonthlyAmountsViewColumnNames.CalculationVersion]!)!.Value);
    }
}
