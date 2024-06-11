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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2;

public class SettlementReportEnergyResultRepositoryTests : TestBase<SettlementReportEnergyResultRepository>, IClassFixture<DatabricksSqlStatementApiFixture>
{
    private readonly DatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportEnergyResultRepositoryTests(DatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
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
        Fixture.Inject<ISettlementReportEnergyResultQueries>(new SettlementReportEnergyResultQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            new Mock<ICalculationsClient>().Object));
    }

    [Fact]
    public async Task Count_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                null,
                "da-DK"));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task CountPerEnergySupplier_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "'8236015961810'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                "8236015961810",
                "da-DK"));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T02:00:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:00:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T04:00:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "019", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                null,
                "da-DK"));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task GetPerEnergySupplier_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T02:00:00.000+00:00'", "26.634", "'8236015961810'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:00:00.000+00:00'", "26.634", "'8236015961810'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'WholesaleFixing'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T04:00:00.000+00:00'", "26.634", "'8236015961810'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "019", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                "8236015961810",
                "da-DK"));

        Assert.Equal(1, actual);
    }
}
