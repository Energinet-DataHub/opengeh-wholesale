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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SettlementReport;
using FluentAssertions;
using Moq;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResults;

public class EnergyResultQueriesTests : TestBase<EnergyResultQueries>
{
    private readonly TableChunk _tableChunk;
    private readonly string _row0BatchId;
    private readonly string _calculationResultId0;

    private readonly Mock<ICalculationsClient> _batchesClientMock;
    private readonly Mock<DatabricksSqlWarehouseQueryExecutor> _databricksSqlWarehouseQueryExecutorMock;

    public EnergyResultQueriesTests()
    {
        // The two rows belongs to different calculation results as they have different calculation result ids
        _row0BatchId = "b78787d5-b544-44ac-87c2-7720aab86ed1";
        _calculationResultId0 = "9913f3bb-1208-400b-9cbe-50300e386d26";
        const string calculationResultId1 = "8c2bb7c6-d8e5-462c-9bce-8537f93ef8e7";
        var row0 = new[]
        {
            _row0BatchId, "100", "200", "non_profiled_consumption", string.Empty, string.Empty,
            "2022-05-16T22:00:00.000Z", "1.111", "[\"measured\"]", _calculationResultId0,
            DeltaTableProcessType.Aggregation, null,
        };
        var row1 = new[]
        {
            "b78787d5-b544-44ac-87c2-7720aab86ed2", "200", "100", "non_profiled_consumption", string.Empty,
            string.Empty, "2022-05-16T22:00:00.000Z", "2.222", "[\"measured\"]", calculationResultId1,
            DeltaTableProcessType.BalanceFixing, null,
        };
        var rows = new List<string?[]> { row0, row1, };

        // Using the columns from the EnergyResultQueries class to ensure that the test is not broken if the columns are changed
        _tableChunk = new TableChunk(EnergyResultQueryStatement.SqlColumnNames, rows);

        // Mocks Setup - This is another way to setup mocks used in tests. The reasons for this are:
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
    public async Task GetAsync_WhenNoRows_ReturnsNoResults(CalculationDto calculation)
    {
        // Arrange
        _batchesClientMock
            .Setup(client => client.GetAsync(calculation.BatchId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock.Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>()))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 0));

        // Act
        var actual = await Sut.GetAsync(calculation.BatchId).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenOneRow_ReturnsSingleResultWithOneTimeSeriesPoint(CalculationDto calculation)
    {
        // Arrange
        _batchesClientMock
            .Setup(client => client.GetAsync(calculation.BatchId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>()))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 1));

        // Act
        var actual = await Sut.GetAsync(calculation.BatchId).ToListAsync();

        // Assert
        actual.Single().TimeSeriesPoints.Length.Should().Be(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsResultRowWithExpectedValues(CalculationDto calculation)
    {
        // Arrange
        _batchesClientMock
            .Setup(client => client.GetAsync(calculation.BatchId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>()))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 1));

        // Act
        var actual = await Sut.GetAsync(calculation.BatchId).SingleAsync();

        // Assert
        actual.Id.Should().Be(_calculationResultId0);
        actual.BatchId.Should().Be(Guid.Parse(_row0BatchId));
        actual.GridArea.Should().Be(_tableChunk[0, EnergyResultColumnNames.GridArea]);
        actual.TimeSeriesType.Should().Be(TimeSeriesType.NonProfiledConsumption);
        actual.BalanceResponsibleId.Should().Be(_tableChunk[0, EnergyResultColumnNames.BalanceResponsibleId]);
        actual.EnergySupplierId.Should().Be(_tableChunk[0, EnergyResultColumnNames.EnergySupplierId]);
        actual.BatchId.Should().Be(_tableChunk[0, EnergyResultColumnNames.BatchId]);
        actual.ProcessType.Should().Be(calculation.ProcessType);
        actual.PeriodStart.Should().Be(calculation.PeriodStart.ToInstant());
        actual.PeriodEnd.Should().Be(calculation.PeriodEnd.ToInstant());
        var actualPoint = actual.TimeSeriesPoints.Single();
        actualPoint.Time.Should().Be(new DateTimeOffset(2022, 5, 16, 22, 0, 0, TimeSpan.Zero));
        actualPoint.Quantity.Should().Be(1.111m);
        actualPoint.Qualities.Should().ContainEquivalentOf(QuantityQuality.Measured);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRowsBelongsToDifferentResults_ReturnsMultipleResults(CalculationDto calculation)
    {
        // Arrange
        _batchesClientMock
            .Setup(client => client.GetAsync(calculation.BatchId))
            .ReturnsAsync(calculation);
        _databricksSqlWarehouseQueryExecutorMock
            .Setup(o => o.ExecuteStatementAsync(It.IsAny<DatabricksStatement>(), It.IsAny<Format>()))
            .Returns(DatabricksTestHelper.GetRowsAsync(_tableChunk, 2));

        // Act
        var actual = await Sut.GetAsync(calculation.BatchId).ToListAsync();

        // Assert
        actual.Count.Should().Be(2);
    }

    [Theory]
    [InlineAutoMoqData("someId", "otherId", true)]
    [InlineAutoMoqData("someId", "someId", false)]
    public void BelongsToDifferentResults_ReturnsExpectedValue(
        string calculationResultIdA,
        string calculationResultIdB,
        bool expected)
    {
        // Arrange
        var listA = new DatabricksSqlRow(new Dictionary<string, object?>
        {
            { EnergyResultColumnNames.BatchId, "batchId" },
            { EnergyResultColumnNames.CalculationResultId, calculationResultIdA },
        });

        var listB = new DatabricksSqlRow(new Dictionary<string, object?>
        {
            { EnergyResultColumnNames.BatchId, "batchId" },
            { EnergyResultColumnNames.CalculationResultId, calculationResultIdB },
        });

        // Act
        var actual = EnergyResultQueries.BelongsToDifferentResults(listA, listB);

        // Assert
        actual.Should().Be(expected);
    }
}
