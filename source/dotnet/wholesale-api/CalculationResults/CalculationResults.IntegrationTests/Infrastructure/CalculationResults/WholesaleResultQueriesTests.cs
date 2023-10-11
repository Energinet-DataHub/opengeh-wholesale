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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.CalculationResults;

public class WholesaleResultQueriesTests : IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string CalculationId = "019703e7-98ee-45c1-b343-0cbf185a47d9";
    private const string HourlyTariffCalculationResultId = "12345678-98ee-45c1-b343-0cbf185a47d9";
    private const string MonthlyAmountTariffCalculationResultId = "23456789-98ee-45c1-b343-0cbf185a47d9";
    private const string DefaultHourlyAmount = "2.34567";
    private const string DefaultMonthlyAmount = "1.123456";

    private readonly DatabricksSqlStatementApiFixture _fixture;

    public WholesaleResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsExpectedWholesaleResult(
        Mock<IHttpClientFactory> httpClientFactoryMock,
        Mock<ILogger<SqlStatusResponseParser>> sqlStatusResponseParserLoggerMock,
        Mock<ILogger<DatabricksSqlStatementClient>> databricksSqlStatementClientLoggerMock,
        Mock<IBatchesClient> batchesClientMock,
        BatchDto batch,
        Mock<ILogger<WholesaleResultQueries>> wholesaleResultQueriesLoggerMock)
    {
        // Arrange
        var deltaTableOptions = _fixture.DatabricksSchemaManager.DeltaTableOptions;
        await InsertHourlyTariffAndMonthlyAmountTariffRows(deltaTableOptions);
        batch = batch with { BatchId = Guid.Parse(CalculationId) };
        var sqlStatementClient = _fixture.CreateSqlStatementClient(
            httpClientFactoryMock,
            sqlStatusResponseParserLoggerMock,
            databricksSqlStatementClientLoggerMock);
        batchesClientMock.Setup(b => b.GetAsync(It.IsAny<Guid>())).ReturnsAsync(batch);
        var sut = new WholesaleResultQueries(sqlStatementClient, batchesClientMock.Object, deltaTableOptions, wholesaleResultQueriesLoggerMock.Object);

        // Act
        var actual = await sut.GetAsync(batch.BatchId).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        var actualHourlyAmount = actual.Single(row => row.Id.ToString() == HourlyTariffCalculationResultId);
        actualHourlyAmount.ChargeResolution.Should().Be(ChargeResolution.Hour);
        actualHourlyAmount.TimeSeriesPoints.First().Amount.Should().Be(decimal.Parse(DefaultHourlyAmount));

        var actualMonthlyAmount = actual.Single(row => row.Id.ToString() == MonthlyAmountTariffCalculationResultId);
        actualMonthlyAmount.ChargeResolution.Should().Be(ChargeResolution.Month);
        actualMonthlyAmount.TimeSeriesPoints.First().Amount.Should().Be(decimal.Parse(DefaultMonthlyAmount));
    }

    private async Task InsertHourlyTariffAndMonthlyAmountTariffRows(IOptions<DeltaTableOptions> options)
    {
        var hourlyTariffRow = WholesaleResultDeltaTableHelper.CreateRowValues(
            calculationId: CalculationId,
            calculationResultId: HourlyTariffCalculationResultId,
            chargeType: "tariff",
            chargeResolution: "PT1H",
            meteringPointType: "production",
            settlementMethod: null,
            amount: DefaultHourlyAmount);

        var monthlyAmountTariffRow = WholesaleResultDeltaTableHelper.CreateRowValues(
            calculationId: CalculationId,
            calculationResultId: MonthlyAmountTariffCalculationResultId,
            chargeType: "tariff",
            chargeResolution: "P1M",
            meteringPointType: null,
            settlementMethod: null,
            amount: DefaultMonthlyAmount);

        var rows = new List<IReadOnlyCollection<string>> { hourlyTariffRow, monthlyAmountTariffRow };
        await _fixture.DatabricksSchemaManager.InsertAsync<WholesaleResultColumnNames>(options.Value.WHOLESALE_RESULTS_TABLE_NAME, rows);
    }
}
