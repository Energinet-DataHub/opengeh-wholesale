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

  In addition to these elements, each metering data point contains additional information
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Id  Quantity        Time        Balance fixing  First correction  Second correction  Third correction
  ───────────────────────────────────────────────────────────────────────────────────────────────────────
    1  FirstQuantity   FirstHour   X               X                 X                  X
    2  SecondQuantity  SecondHour  X               X                 X
    3  ThirdQuantity   ThirdHour   X                                 X                  X
    4  FourthQuantity  SecondDay   X                                                    X
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  The test data also contains aggregation data for `BalanceResponsibleAndGridArea', `EnergySupplierAndGridArea', and
  `GridArea'. This data is derived directly from the points above, distributed as illustrated in the diagram with the
  aggregation level `EnergySupplierAndBalanceResponsibleAndGridArea', as each data point—as seen—is bound to a grid
  area, balance responsible, and energy supplier. The generation of aggregated data is generated as summarised below.
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Aggregation level              Derivation
  ───────────────────────────────────────────────────────────────────
   BalanceResponsibleAndGridArea  Sum of all points within BR in GA
   EnergySupplierAndGridArea      Sum of all points for ES within GA
   GridArea                       Sum of all points within GA
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  It is worth noticing that each generated aggregation is done for each calculation level too, i.e. balance fixing,
  first correction, second correction, and third correction.


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
