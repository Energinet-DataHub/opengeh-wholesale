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

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public class AggregatedTimeSeriesQueries2Tests : TestBase<AggregatedTimeSeriesQueries>,
    IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string BatchId = "019703e7-98ee-45c1-b343-0cbf185a47d9";

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
    private const string EnergySupplierD = "3219876543219";

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
    private const string ThirdDay = "2022-01-03T00:00:00.000Z";

    private readonly DatabricksSqlStatementApiFixture _fixture;

    public AggregatedTimeSeriesQueries2Tests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
        Fixture.Inject(_fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(_fixture.GetDatabricksExecutor());
    }

    /*
                time grid energy balance process
1               prod    2      2       2  second
2                con    2      3     n/a   first
3               prod    1      1       1   first
4           prod+con    2    n/a       1   third
5           prod+con    1      3       2     fix
6               prod    3      3       3   third
7  prod+con+exchange    1      2     n/a   third
8                con  n/a      1       2   third
9                con    1    n/a       3  second
10              prod  n/a    n/a     n/a     fix
11 prod+con+exchange    2      1       3     fix
12 prod+con+exchange  n/a      3       1  second
13          prod+con    3      1     n/a  second
14 prod+con+exchange    3    n/a       2   first
15          prod+con  n/a      2       3   first
16               con    3      2       1     fix
     */

    [Fact]
    public async Task One()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production },
            gridArea: GridAreaCodeB,
            energySupplierId: EnergySupplierB,
            balanceResponsibleId: BalanceResponsibleB,
            processType: ProcessType.SecondCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Two()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeB,
            energySupplierId: EnergySupplierC,
            balanceResponsibleId: null,
            processType: ProcessType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Three()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production },
            gridArea: GridAreaCodeA,
            energySupplierId: EnergySupplierA,
            balanceResponsibleId: BalanceResponsibleA,
            processType: ProcessType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Four()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeB,
            energySupplierId: null,
            balanceResponsibleId: BalanceResponsibleA,
            processType: ProcessType.ThirdCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Five()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeA,
            energySupplierId: EnergySupplierC,
            balanceResponsibleId: BalanceResponsibleB,
            processType: ProcessType.BalanceFixing,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Six()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production },
            gridArea: GridAreaCodeC,
            energySupplierId: EnergySupplierC,
            balanceResponsibleId: BalanceResponsibleC,
            processType: ProcessType.ThirdCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Seven()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NetExchangePerGa },
            gridArea: GridAreaCodeA,
            energySupplierId: EnergySupplierB,
            balanceResponsibleId: null,
            processType: ProcessType.ThirdCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Seven_Fixed()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NetExchangePerGa },
            gridArea: GridAreaCodeA,
            energySupplierId: null,
            balanceResponsibleId: null,
            processType: ProcessType.ThirdCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Eight()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: null,
            energySupplierId: EnergySupplierA,
            balanceResponsibleId: BalanceResponsibleB,
            processType: ProcessType.ThirdCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Nine()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeA,
            energySupplierId: null,
            balanceResponsibleId: BalanceResponsibleC,
            processType: ProcessType.SecondCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Ten()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production },
            gridArea: null,
            energySupplierId: null,
            balanceResponsibleId: null,
            processType: ProcessType.BalanceFixing,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Eleven()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NetExchangePerGa },
            gridArea: GridAreaCodeB,
            energySupplierId: EnergySupplierA,
            balanceResponsibleId: BalanceResponsibleC,
            processType: ProcessType.BalanceFixing,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Twelve()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NetExchangePerGa },
            gridArea: null,
            energySupplierId: EnergySupplierC,
            balanceResponsibleId: BalanceResponsibleA,
            processType: ProcessType.SecondCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Thirteen()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeC,
            energySupplierId: EnergySupplierA,
            balanceResponsibleId: null,
            processType: ProcessType.SecondCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Fourteen()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NetExchangePerGa },
            gridArea: GridAreaCodeC,
            energySupplierId: null,
            balanceResponsibleId: BalanceResponsibleB,
            processType: ProcessType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Fifteen()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.Production, TimeSeriesType.FlexConsumption },
            gridArea: null,
            energySupplierId: EnergySupplierB,
            balanceResponsibleId: BalanceResponsibleC,
            processType: ProcessType.FirstCorrectionSettlement,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task Sixteen()
    {
        var startOfPeriodFilter = Instant.FromUtc(2021, 12, 31, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 4, 0, 0);

        await AddDataAsync();

        var parameters = CreateQueryParameters(
            timeSeriesType: new[] { TimeSeriesType.FlexConsumption },
            gridArea: GridAreaCodeC,
            energySupplierId: EnergySupplierB,
            balanceResponsibleId: BalanceResponsibleA,
            processType: ProcessType.BalanceFixing,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(0);
    }

    private static AggregatedTimeSeriesQueryParameters CreateQueryParameters(
        IReadOnlyCollection<TimeSeriesType>? timeSeriesType = null,
        Instant? startOfPeriod = null,
        Instant? endOfPeriod = null,
        string? gridArea = null,
        string? energySupplierId = null,
        string? balanceResponsibleId = null,
        ProcessType? processType = null)
    {
        return new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: timeSeriesType ?? new[] { TimeSeriesType.Production },
            StartOfPeriod: startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
            EndOfPeriod: endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0),
            GridArea: gridArea,
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId,
            ProcessType: processType);
    }

    private IReadOnlyCollection<IReadOnlyCollection<string>> CreateDataOne(
        string gridAreaCode,
        string balanceResponsibleId,
        string energySupplierId,
        string timeSeriesType)
    {
        var o = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: FirstCalculationResultId,
            time: FirstHour,
            gridArea: gridAreaCode,
            quantity: FirstQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.BalanceFixing);

        var f = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: SecondCalculationResultId,
            time: FirstHour,
            gridArea: gridAreaCode,
            quantity: FirstQuantityFirstCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: ThirdCalculationResultId,
            time: FirstHour,
            gridArea: gridAreaCode,
            quantity: FirstQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: ThirdCalculationResultId,
            time: FirstHour,
            gridArea: gridAreaCode,
            quantity: FirstQuantityThirdCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement);

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
            batchId: BatchId,
            calculationResultId: FirstCalculationResultId,
            time: SecondHour,
            gridArea: gridAreaCode,
            quantity: SecondQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.BalanceFixing);

        var f = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: SecondCalculationResultId,
            time: SecondHour,
            gridArea: gridAreaCode,
            quantity: SecondQuantityFirstCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: ThirdCalculationResultId,
            time: SecondHour,
            gridArea: gridAreaCode,
            quantity: SecondQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement);

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
            batchId: BatchId,
            calculationResultId: FirstCalculationResultId,
            time: ThirdHour,
            gridArea: gridAreaCode,
            quantity: ThirdQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.BalanceFixing);

        var s = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: SecondCalculationResultId,
            time: ThirdHour,
            gridArea: gridAreaCode,
            quantity: ThirdQuantitySecondCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: ThirdCalculationResultId,
            time: ThirdHour,
            gridArea: gridAreaCode,
            quantity: ThirdQuantityThirdCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement);

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
            batchId: BatchId,
            calculationResultId: FirstCalculationResultId,
            time: SecondDay,
            gridArea: gridAreaCode,
            quantity: FourthQuantity,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.BalanceFixing);

        var t = EnergyResultDeltaTableHelper.CreateRowValues(
            batchId: BatchId,
            calculationResultId: SecondCalculationResultId,
            time: SecondDay,
            gridArea: gridAreaCode,
            quantity: FourthQuantityThirdCorrection,
            aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea,
            balanceResponsibleId: balanceResponsibleId,
            energySupplierId: energySupplierId,
            timeSeriesType: timeSeriesType,
            batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement);

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
                    ProcessType = row.ElementAt(2),
                })
                .Select(grouping =>
                    EnergyResultDeltaTableHelper.CreateRowValues(
                        batchId: BatchId,
                        calculationResultId: ThirdCalculationResultId,
                        time: grouping.Key.Time.Replace("'", string.Empty),
                        gridArea: grouping.Key.GridArea.Replace("'", string.Empty),
                        quantity: grouping.Sum(row => decimal.Parse(row.ElementAt(7), CultureInfo.InvariantCulture))
                            .ToString(CultureInfo.InvariantCulture),
                        aggregationLevel: DeltaTableAggregationLevel.BalanceResponsibleAndGridArea,
                        balanceResponsibleId: grouping.Key.BalanceResponsibleId.Replace("'", string.Empty),
                        // energySupplierId: "NULL",
                        timeSeriesType: grouping.Key.TimeSeriesType.Replace("'", string.Empty),
                        batchProcessType: grouping.Key.ProcessType.Replace("'", string.Empty)))
                .ToList();

            var aggregatedByEnergyAndGrid = allThemRows.GroupBy(row => new
                {
                    Time = row.ElementAt(6),
                    GridArea = row.ElementAt(4),
                    TimeSeriesType = row.ElementAt(11),
                    // BalanceResponsibleId = row.ElementAt(8),
                    EnergySupplierId = row.ElementAt(5),
                    CalculationId = row.ElementAt(3),
                    ProcessType = row.ElementAt(2),
                })
                .Select(grouping =>
                    EnergyResultDeltaTableHelper.CreateRowValues(
                        batchId: BatchId,
                        calculationResultId: ThirdCalculationResultId,
                        time: grouping.Key.Time.Replace("'", string.Empty),
                        gridArea: grouping.Key.GridArea.Replace("'", string.Empty),
                        quantity: grouping.Sum(row => decimal.Parse(row.ElementAt(7), CultureInfo.InvariantCulture))
                            .ToString(CultureInfo.InvariantCulture),
                        aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndGridArea,
                        // balanceResponsibleId: grouping.Key.BalanceResponsibleId.Replace("'", string.Empty),
                        energySupplierId: grouping.Key.EnergySupplierId.Replace("'", string.Empty),
                        timeSeriesType: grouping.Key.TimeSeriesType.Replace("'", string.Empty),
                        batchProcessType: grouping.Key.ProcessType.Replace("'", string.Empty)))
                .ToList();

            var aggregatedByGrid = allThemRows.GroupBy(row => new
                {
                    Time = row.ElementAt(6),
                    GridArea = row.ElementAt(4),
                    TimeSeriesType = row.ElementAt(11),
                    // BalanceResponsibleId = row.ElementAt(8),
                    // EnergySupplierId = row.ElementAt(5),
                    CalculationId = row.ElementAt(3),
                    ProcessType = row.ElementAt(2),
                })
                .Select(grouping =>
                    EnergyResultDeltaTableHelper.CreateRowValues(
                        batchId: BatchId,
                        calculationResultId: ThirdCalculationResultId,
                        time: grouping.Key.Time.Replace("'", string.Empty),
                        gridArea: grouping.Key.GridArea.Replace("'", string.Empty),
                        quantity: grouping.Sum(row => decimal.Parse(row.ElementAt(7), CultureInfo.InvariantCulture))
                            .ToString(CultureInfo.InvariantCulture),
                        aggregationLevel: DeltaTableAggregationLevel.GridArea,
                        // balanceResponsibleId: grouping.Key.BalanceResponsibleId.Replace("'", string.Empty),
                        // energySupplierId: grouping.Key.EnergySupplierId.Replace("'", string.Empty),
                        timeSeriesType: grouping.Key.TimeSeriesType.Replace("'", string.Empty),
                        batchProcessType: grouping.Key.ProcessType.Replace("'", string.Empty)))
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
