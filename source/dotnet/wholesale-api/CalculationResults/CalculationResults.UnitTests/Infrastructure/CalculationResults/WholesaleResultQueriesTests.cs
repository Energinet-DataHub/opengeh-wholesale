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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.Fixtures;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Moq;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResults;

public class WholesaleResultQueriesTests
{
    private readonly TableChunk _tableChunk;
    private readonly string _row0CalculationId;
    private readonly string _calculationResultId0;

    public WholesaleResultQueriesTests()
    {
        // The two rows belongs to different calculation results as they have different calculation result ids
        _row0CalculationId = "b78787d5-b544-44ac-87c2-7720aab86ed1";
        _calculationResultId0 = "9913f3bb-1208-400b-9cbe-50300e386d26";
        const string calculationResultId1 = "8c2bb7c6-d8e5-462c-9bce-8537f93ef8e7";
        var row0 = new[] { _row0CalculationId, _calculationResultId0, DeltaTableProcessType.WholesaleFixing, "200", "1234567890000", "consumption", "flex", "tariff", "somChargeCode", "someOwnerId", "PT1H", "true", "kWh", "2022-05-16T22:00:00.000Z", "1.111", "[\"measured\"]", "2.123456", "3.123456" };
        var row1 = new[] { "b78787d5-b544-44ac-87c2-7720aab86ed2", calculationResultId1, DeltaTableProcessType.WholesaleFixing, "200", "1234567890000", "consumption", "flex", "tariff", "somChargeCode", "someOwnerId", "PT1H", "true", "kWh", "2022-05-16T22:00:00.000Z", "1.111", "[\"measured\"]", "2.123456", "3.123456" };
        var rows = new List<string[]> { row0, row1, };

        // Using the columns from the WholesaleResultQueries class to ensure that the test is not broken if the columns are changed
        _tableChunk = new TableChunk(WholesaleResultQueries.SqlColumnNames, rows);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenNoRows_ReturnsNoResults(
        BatchDto batch,
        [Frozen] Mock<IBatchesClient> batchesClientMock,
        [Frozen] Mock<IDatabricksSqlStatementClient> sqlStatementClientMock,
        WholesaleResultQueries sut)
    {
        // Arrange
        var batchId = Guid.Parse(_row0CalculationId);
        batch = batch with { BatchId = batchId };
        batchesClientMock
            .Setup(client => client.GetAsync(batchId))
            .ReturnsAsync(batch);
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), null))
            .Returns(GetRowsAsync(0));

        // Act
        var actual = await sut.GetAsync(batchId).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenOneRow_ReturnsSingleResultWithOneTimeSeriesPoint(
        BatchDto batch,
        [Frozen] Mock<IBatchesClient> batchesClientMock,
        [Frozen] Mock<IDatabricksSqlStatementClient> sqlStatementClientMock,
        WholesaleResultQueries sut)
    {
        // Arrange
        var batchId = Guid.Parse(_row0CalculationId);
        batch = batch with { BatchId = batchId };
        batchesClientMock
            .Setup(client => client.GetAsync(batchId))
            .ReturnsAsync(batch);
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), null))
            .Returns(GetRowsAsync(1));

        // Act
        var actual = await sut.GetAsync(batchId).ToListAsync();

        // Assert
        actual.Single().TimeSeriesPoints.Count.Should().Be(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsResultRowWithExpectedValues(
        BatchDto batch,
        [Frozen] Mock<IBatchesClient> batchesClientMock,
        [Frozen] Mock<IDatabricksSqlStatementClient> sqlStatementClientMock,
        WholesaleResultQueries sut)
    {
        // Arrange
        var batchId = Guid.Parse(_row0CalculationId);
        batch = batch with { BatchId = batchId, ProcessType = ProcessType.WholesaleFixing };
        batchesClientMock
            .Setup(client => client.GetAsync(batchId))
            .ReturnsAsync(batch);
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), null))
            .Returns(GetRowsAsync(1));

        // Act
        var actual = await sut.GetAsync(batchId).SingleAsync();

        // Assert
        actual.Id.Should().Be(_calculationResultId0);
        actual.CalculationId.Should().Be(Guid.Parse(_row0CalculationId));
        actual.GridArea.Should().Be(_tableChunk[0, WholesaleResultColumnNames.GridArea]);
        actual.EnergySupplierId.Should().Be(_tableChunk[0, WholesaleResultColumnNames.EnergySupplierId]);
        actual.CalculationId.Should().Be(_tableChunk[0, WholesaleResultColumnNames.CalculationId]);
        actual.CalculationType.Should().Be(batch.ProcessType);
        actual.PeriodStart.Should().Be(batch.PeriodStart.ToInstant());
        actual.PeriodEnd.Should().Be(batch.PeriodEnd.ToInstant());
        var actualPoint = actual.TimeSeriesPoints.Single();
        actualPoint.Time.Should().Be(new DateTimeOffset(2022, 5, 16, 22, 0, 0, TimeSpan.Zero));
        actualPoint.Quantity.Should().Be(1.111m);
        actualPoint.Qualities.Should().Contain(QuantityQuality.Measured);
        actualPoint.Qualities.Count.Should().Be(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRowsBelongsToDifferentResults_ReturnsMultipleResults(
        BatchDto batch,
        [Frozen] Mock<IBatchesClient> batchesClientMock,
        [Frozen] Mock<IDatabricksSqlStatementClient> sqlStatementClientMock,
        WholesaleResultQueries sut)
    {
        // Arrange
        var batchId = Guid.Parse(_row0CalculationId);
        batch = batch with { BatchId = batchId };
        batchesClientMock
            .Setup(client => client.GetAsync(batchId))
            .ReturnsAsync(batch);
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), null))
            .Returns(GetRowsAsync(2));

        // Act
        var actual = await sut.GetAsync(batchId).ToListAsync();

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
        var listA = new List<KeyValuePair<string, string>>
        {
            new(WholesaleResultColumnNames.CalculationId, "calculationId"),
            new(WholesaleResultColumnNames.CalculationResultId, calculationResultIdA),
        };
        var sqlResultRowA = new TestSqlResultRow(listA);
        var listB = new List<KeyValuePair<string, string>>
        {
            new(WholesaleResultColumnNames.CalculationId, "calculationId"),
            new(WholesaleResultColumnNames.CalculationResultId, calculationResultIdB),
        };
        var sqlResultRowB = new TestSqlResultRow(listB);

        // Act
        var actual = WholesaleResultQueries.BelongsToDifferentResults(sqlResultRowA, sqlResultRowB);

        // Assert
        actual.Should().Be(expected);
    }

    private async IAsyncEnumerable<SqlResultRow> GetRowsAsync(int rowCount)
    {
        await Task.Delay(0);
        for (var i = 0; i < rowCount; i++)
        {
            yield return new SqlResultRow(_tableChunk, i);
        }
    }
}
