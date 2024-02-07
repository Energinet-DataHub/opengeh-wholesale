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

public class AggregatedTimeSeriesQueriesTests : TestBase<AggregatedTimeSeriesQueries>, IClassFixture<DatabricksSqlStatementApiFixture>
{
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
    private static readonly Guid _firstCalculationId = Guid.Parse("019703e7-98ee-45c1-b343-0cbf185a47d9");
    private static readonly Guid _secondCalculationId = Guid.Parse("119703e7-98ee-45c1-b343-0cbf185a47d9");
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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter);

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
        var startOfPeriodFilter = Instant.FromUtc(2020, 1, 1, 1, 1);
        var endOfPeriodFilter = Instant.FromUtc(2021, 1, 2, 1, 1);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: GridAreaCodeA);

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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            balanceResponsibleId: balanceResponsibleIdFilter,
            calculationType: CalculationType.BalanceFixing);

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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
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
    public async Task GetAsync_WhenRequestFromEnergySupplierTotalProductionWithoutGridAreaFilter_ReturnsOneResultPerGridArea()
    {
        // Arrange
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            timeSeriesType: timeSeriesTypeFilter,
            energySupplierId: energySupplierIdFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        var resultsPerGridArea = actual.GroupBy(x => x.GridArea);
        using var assertionScope = new AssertionScope();
        foreach (var resultsInGridArea in resultsPerGridArea)
        {
            resultsInGridArea.Should().ContainSingle($"There should be only one result for grid area: {resultsInGridArea.Key}.");
        }
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionFirstCorrectionSettlement_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var calculationTypeFilter = CalculationType.FirstCorrectionSettlement;
        await AddCreatedRowsInArbitraryOrderAsync(addFirstCorrection: true);

        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            calculationType: calculationTypeFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.CalculationType.Should().Be(CalculationType.FirstCorrectionSettlement);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantityFirstCorrection, SecondQuantityFirstCorrection);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionSecondCorrectionSettlement_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var calculationTypeFilter = CalculationType.SecondCorrectionSettlement;
        await AddCreatedRowsInArbitraryOrderAsync(addSecondCorrection: true);

        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            calculationType: calculationTypeFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.CalculationType.Should().Be(CalculationType.SecondCorrectionSettlement);
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
        var calculationTypeFilter = CalculationType.ThirdCorrectionSettlement;
        await AddCreatedRowsInArbitraryOrderAsync(addThirdCorrection: true);

        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            calculationType: calculationTypeFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        var aggregatedTimeSeries = actual.First();
        aggregatedTimeSeries!.GridArea.Should().Be(gridAreaFilter);
        aggregatedTimeSeries.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
        aggregatedTimeSeries.TimeSeriesType.Should().Be(timeSeriesTypeFilter);
        aggregatedTimeSeries.TimeSeriesPoints
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantityThirdCorrection, SecondQuantityThirdCorrection, FourthQuantityThirdCorrection);
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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter);

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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: gridAreaFilter,
            timeSeriesType: timeSeriesTypeFilter,
            calculationType: CalculationType.BalanceFixing);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromEnergySupplierPerBalanceResponsibleTotalProduction_ReturnsTwoResults()
    {
        // Arrange
        var balanceResponsibleIdFilter = "1234567891234";
        var energySupplierIdFilter = "4321987654321";
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            timeSeriesType: timeSeriesTypeFilter,
            energySupplierId: energySupplierIdFilter,
            balanceResponsibleId: balanceResponsibleIdFilter,
            calculationType: CalculationType.BalanceFixing);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual.Select(result => result.GridArea).Should().Equal(GridAreaCodeB, GridAreaCodeC);
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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            timeSeriesTypeFilter,
            gridAreaFilter);

        // Act
        var actual = await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.First()!.CalculationType.Should().Be(CalculationType.ThirdCorrectionSettlement);
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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            timeSeriesTypeFilter,
            gridAreaFilter);

        // Act
        var actual = await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.First()!.CalculationType.Should().Be(CalculationType.SecondCorrectionSettlement);
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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            timeSeriesTypeFilter,
            gridAreaFilter);

        // Act
        var actual = await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.First()!.CalculationType.Should().Be(CalculationType.FirstCorrectionSettlement);
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
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            timeSeriesTypeFilter,
            gridAreaFilter);

        // Act
        var actual = await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        actual.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetLatestCorrectionAsync_WhenCalculationTypeIsDefined_ThrowsException()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var timeSeriesTypeFilter = TimeSeriesType.Production;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            timeSeriesTypeFilter,
            gridAreaFilter,
            calculationType: CalculationType.BalanceFixing);

        // Act
        var act = async () => await Sut.GetLatestCorrectionForGridAreaAsync(parameters).ToListAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentException>(
            "The calculation type will be overwritten when fetching the latest correction.",
            parameters.CalculationType);
    }

    [Fact]
    public async Task GetAsync_WhenRequestFromGridOperatorTotalProductionWithEmptyGridArea_ReturnsResult()
    {
        // Arrange
        var emptyGridAreaFilter = string.Empty;
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(startOfPeriodFilter, endOfPeriodFilter),
                    _firstCalculationId,
                    1),
            },
            gridArea: emptyGridAreaFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAsync_WithoutCalculations_ReturnsEmptyResult()
    {
        // Arrange
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: Array.Empty<CalculationForPeriod>());

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_WhenRequestedPeriodWithMultipleCalculations_ReturnsResult()
    {
        // Arrange
        var gridAreaFilter = GridAreaCodeC;
        var periodStartForFirstCalculation = Instant.FromUtc(2022, 1, 1, 0, 0);
        var periodEndForFirstCalculation = Instant.FromUtc(2022, 1, 2, 0, 0);
        var periodStartForSecondCalculation = Instant.FromUtc(2022, 1, 3, 0, 0);
        var periodEndForSecondCalculation = Instant.FromUtc(2022, 1, 4, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(periodStartForFirstCalculation, periodEndForFirstCalculation),
                    _firstCalculationId,
                    1),
                new CalculationForPeriod(
                    new Period(periodStartForSecondCalculation, periodEndForSecondCalculation),
                    _secondCalculationId,
                    2),
            },
            gridArea: gridAreaFilter);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);

        actual.Should().ContainSingle(c => c.Version == 1)
            .Which.TimeSeriesPoints.Should().AllSatisfy(point =>
                point.Time.Should().BeOnOrAfter(periodStartForFirstCalculation.ToDateTimeOffset()).And
                    .BeBefore(periodEndForFirstCalculation.ToDateTimeOffset()));
        actual.Should().ContainSingle(c => c.Version == 2)
            .Which.TimeSeriesPoints.Should().AllSatisfy(point =>
                point.Time.Should().BeOnOrAfter(periodStartForSecondCalculation.ToDateTimeOffset()).And
                    .BeBefore(periodEndForSecondCalculation.ToDateTimeOffset()));
    }

    [Fact]
    public async Task GetAsync_WhenNoGridAreaAndMultipleCalculations_ReturnsAResultPerCalculationPerGridArea()
    {
        // Arrange
        var periodStartForFirstCalculation = Instant.FromUtc(2022, 1, 1, 0, 0);
        var periodEndForFirstCalculation = Instant.FromUtc(2022, 1, 2, 0, 0);
        var periodStartForSecondCalculation = Instant.FromUtc(2022, 1, 3, 0, 0);
        var periodEndForSecondCalculation = Instant.FromUtc(2022, 1, 4, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync();
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(periodStartForFirstCalculation, periodEndForFirstCalculation),
                    _firstCalculationId,
                    1),
                new CalculationForPeriod(
                    new Period(periodStartForSecondCalculation, periodEndForSecondCalculation),
                    _secondCalculationId,
                    2),
            });

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(3);
        actual.Should().ContainSingle(x => x.GridArea == GridAreaCodeC && x.Version == 1);
        actual.Should().ContainSingle(x => x.GridArea == GridAreaCodeC && x.Version == 2);
        actual.Should().ContainSingle(x => x.GridArea == GridAreaCodeA && x.Version == 2);
    }

    [Fact]
    public async Task GetAsync_WhenNoResultForRequestedCalculation_ReturnsEmptyResult()
    {
        // Arrange
        var calculationWithoutAnyResults = Guid.NewGuid();
        var periodStart = Instant.FromUtc(2024, 1, 1, 23, 0, 0);
        var periodEnd = Instant.FromUtc(2024, 1, 3, 23, 0, 0);
        await AddCreatedRowsInArbitraryOrderAsync(addSecondCorrection: true);
        var parameters = CreateQueryParameters(
            latestCalculationForPeriods: new[]
            {
                new CalculationForPeriod(
                    new Period(periodStart, periodEnd),
                    calculationWithoutAnyResults,
                    1),
            },
            gridArea: GridAreaCodeC);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    private AggregatedTimeSeriesQueryParameters CreateQueryParameters(
        IReadOnlyCollection<CalculationForPeriod> latestCalculationForPeriods,
        TimeSeriesType? timeSeriesType = null,
        string? gridArea = null,
        string? energySupplierId = null,
        string? balanceResponsibleId = null,
        CalculationType? calculationType = null)
    {
        return new AggregatedTimeSeriesQueryParameters(
            TimeSeriesType: timeSeriesType ?? TimeSeriesType.Production,
            GridArea: gridArea,
            EnergySupplierId: energySupplierId,
            BalanceResponsibleId: balanceResponsibleId,
            CalculationType: calculationType,
            LatestCalculationForPeriod: latestCalculationForPeriods);
    }

    private async Task AddCreatedRowsInArbitraryOrderAsync(
        bool addFirstCorrection = false,
        bool addSecondCorrection = false,
        bool addThirdCorrection = false)
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
        var row1 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantity);
        var row2 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _secondCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantity);

        var row3 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: secondCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: ThirdQuantity, calculationExecutionTimeStart: "2022-03-12T03:00:00.000Z");
        var row4 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: secondCalculationResultId, time: thirdHour, gridArea: GridAreaCodeC, quantity: FourthQuantity, calculationExecutionTimeStart: "2022-03-12T03:00:00.000Z");

        var row5 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndGridArea, energySupplierId: energySupplier);
        var row6 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: secondCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantity, aggregationLevel: DeltaTableAggregationLevel.BalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId);

        var row7 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCodeC, quantity: ThirdQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);
        var row8 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: thirdHour, gridArea: GridAreaCodeB, quantity: FourthQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);

        var row9 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeA, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea);
        var row10 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _secondCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: thirdHour, gridArea: GridAreaCodeB, quantity: FourthQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea, balanceResponsibleId: balanceResponsibleId, energySupplierId: energySupplier);

        var row11 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: firstHour, gridArea: GridAreaCodeB, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndGridArea, energySupplierId: energySupplier);
        var row12 = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: secondHour, gridArea: GridAreaCodeB, quantity: FirstQuantity, aggregationLevel: DeltaTableAggregationLevel.EnergySupplierAndGridArea, energySupplierId: energySupplier);

        var row1FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, calculationType: DeltaTableCalculationType.FirstCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantityFirstCorrection);
        var row2FirstCorrection = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, calculationType: DeltaTableCalculationType.FirstCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantityFirstCorrection);

        var row1SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: secondCalculationResultId, calculationType: DeltaTableCalculationType.SecondCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantitySecondCorrection);
        var row2SecondCorrection = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: secondCalculationResultId, calculationType: DeltaTableCalculationType.SecondCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantitySecondCorrection);

        var row1ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: thirdCalculationResultId, calculationType: DeltaTableCalculationType.ThirdCorrectionSettlement, time: firstHour, gridArea: GridAreaCodeC, quantity: FirstQuantityThirdCorrection);
        var row2ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: thirdCalculationResultId, calculationType: DeltaTableCalculationType.ThirdCorrectionSettlement, time: secondHour, gridArea: GridAreaCodeC, quantity: SecondQuantityThirdCorrection);
        var row4ThirdCorrection = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: thirdCalculationResultId, calculationType: DeltaTableCalculationType.ThirdCorrectionSettlement, time: thirdHour, gridArea: GridAreaCodeC, quantity: FourthQuantityThirdCorrection);

        var row1SecondDay = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: secondDay, gridArea: GridAreaCodeC, quantity: FirstQuantity);
        var row1ThirdDay = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _firstCalculationId.ToString(), calculationResultId: firstCalculationResultId, time: thirdDay, gridArea: GridAreaCodeC, quantity: SecondQuantity);

        var row1SecondBatchThirdDay = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _secondCalculationId.ToString(), calculationResultId: secondCalculationResultId, time: thirdDay, gridArea: GridAreaCodeC, quantity: SecondQuantity);
        var row2SecondBatchThirdDay = EnergyResultDeltaTableHelper.CreateRowValues(calculationId: _secondCalculationId.ToString(), calculationResultId: secondCalculationResultId, time: thirdDay, gridArea: GridAreaCodeA, quantity: SecondQuantity);

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
            row11,
            row12,
            row1SecondDay,
            row1ThirdDay,
            row1SecondBatchThirdDay,
            row2SecondBatchThirdDay,
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
