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
using AutoFixture;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using Xunit;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Period;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public class AggregatedTimeSeriesQueries2Tests : TestBase<AggregatedTimeSeriesQueries>,
    IClassFixture<DatabricksSqlStatementApiFixture>
{
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public AggregatedTimeSeriesQueries2Tests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
        Fixture.Inject(_fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(_fixture.GetDatabricksExecutor());
    }

    /*
1 Test data
═══════════

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

  In addition to these elements, each metering data point contains additional information
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Id  Quantity        Time        Balance fixing            First correction           Second correction          Third correction
  ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    1  FirstQuantity   FirstHour   FirstCalculationResultId  SecondCalculationResultId  ThirdCalculationResultId   ThirdCalculationResultId
    2  SecondQuantity  SecondHour  FirstCalculationResultId  SecondCalculationResultId  ThirdCalculationResultId
    3  ThirdQuantity   ThirdHour   FirstCalculationResultId                             SecondCalculationResultId  ThirdCalculationResultId
    4  FourthQuantity  SecondDay   FirstCalculationResultId                                                        SecondCalculationResultId
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  The test data also contains aggregation data for `BalanceResponsibleAndGridArea', `EnergySupplierAndGridArea', and
  `GridArea'. This data is derived directly from the points above, distributed as illustrated in the diagram with the
  aggregation level `EnergySupplierAndBalanceResponsibleAndGridArea' as each data point — as seen — is bound to a grid
  area, balance responsible, and energy supplier. The generated aggregated data is derived using the calculated ids as
  follow
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Aggregation level              Derivation                          Calculation id
  ─────────────────────────────────────────────────────────────────────────────────────────────
   BalanceResponsibleAndGridArea  Sum of all points within BR in GA   ThirdCalculationResultId
   EnergySupplierAndGridArea      Sum of all points for ES within GA  ThirdCalculationResultId
   GridArea                       Sum of all points within GA         ThirdCalculationResultId
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  It is worth noticing that each generated aggregation is done for each calculation level too, i.e. balance fixing,
  first correction, second correction, and third correction.

  The utilised `CalculationForPeriod' as part of the query parameters are generated as follows
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Calculation id             Calculation version
  ────────────────────────────────────────────────
   FirstCalculationResultId                  1024
   SecondCalculationResultId                  512
   ThirdCalculationResultId                    42
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  with the period start and end being the same for all `CalculationForPeriod'.


2 Test purpose
══════════════

  The purpose of the tests in this class is to ensure that the SQL queries made by `AggregatedTimeSeriesQueryStatement'
  using `AggregatedTimeSeriesQueryParameters' are resulting in the expected and correct data. The structure of the
  queries is not considered, neither is performance. The tests focus solely on whether the returned data correlates
  with our expectations for a particular query parameter.


2.1 Test structure
──────────────────

  The tests in this file all follow the same pattern:
  1. We generate a specific query parameter
  2. We execute the generate query using the sut
  3. We validate that the expected data is returned


  In particular we change the query parameter fields listed below
  ━━━━━━━━━━━━━━━━━━━━━━━━
   Query parameter field
  ────────────────────────
   Time series type
   Grid area
   Energy supplier id
   Balance responsible id
   Calculation type
   Start of period
   End of period
  ━━━━━━━━━━━━━━━━━━━━━━━━


2.2 Result validation
─────────────────────

  We usually validate the following for time series
  ━━━━━━━━━━━━━━━━━━
   Time series
  ──────────────────
   Grid area
   Calculation type
   Time series type
   Version
   Number of points
  ━━━━━━━━━━━━━━━━━━

  and for the points we limit ourselves to
  ━━━━━━━━━━━━━━━━━━━
   Time series point
  ───────────────────
   Quantity
   Time
  ━━━━━━━━━━━━━━━━━━━
  as these are sufficient to distinguish the individual data points in conjunction with the time series validation, and
  thus ensure the correct subset of the data is returned.
     */

    [Fact]
    public async Task GetAsync_EnergySupplierWithSpecificBalanceResponsibleAndGridArea_Production()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production },
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeB,
            energySupplierId: AggregatedTimeSeriesQueries2Constants.EnergySupplierB,
            balanceResponsibleId: AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => ats.Version).Should().BeEquivalentTo([256L, 512L, 1024L, 2048L]);

        actual.Single(ats => ats.Version == 256L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.222, 3.333]);

        actual.Single(ats => ats.Version == 512L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.333]);

        actual.Single(ats => ats.Version == 1024L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.444, 3.555]);

        actual.Single(ats => ats.Version == 2048L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([3.666]);
    }

    [Fact]
    public async Task GetAsync_EnergySupplierWithSpecificBalanceResponsibleAndGridArea_ProductionConsumption()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
            energySupplierId: AggregatedTimeSeriesQueries2Constants.EnergySupplierC,
            balanceResponsibleId: AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual
            .Select(ats => (ats.Version, ats.GridArea, ats.TimeSeriesType))
            .Should()
            .BeEquivalentTo([
                (256L, "101", TimeSeriesType.FlexConsumption), (512L, "101", TimeSeriesType.FlexConsumption),
                (1024L, "101", TimeSeriesType.FlexConsumption), (2048L, "101", TimeSeriesType.FlexConsumption),
                (256L, "101", TimeSeriesType.Production), (512L, "101", TimeSeriesType.Production),
                (1024L, "101", TimeSeriesType.Production), (2048L, "101", TimeSeriesType.Production)
            ]);
    }

    [Fact]
    public async Task GetAsync_EnergySupplierWithGridArea_Consumption()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeB,
            energySupplierId: AggregatedTimeSeriesQueries2Constants.EnergySupplierC,
            balanceResponsibleId: null,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => ats.Version).Should().BeEquivalentTo([11L, 22L, 33L, 55L, 77L, 88L]);

        actual.Single(ats => ats.Version == 11L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([1.111]);

        actual.Single(ats => ats.Version == 22L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([4.444]);

        actual.Single(ats => ats.Version == 33L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([1.222]);

        actual.Single(ats => ats.Version == 55L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([1.333]);

        actual.Single(ats => ats.Version == 77L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([1.444]);

        actual.Single(ats => ats.Version == 88L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([4.777]);
    }

    [Fact]
    public async Task GetAsync_EnergySupplierWithGridArea_ProductionConsumption()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
            energySupplierId: AggregatedTimeSeriesQueries2Constants.EnergySupplierA,
            balanceResponsibleId: null,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual
            .Select(ats => (ats.Version, ats.GridArea, ats.TimeSeriesType))
            .Should()
            .BeEquivalentTo([
                (11L, "301", TimeSeriesType.FlexConsumption), (22L, "301", TimeSeriesType.FlexConsumption),
                (33L, "301", TimeSeriesType.FlexConsumption), (55L, "301", TimeSeriesType.FlexConsumption),
                (77L, "301", TimeSeriesType.FlexConsumption), (88L, "301", TimeSeriesType.FlexConsumption),
                (11L, "301", TimeSeriesType.Production), (22L, "301", TimeSeriesType.Production),
                (33L, "301", TimeSeriesType.Production), (55L, "301", TimeSeriesType.Production),
                (77L, "301", TimeSeriesType.Production), (88L, "301", TimeSeriesType.Production)
            ]);
    }

    [Fact]
    public async Task GetAsync_EnergySupplierWithSpecificBalanceResponsible_Consumption()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: null,
            energySupplierId: AggregatedTimeSeriesQueries2Constants.EnergySupplierC,
            balanceResponsibleId: AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        var groupedByVersionAndGridArea = actual.GroupBy(ats => (ats.Version, ats.GridArea)).ToList();

        groupedByVersionAndGridArea.Select(g => g.Key)
            .Should()
            .BeEquivalentTo([
                (256L, "101"), (512L, "101"), (1024L, "101"), (2048L, "101"),
                (256L, "201"), (512L, "201"), (1024L, "201"), (2048L, "201")
            ]);

        groupedByVersionAndGridArea.Single(g => g.Key == (256L, "101"))
            .Single()
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.222, 3.333]);

        groupedByVersionAndGridArea.Single(g => g.Key == (512L, "101"))
            .Single()
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.333]);

        groupedByVersionAndGridArea.Single(g => g.Key == (1024L, "101"))
            .Single()
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.444, 3.555]);

        groupedByVersionAndGridArea.Single(g => g.Key == (2048L, "101"))
            .Single()
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([3.666]);

        groupedByVersionAndGridArea.Single(g => g.Key == (256L, "201"))
            .Single()
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([1.111, 4.444]);

        groupedByVersionAndGridArea.Single(g => g.Key == (512L, "201"))
            .Single()
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([1.222]);

        groupedByVersionAndGridArea.Single(g => g.Key == (1024L, "201"))
            .Single()
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([1.333]);

        groupedByVersionAndGridArea.Single(g => g.Key == (2048L, "201"))
            .Single()
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([1.444, 4.777]);
    }

    [Fact]
    public async Task GetAsync_EnergySupplierWithSpecificBalanceResponsible_ProductionConsumption()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: null,
            energySupplierId: AggregatedTimeSeriesQueries2Constants.EnergySupplierC,
            balanceResponsibleId: AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual
            .Select(ats => (ats.Version, ats.GridArea, ats.TimeSeriesType))
            .Should()
            .BeEquivalentTo([
                (256L, "201", TimeSeriesType.FlexConsumption),
                (512L, "201", TimeSeriesType.FlexConsumption),
                (1024L, "201", TimeSeriesType.FlexConsumption),
                (2048L, "201", TimeSeriesType.FlexConsumption),
                (256L, "201", TimeSeriesType.Production),
                (512L, "201", TimeSeriesType.Production),
                (1024L, "201", TimeSeriesType.Production),
                (2048L, "201", TimeSeriesType.Production),
                (256L, "101", TimeSeriesType.FlexConsumption),
                (512L, "101", TimeSeriesType.FlexConsumption),
                (1024L, "101", TimeSeriesType.FlexConsumption),
                (2048L, "101", TimeSeriesType.FlexConsumption),
                (256L, "101", TimeSeriesType.Production),
                (512L, "101", TimeSeriesType.Production),
                (1024L, "101", TimeSeriesType.Production),
                (2048L, "101", TimeSeriesType.Production)
            ]);
    }

    [Fact]
    public async Task GetAsync_BalanceResponsibleWithGridArea_ProductionConsumption()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
            energySupplierId: null,
            balanceResponsibleId: AggregatedTimeSeriesQueries2Constants.BalanceResponsibleA,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual
            .Select(ats => (ats.Version, ats.GridArea, ats.TimeSeriesType))
            .Should()
            .BeEquivalentTo([
                (1L, "101", TimeSeriesType.FlexConsumption), (2L, "101", TimeSeriesType.FlexConsumption),
                (3L, "101", TimeSeriesType.FlexConsumption), (5L, "101", TimeSeriesType.FlexConsumption),
                (7L, "101", TimeSeriesType.FlexConsumption), (8L, "101", TimeSeriesType.FlexConsumption),
                (1L, "101", TimeSeriesType.Production), (2L, "101", TimeSeriesType.Production),
                (3L, "101", TimeSeriesType.Production), (5L, "101", TimeSeriesType.Production),
                (7L, "101", TimeSeriesType.Production), (8L, "101", TimeSeriesType.Production)
            ]);
    }

    [Fact]
    public async Task GetAsync_BalanceResponsibleWithGridArea_Consumption()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
            energySupplierId: null,
            balanceResponsibleId: AggregatedTimeSeriesQueries2Constants.BalanceResponsibleB,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => ats.Version).Should().BeEquivalentTo([1L, 2L, 3L, 5L, 7L, 8L]);

        actual.Single(ats => ats.Version == 1L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.222, 6.666]);

        actual.Single(ats => ats.Version == 2L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([4.444]);

        actual.Single(ats => ats.Version == 3L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.333]);

        actual.Single(ats => ats.Version == 5L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.444, 7.110]);

        actual.Single(ats => ats.Version == 7L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([7.332]);

        actual.Single(ats => ats.Version == 8L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([4.777]);
    }

    [Fact]
    public async Task GetAsync_GridArea_Production()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production },
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
            energySupplierId: null,
            balanceResponsibleId: null,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => ats.Version).Should().BeEquivalentTo([111L, 222L, 333L, 555L, 777L, 888L]);

        actual.Single(ats => ats.Version == 111L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.222, 4.444, 6.666]);

        actual.Single(ats => ats.Version == 222L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([8.888]);

        actual.Single(ats => ats.Version == 333L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.444, 4.666]);

        actual.Single(ats => ats.Version == 555L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.666, 4.888, 7.110]);

        actual.Single(ats => ats.Version == 777L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([2.888, 7.332]);

        actual.Single(ats => ats.Version == 888L)
            .TimeSeriesPoints
            .Select(tsp => tsp.Quantity)
            .Should()
            .BeEquivalentTo([9.554]);
    }

    [Fact]
    public async Task GetAsync_GridArea_ProductionConsumptionExchange()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[]
            {
                TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NetExchangePerGa,
            },
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
            energySupplierId: null,
            balanceResponsibleId: null,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual
            .Select(ats => (ats.Version, ats.GridArea, ats.TimeSeriesType))
            .Should()
            .BeEquivalentTo([
                (111L, "101", TimeSeriesType.FlexConsumption), (222L, "101", TimeSeriesType.FlexConsumption),
                (333L, "101", TimeSeriesType.FlexConsumption), (555L, "101", TimeSeriesType.FlexConsumption),
                (777L, "101", TimeSeriesType.FlexConsumption), (888L, "101", TimeSeriesType.FlexConsumption),
                (111L, "101", TimeSeriesType.Production), (222L, "101", TimeSeriesType.Production),
                (333L, "101", TimeSeriesType.Production), (555L, "101", TimeSeriesType.Production),
                (777L, "101", TimeSeriesType.Production), (888L, "101", TimeSeriesType.Production),
                (111L, "101", TimeSeriesType.NetExchangePerGa), (222L, "101", TimeSeriesType.NetExchangePerGa),
                (333L, "101", TimeSeriesType.NetExchangePerGa), (555L, "101", TimeSeriesType.NetExchangePerGa),
                (777L, "101", TimeSeriesType.NetExchangePerGa), (888L, "101", TimeSeriesType.NetExchangePerGa)
            ]);
    }

    [Fact]
    public async Task GetAsync_NoEnergySupplierBalanceResponsibleOrGridArea_IsEquivalentToAskingForEachGridArea()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production },
            gridArea: null,
            energySupplierId: null,
            balanceResponsibleId: null,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        var eachGridAreaIndividually = new List<AggregatedTimeSeries>();
        foreach (var parametersForGridArea in new[]
                     {
                         AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
                         AggregatedTimeSeriesQueries2Constants.GridAreaCodeB,
                         AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
                     }
                     .Select(gridArea => parameters with { GridArea = gridArea }))
        {
            eachGridAreaIndividually.AddRange(await Sut.GetAsync(parametersForGridArea).ToListAsync());
        }

        // Assert
        actual.Should().BeEquivalentTo(eachGridAreaIndividually);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionInWrongPeriod_ReturnsNoResults()
    {
        // Arrange
        await AddDataAsync();
        var parameters = CreateQueryParameters(
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
            startOfPeriod: Instant.FromUtc(2020, 1, 1, 1, 1),
            endOfPeriod: Instant.FromUtc(2021, 1, 2, 1, 1));

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromEnergySupplierTotalProductionBadId_ReturnsNoResults()
    {
        // Arrange
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
            timeSeriesType: [TimeSeriesType.Production],
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: "badId");

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task
        GetAsync_WhenRequestFromEnergySupplierTotalProductionWithoutGridAreaFilter_ReturnsOneResultPerGridArea()
    {
        // Arrange
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: [TimeSeriesType.Production],
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: AggregatedTimeSeriesQueries2Constants.EnergySupplierA);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        var resultsPerGridArea = actual.GroupBy(x => x.GridArea);
        using var assertionScope = new AssertionScope();
        foreach (var resultsInGridArea in resultsPerGridArea)
        {
            resultsInGridArea.Should()
                .ContainSingle($"There should be only one result for grid area: {resultsInGridArea.Key}.");
        }
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorForOneDay_ReturnsResult()
    {
        // Arrange
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 3, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
            timeSeriesType: [TimeSeriesType.Production],
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries.GridArea.Should().Be(AggregatedTimeSeriesQueries2Constants.GridAreaCodeC);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(TimeSeriesType.Production);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .Should()
            .BeEquivalentTo(
                AggregatedTimeSeriesQueries2Constants.FourthQuantity,
                AggregatedTimeSeriesQueries2Constants.FourthQuantityThirdCorrection);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorStartAndEndDataAreEqual_ReturnsNoResult()
    {
        // Arrange
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
            timeSeriesType: [TimeSeriesType.Production],
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    private static AggregatedTimeSeriesQueryParameters CreateQueryParameters(
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

    private IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataOne(
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

    private IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataTwo(
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

    private IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataThree(
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

    private IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataFour(
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

    private async Task AddDataAsync()
    {
        await _fixture.DatabricksSchemaManager.EmptyAsync(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value
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

            await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(
                _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME, allThemRows);
        }
    }

    private string GetCalculationIdForBrGaAggregation(string time, string calculationType)
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

    private string GetCalculationIdForEsGaAggregation(string time, string calculationType)
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

    private string GetCalculationIdForGaAggregation(string time, string calculationType)
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
