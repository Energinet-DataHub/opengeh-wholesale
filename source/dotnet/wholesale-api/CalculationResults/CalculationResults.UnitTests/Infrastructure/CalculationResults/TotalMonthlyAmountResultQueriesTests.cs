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
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResults;

public class TotalMonthlyAmountResultQueriesTests : TestBase<TotalMonthlyAmountResultQueries>
{
    private readonly TableChunk _tableChunk;
    private readonly string _row0CalculationId;
    private readonly string _calculationResultId0;

    private readonly Mock<ICalculationsClient> _calculationsClientMock;
    private readonly Mock<DatabricksSqlWarehouseQueryExecutor> _databricksSqlWarehouseQueryExecutorMock;

    public TotalMonthlyAmountResultQueriesTests()
    {
        _row0CalculationId = "b78787d5-b544-44ac-87c2-7720aab86ed1";
        // The two rows belongs to different calculation results as they have different calculation result ids
        _calculationResultId0 = "9913f3bb-1208-400b-9cbe-50300e386d26";
        const string calculationResultId1 = "8c2bb7c6-d8e5-462c-9bce-8537f93ef8e7";
        var row0 = new[] { _row0CalculationId, _calculationResultId0, DeltaTableCalculationType.WholesaleFixing, "200", "1234567890000", "5790001330552", "128533.784163" };
        var row1 = new[] { "9913f3bb-1208-400b-9cbe-50300e386d01", calculationResultId1, DeltaTableCalculationType.WholesaleFixing, "200", "1234567890000", "5790001330552", "1018490.014788" };
        var rows = new List<string?[]> { row0, row1, };

        // Using the columns from the WholesaleResultQueries class to ensure that the test is not broken if the columns are changed
        _tableChunk = new TableChunk(TotalMonthlyAmountResultQueryStatement.SqlColumnNames, rows);

        // Mocks Setup - This is another way to set up mocks used in tests. The reason for this are:
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
    public async Task GetAsync_WhenOneRow_ReturnsSingleResultWithExceptedAmount(CalculationDto calculation)
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
        actual.Single().Amount.Should().Be(128533.784163m);
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

        // Act
        var actual = await Sut.GetAsync(calculation.CalculationId).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Single().Id.Should().Be(_row0CalculationId);
        actual.Single().CalculationId.Should().Be(Guid.Parse(_calculationResultId0));
        actual.Single().GridAreaCode.Should().Be(_tableChunk[0, TotalMonthlyAmountsColumnNames.GridAreaCode]);
        actual.Single().EnergySupplierId.Should().Be(_tableChunk[0, TotalMonthlyAmountsColumnNames.EnergySupplierId]);
        actual.Single().CalculationId.Should().Be(_tableChunk[0, TotalMonthlyAmountsColumnNames.CalculationId]);
        actual.Single().CalculationType.Should().Be(calculation.CalculationType);
        actual.Single().PeriodStart.Should().Be(calculation.PeriodStart.ToInstant());
        actual.Single().PeriodEnd.Should().Be(calculation.PeriodEnd.ToInstant());
        actual.Single().Amount.Should().Be(128533.784163m);
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
}
