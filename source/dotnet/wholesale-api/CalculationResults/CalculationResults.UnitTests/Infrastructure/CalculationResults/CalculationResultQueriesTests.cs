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
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using FluentAssertions;
using Moq;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResults;

public class CalculationResultQueriesTests
{
    private readonly TableChunk _tableChunk;
    private readonly string _row0CalculationId;
    private readonly string _calculationResultId0;

    public CalculationResultQueriesTests()
    {
        // The two rows belongs to different calculation results as they have different calculation result ids
        _row0CalculationId = "b78787d5-b544-44ac-87c2-7720aab86ed1";
        _calculationResultId0 = "9913f3bb-1208-400b-9cbe-50300e386d26";
        const string calculationResultId1 = "8c2bb7c6-d8e5-462c-9bce-8537f93ef8e7";
        var row0 = new[] { _row0CalculationId, "100", "200", "non_profiled_consumption", string.Empty, string.Empty, "2022-05-16T22:00:00.000Z", "1.111", "measured", _calculationResultId0 };
        var row1 = new[] { "b78787d5-b544-44ac-87c2-7720aab86ed2", "200", "100", "non_profiled_consumption", string.Empty, string.Empty, "2022-05-16T22:00:00.000Z", "2.222", "measured", calculationResultId1 };
        var rows = new List<string[]> { row0, row1, };

        // Using the columns from the CalculationResultQueries class to ensure that the test is not broken if the columns are changed
        _tableChunk = new TableChunk(CalculationResultQueries.SqlColumnNames, rows);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenNoRows_ReturnsNoResults(
        Guid calculationId,
        [Frozen] Mock<ISqlStatementClient> sqlStatementClientMock,
        CalculationResultQueries sut)
    {
        // Arrange
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
            .Returns(GetRowsAsync(0));

        // Act
        var actual = await sut.GetAsync(calculationId).ToListAsync();

        // Assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenOneRow_ReturnsSingleResultWithOneTimeSeriesPoint(
        CalculationDto calculation,
        [Frozen] Mock<ICalculationsClient> calculationsClientMock,
        [Frozen] Mock<ISqlStatementClient> sqlStatementClientMock,
        CalculationResultQueries sut)
    {
        // Arrange
        var calculationId = Guid.Parse(_row0CalculationId);
        calculation = calculation with { CalculationId = calculationId };
        calculationsClientMock
            .Setup(client => client.GetAsync(calculationId))
            .ReturnsAsync(calculation);
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
            .Returns(GetRowsAsync(1));

        // Act
        var actual = await sut.GetAsync(calculationId).ToListAsync();

        // Assert
        actual.Single().TimeSeriesPoints.Length.Should().Be(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsResultRowWithExpectedValues(
        CalculationDto calculation,
        [Frozen] Mock<ICalculationsClient> calculationsClientMock,
        [Frozen] Mock<ISqlStatementClient> sqlStatementClientMock,
        CalculationResultQueries sut)
    {
        // Arrange
        var calculationId = Guid.Parse(_row0CalculationId);
        calculation = calculation with { CalculationId = calculationId };
        calculationsClientMock
            .Setup(client => client.GetAsync(calculationId))
            .ReturnsAsync(calculation);
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
            .Returns(GetRowsAsync(1));

        // Act
        var actual = await sut.GetAsync(calculationId).SingleAsync();

        // Assert
        actual.Id.Should().Be(_calculationResultId0);
        actual.CalculationId.Should().Be(Guid.Parse(_row0CalculationId));
        actual.GridArea.Should().Be(_tableChunk[0, ResultColumnNames.GridArea]);
        actual.TimeSeriesType.Should().Be(TimeSeriesType.NonProfiledConsumption);
        actual.BalanceResponsibleId.Should().Be(_tableChunk[0, ResultColumnNames.BalanceResponsibleId]);
        actual.EnergySupplierId.Should().Be(_tableChunk[0, ResultColumnNames.EnergySupplierId]);
        actual.CalculationId.Should().Be(_tableChunk[0, ResultColumnNames.CalculationId]);
        actual.ProcessType.Should().Be(calculation.ProcessType);
        actual.PeriodStart.Should().Be(calculation.PeriodStart.ToInstant());
        actual.PeriodEnd.Should().Be(calculation.PeriodEnd.ToInstant());
        var actualPoint = actual.TimeSeriesPoints.Single();
        actualPoint.Time.Should().Be(new DateTimeOffset(2022, 5, 16, 22, 0, 0, TimeSpan.Zero));
        actualPoint.Quantity.Should().Be(1.111m);
        actualPoint.Quality.Should().Be(QuantityQuality.Measured);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenRowsBelongsToDifferentResults_ReturnsMultipleResults(
        CalculationDto calculation,
        [Frozen] Mock<ICalculationsClient> calculationsClientMock,
        [Frozen] Mock<ISqlStatementClient> sqlStatementClientMock,
        CalculationResultQueries sut)
    {
        // Arrange
        var calculationId = Guid.Parse(_row0CalculationId);
        calculation = calculation with { CalculationId = calculationId };
        calculationsClientMock
            .Setup(client => client.GetAsync(calculationId))
            .ReturnsAsync(calculation);
        sqlStatementClientMock
            .Setup(x => x.ExecuteAsync(It.IsAny<string>()))
            .Returns(GetRowsAsync(2));

        // Act
        var actual = await sut.GetAsync(calculationId).ToListAsync();

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
            new(ResultColumnNames.CalculationId, "calculationId"),
            new(ResultColumnNames.CalculationResultId, calculationResultIdA),
        };
        var sqlResultRowA = new TestRow(listA);
        var listB = new List<KeyValuePair<string, string>>
        {
            new(ResultColumnNames.CalculationId, "calculationId"),
            new(ResultColumnNames.CalculationResultId, calculationResultIdB),
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
