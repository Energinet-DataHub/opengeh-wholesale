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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient;

[UnitTest]
public class TableTests
{
    private const int InvalidRowIndex = 100;
    private const int ValidRowIndex = 1;
    private const string ValidColumnName = "col2";
    private const string InvalidColumnName = "invalid_column_name";
    private readonly List<string> _columnNames;
    private readonly List<string[]> _rowData;

    public TableTests()
    {
        _columnNames = new List<string>() { "col1", "col2", "col3", };
        _rowData = new List<string[]>() { new[] { "cell11", "cell12", "cell13" }, new[] { "cell21", "cell22", "cell23" }, };
    }

    [Fact]
    public void Count_ReturnsNumberOfRows()
    {
        // Arrange
        var sut = new Table(_columnNames, _rowData);

        // Act
        var rowCount = sut.RowCount;

        // Assert
        rowCount.Should().Be(2);
    }

    [Fact]
    public void IndexerWithRowIndex_ReturnsCorrectRow()
    {
        // Arrange
        var expectedRow = new[] { "cell21", "cell22", "cell23" };
        var sut = new Table(_columnNames, _rowData);

        // Act
        var actualRow = sut[1];

        // Assert
        actualRow.Should().Equal(expectedRow);
    }

    [Fact]
    public void IndexerWithColumnName_ReturnsExpectedCell()
    {
        // Arrange
        var sut = new Table(_columnNames, _rowData);

        // Act
        var actual = sut[1, "col2"];

        // Assert
        actual.Should().Be("cell22");
    }

    [Fact]
    public void IndexerWithInvalidColumnName_ThrowsException()
    {
        // Arrange
        var sut = new Table(_columnNames, _rowData);

        // Act + Assert
        Assert.ThrowsAny<Exception>(() => sut[ValidRowIndex, InvalidColumnName]);
    }

    [Fact]
    public void IndexerWithInvalidRowIndex_ThrowsException()
    {
        // Arrange
        var sut = new Table(_columnNames, _rowData);

        // Act + Assert
        Assert.ThrowsAny<Exception>(() => sut[InvalidRowIndex, ValidColumnName]);
    }
}
