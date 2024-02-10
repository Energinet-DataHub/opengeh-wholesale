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
    private const string FirstQuantity = "1.111";
    private const string FirstQuantityFirstCorrection = "1.222";
    private const string FirstQuantitySecondCorrection = "1.333";
    private const string FirstQuantityThirdCorrection = "1.444";

    private const string SecondQuantity = "2.222";
    private const string SecondQuantityFirstCorrection = "2.333";
    private const string SecondQuantitySecondCorrection = "2.444";

    private const string ThirdQuantity = "3.333";
    private const string ThirdQuantitySecondCorrection = "3.555";
    private const string ThirdQuantityThirdCorrection = "3.666";

    private const string FourthQuantity = "4.444";
    private const string FourthQuantityThirdCorrection = "4.777";

    private const string GridAreaCodeA = "101";
    private const string GridAreaCodeB = "201";
    private const string GridAreaCodeC = "301";

    private const string EnergySupplierA = "4321987654321";
    private const string EnergySupplierB = "7654321987654";
    private const string EnergySupplierC = "1987654321987";

    private const string BalanceResponsibleA = "2345678912345";
    private const string BalanceResponsibleB = "5678912345678";
    private const string BalanceResponsibleC = "8912345678912";

    private const string FirstCalculationResultId = "aaaaaaaa-386f-49eb-8b56-63fae62e4fc7";
    private const string SecondCalculationResultId = "bbbbbbbb-b58b-4190-a873-eded0ed50c20";
    private const string ThirdCalculationResultId = "cccccccc-386f-49eb-8b56-63fae62e4fc7";

    private const string FirstHour = "2022-01-01T01:00:00.000Z";
    private const string SecondHour = "2022-01-01T02:00:00.000Z";
    private const string ThirdHour = "2022-01-01T03:00:00.000Z";

    private const string SecondDay = "2022-01-02T00:00:00.000Z";

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
    public async Task
        GetAsync_EnergySupplierWithSpecificBalanceResponsibleAndGridArea_SecondCorrectionSettlement_Production_OneSeriesWithTwoPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production },
            gridArea: GridAreaCodeB,
            energySupplierId: EnergySupplierB,
            balanceResponsibleId: BalanceResponsibleB,
            calculationType: CalculationType.SecondCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual.OrderBy(ts => ts.TimeSeriesType).Should().SatisfyRespectively(
            first =>
            {
                first.GridArea.Should().Be(GridAreaCodeB);
                first.CalculationType.Should().Be(CalculationType.SecondCorrectionSettlement);
                first.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                first.Version.Should().Be(512L);
                first.TimeSeriesPoints.Should().HaveCount(1);
                var energyTimeSeriesPoint = first.TimeSeriesPoints.Single();
                energyTimeSeriesPoint.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantitySecondCorrection);
                energyTimeSeriesPoint.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
            },
            second =>
            {
                second.GridArea.Should().Be(GridAreaCodeB);
                second.CalculationType.Should().Be(CalculationType.SecondCorrectionSettlement);
                second.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                second.Version.Should().Be(42L);
                second.TimeSeriesPoints.Should().HaveCount(1);
                var energyTimeSeriesPoint = second.TimeSeriesPoints.Single();
                energyTimeSeriesPoint.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantitySecondCorrection);
                energyTimeSeriesPoint.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
            });
    }

    [Fact]
    public async Task
        GetAsync_EnergySupplierWithSpecificBalanceResponsibleAndGridArea_BalanceFixing_ProductionConsumption_TwoSeriesWithTwoPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeA,
            energySupplierId: EnergySupplierC,
            balanceResponsibleId: BalanceResponsibleB,
            calculationType: CalculationType.BalanceFixing,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual.OrderBy(ts => ts.TimeSeriesType).Should().SatisfyRespectively(
            first =>
            {
                first.GridArea.Should().Be(GridAreaCodeA);
                first.CalculationType.Should().Be(CalculationType.BalanceFixing);
                first.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
                first.TimeSeriesPoints.Should().HaveCount(2);
                first.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantity);
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantity);
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    });
            },
            second =>
            {
                second.GridArea.Should().Be(GridAreaCodeA);
                second.CalculationType.Should().Be(CalculationType.BalanceFixing);
                second.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                second.TimeSeriesPoints.Should().HaveCount(2);
                second.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantity);
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantity);
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    });
            });
    }

    [Fact]
    public async Task
        GetAsync_EnergySupplierWithGridArea_FirstCorrectionSettlement_Consumption_OneSeriesWithOnePoint()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeB,
            energySupplierId: EnergySupplierC,
            balanceResponsibleId: null,
            calculationType: CalculationType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.Single();
        aggregatedTimeSeries.GridArea.Should().Be(GridAreaCodeB);
        aggregatedTimeSeries.CalculationType.Should().Be(CalculationType.FirstCorrectionSettlement);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
        aggregatedTimeSeries.TimeSeriesPoints.Should().HaveCount(1);
        var energyTimeSeriesPoint = aggregatedTimeSeries.TimeSeriesPoints.Single();
        energyTimeSeriesPoint.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FirstQuantityFirstCorrection);
        energyTimeSeriesPoint.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
    }

    [Fact]
    public async Task
        GetAsync_EnergySupplierWithGridArea_SecondCorrectionSettlement_ProductionConsumption_TwoSeriesWithThreePoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeC,
            energySupplierId: EnergySupplierA,
            balanceResponsibleId: null,
            calculationType: CalculationType.SecondCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual.OrderBy(ts => ts.TimeSeriesType).Should().SatisfyRespectively(
            first =>
            {
                first.GridArea.Should().Be(GridAreaCodeC);
                first.CalculationType.Should().Be(CalculationType.SecondCorrectionSettlement);
                first.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
                first.TimeSeriesPoints.Should().HaveCount(3);
                first.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FirstQuantitySecondCorrection);
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantitySecondCorrection);
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantitySecondCorrection);
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    });
            },
            second =>
            {
                second.GridArea.Should().Be(GridAreaCodeC);
                second.CalculationType.Should().Be(CalculationType.SecondCorrectionSettlement);
                second.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                second.TimeSeriesPoints.Should().HaveCount(3);
                second.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FirstQuantitySecondCorrection);
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantitySecondCorrection);
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantitySecondCorrection);
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    });
            });
    }

    [Fact]
    public async Task
        GetAsync_EnergySupplierWithSpecificBalanceResponsible_ThirdCorrectionSettlement_Consumption_OneSeriesWithTwoPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: null,
            energySupplierId: EnergySupplierA,
            balanceResponsibleId: BalanceResponsibleB,
            calculationType: CalculationType.ThirdCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual
            .OrderBy(ts => ts.GridArea)
            .ThenBy(ts => ts.TimeSeriesType)
            .ThenBy(ts => ts.Version)
            .Should()
            .SatisfyRespectively(
                first =>
            {
                first.GridArea.Should().Be(GridAreaCodeA);
                first.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
                first.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
                first.Version.Should().Be(42L);
                first.TimeSeriesPoints.Should().HaveCount(1);
                var energyTimeSeriesPoint = first.TimeSeriesPoints.Single();
                energyTimeSeriesPoint.Quantity
                    .ToString(CultureInfo.InvariantCulture)
                    .Should()
                    .Be(ThirdQuantityThirdCorrection);

                energyTimeSeriesPoint.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
            },
                second =>
            {
                second.GridArea.Should().Be(GridAreaCodeA);
                second.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
                second.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
                second.Version.Should().Be(512L);
                second.TimeSeriesPoints.Should().HaveCount(1);
                var energyTimeSeriesPoint = second.TimeSeriesPoints.Single();
                energyTimeSeriesPoint.Quantity
                    .ToString(CultureInfo.InvariantCulture)
                    .Should()
                    .Be(FourthQuantityThirdCorrection);

                energyTimeSeriesPoint.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
            });
    }

    [Fact]
    public async Task
        GetAsync_EnergySupplierWithSpecificBalanceResponsible_FirstCorrectionSettlement_ProductionConsumption_TwoSeriesWithOnePoint()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: null,
            energySupplierId: EnergySupplierB,
            balanceResponsibleId: BalanceResponsibleB,
            calculationType: CalculationType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual.OrderBy(ts => ts.GridArea).ThenBy(ts => ts.TimeSeriesType).Should().SatisfyRespectively(
            first =>
            {
                first.GridArea.Should().Be(GridAreaCodeB);
                first.CalculationType.Should().Be(CalculationType.FirstCorrectionSettlement);
                first.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
                first.TimeSeriesPoints.Should().HaveCount(1);
                var energyTimeSeriesPoint = first.TimeSeriesPoints.Single();
                energyTimeSeriesPoint.Quantity.ToString(CultureInfo.InvariantCulture).Should()
                    .Be(SecondQuantityFirstCorrection);
                energyTimeSeriesPoint.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
            },
            second =>
            {
                second.GridArea.Should().Be(GridAreaCodeB);
                second.CalculationType.Should().Be(CalculationType.FirstCorrectionSettlement);
                second.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                second.TimeSeriesPoints.Should().HaveCount(1);
                var energyTimeSeriesPoint = second.TimeSeriesPoints.Single();
                energyTimeSeriesPoint.Quantity.ToString(CultureInfo.InvariantCulture).Should()
                    .Be(SecondQuantityFirstCorrection);
                energyTimeSeriesPoint.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
            });
    }

    [Fact]
    public async Task
        GetAsync_BalanceResponsibleWithGridArea_ThirdCorrectionSettlement_ProductionConsumption_TwoSeriesWithTwoPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeA,
            energySupplierId: null,
            balanceResponsibleId: BalanceResponsibleA,
            calculationType: CalculationType.ThirdCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual.OrderBy(ts => ts.TimeSeriesType).Should().SatisfyRespectively(
            first =>
            {
                first.GridArea.Should().Be(GridAreaCodeA);
                first.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
                first.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
                first.TimeSeriesPoints.Should().HaveCount(2);
                first.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FourthQuantityThirdCorrection);
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            },
            second =>
            {
                second.GridArea.Should().Be(GridAreaCodeA);
                second.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
                second.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                second.TimeSeriesPoints.Should().HaveCount(2);
                second.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FourthQuantityThirdCorrection);
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            });
    }

    [Fact]
    public async Task
        GetAsync_BalanceResponsibleWithGridArea_SecondCorrectionSettlement_Consumption_OneSeriesWithThreePoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeC,
            energySupplierId: null,
            balanceResponsibleId: BalanceResponsibleC,
            calculationType: CalculationType.SecondCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.Single();
        aggregatedTimeSeries.GridArea.Should().Be(GridAreaCodeC);
        aggregatedTimeSeries.CalculationType.Should().Be(CalculationType.SecondCorrectionSettlement);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
        aggregatedTimeSeries.TimeSeriesPoints.Should().HaveCount(3);
        aggregatedTimeSeries.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
            p =>
            {
                p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FirstQuantitySecondCorrection);
                p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
            },
            p =>
            {
                p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantitySecondCorrection);
                p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
            },
            p =>
            {
                p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantitySecondCorrection);
                p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
            });
    }

    [Fact]
    public async Task
        GetAsync_GridArea_ThirdCorrectionSettlement_ProductionConsumptionExchange_ThreeSeriesWithThreePoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[]
            {
                TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NetExchangePerGa,
            },
            gridArea: GridAreaCodeA,
            energySupplierId: null,
            balanceResponsibleId: null,
            calculationType: CalculationType.ThirdCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(3);
        actual.OrderBy(ts => ts.TimeSeriesType).Should().SatisfyRespectively(
            first =>
            {
                first.GridArea.Should().Be(GridAreaCodeA);
                first.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
                first.TimeSeriesType.Should().Be(TimeSeriesType.FlexConsumption);
                first.TimeSeriesPoints.Should().HaveCount(3);
                first.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(ThirdQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(ThirdQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FourthQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(FourthQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            },
            second =>
            {
                second.GridArea.Should().Be(GridAreaCodeA);
                second.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
                second.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                second.TimeSeriesPoints.Should().HaveCount(3);
                second.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(ThirdQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(ThirdQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FourthQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(FourthQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            },
            third =>
            {
                third.GridArea.Should().Be(GridAreaCodeA);
                third.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
                third.TimeSeriesType.Should().Be(TimeSeriesType.NetExchangePerGa);
                third.TimeSeriesPoints.Should().HaveCount(3);
                third.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(FirstQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(ThirdQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(ThirdQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FourthQuantityThirdCorrection, CultureInfo.InvariantCulture)
                                 + decimal.Parse(FourthQuantityThirdCorrection, CultureInfo.InvariantCulture))
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            });
    }

    [Fact]
    public async Task
        GetAsync_NoEnergySupplierBalanceResponsibleOrGridArea_BalanceFixing_Production_ThreeSeriesWithFourPoints()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.NetExchangePerGa },
            gridArea: null,
            energySupplierId: null,
            balanceResponsibleId: null,
            calculationType: CalculationType.BalanceFixing,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(6);
        actual.OrderBy(ts => ts.GridArea).ThenBy(ts => ts.TimeSeriesType).Should().SatisfyRespectively(
            first =>
            {
                first.GridArea.Should().Be(GridAreaCodeA);
                first.CalculationType.Should().Be(CalculationType.BalanceFixing);
                first.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                first.TimeSeriesPoints.Should().HaveCount(4);
                first.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FirstQuantity, CultureInfo.InvariantCulture) // ES1, BR1
                                 + decimal.Parse(FirstQuantity, CultureInfo.InvariantCulture)) // ES2, BR1
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(SecondQuantity, CultureInfo.InvariantCulture) // ES1, BR1
                                 + decimal.Parse(SecondQuantity, CultureInfo.InvariantCulture)) // ES3, BR2
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(ThirdQuantity, CultureInfo.InvariantCulture) // ES1, BR2
                                 + decimal.Parse(ThirdQuantity, CultureInfo.InvariantCulture)) // ES3, BR2
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FourthQuantity, CultureInfo.InvariantCulture) // ES1, BR2
                                 + decimal.Parse(FourthQuantity, CultureInfo.InvariantCulture)) // ES2, BR1
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            },
            second =>
            {
                second.GridArea.Should().Be(GridAreaCodeA);
                second.CalculationType.Should().Be(CalculationType.BalanceFixing);
                second.TimeSeriesType.Should().Be(TimeSeriesType.NetExchangePerGa);
                second.TimeSeriesPoints.Should().HaveCount(4);
                second.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FirstQuantity, CultureInfo.InvariantCulture) // ES1, BR1
                                 + decimal.Parse(FirstQuantity, CultureInfo.InvariantCulture)) // ES2, BR1
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(SecondQuantity, CultureInfo.InvariantCulture) // ES1, BR1
                                 + decimal.Parse(SecondQuantity, CultureInfo.InvariantCulture)) // ES3, BR2
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(ThirdQuantity, CultureInfo.InvariantCulture) // ES1, BR2
                                 + decimal.Parse(ThirdQuantity, CultureInfo.InvariantCulture)) // ES3, BR2
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture)
                            .Should()
                            .Be((decimal.Parse(FourthQuantity, CultureInfo.InvariantCulture) // ES1, BR2
                                 + decimal.Parse(FourthQuantity, CultureInfo.InvariantCulture)) // ES2, BR1
                                .ToString(CultureInfo.InvariantCulture));
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            },
            third =>
            {
                third.GridArea.Should().Be(GridAreaCodeB);
                third.CalculationType.Should().Be(CalculationType.BalanceFixing);
                third.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                third.TimeSeriesPoints.Should().HaveCount(4);
                third.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FirstQuantity); // ES3, BR2
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantity); // ES2, BR2
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantity); // ES2, BR2
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FourthQuantity); // ES3, BR2
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            },
            fourth =>
            {
                fourth.GridArea.Should().Be(GridAreaCodeB);
                fourth.CalculationType.Should().Be(CalculationType.BalanceFixing);
                fourth.TimeSeriesType.Should().Be(TimeSeriesType.NetExchangePerGa);
                fourth.TimeSeriesPoints.Should().HaveCount(4);
                fourth.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FirstQuantity); // ES3, BR2
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantity); // ES2, BR2
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantity); // ES2, BR2
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FourthQuantity); // ES3, BR2
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            },
            fifth =>
            {
                fifth.GridArea.Should().Be(GridAreaCodeC);
                fifth.CalculationType.Should().Be(CalculationType.BalanceFixing);
                fifth.TimeSeriesType.Should().Be(TimeSeriesType.Production);
                fifth.TimeSeriesPoints.Should().HaveCount(4);
                fifth.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FirstQuantity); // ES1, BR3
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantity); // ES1, BR3
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantity); // ES1, BR3
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FourthQuantity); // ES1, BR3
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            },
            sixth =>
            {
                sixth.GridArea.Should().Be(GridAreaCodeC);
                sixth.CalculationType.Should().Be(CalculationType.BalanceFixing);
                sixth.TimeSeriesType.Should().Be(TimeSeriesType.NetExchangePerGa);
                sixth.TimeSeriesPoints.Should().HaveCount(4);
                sixth.TimeSeriesPoints.OrderBy(p => p.Time).Should().SatisfyRespectively(
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FirstQuantity); // ES1, BR3
                        p.Time.Should().Be(DateTimeOffset.Parse(FirstHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(SecondQuantity); // ES1, BR3
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(ThirdQuantity); // ES1, BR3
                        p.Time.Should().Be(DateTimeOffset.Parse(ThirdHour));
                    },
                    p =>
                    {
                        p.Quantity.ToString(CultureInfo.InvariantCulture).Should().Be(FourthQuantity); // ES1, BR3
                        p.Time.Should().Be(DateTimeOffset.Parse(SecondDay));
                    });
            });
    }

    [Fact]
    public async Task GetLatestCorrectionForGridAreaAsync_EnergySupplierWithSpecificBalanceResponsibleAndGridArea_EquivalentToGetAsyncForAllSettlementTypes()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeB,
            energySupplierId: EnergySupplierB,
            balanceResponsibleId: BalanceResponsibleB,
            calculationType: CalculationType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var getAsyncResult = await Sut.GetAsync(parameters).ToListAsync();
        getAsyncResult.AddRange(await Sut
            .GetAsync(parameters with { CalculationType = CalculationType.SecondCorrectionSettlement }).ToListAsync());
        getAsyncResult.AddRange(await Sut
            .GetAsync(parameters with { CalculationType = CalculationType.ThirdCorrectionSettlement }).ToListAsync());

        var latestCorrectionForGridAreaAsyncResult =
            await Sut.GetLatestCorrectionForGridAreaAsync(parameters with { CalculationType = null }).ToListAsync();

        using var assertionScope = new AssertionScope();
        getAsyncResult.Should().BeEquivalentTo(latestCorrectionForGridAreaAsyncResult);
    }

    [Fact]
    public async Task GetLatestCorrectionForGridAreaAsync_EnergySupplierWithGridArea_EquivalentToGetAsyncForAllSettlementTypes()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeB,
            energySupplierId: EnergySupplierB,
            balanceResponsibleId: null,
            calculationType: CalculationType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var getAsyncResult = await Sut.GetAsync(parameters).ToListAsync();
        getAsyncResult.AddRange(await Sut
            .GetAsync(parameters with { CalculationType = CalculationType.SecondCorrectionSettlement }).ToListAsync());
        getAsyncResult.AddRange(await Sut
            .GetAsync(parameters with { CalculationType = CalculationType.ThirdCorrectionSettlement }).ToListAsync());

        var latestCorrectionForGridAreaAsyncResult =
            await Sut.GetLatestCorrectionForGridAreaAsync(parameters with { CalculationType = null }).ToListAsync();

        using var assertionScope = new AssertionScope();
        getAsyncResult.Should().BeEquivalentTo(latestCorrectionForGridAreaAsyncResult);
    }

    [Fact]
    public async Task GetLatestCorrectionForGridAreaAsync_BalanceResponsibleWithGridArea_EquivalentToGetAsyncForAllSettlementTypes()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeB,
            energySupplierId: null,
            balanceResponsibleId: BalanceResponsibleB,
            calculationType: CalculationType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var getAsyncResult = await Sut.GetAsync(parameters).ToListAsync();
        getAsyncResult.AddRange(await Sut
            .GetAsync(parameters with { CalculationType = CalculationType.SecondCorrectionSettlement }).ToListAsync());
        getAsyncResult.AddRange(await Sut
            .GetAsync(parameters with { CalculationType = CalculationType.ThirdCorrectionSettlement }).ToListAsync());

        var latestCorrectionForGridAreaAsyncResult =
            await Sut.GetLatestCorrectionForGridAreaAsync(parameters with { CalculationType = null }).ToListAsync();

        using var assertionScope = new AssertionScope();
        getAsyncResult.Should().BeEquivalentTo(latestCorrectionForGridAreaAsyncResult);
    }

    [Fact]
    public async Task GetLatestCorrectionForGridAreaAsync_GridArea_EquivalentToGetAsyncForAllSettlementTypes()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeB,
            energySupplierId: null,
            balanceResponsibleId: null,
            calculationType: CalculationType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var getAsyncResult = await Sut.GetAsync(parameters).ToListAsync();
        getAsyncResult.AddRange(await Sut
            .GetAsync(parameters with { CalculationType = CalculationType.SecondCorrectionSettlement }).ToListAsync());
        getAsyncResult.AddRange(await Sut
            .GetAsync(parameters with { CalculationType = CalculationType.ThirdCorrectionSettlement }).ToListAsync());

        var latestCorrectionForGridAreaAsyncResult =
            await Sut.GetLatestCorrectionForGridAreaAsync(parameters with { CalculationType = null }).ToListAsync();

        using var assertionScope = new AssertionScope();
        getAsyncResult.Should().BeEquivalentTo(latestCorrectionForGridAreaAsyncResult);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionInWrongPeriod_ReturnsNoResults()
    {
        // Arrange
        await AddDataAsync();
        var parameters = CreateQueryParameters(
            gridArea: GridAreaCodeA,
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
            gridArea: GridAreaCodeC,
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
            energySupplierId: EnergySupplierA);

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
            gridArea: GridAreaCodeC,
            timeSeriesType: [TimeSeriesType.Production],
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries.GridArea.Should().Be(GridAreaCodeC);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(TimeSeriesType.Production);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .Should()
            .BeEquivalentTo(FourthQuantity, FourthQuantityThirdCorrection);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorStartAndEndDataAreEqual_ReturnsNoResult()
    {
        // Arrange
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            gridArea: GridAreaCodeC,
            timeSeriesType: [TimeSeriesType.Production],
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            calculationType: CalculationType.BalanceFixing);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetLatestCorrectionAsync_WhenCalculationTypeIsDefined_ThrowsException()
    {
        // Arrange
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            [TimeSeriesType.Production],
            startOfPeriodFilter,
            endOfPeriodFilter,
            GridAreaCodeC,
            calculationType: CalculationType.BalanceFixing);

        // Act
        var act = async () => await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "The calculation type will be overwritten when fetching the latest correction.",
            parameters.CalculationType);
    }

    private static AggregatedTimeSeriesQueryParameters CreateQueryParameters(
        IReadOnlyCollection<TimeSeriesType>? timeSeriesType = null,
        Instant? startOfPeriod = null,
        Instant? endOfPeriod = null,
        string? gridArea = null,
        string? energySupplierId = null,
        string? balanceResponsibleId = null,
        CalculationType? calculationType = null)
    {
        IReadOnlyCollection<CalculationForPeriod> calculationForPeriods =
        [
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(FirstCalculationResultId),
                1024),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(SecondCalculationResultId),
                512),
            new CalculationForPeriod(
                new Period(
                    startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
                    endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0)),
                Guid.Parse(ThirdCalculationResultId),
                42)
        ];

        return new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: timeSeriesType ?? new[] { TimeSeriesType.Production },
            LatestCalculationForPeriod: calculationForPeriods,
            GridArea: gridArea,
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId,
            CalculationType: calculationType);
    }

    private IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataOne(
        string gridAreaCode,
        string balanceResponsibleId,
        string energySupplierId,
        string timeSeriesType)
    {
        var o = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: FirstCalculationResultId,
            calculationResultId: FirstCalculationResultId,
            time: FirstHour,
            gridArea: gridAreaCode,
            quantity: FirstQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var f = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: SecondCalculationResultId,
            calculationResultId: SecondCalculationResultId,
            time: FirstHour,
            gridArea: gridAreaCode,
            quantity: FirstQuantityFirstCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.FirstCorrectionSettlement);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: ThirdCalculationResultId,
            calculationResultId: ThirdCalculationResultId,
            time: FirstHour,
            gridArea: gridAreaCode,
            quantity: FirstQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.SecondCorrectionSettlement);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: ThirdCalculationResultId,
            calculationResultId: ThirdCalculationResultId,
            time: FirstHour,
            gridArea: gridAreaCode,
            quantity: FirstQuantityThirdCorrection,
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
            calculationId: FirstCalculationResultId,
            calculationResultId: FirstCalculationResultId,
            time: SecondHour,
            gridArea: gridAreaCode,
            quantity: SecondQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var f = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: SecondCalculationResultId,
            calculationResultId: SecondCalculationResultId,
            time: SecondHour,
            gridArea: gridAreaCode,
            quantity: SecondQuantityFirstCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.FirstCorrectionSettlement);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: ThirdCalculationResultId,
            calculationResultId: ThirdCalculationResultId,
            time: SecondHour,
            gridArea: gridAreaCode,
            quantity: SecondQuantitySecondCorrection,
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
            calculationId: FirstCalculationResultId,
            calculationResultId: FirstCalculationResultId,
            time: ThirdHour,
            gridArea: gridAreaCode,
            quantity: ThirdQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: SecondCalculationResultId,
            calculationResultId: SecondCalculationResultId,
            time: ThirdHour,
            gridArea: gridAreaCode,
            quantity: ThirdQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.SecondCorrectionSettlement);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: ThirdCalculationResultId,
            calculationResultId: ThirdCalculationResultId,
            time: ThirdHour,
            gridArea: gridAreaCode,
            quantity: ThirdQuantityThirdCorrection,
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
            calculationId: FirstCalculationResultId,
            calculationResultId: FirstCalculationResultId,
            time: SecondDay,
            gridArea: gridAreaCode,
            quantity: FourthQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            calculationType: DeltaTableCalculationType.BalanceFixing);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            calculationId: SecondCalculationResultId,
            calculationResultId: SecondCalculationResultId,
            time: SecondDay,
            gridArea: gridAreaCode,
            quantity: FourthQuantityThirdCorrection,
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
                GridAreaCodeA,
                BalanceResponsibleA,
                EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataTwo(
                GridAreaCodeA,
                BalanceResponsibleA,
                EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataOne(
                GridAreaCodeA,
                BalanceResponsibleA,
                EnergySupplierB,
                timeSeriesType));
            allThemRows.AddRange(CreateDataFour(
                GridAreaCodeA,
                BalanceResponsibleA,
                EnergySupplierB,
                timeSeriesType));

            // BalanceResponsibleB
            allThemRows.AddRange(CreateDataThree(
                GridAreaCodeA,
                BalanceResponsibleB,
                EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataFour(
                GridAreaCodeA,
                BalanceResponsibleB,
                EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataTwo(
                GridAreaCodeA,
                BalanceResponsibleB,
                EnergySupplierC,
                timeSeriesType));
            allThemRows.AddRange(CreateDataThree(
                GridAreaCodeA,
                BalanceResponsibleB,
                EnergySupplierC,
                timeSeriesType));

            // GridAreaCodeB
            // BalanceResponsibleB
            allThemRows.AddRange(CreateDataTwo(
                GridAreaCodeB,
                BalanceResponsibleB,
                EnergySupplierB,
                timeSeriesType));
            allThemRows.AddRange(CreateDataThree(
                GridAreaCodeB,
                BalanceResponsibleB,
                EnergySupplierB,
                timeSeriesType));
            allThemRows.AddRange(CreateDataOne(
                GridAreaCodeB,
                BalanceResponsibleB,
                EnergySupplierC,
                timeSeriesType));
            allThemRows.AddRange(CreateDataFour(
                GridAreaCodeB,
                BalanceResponsibleB,
                EnergySupplierC,
                timeSeriesType));

            // GridAreaCodeC
            // BalanceResponsibleC
            allThemRows.AddRange(CreateDataOne(
                GridAreaCodeC,
                BalanceResponsibleC,
                EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataTwo(
                GridAreaCodeC,
                BalanceResponsibleC,
                EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataThree(
                GridAreaCodeC,
                BalanceResponsibleC,
                EnergySupplierA,
                timeSeriesType));
            allThemRows.AddRange(CreateDataFour(
                GridAreaCodeC,
                BalanceResponsibleC,
                EnergySupplierA,
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
                        calculationId: ThirdCalculationResultId,
                        calculationResultId: ThirdCalculationResultId,
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
                        calculationId: ThirdCalculationResultId,
                        calculationResultId: ThirdCalculationResultId,
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
                        calculationId: ThirdCalculationResultId,
                        calculationResultId: ThirdCalculationResultId,
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
}
