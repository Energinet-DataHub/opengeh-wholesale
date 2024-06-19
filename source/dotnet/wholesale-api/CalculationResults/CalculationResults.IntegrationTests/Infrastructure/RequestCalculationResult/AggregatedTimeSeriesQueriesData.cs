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

using System.Globalization;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using NodaTime;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

/*
1 Test data
═══════════

1.1 Overall structure
─────────────────────

  The following illustration shows how the different grid areas, balance responsibles, energy suppliers, and data points
  are related for this particular test.
  ┌────
  │ +-------------------------------+    +----------------------+
  │ |   GA1                         |    |   GA3                |
  │ |   +-----------------------+   |    |   +--------------+   |
  │ |   |   BR1                 |   |    |   |   BR3        |   |
  │ |   |   +-----+   +-----+   |   |    |   |   +------+   |   |
  │ |   |   | ES1 |   | ES2 |   |   |    |   |   | ES1  |   |   |
  │ |   |   | 1   |   | 1   |   |   |    |   |   | 1  3 |   |   |
  │ |   |   | 2   |   | 4   |   |   |    |   |   | 2  4 |   |   |
  │ |   |   +-----+   +-----+   |   |    |   |   +------+   |   |
  │ |   +-----------------------+   |    |   +--------------+   |
  │ |                               |    +----------------------+
  │ |                               |
  │ |                               |    +---------------------------+
  │ |                               |    |   GA2                     |
  │ |   +---------------------------+----+-----------------------+   |
  │ |   |   BR2                     |    |                       |   |
  │ |   |   +-----+   +-----+       |    |   +-----+   +-----+   |   |
  │ |   |   | ES1 |   | ES3 |       |    |   | ES2 |   | ES3 |   |   |
  │ |   |   | 3   |   | 2   |       |    |   | 2   |   | 1   |   |   |
  │ |   |   | 4   |   | 3   |       |    |   | 3   |   | 4   |   |   |
  │ |   |   +-----+   +-----+       |    |   +-----+   +-----+   |   |
  │ |   +---------------------------+----+-----------------------+   |
  │ +-------------------------------+    +---------------------------+
  └────

  The following table works as a kind of legend to the diagram above:
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Id   Name
  ────────────────────────────
   GAX  Grid area X
   BRX  Balance responsible X
   ESX  Energy supplier X
   X    Metering data point X
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━


1.2 Data points
───────────────

  The data points consists primarily of a quantity and time (for the purpose of this data set, aspects like quality is
  ignored). In addition to these base elements—or base points—each metering data point is "corrected" a fixed number of
  times
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Id  Quantity        Time        Balance fixing  First correction  Second correction  Third correction
  ───────────────────────────────────────────────────────────────────────────────────────────────────────
    1  FirstQuantity   FirstHour   X               X                 X                  X
    2  SecondQuantity  SecondHour  X               X                 X
    3  ThirdQuantity   ThirdHour   X                                 X                  X
    4  FourthQuantity  SecondDay   X                                                    X
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  which in the end results in generating up to four distinct data points for each data point in the diagram; e.g. for
  `1' we have a balance fixing, first correction, second correction, and third correction point/element/row.

  The test data also contains aggregation data for `BalanceResponsibleAndGridArea', `EnergySupplierAndGridArea', and
  `GridAreaCode'. This data is derived directly from the points above, which are all of the aggregation level
  `EnergySupplierAndBalanceResponsibleAndGridArea', as each data point—as seen in the diagram—is bound to a grid area,
  balance responsible, and energy supplier. The generation of aggregated data is generated as summarised below.
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Aggregation level              Derivation
  ───────────────────────────────────────────────────────────────────
   BalanceResponsibleAndGridArea  Sum of all points within BR in GA
   EnergySupplierAndGridArea      Sum of all points for ES within GA
   GridAreaCode                       Sum of all points within GA
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  It is worth noticing that each generated aggregation is done for each calculation level too, i.e. balance fixing,
  first correction, second correction, and third correction.

  ┌────
  │ Consider the case of ~BalanceResponsibleAndGridArea~ for BR2 and GA1. For balance fixing aggregation, we have one
  │ point at time ~SecondHour~ (from ES3), two for ~ThirdHour~ (one from ES2 and one from ES3), and one for ~SecondDay~
  │ (from ES2). The aggregated points will thus be the points
  │
  │ { (SecondHour, SecondQuantity), (ThirdHour, ThirdQuantity x 2), (SecondDay, FourthQuantity) }
  │
  │ We repeat the process for the other calculation types and for instance for first correction we have
  │
  │ { (SecondQuantity, SecondQuantity) }
  └────

1.3 Calculations
────────────────

  Each data point is equipped with a calculation id. There are quite a few of these, as they—the calculation ids that
  is—must adhere to a few rules:
  1. All ids must be unique
  2. Each id can only be used for a specific calculation type, e.g. balance fixing or second correction.


  We additionally adhere to the following too:
  1. Each id can only be used for a specific aggregation level, e.g. we cannot reuse the same id for first corrections
     for grid areas and energy supplier per grid area
  2. Each id can only appear in one specific version; if we want different versions we need different calculation ids


  The following table details the calculation ids and which data points they encompass
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Calculation id                                 Description
  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   BalanceFixingCalculationResultId               Used for energy supplier, balance responsible, and grid area aggregation of balance fixing measurements, i.e. the initial balance fixing data points
   FirstCorrectionSettlementCalculationResultId   Used for energy supplier, balance responsible, and grid area aggregation of first corrections, i.e. the initial first correction data points
   SecondCorrectionSettlementCalculationResultId  Used for energy supplier, balance responsible, and grid area aggregation of second corrections, i.e. the initial second correction data points
   ThirdCorrectionSettlementCalculationResultId   Used for energy supplier, balance responsible, and grid area aggregation of third corrections, i.e. the initial third correction data points
   BrGaAgBf1CalculationResultId                   Used for balance responsible and grid area aggregation of balance fixing measurements before SecondDay
   BrGaAgBf2CalculationResultId                   Used for balance responsible and grid area aggregation of balance fixing measurements at or after SecondDay
   BrGaAgFc1CalculationResultId                   Used for balance responsible and grid area aggregation of first corrections before SecondDay
   BrGaAgFc2CalculationResultId                   Used for balance responsible and grid area aggregation of first corrections at or after SecondDay
   BrGaAgSc1CalculationResultId                   Used for balance responsible and grid area aggregation of second corrections before SecondDay
   BrGaAgSc2CalculationResultId                   Used for balance responsible and grid area aggregation of second corrections at or after SecondDay
   BrGaAgTc1CalculationResultId                   Used for balance responsible and grid area aggregation of third corrections before SecondDay
   BrGaAgTc2CalculationResultId                   Used for balance responsible and grid area aggregation of third corrections at or after SecondDay
   EsGaAgBf1CalculationResultId                   Used for energy supplier and grid area aggregation of balance fixing measurements before SecondDay
   EsGaAgBf2CalculationResultId                   Used for energy supplier and grid area aggregation of balance fixing measurements at or after SecondDay
   EsGaAgFc1CalculationResultId                   Used for energy supplier and grid area aggregation of first corrections before SecondDay
   EsGaAgFc2CalculationResultId                   Used for energy supplier and grid area aggregation of first corrections at or after SecondDay
   EsGaAgSc1CalculationResultId                   Used for energy supplier and grid area aggregation of second corrections before SecondDay
   EsGaAgSc2CalculationResultId                   Used for energy supplier and grid area aggregation of second corrections at or after SecondDay
   EsGaAgTc1CalculationResultId                   Used for energy supplier and grid area aggregation of third corrections before SecondDay
   EsGaAgTc2CalculationResultId                   Used for energy supplier and grid area aggregation of third corrections at or after SecondDay
   GaAgBf1CalculationResultId                     Used for grid area aggregation of balance fixing measurements before SecondDay
   GaAgBf2CalculationResultId                     Used for grid area aggregation of balance fixing measurements at or after SecondDay
   GaAgFc1CalculationResultId                     Used for grid area aggregation of first corrections before SecondDay
   GaAgFc2CalculationResultId                     Used for grid area aggregation of first corrections at or after SecondDay
   GaAgSc1CalculationResultId                     Used for grid area aggregation of second corrections before SecondDay
   GaAgSc2CalculationResultId                     Used for grid area aggregation of second corrections at or after SecondDay
   GaAgTc1CalculationResultId                     Used for grid area aggregation of third corrections before SecondDay
   GaAgTc2CalculationResultId                     Used for grid area aggregation of third corrections at or after SecondDay
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  The utilised `CalculationForPeriod' as part of the query parameters are generated with calculation id and version as
  follows
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Calculation id                                 Version
  ────────────────────────────────────────────────────────
   BalanceFixingCalculationResultId                   256
   FirstCorrectionSettlementCalculationResultId       512
   SecondCorrectionSettlementCalculationResultId     1024
   ThirdCorrectionSettlementCalculationResultId      2048
   BrGaAgBf1CalculationResultId                         1
   BrGaAgBf2CalculationResultId                         2
   BrGaAgFc1CalculationResultId                         3
   BrGaAgFc2CalculationResultId                         4
   BrGaAgSc1CalculationResultId                         5
   BrGaAgSc2CalculationResultId                         6
   BrGaAgTc1CalculationResultId                         7
   BrGaAgTc2CalculationResultId                         8
   EsGaAgBf1CalculationResultId                        11
   EsGaAgBf2CalculationResultId                        22
   EsGaAgFc1CalculationResultId                        33
   EsGaAgFc2CalculationResultId                        44
   EsGaAgSc1CalculationResultId                        55
   EsGaAgSc2CalculationResultId                        66
   EsGaAgTc1CalculationResultId                        77
   EsGaAgTc2CalculationResultId                        88
   GaAgBf1CalculationResultId                         111
   GaAgBf2CalculationResultId                         222
   GaAgFc1CalculationResultId                         333
   GaAgFc2CalculationResultId                         444
   GaAgSc1CalculationResultId                         555
   GaAgSc2CalculationResultId                         666
   GaAgTc1CalculationResultId                         777
   GaAgTc2CalculationResultId                         888
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  with the period start and end being the same for all `CalculationForPeriod'.
 */
