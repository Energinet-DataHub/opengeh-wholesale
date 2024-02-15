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
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public sealed class AggregatedTimeSeriesQueries2Tests : TestBase<AggregatedTimeSeriesQueries>,
    IClassFixture<DatabricksSqlStatementApiFixture>
{
    private readonly AggregatedTimeSeriesQueries2Data _aggregatedTimeSeriesQueries2Data;

    public AggregatedTimeSeriesQueries2Tests(DatabricksSqlStatementApiFixture fixture)
    {
        Fixture.Inject(fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(fixture.GetDatabricksExecutor());
        _aggregatedTimeSeriesQueries2Data = new AggregatedTimeSeriesQueries2Data(fixture);
    }

    /*
2 Test purpose
══════════════

  The purpose of the tests in this class is to ensure that the SQL queries made by `AggregatedTimeSeriesQueryStatement'
  using `AggregatedTimeSeriesQueryParameters' are resulting in the expected and correct data. The structure of the
  queries is not considered, neither is performance. The tests focus solely on whether the returned data correlates with
  our expectations for a particular query parameter.


2.1 Test structure
──────────────────

  The tests in this file all follow the same pattern:
  1. We generate a specific query parameter
  2. We execute the generated query using the sut
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

  The tests are for the most part grouped in two
  1. We consider only one time series type
  2. We consider multiple time series types


2.2 Result validation
─────────────────────

  For the case with only one time series type, we, for the most part, consider
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Type                      Validation
  ──────────────────────────────────────────────
   Aggregated time series    version, grid area
   Energy time series point  quantity
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  For the case with multiple time series types, we limit ourselves to only validating the time series, as the point
  validating is achieved in the former case
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Type                      Validation
  ────────────────────────────────────────────────────────────────
   Aggregated time series    version, grid area, time series type
   Energy time series point
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
     */

    [Fact]
    public async Task
        GetAsync_WhenRequestFromEnergySupplierWithSpecificBalanceResponsibleAndGridArea_CorrectTimeSeriesPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task
        GetAsync_WhenRequestFromEnergySupplierWithSpecificBalanceResponsibleAndGridArea_CorrectTimeSeriesSplit()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestFromEnergySupplierWithGridArea_CorrectTimeSeriesPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestFromEnergySupplierWithGridArea_CorrectTimeSeriesSplit()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestFromEnergySupplierWithSpecificBalanceResponsible_CorrectTimeSeriesPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestFromEnergySupplierWithSpecificBalanceResponsible_CorrectTimeSeriesSplit()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestFromBalanceResponsibleWithGridArea_CorrectTimeSeriesPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestFromBalanceResponsibleWithGridArea_CorrectTimeSeriesSplit()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestForGridArea_CorrectTimeSeriesPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestForGridArea_CorrectTimeSeriesSplit()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestForNoEnergySupplierBalanceResponsibleOrGridArea_CorrectTimeSeriesPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();
        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
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
    public async Task GetAsync_WhenRequestReceived_PeriodFilterDenotesHalfClosedIntervalWhereStarIsIncludedAndEndIsNot()
    {
        // Arrange
        var startOfPeriodFilter =
            Instant.FromDateTimeOffset(DateTimeOffset.Parse(AggregatedTimeSeriesQueries2Constants.FirstHour));

        var endOfPeriodFilter =
            Instant.FromDateTimeOffset(DateTimeOffset.Parse(AggregatedTimeSeriesQueries2Constants.SecondHour));

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeA,
            timeSeriesType: [TimeSeriesType.Production],
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeEmpty();
        actual.Should().AllSatisfy(ats => ats.TimeSeriesPoints.Should().NotBeEmpty());

        actual.SelectMany(ats => ats.TimeSeriesPoints.Select(tsp => tsp.Time))
            .Should()
            .AllSatisfy(dto => dto.Should().BeOnOrAfter(startOfPeriodFilter.ToDateTimeOffset()))
            .And
            .AllSatisfy(dto => dto.Should().BeBefore(endOfPeriodFilter.ToDateTimeOffset()));
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorStartAndEndDataAreEqual_ReturnsNoResult()
    {
        // Arrange
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);

        await _aggregatedTimeSeriesQueries2Data.AddDataAsync();

        var parameters = AggregatedTimeSeriesQueries2Data.CreateQueryParameters(
            gridArea: AggregatedTimeSeriesQueries2Constants.GridAreaCodeC,
            timeSeriesType: [TimeSeriesType.Production],
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }
}
