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

using AutoFixture;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResults;

public class WholesaleResultQueriesTests : TestBase<WholesaleResultQueries>
{
    private readonly TableChunk _tableChunk;
    private readonly string _row0CalculationId;
    private readonly string _calculationResultId0;

    private readonly Mock<ICalculationsClient> _calculationsClientMock;
    private readonly Mock<DatabricksSqlWarehouseQueryExecutor> _databricksSqlWarehouseQueryExecutorMock;

    public WholesaleResultQueriesTests()
    {
        // The two rows belongs to different calculation results as they have different calculation result ids
        _row0CalculationId = "b78787d5-b544-44ac-87c2-7720aab86ed1";
        _calculationResultId0 = "9913f3bb-1208-400b-9cbe-50300e386d26";
        const string calculationResultId1 = "8c2bb7c6-d8e5-462c-9bce-8537f93ef8e7";
        var row0 = new[] { _row0CalculationId, _calculationResultId0, DeltaTableCalculationType.WholesaleFixing, "200", "1234567890000", "amount_per_charge", "consumption", "flex", "tariff", "somChargeCode", "someOwnerId", "PT1H", "true", "kWh", "2022-05-16T22:00:00.000Z", "1.111", "[\"measured\"]", "2.123456", "3.123456", null };
        var row1 = new[] { "b78787d5-b544-44ac-87c2-7720aab86ed2", calculationResultId1, DeltaTableCalculationType.WholesaleFixing, "200", "1234567890000", "amount_per_charge", "consumption", "flex", "tariff", "somChargeCode", "someOwnerId", "PT1H", "true", "kWh", "2022-05-16T22:00:00.000Z", "1.111", "[\"measured\"]", "2.123456", "3.123456", null };
        var rows = new List<string?[]> { row0, row1, };

        // Using the columns from the WholesaleResultQueries class to ensure that the test is not broken if the columns are changed
        _tableChunk = new TableChunk(WholesaleResultQueryStatement.SqlColumnNames, rows);

        // Mocks Setup - This is another way to setup mocks used in tests. The reason for this are:
        // 1. Because DatabricksSqlWarehouseQueryExecutor doesn't implement an interface and the constructor is protected
        // AutoFixture combined with inline is unable to create an instance of it.
        // 2. The many mock parameters are avoided in tests
        _calculationsClientMock = Fixture.Freeze<Mock<ICalculationsClient>>();
        _databricksSqlWarehouseQueryExecutorMock = Fixture.Freeze<Mock<DatabricksSqlWarehouseQueryExecutor>>();
        Fixture.Inject(_calculationsClientMock.Object);
        Fixture.Inject(_databricksSqlWarehouseQueryExecutorMock.Object);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenNoRows_ReturnsNoResults(CalculationDto calculation)
    {
        // Arrange
        _calculationsClientMock
            .Setup(client => client.GetAsync(calculation.CalculationId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>(), default))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 0));

