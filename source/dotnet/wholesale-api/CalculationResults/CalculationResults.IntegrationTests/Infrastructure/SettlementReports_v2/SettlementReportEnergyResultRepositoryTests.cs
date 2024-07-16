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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Persistence.Databricks;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2;

[Collection(nameof(SettlementReportCollectionFixture))]
public class SettlementReportEnergyResultRepositoryTests : TestBase<SettlementReportEnergyResultRepository>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportEnergyResultRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;

        _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SettlementReportSchemaName =
            databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME;

        Fixture.Inject<ISettlementReportDatabricksContext>(new SettlementReportDatabricksContext(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor()));
    }

    [Fact]
    public async Task Count_ValidGridAccessProviderFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null),
            1);

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Count_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "'0236015961810'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            1);

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task LatestCount_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                CalculationType.BalanceFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null),
            1);

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task LatestCount_FilterOutOfRange_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'balance_fixing'", "'5'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                CalculationType.BalanceFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null),
            1);

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task CountPerEnergySupplier_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "'8236015961810'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "8236015961810",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            1);

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task LatestCountPerEnergySupplier_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "'8236015961810'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                CalculationType.BalanceFixing,
                "8236015961810",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            1);

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task LatestCountPerEnergySupplier_FilterOutOfRange_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'balance_fixing'", "'5'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'018'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "'8236015961810'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "018", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T03:30:00.000+00:00"),
                CalculationType.BalanceFixing,
                "8236015961810",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            1);

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:30:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:45:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035308'", "'019'", "'consumption'", "NULL", "'PT15M'", "'2022-01-10T04:00:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "019", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T04:15:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null),
            1,
            skip: 3,
            take: 1).ToListAsync();

        Assert.Single(actual);
        Assert.Equal(4, actual[0].Time.ToDateTimeOffset().Hour);
    }

    [Fact]
    public async Task Get_NullableValues_ReturnsNull()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'52d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "NULL", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634"],
                ["'52d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'019'", "'consumption'", "NULL", "'PT15M'", "'2022-01-10T03:30:00.000+00:00'", "26.634"],
                ["'52d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'019'", "'consumption'", "NULL", "'PT15M'", "'2022-01-10T03:45:00.000+00:00'", "26.634"],
                ["'52d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035308'", "'019'", "'consumption'", "NULL", "'PT15M'", "'2022-01-10T04:00:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "019", new CalculationId(Guid.Parse("52d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T04:15:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null),
            1,
            skip: 3,
            take: 1).ToListAsync();

        Assert.Single(actual);
        Assert.Equal(4, actual[0].Time.ToDateTimeOffset().Hour);
        Assert.Null(actual[0].SettlementMethod);
    }

    [Fact]
    public async Task LatestGet_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b3'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b4'", "'balance_fixing'", "'2'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T05:00:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b5'", "'balance_fixing'", "'3'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T03:30:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b6'", "'balance_fixing'", "'4'", "'47433af6-03c1-46bd-ab9b-dd0497035308'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T05:00:00.000+00:00'", "26.634"],

                // Expected output.
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1c0'", "'balance_fixing'", "'8'", "'47433af6-03c1-46bd-ab9b-dd0497035312'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-13T05:00:00.000+00:00'", "26.634"],

                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b7'", "'balance_fixing'", "'5'", "'47433af6-03c1-46bd-ab9b-dd0497035309'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-12T03:45:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b8'", "'balance_fixing'", "'6'", "'47433af6-03c1-46bd-ab9b-dd0497035310'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-12T05:00:00.000+00:00'", "26.634"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b9'", "'balance_fixing'", "'7'", "'47433af6-03c1-46bd-ab9b-dd0497035311'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-13T04:00:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "019", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-14T00:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null),
            10,
            skip: 3,
            take: 1).ToListAsync();

        Assert.Single(actual);
        Assert.Equal(5, actual[0].Time.ToDateTimeOffset().Hour);
    }

    [Fact]
    public async Task GetPerEnergySupplier_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035312'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T04:00:00.000+00:00'", "26.634", "'8236015961811'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "'8236015961810'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:30:00.000+00:00'", "26.634", "'8236015961810'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:45:00.000+00:00'", "26.634", "'8236015961810'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035308'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T04:00:00.000+00:00'", "26.634", "'8236015961810'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035309'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "'8236015961811'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035310'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:30:00.000+00:00'", "26.634", "'8236015961811'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b2'", "'wholesale_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035311'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:45:00.000+00:00'", "26.634", "'8236015961811'"],
            ]);

        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "019", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T04:15:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "8236015961811",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            1,
            skip: 3,
            take: 1).ToListAsync();

        Assert.Single(actual);
        Assert.Equal(4, actual[0].Time.ToDateTimeOffset().Hour);
    }

    [Fact]
    public async Task LatestGetPerEnergySupplier_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b3'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "'8236015961811'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b4'", "'balance_fixing'", "'2'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T05:00:00.000+00:00'", "26.634", "'8236015961811'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b5'", "'balance_fixing'", "'3'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T03:30:00.000+00:00'", "26.634", "'8236015961811'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b6'", "'balance_fixing'", "'4'", "'47433af6-03c1-46bd-ab9b-dd0497035308'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T05:00:00.000+00:00'", "26.634", "'8236015961811'"],

                // Expected output.
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1c0'", "'balance_fixing'", "'8'", "'47433af6-03c1-46bd-ab9b-dd0497035312'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-13T05:00:00.000+00:00'", "26.634", "'8236015961811'"],

                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b7'", "'balance_fixing'", "'5'", "'47433af6-03c1-46bd-ab9b-dd0497035309'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-12T03:45:00.000+00:00'", "26.634", "'8236015961811'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b8'", "'balance_fixing'", "'6'", "'47433af6-03c1-46bd-ab9b-dd0497035310'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-12T05:00:00.000+00:00'", "26.634", "'8236015961811'"],
                ["'51d60f89-bbc5-4f7a-be98-6139aab1c1b9'", "'balance_fixing'", "'7'", "'47433af6-03c1-46bd-ab9b-dd0497035311'", "'019'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-13T04:00:00.000+00:00'", "26.634", "'8236015961811'"],
            ]);

        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "019", new CalculationId(Guid.Parse("51d60f89-bbc5-4f7a-be98-6139aab1c1b2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-14T00:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                "8236015961811",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            10,
            skip: 3,
            take: 1).ToListAsync();

        Assert.Single(actual);
        Assert.Equal(5, actual[0].Time.ToDateTimeOffset().Hour);
    }
}
