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
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResults;

public class WholesaleResultQueriesTests : TestBase<WholesaleResultQueries>
{
    private readonly TableChunk _tableChunk;
    private readonly string _row0CalculationId;
    private readonly string _calculationResultId0;

    private readonly Mock<ICalculationsClient> _batchesClientMock;
    private readonly Mock<DatabricksSqlWarehouseQueryExecutor> _databricksSqlWarehouseQueryExecutorMock;

    public WholesaleResultQueriesTests()
    {
        // The two rows belongs to different calculation results as they have different calculation result ids
        _row0CalculationId = "b78787d5-b544-44ac-87c2-7720aab86ed1";
        _calculationResultId0 = "9913f3bb-1208-400b-9cbe-50300e386d26";
        const string calculationResultId1 = "8c2bb7c6-d8e5-462c-9bce-8537f93ef8e7";
        var row0 = new[] { _row0CalculationId, _calculationResultId0, DeltaTableProcessType.WholesaleFixing, "200", "1234567890000", "amount_per_charge", "consumption", "flex", "tariff", "somChargeCode", "someOwnerId", "PT1H", "true", "kWh", "2022-05-16T22:00:00.000Z", "1.111", "[\"measured\"]", "2.123456", "3.123456" };
        var row1 = new[] { "b78787d5-b544-44ac-87c2-7720aab86ed2", calculationResultId1, DeltaTableProcessType.WholesaleFixing, "200", "1234567890000", "amount_per_charge", "consumption", "flex", "tariff", "somChargeCode", "someOwnerId", "PT1H", "true", "kWh", "2022-05-16T22:00:00.000Z", "1.111", "[\"measured\"]", "2.123456", "3.123456" };
        var rows = new List<string[]> { row0, row1, };

        // Using the columns from the WholesaleResultQueries class to ensure that the test is not broken if the columns are changed
        _tableChunk = new TableChunk(WholesaleResultQueryStatement.SqlColumnNames, rows);

        // Mocks Setup - This is another way to setup mocks used in tests. The reason for this are:
        // 1. Because DatabricksSqlWarehouseQueryExecutor doesn't implement an interface and the constructor is protected
        // AutoFixture combined with inline is unable to create an instance of it.
        // 2. The many mock parameters are avoided in tests
        _batchesClientMock = Fixture.Freeze<Mock<ICalculationsClient>>();
        _databricksSqlWarehouseQueryExecutorMock = Fixture.Freeze<Mock<DatabricksSqlWarehouseQueryExecutor>>();
        Fixture.Inject(_batchesClientMock.Object);
        Fixture.Inject(_databricksSqlWarehouseQueryExecutorMock.Object);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenNoRows_ReturnsNoResults(BatchDto batch)
    {
        // Arrange
        _batchesClientMock
            .Setup(client => client.GetAsync(batch.BatchId))
            .ReturnsAsync(batch);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>()))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 0));

        // Act
        var actual = await Sut.GetAsync(batch.BatchId).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenOneRow_ReturnsSingleResultWithOneTimeSeriesPoint(BatchDto batch)
    {
        // Arrange
        _batchesClientMock
            .Setup(client => client.GetAsync(batch.BatchId))
            .ReturnsAsync(batch);
        _databricksSqlWarehouseQueryExecutorMock.Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>()))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 1));

        // Act
        var actual = await Sut.GetAsync(batch.BatchId).ToListAsync();

        // Assert
        actual.Single().TimeSeriesPoints.Count.Should().Be(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenCalculationHasOneResult_ReturnsResultRowWithExpectedValues(BatchDto batch)
    {
        // Arrange
        batch = batch with { BatchId = Guid.Parse(_row0CalculationId), ProcessType = ProcessType.WholesaleFixing };
        _batchesClientMock
            .Setup(client => client.GetAsync(batch.BatchId))
            .ReturnsAsync(batch);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>()))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 1));

        // Act
        var actual = await Sut.GetAsync(batch.BatchId).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Single().Id.Should().Be(_calculationResultId0);
        actual.Single().CalculationId.Should().Be(Guid.Parse(_row0CalculationId));
        actual.Single().GridArea.Should().Be(_tableChunk[0, WholesaleResultColumnNames.GridArea]);
        actual.Single().EnergySupplierId.Should().Be(_tableChunk[0, WholesaleResultColumnNames.EnergySupplierId]);
        actual.Single().CalculationId.Should().Be(_tableChunk[0, WholesaleResultColumnNames.CalculationId]);
        actual.Single().CalculationType.Should().Be(batch.ProcessType);
        actual.Single().PeriodStart.Should().Be(batch.PeriodStart.ToInstant());
        actual.Single().PeriodEnd.Should().Be(batch.PeriodEnd.ToInstant());
        var actualPoint = actual.Single().TimeSeriesPoints.Single();
        actualPoint.Time.Should().Be(new DateTimeOffset(2022, 5, 16, 22, 0, 0, TimeSpan.Zero));
        actualPoint.Quantity.Should().Be(1.111m);
        actualPoint.Qualities.Should().Contain(QuantityQuality.Measured);
        actualPoint.Qualities.Count.Should().Be(1);
        actualPoint.Price.Should().Be(2.123456m);
        actualPoint.Amount.Should().Be(3.123456m);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRowsBelongsToDifferentResults_ReturnsMultipleResults(BatchDto batch)
    {
        // Arrange
        _batchesClientMock
            .Setup(client => client.GetAsync(batch.BatchId))
            .ReturnsAsync(batch);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>()))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 2));

        // Act
        var actual = await Sut.GetAsync(batch.BatchId).ToListAsync();

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
}