        // Act
        var actual = await Sut.GetAsync(calculation.CalculationId).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenOneRow_ReturnsSingleResultWithOneTimeSeriesPoint(CalculationDto calculation)
    {
        // Arrange
        _calculationsClientMock
            .Setup(client => client.GetAsync(calculation.CalculationId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock.Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>(), default))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 1));

        // Act
        var actual = await Sut.GetAsync(calculation.CalculationId).ToListAsync();

        // Assert
        actual.Single().TimeSeriesPoints.Count.Should().Be(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenCalculationHasOneResult_ReturnsResultRowWithExpectedValues(CalculationDto calculation)
    {
        // Arrange
        calculation = calculation with { CalculationId = Guid.Parse(_row0CalculationId), CalculationType = CalculationType.WholesaleFixing };
        _calculationsClientMock
            .Setup(client => client.GetAsync(calculation.CalculationId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>(), default))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 1));
        var expectedStartDate = SqlResultValueConverters.ToDateTimeOffset(_tableChunk[0, WholesaleResultColumnNames.Time])!.Value;
        // End date is the first point in the next periode
        var expectedEndDate = SqlResultValueConverters.ToDateTimeOffset(_tableChunk[0, WholesaleResultColumnNames.Time])!.Value.AddHours(1);

        // Act
        var actual = await Sut.GetAsync(calculation.CalculationId).SingleAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Id.Should().Be(_calculationResultId0);
        actual.CalculationId.Should().Be(Guid.Parse(_row0CalculationId));
        actual.GridArea.Should().Be(_tableChunk[0, WholesaleResultColumnNames.GridArea]);
        actual.EnergySupplierId.Should().Be(_tableChunk[0, WholesaleResultColumnNames.EnergySupplierId]);
        actual.CalculationId.Should().Be(_tableChunk[0, WholesaleResultColumnNames.CalculationId]);
        actual.CalculationType.Should().Be(calculation.CalculationType);
        actual.PeriodStart.Should().Be(expectedStartDate.ToInstant());
        actual.PeriodEnd.Should().Be(expectedEndDate.ToInstant());
        var actualPoint = actual.TimeSeriesPoints.Single();
        actualPoint.Time.Should().Be(new DateTimeOffset(2022, 5, 16, 22, 0, 0, TimeSpan.Zero));
        actualPoint.Quantity.Should().Be(1.111m);
        actualPoint.Qualities.Should().Contain(QuantityQuality.Measured);
        actualPoint.Qualities.Should().HaveCount(1);
        actualPoint.Price.Should().Be(2.123456m);
        actualPoint.Amount.Should().Be(3.123456m);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRowsBelongsToDifferentResults_ReturnsMultipleResults(CalculationDto calculation)
    {
        // Arrange
        _calculationsClientMock
            .Setup(client => client.GetAsync(calculation.CalculationId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>(), default))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 2));

        // Act
        var actual = await Sut.GetAsync(calculation.CalculationId).ToListAsync();

        // Assert
        actual.Count.Should().Be(2);
    }

    [Theory]
    [InlineAutoMoqData("someId", "otherId", true)]
    [InlineAutoMoqData("someId", "someId", false)]
    public void BelongsToDifferentResults_WhenHavingTwoResultRows_ReturnsExpectedValue(
        string calculationResultIdA,
        string calculationResultIdB,
        bool expected)
    {
        // Arrange
        var listA = new DatabricksSqlRow(new Dictionary<string, object?>
        {
            { WholesaleResultColumnNames.CalculationId, "calculationId" },
            { WholesaleResultColumnNames.CalculationResultId, calculationResultIdA },
        });
        var listB = new DatabricksSqlRow(new Dictionary<string, object?>
        {
            { WholesaleResultColumnNames.CalculationId, "calculationId" },
            { WholesaleResultColumnNames.CalculationResultId, calculationResultIdB },
        });

        // Act
        var actual = WholesaleResultQueries.BelongsToDifferentResults(listA, listB);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenOnlyResultFromHalfTheRequestedPeriod_ReturnedPointsAreInCorrectPeriod(CalculationDto calculation)
    {
        // Business case example:
        // When a new Energy Supplier is being made responsible for a metering point in the middle of the month
        // and they do not already have a metering point in the grid area from the beginning of the month.
        // The result is that the Energy Supplier will only have results for the last half of the month.

        // Arrange
        var calculationPeriodStart = Instant.FromUtc(2022, 1, 1, 0, 0);
        var calculationPeriodEnd = Instant.FromUtc(2022, 2, 1, 0, 0);
        calculation = calculation with
        {
            CalculationId = Guid.Parse(_row0CalculationId),
            CalculationType = CalculationType.WholesaleFixing,
            PeriodStart = calculationPeriodStart.ToDateTimeOffset(),
            PeriodEnd = calculationPeriodEnd.ToDateTimeOffset(),
        };

        var calculationResults = new List<string?[]>();
        var middleOfPeriod = calculationPeriodStart.Plus(Duration.FromDays(15));
        var currentTime = middleOfPeriod;
        while (currentTime < calculationPeriodEnd)
        {
            var row = new[]
            {
                calculation.CalculationId.ToString(), _calculationResultId0, DeltaTableCalculationType.WholesaleFixing, "200",
                "1234567890000", "amount_per_charge", "consumption", "flex", "tariff", "somChargeCode",
                "someOwnerId", "PT1H", "true", "kWh", currentTime.ToString(),
                "1.111", "[\"measured\"]", "2.123456", "3.123456", null,
            };

            calculationResults.Add(row);
            currentTime = currentTime.Plus(Duration.FromHours(1));
        }

        _calculationsClientMock
            .Setup(client => client.GetAsync(calculation.CalculationId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>(), default))
            .Returns(DatabricksTestHelper.GetRowsAsync(new TableChunk(WholesaleResultQueryStatement.SqlColumnNames, calculationResults), calculationResults.Count));

        // Act
        var actual = await Sut.GetAsync(calculation.CalculationId).SingleAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Id.Should().Be(_calculationResultId0);
        actual.CalculationId.Should().Be(calculation.CalculationId);
        actual.GridArea.Should().Be(_tableChunk[0, WholesaleResultColumnNames.GridArea]);
        actual.EnergySupplierId.Should().Be(_tableChunk[0, WholesaleResultColumnNames.EnergySupplierId]);
        actual.CalculationId.Should().Be(_tableChunk[0, WholesaleResultColumnNames.CalculationId]);
        actual.CalculationType.Should().Be(calculation.CalculationType);
        actual.PeriodStart.Should().Be(middleOfPeriod);
        actual.PeriodEnd.Should().Be(calculation.PeriodEnd.ToInstant());
        actual.TimeSeriesPoints.Should().HaveCount(calculationResults.Count);
        var actualPoint = actual.TimeSeriesPoints.First();
        actualPoint.Quantity.Should().Be(1.111m);
        actualPoint.Qualities.Should().Contain(QuantityQuality.Measured);
        actualPoint.Qualities.Should().HaveCount(1);
        actualPoint.Price.Should().Be(2.123456m);
        actualPoint.Amount.Should().Be(3.123456m);
    }
}
