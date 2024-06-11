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
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2;

public class SettlementReportWholesaleRepositoryTests : TestBase<SettlementReportWholesaleRepository>, IClassFixture<DatabricksSqlStatementApiFixture>
{
    private readonly DatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportWholesaleRepositoryTests(DatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;

        var mockedOptions = new Mock<IOptions<DeltaTableOptions>>();
        mockedOptions.Setup(x => x.Value).Returns(new DeltaTableOptions
        {
            SettlementReportSchemaName = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME,
            WHOLESALE_RESULTS_V1_VIEW_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            BasisDataSchemaName = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.BasisDataSchemaName,
            SCHEMA_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME,
            ENERGY_RESULTS_TABLE_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME,
            WHOLESALE_RESULTS_TABLE_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_TABLE_NAME,
            TOTAL_MONTHLY_AMOUNTS_TABLE_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
            CalculationResultsSchemaName = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CalculationResultsSchemaName,
        });

        Fixture.Inject(mockedOptions);
        Fixture.Inject(_databricksSqlStatementApiFixture.GetDatabricksExecutor());
        var calculationsClientMock = new Mock<ICalculationsClient>();
        calculationsClientMock.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(new CalculationDto(null, Guid.Empty, DateTimeOffset.Now, DateTimeOffset.Now, "a", "b", DateTimeOffset.Now, DateTimeOffset.Now, CalculationState.Completed, true, [], CalculationType.Aggregation, Guid.Empty, 1, CalculationOrchestrationState.Calculated));
        Fixture.Inject<ISettlementReportWholesaleResultQueries>(new SettlementReportWholesaleResultQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            calculationsClientMock.Object));
    }

    [Fact]
    public async Task Count_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'403'", "'8397670583196'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'"],
            ]);

        var actual = await Sut.CountAsync(
            CalculationType.WholesaleFixing,
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "403", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-03T02:00:00.000+00:00"),
                null,
                "da-DK"));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'404'", "'8397670583196'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ea'", "'404'", "'8397670583196'", "'2024-01-02T03:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'404'", "'8397670583196'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'"],
            ]);

        var results = await Sut.GetAsync(
            CalculationType.WholesaleFixing,
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>()
                {
                    {
                        "404", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-02T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-03T00:00:00.000+00:00"),
                null,
                "da-DK"),
            skip: 2,
            take: 1).ToListAsync();

        Assert.Single(results);
        Assert.Equal(4, results[0].StartDateTime.ToDateTimeOffset().Hour);
    }
}
