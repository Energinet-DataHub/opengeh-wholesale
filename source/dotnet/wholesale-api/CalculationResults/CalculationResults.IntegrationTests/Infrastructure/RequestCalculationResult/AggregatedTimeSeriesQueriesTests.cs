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

public class AggregatedTimeSeriesQueriesTests : TestBase<AggregatedTimeSeriesQueries>, IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string BatchId = "019703e7-98ee-45c1-b343-0cbf185a47d9";
    private const string FirstQuantity = "1.111";
    private const string FirstQuantityFirstCorrection = "1.222";
    private const string FirstQuantitySecondCorrection = "1.333";
    private const string FirstQuantityThirdCorrection = "1.444";
    private const string SecondQuantity = "2.222";
    private const string SecondQuantityFirstCorrection = "2.333";
    private const string SecondQuantitySecondCorrection = "2.444";
    private const string SecondQuantityThirdCorrection = "2.555";
    private const string ThirdQuantity = "3.333";
    private const string FourthQuantity = "4.444";
    private const string FourthQuantityThirdCorrection = "4.555";
    private const string GridAreaCodeA = "101";
    private const string GridAreaCodeB = "201";
    private const string GridAreaCodeC = "301";
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public AggregatedTimeSeriesQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
        Fixture.Inject(_fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(_fixture.GetDatabricksExecutor());
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProduction_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity, ThirdQuantity, FourthQuantity);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionInWrongPeriod_ReturnsNoResults()
    {
        // Arrange
        await AddCreatedRowsInArbitraryOrderAsync();
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
    public async Task GetAsync_WhenRequestFromEnergySupplierTotalProduction_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierIdFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity);
        aggregatedTimeSeries.TimeSeriesPoints.Select(p => p.Time)
            .ToArray()
            .Should()
            .AllSatisfy(p =>
            {
                p.Should().BeOnOrAfter(startOfPeriodFilter.ToDateTimeOffset());
                p.Should().BeBefore(endOfPeriodFilter.ToDateTimeOffset());
            });
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromEnergySupplierTotalProductionBadId_ReturnsNoResults()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var energySupplierId = "badId";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierId);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromBalanceResponsibleTotalProduction_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var balanceResponsibleIdFilter = "1234567891234";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            balanceResponsibleId: balanceResponsibleIdFilter,
            processType: ProcessType.BalanceFixing);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(SecondQuantity);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromEnergySupplierPerBalanceResponsibleTotalProduction_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var balanceResponsibleIdFilter = "1234567891234";
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierIdFilter,
            balanceResponsibleId: balanceResponsibleIdFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(ThirdQuantity);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionFirstCorrectionSettlement_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var processTypeFilter = ProcessType.FirstCorrectionSettlement;
        await AddCreatedRowsInArbitraryOrderAsync(addFirstCorrection: true);

        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            processType: processTypeFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.ProcessType.Should().Be(ProcessType.FirstCorrectionSettlement);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantityFirstCorrection, SecondQuantityFirstCorrection)
            ;
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionSecondCorrectionSettlement_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var processTypeFilter = ProcessType.SecondCorrectionSettlement;
        await AddCreatedRowsInArbitraryOrderAsync(addSecondCorrection: true);

        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            processType: processTypeFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.ProcessType.Should().Be(ProcessType.SecondCorrectionSettlement);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantitySecondCorrection, SecondQuantitySecondCorrection)
            ;
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionThirdCorrectionSettlement_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var processTypeFilter = ProcessType.ThirdCorrectionSettlement;
        await AddCreatedRowsInArbitraryOrderAsync(addThirdCorrection: true);

        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            processType: processTypeFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.ProcessType.Should().Be(ProcessType.ThirdCorrectionSettlement);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantityThirdCorrection, SecondQuantityThirdCorrection, FourthQuantityThirdCorrection)
            ;
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorForOneDay_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 3, 0, 0);

        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity)
            ;
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorStartAndEndDataAreEqual_ReturnsNoResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        await AddCreatedRowsInArbitraryOrderAsync();

        var parameters = CreateQueryParameters(
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            processType: ProcessType.BalanceFixing);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetAsync_RequestFromEnergySupplierPerBalanceResponsibleTotalProduction_ReturnsTwoResults()
    {
        // Arrange
        var balanceResponsibleIdFilter = "1234567891234";
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var parameters = CreateQueryParameters(
            timeSeriesType: timeSeriesTypeFilter,
            startOfPeriod: startOfPeriodFilter,
            endOfPeriod: endOfPeriodFilter,
            energySupplierId: energySupplierIdFilter,
            balanceResponsibleId: balanceResponsibleIdFilter,
            processType: ProcessType.BalanceFixing);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual.Select(result => result.GridArea).Should().Equal(GridAreaCodeC, GridAreaCodeB);
        actual.Should().AllSatisfy(aggregatedTimeSeries => aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter));
    }

    [Fact]
    public async Task GetLatestCorrectionAsync_WhenLatestCorrectionSettlementIsThirdCorrection_ReturnsThirdCorrection()
    {
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync(addThirdCorrection: true);
        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);

        // Act
        var actual = await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.First()!.ProcessType.Should().Be(ProcessType.ThirdCorrectionSettlement);
    }

    [Fact]
    public async Task GetLatestCorrectionAsync_WhenLatestCorrectionSettlementIsSecondCorrection_ReturnsSecondCorrection()
    {
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync(addSecondCorrection: true);
        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);

        // Act
        var actual = await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.First()!.ProcessType.Should().Be(ProcessType.SecondCorrectionSettlement);
    }

    [Fact]
    public async Task GetLatestCorrectionAsync_WhenLatestCorrectionSettlementIsFirstCorrection_ReturnsFirstCorrection()
    {
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync(addFirstCorrection: true);
        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);

        // Act
        var actual = await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.First()!.ProcessType.Should().Be(ProcessType.FirstCorrectionSettlement);
    }

    [Fact]
    public async Task GetLatestCorrectionAsync_WhenNoCorrectionsExists_ReturnsNoResult()
    {
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync(addFirstCorrection: false, addSecondCorrection: false, addThirdCorrection: false);
        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter);

        // Act
        var actual = await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetLatestCorrectionAsync_WhenProcessTypeIsDefined_ThrowsException()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var parameters = CreateQueryParameters(
            timeSeriesTypeFilter,
            startOfPeriodFilter,
            endOfPeriodFilter,
            gridAreaFilter,
            processType: ProcessType.BalanceFixing);

        // Act
        var act = async () => await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "The process type will be overwritten when fetching the latest correction.",
            parameters.ProcessType);
    }

    private AggregatedTimeSeriesQueryParameters CreateQueryParameters(
        TimeSeriesType? timeSeriesType = null,
        Instant? startOfPeriod = null,
        Instant? endOfPeriod = null,
        string? gridArea = null,
        string? energySupplierId = null,
        string? balanceResponsibleId = null,
        ProcessType? processType = null)
    {
        return new AggregatedTimeSeriesQueryParameters(
            TimeSeriesType: timeSeriesType ?? TimeSeriesType.Production,
            StartOfPeriod: startOfPeriod ?? Instant.FromUtc(2022, 1, 1, 0, 0),
            EndOfPeriod: endOfPeriod ?? Instant.FromUtc(2022, 1, 2, 0, 0),
            GridArea: gridArea,
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId,
            ProcessType: processType);
    }

    private async Task AddCreatedRowsInArbitraryOrderAsync(bool addFirstCorrection = false, bool addSecondCorrection = false, bool addThirdCorrection = false)
    {
        const string firstCalculationResultId = "aaaaaaaa-386f-49eb-8b56-63fae62e4fc7";
        const string secondCalculationResultId = "bbbbbbbb-b58b-4190-a873-eded0ed50c20";
        const string thirdCalculationResultId = "cccccccc-386f-49eb-8b56-63fae62e4fc7";

        const string firstHour = "2022-01-01T01:00:00.000Z";
        const string secondHour = "2022-01-01T02:00:00.000Z";
        const string thirdHour = "2022-01-01T03:00:00.000Z";

        const string secondDay = "2022-01-02T00:00:00.000Z";
        const string thirdDay = "2022-01-03T00:00:00.000Z";

        const string energySupplier = "4321987654321";
        const string balanceResponsibleId = "1234567891234";
        var row1 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantity);
        var row2 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantity);

        var row3 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: ThirdQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");
        var row4 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: thirdHour, gridArea: GridAreaCodeC, quantity: FourthQuantity, batchExecutionTimeStart: "2022-03-12T03:00:00.000Z");

        var row5 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndGridArea, energySupplierId: energySupplier);
        var row6 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantity, aggregationLevel: DeltaTableAggregationLevel.BalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId);

        var row7 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: ThirdQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);
        var row8 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: thirdHour, gridArea: GridAreaCodeB, quantity: FourthQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);

        var row9 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeA, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea);
        var row10 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: thirdHour, gridArea: GridAreaCodeB, quantity: FourthQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);

        var row1FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantityFirstCorrection);
        var row2FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, batchProcessType: DeltaTableProcessType.FirstCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantityFirstCorrection);

        var row1SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantitySecondCorrection);
        var row2SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, batchProcessType: DeltaTableProcessType.SecondCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantitySecondCorrection);

        var row1ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantityThirdCorrection);
        var row2ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantityThirdCorrection);
        var row4ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, batchProcessType: DeltaTableProcessType.ThirdCorrectionSettlement, time: thirdHour, gridArea: GridAreaCodeC, quantity: FourthQuantityThirdCorrection);

        var row1SecondDay = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondDay, gridArea: GridAreaCodeC, quantity: FirstQuantity);
        var row1ThirdDay = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: thirdDay, gridArea: GridAreaCodeC, quantity: SecondQuantity);

        var rows = new List<IReadOnlyCollection<string>>
        {
            row1,
            row2,
            row3,
            row4,
            row5,
            row6,
            row7,
            row8,
            row9,
            row10,
            row1SecondDay,
            row1ThirdDay,
        };

        if (addFirstCorrection)
        {
            rows.AddRange(new[]
            {
                row1FirstCorrection,
                row2FirstCorrection,
            });
        }

        if (addSecondCorrection)
        {
            rows.AddRange(new[]
            {
                row1SecondCorrection,
                row2SecondCorrection,
            });
        }

        if (addThirdCorrection)
        {
            rows.AddRange(new[]
            {
                row1ThirdCorrection,
                row2ThirdCorrection,
                row4ThirdCorrection,
            });
        }

        rows = rows.OrderBy(_ => Guid.NewGuid()).ToList();

        await _fixture.DatabricksSchemaManager.EmptyAsync(_fixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME);
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(_fixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME, rows);
    }
}
