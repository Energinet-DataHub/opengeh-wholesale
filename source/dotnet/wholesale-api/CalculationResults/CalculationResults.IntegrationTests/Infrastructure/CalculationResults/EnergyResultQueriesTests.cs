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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.CalculationResults;

public class EnergyResultQueriesTests : TestBase<EnergyResultQueries>, IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string BatchId = "019703e7-98ee-45c1-b343-0cbf185a47d9";
    private const string FirstQuantity = "1.111";
    private const string SecondQuantity = "2.222";
    private const string ThirdQuantity = "3.333";
    private const string FourthQuantity = "4.444";
    private const string FifthQuantity = "5.555";
    private const string SixthQuantity = "6.666";
    private readonly DatabricksSqlStatementApiFixture _fixture;

    private readonly Mock<IBatchesClient> _batchesClientMock;
    ////private readonly Mock<DatabricksSqlWarehouseQueryExecutor> _databricksSqlWarehouseQueryExecutorMock;
    ////private readonly Mock<ILogger<EnergyResultQueries>> _energyResultQueriesLoggerMock;
    ////private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

    public EnergyResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;

        // Mocks Setup - This is another way to setup mocks used in tests. The reason for this are:
        // 1. Because DatabricksSqlWarehouseQueryExecutor doesn't implement an interface and the constructor is protected
        // AutoFixture combined with inline is unable to create an instance of it.
        // 2. The many mock parameters are avoided in tests
        _batchesClientMock = Fixture.Freeze<Mock<IBatchesClient>>();
        ////_databricksSqlWarehouseQueryExecutorMock = Fixture.Freeze<Mock<DatabricksSqlWarehouseQueryExecutor>>();
        ////_energyResultQueriesLoggerMock = Fixture.Freeze<Mock<ILogger<EnergyResultQueries>>>();
        ////_httpClientFactoryMock = Fixture.Freeze<Mock<IHttpClientFactory>>();
        //var options = Fixture.Freeze<IOptions<DeltaTableOptions>>();
        Fixture.Inject(_batchesClientMock.Object);
        Fixture.Inject(_fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(fixture.GetExecutor());
        //Fixture.Inject(fixture.DatabricksSqlStatementOptionsMock.Object);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsExpectedEnergyResult(BatchDto batch)
    {
        // Arrange
        const int expectedResultCount = 3;
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        batch = batch with { BatchId = Guid.Parse(BatchId) };
        _batchesClientMock.Setup(b => b.GetAsync(It.IsAny<Guid>())).ReturnsAsync(batch);
        //var sut = new EnergyResultQueries(databricksSqlWarehouseQueryExecutorMock.Object, batchesClientMock.Object, deltaTableOptions, energyResultQueriesLoggerMock.Object);

        // Act
        var actual = await Sut.GetAsync(batch.BatchId).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(expectedResultCount);
        actual.SelectMany(a => a.TimeSeriesPoints)
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity, SecondQuantity, ThirdQuantity, FourthQuantity, FifthQuantity, SixthQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsExpectedEnergyResult2(BatchDto batch)
    {
        // Arrange
        const int expectedResultCount = 3;
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await AddCreatedRowsInArbitraryOrderAsync(deltaTableOptions);
        batch = batch with { BatchId = Guid.Parse(BatchId) };
        _batchesClientMock.Setup(b => b.GetAsync(It.IsAny<Guid>())).ReturnsAsync(batch);

        // Act
        var actual = await Sut.GetAsync(batch.BatchId).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Count.Should().Be(expectedResultCount);
        actual.SelectMany(a => a.TimeSeriesPoints)
            .Select(p => p.Quantity.ToString(CultureInfo.InvariantCulture))
            .ToArray()
            .Should()
            .Equal(FirstQuantity, SecondQuantity, ThirdQuantity, FourthQuantity, FifthQuantity, SixthQuantity);
    }

    private async Task AddCreatedRowsInArbitraryOrderAsync(IOptions<DeltaTableOptions> options)
    {
        const string firstCalculationResultId = "aaaaaaaa-386f-49eb-8b56-63fae62e4fc7";
        const string secondCalculationResultId = "bbbbbbbb-b58b-4190-a873-eded0ed50c20";
        const string thirdCalculationResultId = "cccccccc-b58b-4190-a873-eded0ed50c20";
        const string firstHour = "2022-01-01T01:00:00.000Z";
        const string secondHour = "2022-01-01T02:00:00.000Z";
        const string gridAreaA = "301";
        const string gridAreaB = "101";
        const string gridAreaC = "501";

        var row1 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: firstHour, gridArea: gridAreaA, quantity: FirstQuantity);
        var row2 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: firstCalculationResultId, time: secondHour, gridArea: gridAreaA, quantity: SecondQuantity);

        var row3 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: firstHour, gridArea: gridAreaB, quantity: ThirdQuantity);
        var row4 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: secondCalculationResultId, time: secondHour, gridArea: gridAreaB, quantity: FourthQuantity);

        var row5 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, time: firstHour, gridArea: gridAreaC, quantity: FifthQuantity);
        var row6 = EnergyResultDeltaTableHelper.CreateRowValues(batchId: BatchId, calculationResultId: thirdCalculationResultId, time: secondHour, gridArea: gridAreaC, quantity: SixthQuantity);

        // mix up the order of the rows
        var rows = new List<IReadOnlyCollection<string>> { row3, row5, row1, row2, row6, row4 };
        await _fixture.DatabricksSchemaManager.InsertAsync<EnergyResultColumnNames>(options.Value.ENERGY_RESULTS_TABLE_NAME, rows);
    }
}
