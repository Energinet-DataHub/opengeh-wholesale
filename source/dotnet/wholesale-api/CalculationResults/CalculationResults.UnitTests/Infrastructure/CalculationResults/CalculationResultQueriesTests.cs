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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResults;

public class CalculationResultQueriesTests
{
    private readonly TableChunk _tableChunk;
    private readonly string _row0BatchId;

    public CalculationResultQueriesTests()
    {
        // The two rows belongs to different calculation results as they are for different grid areas
        _row0BatchId = "b78787d5-b544-44ac-87c2-7720aab86ed1";
        var row0 = new[] { _row0BatchId, "100", "BalanceFixing", "non_profiled_consumption", string.Empty, string.Empty, "2022-05-16T22:00:00.000Z", "1.111", "measured" };
        var row1 = new[] { "b78787d5-b544-44ac-87c2-7720aab86ed2", "200", "BalanceFixing", "non_profiled_consumption", string.Empty, string.Empty, "2022-05-16T22:00:00.000Z", "2.222", "measured" };
        var rows = new List<string[]> { row0, row1, };

        // Using the columns from the CalculationResultQueries class to ensure that the test is not broken if the columns are changed
        _tableChunk = new TableChunk(CalculationResultQueries.SqlColumnNames, rows);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenNoRows_ReturnsNoResults(
        Guid batchId,
        [Frozen] Mock<ISqlStatementClient> sqlStatementClientMock,
        CalculationResultQueries sut)
    {
        // Arrange
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
            .Returns(GetRowsAsync(0));

        // Act
        var actual = await sut.GetAsync(batchId).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenOneRow_ReturnsSingleResultWithOneTimeSeriesPoint(
        Guid batchId,
        [Frozen] Mock<ISqlStatementClient> sqlStatementClientMock,
        CalculationResultQueries sut)
    {
        // Arrange
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
            .Returns(GetRowsAsync(1));

        // Act
        var actual = await sut.GetAsync(batchId).ToListAsync();

        // Assert
        actual.Single().TimeSeriesPoints.Length.Should().Be(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsResultRowWithExpectedValues(
        [Frozen] Mock<ISqlStatementClient> sqlStatementClientMock,
        CalculationResultQueries sut)
    {
        // Arrange
        var batchId = Guid.Parse(_row0BatchId);
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
            .Returns(GetRowsAsync(1));

        // Act
        var actual = await sut.GetAsync(batchId).SingleAsync();

        // Assert
        actual.BatchId.Should().Be(Guid.Parse("b78787d5-b544-44ac-87c2-7720aab86ed1"));
        actual.ProcessType.Should().Be(ProcessType.BalanceFixing);
        actual.GridArea.Should().Be(_tableChunk[0, ResultColumnNames.GridArea]);
        actual.TimeSeriesType.Should().Be(TimeSeriesType.NonProfiledConsumption);
        actual.BalanceResponsibleId.Should().Be(_tableChunk[0, ResultColumnNames.BalanceResponsibleId]);
        actual.EnergySupplierId.Should().Be(_tableChunk[0, ResultColumnNames.EnergySupplierId]);
        actual.BatchId.Should().Be(_tableChunk[0, ResultColumnNames.BatchId]);
        var actualPoint = actual.TimeSeriesPoints.Single();
        actualPoint.Time.Should().Be(new DateTimeOffset(2022, 5, 16, 22, 0, 0, TimeSpan.Zero));
        actualPoint.Quantity.Should().Be(1.111m);
        actualPoint.Quality.Should().Be(QuantityQuality.Measured);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRowsBelongsToDifferentResults_ReturnsMultipleResults(
        Guid batchId,
        [Frozen] Mock<ISqlStatementClient> sqlStatementClientMock,
        CalculationResultQueries sut)
    {
        // Arrange
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
            .Returns(GetRowsAsync(2));

        // Act
        var actual = await sut.GetAsync(batchId).ToListAsync();

        // Assert
        actual.Count.Should().Be(2);
    }

    [Theory]
    [InlineAutoMoqData("id", "id", "ga", "ga", "ts", "ts", "es", "es", "brp", "brp", false)]
    [InlineAutoMoqData("idx", "id", "ga", "ga", "ts", "ts", "es", "es", "brp", "brp", true)]
    [InlineAutoMoqData("id", "idx", "ga", "ga", "ts", "ts", "es", "es", "brp", "brp", true)]
    [InlineAutoMoqData("id", "id", "gax", "ga", "ts", "ts", "es", "es", "brp", "brp", true)]
    [InlineAutoMoqData("id", "id", "ga", "gax", "ts", "ts", "es", "es", "brp", "brp", true)]
    [InlineAutoMoqData("id", "id", "ga", "ga", "tsx", "ts", "es", "es", "brp", "brp", true)]
    [InlineAutoMoqData("id", "id", "ga", "ga", "ts", "tsx", "es", "es", "brp", "brp", true)]
    [InlineAutoMoqData("id", "id", "ga", "ga", "ts", "ts", "esx", "es", "brp", "brp", true)]
    [InlineAutoMoqData("id", "id", "ga", "ga", "ts", "ts", "es", "esx", "brp", "brp", true)]
    [InlineAutoMoqData("id", "id", "ga", "ga", "ts", "ts", "es", "es", "brpx", "brp", true)]
    [InlineAutoMoqData("id", "id", "ga", "ga", "ts", "ts", "es", "es", "brp", "brpx", true)]
    [InlineAutoMoqData("idx", "id", "gax", "ga", "tsx", "ts", "esx", "es", "brpx", "brp", true)]
    public void BelongsToDifferentResults_ReturnsExpectedValue(
        string batchIdA,
        string batchIdB,
        string gridAreaA,
        string gridAreaB,
        string timeSeriesTypeA,
        string timeSeriesTypeB,
        string energySupplierIdA,
        string energySupplierIdB,
        string balanceResponsibleIdA,
        string balanceResponsibleIdB,
        bool expected)
    {
        // Arrange
        var listA = new List<KeyValuePair<string, string>>
        {
            new(ResultColumnNames.BatchId, batchIdA),
            new(ResultColumnNames.GridArea, gridAreaA),
            new(ResultColumnNames.TimeSeriesType, timeSeriesTypeA),
            new(ResultColumnNames.EnergySupplierId, energySupplierIdA),
            new(ResultColumnNames.BalanceResponsibleId, balanceResponsibleIdA),
        };
        var sqlResultRowA = new TestRow(listA);
        var listB = new List<KeyValuePair<string, string>>
        {
            new(ResultColumnNames.BatchId, batchIdB),
            new(ResultColumnNames.GridArea, gridAreaB),
            new(ResultColumnNames.TimeSeriesType, timeSeriesTypeB),
            new(ResultColumnNames.EnergySupplierId, energySupplierIdB),
            new(ResultColumnNames.BalanceResponsibleId, balanceResponsibleIdB),
        };
        var sqlResultRowB = new TestRow(listB);

        // Act
        var actual = CalculationResultQueries.BelongsToDifferentResults(sqlResultRowA, sqlResultRowB);

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

    private record TestRow : SqlResultRow
    {
        private readonly List<KeyValuePair<string, string>> _list;

        public TestRow(List<KeyValuePair<string, string>> list)
            : base(null!, 0)
        {
            _list = list;
        }

        public override string this[string column] => _list.Single(pair => pair.Key == column).Value;
    }
}
