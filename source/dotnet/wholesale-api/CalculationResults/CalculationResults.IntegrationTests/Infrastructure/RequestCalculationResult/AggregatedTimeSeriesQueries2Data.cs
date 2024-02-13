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

using System.Globalization;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using NodaTime;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Period;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public sealed class AggregatedTimeSeriesQueries2Data(DatabricksSqlStatementApiFixture sqlStatementApiFixture)
{
    public static AggregatedTimeSeriesQueryParameters CreateQueryParameters(
        IReadOnlyCollection<TimeSeriesType>? timeSeriesType = null,
        Instant? startOfPeriod = null,
        Instant? endOfPeriod = null,
        string? gridArea = null,
        string? energySupplierId = null,
        string? balanceResponsibleId = null)
    {
        IReadOnlyCollection<CalculationForPeriod> calculationForPeriods =
        [
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId),
                256),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.FirstCorrectionSettlementCalculationResultId),
                512),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.SecondCorrectionSettlementCalculationResultId),
                1024),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.ThirdCorrectionSettlementCalculationResultId),
                2048),

            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BrGaAgBf1CalculationResultId),
                1),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BrGaAgBf2CalculationResultId),
                2),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BrGaAgFc1CalculationResultId),
                3),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BrGaAgFc2CalculationResultId),
                4),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BrGaAgSc1CalculationResultId),
                5),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BrGaAgSc2CalculationResultId),
                6),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BrGaAgTc1CalculationResultId),
                7),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.BrGaAgTc2CalculationResultId),
                8),

            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.EsGaAgBf1CalculationResultId),
                11),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.EsGaAgBf2CalculationResultId),
                22),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.EsGaAgFc1CalculationResultId),
                33),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.EsGaAgFc2CalculationResultId),
                44),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.EsGaAgSc1CalculationResultId),
                55),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.EsGaAgSc2CalculationResultId),
                66),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.EsGaAgTc1CalculationResultId),
                77),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.EsGaAgTc2CalculationResultId),
                88),

            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.GaAgBf1CalculationResultId),
                111),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.GaAgBf2CalculationResultId),
                222),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.GaAgFc1CalculationResultId),
                333),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.GaAgFc2CalculationResultId),
                444),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.GaAgSc1CalculationResultId),
                555),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.GaAgSc2CalculationResultId),
                666),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.GaAgTc1CalculationResultId),
                777),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(AggregatedTimeSeriesQueries2Constants.GaAgTc2CalculationResultId),
                888)
        ];

        return new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: timeSeriesType ?? new[] { TimeSeriesType.Production },
            LatestCalculationForPeriod: calculationForPeriods,
            GridArea: gridArea,
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId);
    }

    public async Task AddDataAsync()
    {
        await sqlStatementApiFixture.DatabricksSchemaManager.EmptyAsync(sqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value
            .ENERGY_RESULTS_TABLE_NAME);

        foreach (var timeSeriesType in new[]
                 {
                     DeltaTableTimeSeriesType.Production, DeltaTableTimeSeriesType.FlexConsumption,
                     DeltaTableTimeSeriesType.NetExchangePerGridArea,
                 })
        {
            var allThemRows = new List<IReadOnlyCollection<string>>();

            // GridAreaCodeA
            // BalanceResponsibleA
            allThemRows.AddRange(CreateDataOne(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleA,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataTwo(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleA,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataOne(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleA,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierB,
                timeSeriesType));
            allThemRows.AddRange(CreateDataFour(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleA,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierB,
                timeSeriesType));

            // BalanceResponsibleB
            allThemRows.AddRange(CreateDataThree(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataFour(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataTwo(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierC,
                timeSeriesType));
            allThemRows.AddRange(CreateDataThree(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierC,
                timeSeriesType));

            // GridAreaCodeB
            // BalanceResponsibleB
            allThemRows.AddRange(CreateDataTwo(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeB,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierB,
                timeSeriesType));
            allThemRows.AddRange(CreateDataThree(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeB,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierB,
                timeSeriesType));
            allThemRows.AddRange(CreateDataOne(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeB,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierC,
                timeSeriesType));
            allThemRows.AddRange(CreateDataFour(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeB,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierC,
                timeSeriesType));

            // GridAreaCodeC
            // BalanceResponsibleC
            allThemRows.AddRange(CreateDataOne(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleC,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataTwo(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleC,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataThree(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleC,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataFour(
                AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
                AggregatedTimeSeriesQueries2Constants.BalanceResponsibleC,
                AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
                timeSeriesType));

            // Do the aggregations
            var aggregatedByBalanceAndGrid = allThemRows.GroupBy(row => new
                {
                    Time = row.ElementAt(6),
                    GridArea = row.ElementAt(4),
                    TimeSeriesType = row.ElementAt(11),
                    BalanceResponsibleId = row.ElementAt(8),
                    CalculationId = row.ElementAt(3),
                    CalculationType = row.ElementAt(2),
                })
                .Select(grouping =>
                    EnergyResultDeltaTableHelper.CreateRowValues(
                        calculationId: GetCalculationIdForBrGaAggregation(
                            grouping.Key.Time.Replace("'", string.Empty),
                            grouping.Key.CalculationType.Replace("'", string.Empty)),
                        calculationResultId: GetCalculationIdForBrGaAggregation(
                            grouping.Key.Time.Replace("'", string.Empty),
                            grouping.Key.CalculationType.Replace("'", string.Empty)),
                        time: grouping.Key.Time.Replace("'", string.Empty),
                        gridArea: grouping.Key.GridArea.Replace("'", string.Empty),
                        quantity: grouping.Sum(row => decimal.Parse(row.ElementAt(7), CultureInfo.InvariantCulture))
                            .ToString(CultureInfo.InvariantCulture),
                        aggregationLevel: DeltaTableAggregationLevel.BalanceResponsibleAndGridArea,
                        balanceResponsibleId: grouping.Key.BalanceResponsibleId.Replace("'", string.Empty),
                        // energySupplierId: "NULL",
                        timeSeriesType: grouping.Key.TimeSeriesType.Replace("'", string.Empty),
                        calculationType: grouping.Key.CalculationType.Replace("'", string.Empty)))
                .ToList();

            var aggregatedByEnergyAndGrid = allThemRows.GroupBy(row => new
                {
                    Time = row.ElementAt(6),
                    GridArea = row.ElementAt(4),
                    TimeSeriesType = row.ElementAt(11),
                    // BalanceResponsibleId = row.ElementAt(8),
                    EnergySupplierId = row.ElementAt(5),
                    CalculationId = row.ElementAt(3),
                    CalculationType = row.ElementAt(2),
                })
                .Select(grouping =>
                    EnergyResultDeltaTableHelper.CreateRowValues(
                        calculationId: GetCalculationIdForEsGaAggregation(
                            grouping.Key.Time.Replace("'", string.Empty),
                            grouping.Key.CalculationType.Replace("'", string.Empty)),
                        calculationResultId: GetCalculationIdForEsGaAggregation(
                            grouping.Key.Time.Replace("'", string.Empty),
                            grouping.Key.CalculationType.Replace("'", string.Empty)),
                        time: grouping.Key.Time.Replace("'", string.Empty),
                        gridArea: grouping.Key.GridArea.Replace("'", string.Empty),
                        quantity: grouping.Sum(row => decimal.Parse(row.ElementAt(7), CultureInfo.InvariantCulture))
                            .ToString(CultureInfo.InvariantCulture),
                        aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndGridArea,
                        // balanceResponsibleId: grouping.Key.BalanceResponsibleId.Replace("'", string.Empty),
                        energySupplierId: grouping.Key.EnergySupplierId.Replace("'", string.Empty),
                        timeSeriesType: grouping.Key.TimeSeriesType.Replace("'", string.Empty),
                        calculationType: grouping.Key.CalculationType.Replace("'", string.Empty)))
                .ToList();

            var aggregatedByGrid = allThemRows.GroupBy(row => new
                {
                    Time = row.ElementAt(6),
                    GridArea = row.ElementAt(4),
                    TimeSeriesType = row.ElementAt(11),
                    // BalanceResponsibleId = row.ElementAt(8),
                    // EnergySupplierId = row.ElementAt(5),
                    CalculationId = row.ElementAt(3),
                    CalculationType = row.ElementAt(2),
                })
                .Select(grouping =>
                    EnergyResultDeltaTableHelper.CreateRowValues(
                        calculationId: GetCalculationIdForGaAggregation(
                            grouping.Key.Time.Replace("'", string.Empty),
                            grouping.Key.CalculationType.Replace("'", string.Empty)),
                        calculationResultId: GetCalculationIdForGaAggregation(
                            grouping.Key.Time.Replace("'", string.Empty),
                            grouping.Key.CalculationType.Replace("'", string.Empty)),
                        time: grouping.Key.Time.Replace("'", string.Empty),
                        gridArea: grouping.Key.GridArea.Replace("'", string.Empty),
                        quantity: grouping.Sum(row => decimal.Parse(row.ElementAt(7), CultureInfo.InvariantCulture))
                            .ToString(CultureInfo.InvariantCulture),
                        aggregationLevel: DeltaTableAggregationLevel.GridArea,
                        // balanceResponsibleId: grouping.Key.BalanceResponsibleId.Replace("'", string.Empty),
                        // energySupplierId: grouping.Key.EnergySupplierId.Replace("'", string.Empty),
                        timeSeriesType: grouping.Key.TimeSeriesType.Replace("'", string.Empty),
                        calculationType: grouping.Key.CalculationType.Replace("'", string.Empty)))
                .ToList();

            allThemRows.AddRange(aggregatedByBalanceAndGrid);
            allThemRows.AddRange(aggregatedByEnergyAndGrid);
            allThemRows.AddRange(aggregatedByGrid);

            allThemRows = allThemRows.OrderBy(_ => Random.Shared.NextInt64()).ToList();

            await sqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(sqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME, allThemRows);
        }
    }

    private static IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataOne(
        string gridAreaCode,
        string balanceResponsibleId,
        string energySupplierId,
        string timeSeriesType)
    {
        var o = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.FirstHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.FirstQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var f = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.FirstCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.FirstCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.FirstHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.FirstQuantityFirstCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.FirstCorrectionSettlement);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.SecondCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.SecondCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.FirstHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.FirstQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.SecondCorrectionSettlement);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.ThirdCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.ThirdCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.FirstHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.FirstQuantityThirdCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.ThirdCorrectionSettlement);

        return
            [o, f, s, t];
    }

    private static IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataTwo(
        string gridAreaCode,
        string balanceResponsibleId,
        string energySupplierId,
        string timeSeriesType)
    {
        var o = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.SecondHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.SecondQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var f = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.FirstCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.FirstCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.SecondHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.SecondQuantityFirstCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.FirstCorrectionSettlement);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.SecondCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.SecondCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.SecondHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.SecondQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.SecondCorrectionSettlement);

        return
            [o, f, s];
    }

    private static IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataThree(
        string gridAreaCode,
        string balanceResponsibleId,
        string energySupplierId,
        string timeSeriesType)
    {
        var o = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.ThirdHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.ThirdQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.SecondCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.SecondCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.ThirdHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.ThirdQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.SecondCorrectionSettlement);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.ThirdCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.ThirdCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.ThirdHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.ThirdQuantityThirdCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.ThirdCorrectionSettlement);

        return
            [o, s, t];
    }

    private static IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataFour(
        string gridAreaCode,
        string balanceResponsibleId,
        string energySupplierId,
        string timeSeriesType)
    {
        var o = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.BalanceFixingCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.SecondDay,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.FourthQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueries2Constants.ThirdCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueries2Constants.ThirdCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueries2Constants.SecondDay,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueries2Constants.FourthQuantityThirdCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.ThirdCorrectionSettlement);

        return
            [o, t];
    }

    private static string GetCalculationIdForBrGaAggregation(string time, string calculationType)
    {
        var isTimeBeforeCut = Instant.FromDateTimeOffset(DateTimeOffset.Parse(time)) <
                              Instant.FromDateTimeOffset(
                                  DateTimeOffset.Parse(AggregatedTimeSeriesQueries2Constants.SecondDay));
        var tryParse = Enum.TryParse<CalculationType>(calculationType, out var calculationTypeAsEnum);

        tryParse.Should().BeTrue("Must be able to parse calculation type in order to determine calculation id");

        return calculationTypeAsEnum switch
        {
            CalculationType.BalanceFixing when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .BrGaAgBf1CalculationResultId,
            CalculationType.BalanceFixing when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .BrGaAgBf2CalculationResultId,

            CalculationType.FirstCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .BrGaAgFc1CalculationResultId,
            CalculationType.FirstCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .BrGaAgFc2CalculationResultId,

            CalculationType.SecondCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .BrGaAgSc1CalculationResultId,
            CalculationType.SecondCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .BrGaAgSc2CalculationResultId,

            CalculationType.ThirdCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .BrGaAgTc1CalculationResultId,
            CalculationType.ThirdCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .BrGaAgTc2CalculationResultId,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private static string GetCalculationIdForEsGaAggregation(string time, string calculationType)
    {
        var isTimeBeforeCut = Instant.FromDateTimeOffset(DateTimeOffset.Parse(time)) <
                              Instant.FromDateTimeOffset(
                                  DateTimeOffset.Parse(AggregatedTimeSeriesQueries2Constants.SecondDay));
        var tryParse = Enum.TryParse<CalculationType>(calculationType, out var calculationTypeAsEnum);

        tryParse.Should().BeTrue("Must be able to parse calculation type in order to determine calculation id");

        return calculationTypeAsEnum switch
        {
            CalculationType.BalanceFixing when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .EsGaAgBf1CalculationResultId,
            CalculationType.BalanceFixing when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .EsGaAgBf2CalculationResultId,

            CalculationType.FirstCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .EsGaAgFc1CalculationResultId,
            CalculationType.FirstCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .EsGaAgFc2CalculationResultId,

            CalculationType.SecondCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .EsGaAgSc1CalculationResultId,
            CalculationType.SecondCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .EsGaAgSc2CalculationResultId,

            CalculationType.ThirdCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .EsGaAgTc1CalculationResultId,
            CalculationType.ThirdCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .EsGaAgTc2CalculationResultId,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private static string GetCalculationIdForGaAggregation(string time, string calculationType)
    {
        var isTimeBeforeCut = Instant.FromDateTimeOffset(DateTimeOffset.Parse(time)) <
                              Instant.FromDateTimeOffset(
                                  DateTimeOffset.Parse(AggregatedTimeSeriesQueries2Constants.SecondDay));
        var tryParse = Enum.TryParse<CalculationType>(calculationType, out var calculationTypeAsEnum);

        tryParse.Should().BeTrue("Must be able to parse calculation type in order to determine calculation id");

        return calculationTypeAsEnum switch
        {
            CalculationType.BalanceFixing when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .GaAgBf1CalculationResultId,
            CalculationType.BalanceFixing when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .GaAgBf2CalculationResultId,

            CalculationType.FirstCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .GaAgFc1CalculationResultId,
            CalculationType.FirstCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .GaAgFc2CalculationResultId,

            CalculationType.SecondCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .GaAgSc1CalculationResultId,
            CalculationType.SecondCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .GaAgSc2CalculationResultId,

            CalculationType.ThirdCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .GaAgTc1CalculationResultId,
            CalculationType.ThirdCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueries2Constants
                .GaAgTc2CalculationResultId,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
