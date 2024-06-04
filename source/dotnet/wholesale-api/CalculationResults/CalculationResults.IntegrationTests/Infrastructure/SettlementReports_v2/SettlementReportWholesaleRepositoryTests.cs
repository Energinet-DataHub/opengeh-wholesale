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
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2;

public class SettlementReportWholesaleRepositoryTests : TestBase<SettlementReportWholesaleRepository>, IClassFixture<DatabricksSqlStatementApiFixture>
{
    private readonly DatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportWholesaleRepositoryTests(DatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;

        Fixture.Inject(_databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(_databricksSqlStatementApiFixture.GetDatabricksExecutor());
        Fixture.Inject<ISettlementReportWholesaleResultQueries>(new SettlementReportWholesaleResultQueries(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            new Mock<ICalculationsClient>().Object));
    }

    [Fact]
    public async Task Count_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'BalanceFixing'", "'403'", "'8397670583196'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>()
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
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'BalanceFixing'", "'404'", "'8397670583196'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'BalanceFixing'", "'404'", "'8397670583196'", "'2024-01-02T03:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'BalanceFixing'", "'404'", "'8397670583196'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'"],
            ]);

        var results = await Sut.GetAsync(
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
            2,
            1).ToListAsync();

        Assert.Single(results);
        Assert.Equal(4, results[0].StartDateTime.ToDateTimeOffset().Hour);
    }
}