public sealed class AggregatedTimeSeriesQueriesData(MigrationsFreeDatabricksSqlStatementApiFixture sqlStatementApiFixture)
{
    public static AggregatedTimeSeriesQueryParameters CreateQueryParameters(
        Instant startOfPeriod,
        Instant endOfPeriod,
        IReadOnlyCollection<TimeSeriesType>? timeSeriesType = null,
        string? gridArea = null,
        string? energySupplierId = null,
        string? balanceResponsibleId = null)
    {
        IReadOnlyCollection<CalculationForPeriod> calculationForPeriods =
        [
            // Energy supplier, balance responsible, and grid area aggregation data
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId),
                256),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.FirstCorrectionSettlementCalculationResultId),
                512),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.SecondCorrectionSettlementCalculationResultId),
                1024),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.ThirdCorrectionSettlementCalculationResultId),
                2048),

            // Balance responsible and grid area aggregation data
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BrGaAgBf1CalculationResultId),
                1),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BrGaAgBf2CalculationResultId),
                2),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BrGaAgFc1CalculationResultId),
                3),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BrGaAgFc2CalculationResultId),
                4),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BrGaAgSc1CalculationResultId),
                5),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BrGaAgSc2CalculationResultId),
                6),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BrGaAgTc1CalculationResultId),
                7),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.BrGaAgTc2CalculationResultId),
                8),

            // Energy supplier and grid area aggregation data
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.EsGaAgBf1CalculationResultId),
                11),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.EsGaAgBf2CalculationResultId),
                22),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.EsGaAgFc1CalculationResultId),
                33),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.EsGaAgFc2CalculationResultId),
                44),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.EsGaAgSc1CalculationResultId),
                55),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.EsGaAgSc2CalculationResultId),
                66),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.EsGaAgTc1CalculationResultId),
                77),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.EsGaAgTc2CalculationResultId),
                88),

            // Grid area aggregation data
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.GaAgBf1CalculationResultId),
                111),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.GaAgBf2CalculationResultId),
                222),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.GaAgFc1CalculationResultId),
                333),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.GaAgFc2CalculationResultId),
                444),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.GaAgSc1CalculationResultId),
                555),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.GaAgSc2CalculationResultId),
                666),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.GaAgTc1CalculationResultId),
                777),
            new CalculationForPeriod(
                new Period(startOfPeriod, endOfPeriod),
                Guid.Parse(AggregatedTimeSeriesQueriesConstants.GaAgTc2CalculationResultId),
                888)
        ];

        return new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: timeSeriesType ?? [TimeSeriesType.Production],
            LatestCalculationForPeriod: calculationForPeriods,
            GridAreaCodes: gridArea != null ? [gridArea] : [],
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId);
    }

    public async Task AddDataAsync()
    {
        await sqlStatementApiFixture.DatabricksSchemaManager
            .EmptyAsync(
                sqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME);

        foreach (var timeSeriesType in new[]
                 {
                     DeltaTableTimeSeriesType.Production,
                     DeltaTableTimeSeriesType.FlexConsumption,
                     DeltaTableTimeSeriesType.NetExchangePerGridArea,
                 })
        {
            var dataRowsToAddToDatabricks = new List<IReadOnlyCollection<string>>();

            // GridAreaCodeA
            // BalanceResponsibleA
            dataRowsToAddToDatabricks.AddRange(CreateDataOne(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeA,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleA,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierA,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataTwo(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeA,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleA,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierA,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataOne(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeA,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleA,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierB,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataFour(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeA,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleA,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierB,
                timeSeriesType));

            // BalanceResponsibleB
            dataRowsToAddToDatabricks.AddRange(CreateDataThree(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeA,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleB,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierA,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataFour(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeA,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleB,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierA,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataTwo(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeA,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleB,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierC,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataThree(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeA,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleB,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierC,
                timeSeriesType));

            // GridAreaCodeB
            // BalanceResponsibleB
            dataRowsToAddToDatabricks.AddRange(CreateDataTwo(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeB,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleB,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierB,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataThree(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeB,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleB,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierB,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataOne(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeB,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleB,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierC,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataFour(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeB,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleB,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierC,
                timeSeriesType));

            // GridAreaCodeC
            // BalanceResponsibleC
            dataRowsToAddToDatabricks.AddRange(CreateDataOne(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeC,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleC,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierA,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataTwo(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeC,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleC,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierA,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataThree(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeC,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleC,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierA,
                timeSeriesType));
            dataRowsToAddToDatabricks.AddRange(CreateDataFour(
                AggregatedTimeSeriesQueriesConstants.GridAreaCodeC,
                AggregatedTimeSeriesQueriesConstants.BalanceResponsibleC,
                AggregatedTimeSeriesQueriesConstants.EnergySupplierA,
                timeSeriesType));

            // Do the aggregations
            var aggregatedByBalanceAndGrid = dataRowsToAddToDatabricks
                .GroupBy(row => new
                {
                    Time = row.ElementAt(6),
                    GridArea = row.ElementAt(4),
                    TimeSeriesType = row.ElementAt(11),
                    BalanceResponsibleId = row.ElementAt(8),
                    // EnergySupplierId = row.ElementAt(5),
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

            var aggregatedByGrid = dataRowsToAddToDatabricks
                .GroupBy(row => new
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

            dataRowsToAddToDatabricks.AddRange(aggregatedByBalanceAndGrid);
            dataRowsToAddToDatabricks.AddRange(aggregatedByGrid);

            dataRowsToAddToDatabricks = dataRowsToAddToDatabricks.OrderBy(_ => Random.Shared.NextInt64()).ToList();

            await sqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(
                sqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME,
                dataRowsToAddToDatabricks);
        }
    }

    private static IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataOne(
        string gridAreaCode,
        string balanceResponsibleId,
        string energySupplierId,
        string timeSeriesType)
    {
        var o = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.FirstHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.FirstQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var f = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.FirstCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.FirstCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.FirstHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.FirstQuantityFirstCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.FirstCorrectionSettlement);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.SecondCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.SecondCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.FirstHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.FirstQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.SecondCorrectionSettlement);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.ThirdCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.ThirdCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.FirstHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.FirstQuantityThirdCorrection,
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
            calculationId: AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.SecondHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.SecondQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var f = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.FirstCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.FirstCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.SecondHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.SecondQuantityFirstCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.FirstCorrectionSettlement);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.SecondCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.SecondCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.SecondHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.SecondQuantitySecondCorrection,
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
            calculationId: AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.ThirdHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.ThirdQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.SecondCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.SecondCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.ThirdHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.ThirdQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.SecondCorrectionSettlement);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.ThirdCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.ThirdCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.ThirdHour,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.ThirdQuantityThirdCorrection,
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
            calculationId: AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.BalanceFixingCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.SecondDay,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.FourthQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: AggregatedTimeSeriesQueriesConstants.ThirdCorrectionSettlementCalculationResultId,
            calculationResultId: AggregatedTimeSeriesQueriesConstants.ThirdCorrectionSettlementCalculationResultId,
            time: AggregatedTimeSeriesQueriesConstants.SecondDay,
            gridArea: gridAreaCode,
            quantity: AggregatedTimeSeriesQueriesConstants.FourthQuantityThirdCorrection,
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
        var isTimeBeforeCut = IsTimeBeforeCut(time);
        var calculationTypeAsEnum = CalculationTypeMapper.FromDeltaTableValue(calculationType);

        return calculationTypeAsEnum switch
        {
            CalculationType.BalanceFixing when isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .BrGaAgBf1CalculationResultId,
            CalculationType.BalanceFixing when !isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .BrGaAgBf2CalculationResultId,

            CalculationType.FirstCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .BrGaAgFc1CalculationResultId,
            CalculationType.FirstCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .BrGaAgFc2CalculationResultId,

            CalculationType.SecondCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .BrGaAgSc1CalculationResultId,
            CalculationType.SecondCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .BrGaAgSc2CalculationResultId,

            CalculationType.ThirdCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .BrGaAgTc1CalculationResultId,
            CalculationType.ThirdCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .BrGaAgTc2CalculationResultId,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private static string GetCalculationIdForGaAggregation(string time, string calculationType)
    {
        var isTimeBeforeCut = IsTimeBeforeCut(time);
        var calculationTypeAsEnum = CalculationTypeMapper.FromDeltaTableValue(calculationType);

        return calculationTypeAsEnum switch
        {
            CalculationType.BalanceFixing when isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .GaAgBf1CalculationResultId,
            CalculationType.BalanceFixing when !isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .GaAgBf2CalculationResultId,

            CalculationType.FirstCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .GaAgFc1CalculationResultId,
            CalculationType.FirstCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .GaAgFc2CalculationResultId,

            CalculationType.SecondCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .GaAgSc1CalculationResultId,
            CalculationType.SecondCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .GaAgSc2CalculationResultId,

            CalculationType.ThirdCorrectionSettlement when isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .GaAgTc1CalculationResultId,
            CalculationType.ThirdCorrectionSettlement when !isTimeBeforeCut => AggregatedTimeSeriesQueriesConstants
                .GaAgTc2CalculationResultId,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private static bool IsTimeBeforeCut(string time)
    {
        return Instant.FromDateTimeOffset(DateTimeOffset.Parse(time))
               < Instant.FromDateTimeOffset(DateTimeOffset.Parse(AggregatedTimeSeriesQueriesConstants.SecondDay));
    }
}
