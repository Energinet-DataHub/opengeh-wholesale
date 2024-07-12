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
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2;

[Collection(nameof(SettlementReportCollectionFixture))]
public class SettlementReportMeteringPointMasterDataRepositoryTests : TestBase<SettlementReportMeteringPointMasterDataRepository>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportMeteringPointMasterDataRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;

        _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SettlementReportSchemaName =
            databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME;

        Fixture.Inject<ISettlementReportDatabricksContext>(new SettlementReportDatabricksContext(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor()));
    }

    [Fact]
    public async Task Count_ValidFilterNoEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d2'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d2'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            int.MaxValue);

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_ValidFilterWithEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8597670583196'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8697670583197'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "8597670583196",
                "da-DK"),
            int.MaxValue);

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Count_ValidFilterWithNoEnergySupplierInFilterAndAlsoNullInData_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            int.MaxValue);

        actual.Should().Be(2);
    }

    [Fact]
    public async Task Count_ValidFilterWithEnergySupplierInFilterButNullInData_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d5'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d5'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d5"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "8397670583196",
                "da-DK"),
            int.MaxValue);

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task Count_LatestValidFilterNoEnergySupplier_ReturnsCount()
    {
        var calculationId1 = Guid.NewGuid().ToString();
        var calculationId2 = Guid.NewGuid().ToString();
        var calculationId3 = Guid.NewGuid().ToString();
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2019-01-02T02:00:00.000+00:00'", "'2019-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'7297670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2019-01-02T02:00:00.000+00:00'", "'2019-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'7497670583197'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2019-01-02T02:00:00.000+00:00'", "'2019-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'7497670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ef'", "'2019-01-02T02:00:00.000+00:00'", "'2019-01-13T02:00:00.000+00:00'", "'410'", "'406'", "'407'", "'consumption'", "'flex'", "'7497670583196'"],
                [$"'{calculationId3}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ee'", "'2019-01-02T02:00:00.000+00:00'", "'2019-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'7497670583196'"],
            ]);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2019-01-10T03:15:00.000+00:00'", "26.634"],
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2019-01-11T03:15:00.000+00:00'", "26.634"],
                [$"'{calculationId2}'", "'balance_fixing'", "'2'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2019-01-11T03:15:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2019-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2019-01-15T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                null,
                "da-DK"),
            int.MaxValue);

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_LatestValidFilterWithEnergySupplier_ReturnsCount()
    {
        var calculationId1 = "a51a233f-3c67-4103-a5d5-49c5e177b8cc"; // Guid.NewGuid().ToString();
        var calculationId2 = "9d26bcb0-4e8c-47f3-95b9-6c24aaf40f8c"; // Guid.NewGuid().ToString();
        var calculationId3 = "ca2a7adb-89ff-4412-b161-7280586540ab"; // Guid.NewGuid().ToString();
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ea'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583197'"],
                [$"'{calculationId1}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583197'"],
                [$"'{calculationId3}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ed'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583197'"],
                [$"'{calculationId3}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ee'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'410'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583197'"],
            ]);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "8297670583196"],
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "8297670583197"],
                [$"'{calculationId2}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T03:15:00.000+00:00'", "26.634", "8297670583197"],
                [$"'{calculationId3}'", "'balance_fixing'", "'2'", "'47433af6-03c1-46bd-ab9b-dd0497035308'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T03:15:00.000+00:00'", "26.634", "8297670583197"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-17T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                "8297670583197",
                "da-DK"),
            int.MaxValue);

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Get_LatestValidFilterNoEnergySupplier_ReturnsCorrectRows()
    {
        var calculationId1 = Guid.NewGuid().ToString();
        var calculationId2 = Guid.NewGuid().ToString();
        var calculationId3 = Guid.NewGuid().ToString();
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2020-01-02T02:00:00.000+00:00'", "'2020-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2020-01-02T02:00:00.000+00:00'", "'2020-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
                [$"'{calculationId3}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ed'", "'2020-01-02T02:00:00.000+00:00'", "'2020-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
            ]);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2020-01-10T03:15:00.000+00:00'", "26.634"],
                [$"'{calculationId2}'", "'balance_fixing'", "'2'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2020-01-10T05:15:00.000+00:00'", "26.634"],
                [$"'{calculationId3}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2020-01-11T04:15:00.000+00:00'", "26.634"],
            ]);

        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2020-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2020-01-15T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                null,
                "da-DK"),
            0,
            5,
            int.MaxValue).ToListAsync();

        Assert.Equal(2, actual.Count);
        Assert.Equal("15cba911-b91e-4782-bed4-f0d2841829ec", actual.First().MeteringPointId);
        Assert.Equal("15cba911-b91e-4782-bed4-f0d2841829ed", actual.Last().MeteringPointId);
    }

    [Fact]
    public async Task Get_LatestValidFilterNoEnergySupplier_SkipTakeReturnsCorrectRows()
    {
        var calculationId1 = Guid.NewGuid().ToString();
        var calculationId2 = Guid.NewGuid().ToString();
        var calculationId3 = Guid.NewGuid().ToString();
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2023-01-02T02:00:00.000+00:00'", "'2023-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2023-01-02T02:00:00.000+00:00'", "'2023-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583197'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2023-01-02T02:00:00.000+00:00'", "'2023-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ee'", "'2023-01-02T02:00:00.000+00:00'", "'2023-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ef'", "'2023-01-02T02:00:00.000+00:00'", "'2023-01-13T02:00:00.000+00:00'", "'410'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
                [$"'{calculationId3}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2023-01-02T02:00:00.000+00:00'", "'2023-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
            ]);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2023-01-10T03:15:00.000+00:00'", "26.634"],
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2023-01-11T03:15:00.000+00:00'", "26.634"],
                [$"'{calculationId2}'", "'balance_fixing'", "'2'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2023-01-11T03:15:00.000+00:00'", "26.634"],
            ]);

        var count = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2023-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2023-01-15T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                null,
                "da-DK"),
            int.MaxValue);

        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2023-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2023-01-15T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                null,
                "da-DK"),
            1,
            1,
            int.MaxValue).ToListAsync();

        var actual2 = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2023-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2023-01-15T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                null,
                "da-DK"),
            2,
            1,
            int.MaxValue).ToListAsync();

        Assert.Equal(3, count);
        Assert.Single(actual);
        Assert.Single(actual2);
        Assert.Equal("15cba911-b91e-4782-bed4-f0d2841829ec", actual.First().MeteringPointId);
        Assert.Equal("15cba911-b91e-4782-bed4-f0d2841829ee", actual2.First().MeteringPointId);
    }

    [Fact]
    public async Task Get_LatestValidFilterWithEnergySupplier_SkipTakeReturnsCorrectRows()
    {
        var calculationId1 = Guid.NewGuid().ToString();
        var calculationId2 = Guid.NewGuid().ToString();
        var calculationId3 = Guid.NewGuid().ToString();
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
                [$"'{calculationId1}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ef'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583198'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583197'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ee'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
                [$"'{calculationId2}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829ef'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'410'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
                [$"'{calculationId3}'", "'balance_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2022-01-02T02:00:00.000+00:00'", "'2022-01-13T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
            ]);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportEnergyResultPerEnergySupplierViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME,
            [
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "8297670583196"],
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035305'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-10T03:15:00.000+00:00'", "26.634", "8297670583198"],
                [$"'{calculationId1}'", "'balance_fixing'", "'1'", "'47433af6-03c1-46bd-ab9b-dd0497035306'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T03:15:00.000+00:00'", "26.634", "8297670583196"],
                [$"'{calculationId2}'", "'balance_fixing'", "'2'", "'47433af6-03c1-46bd-ab9b-dd0497035307'", "'405'", "'consumption'", "'non_profiled'", "'PT15M'", "'2022-01-11T03:15:00.000+00:00'", "26.634", "8297670583196"],
            ]);

        var count = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-15T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                "8297670583196",
                "da-DK"),
            int.MaxValue);

        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-15T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                "8297670583196",
                "da-DK"),
            0,
            1,
            int.MaxValue).ToListAsync();

        var actual2 = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2022-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-15T02:00:00.000+00:00"),
                CalculationType.BalanceFixing,
                "8297670583196",
                "da-DK"),
            1,
            1,
            int.MaxValue).ToListAsync();

        Assert.Equal(3, count);
        Assert.Single(actual);
        Assert.Single(actual2);
        Assert.Equal("15cba911-b91e-4782-bed4-f0d2841829eb", actual.First().MeteringPointId);
        Assert.Equal("15cba911-b91e-4782-bed4-f0d2841829ec", actual2.First().MeteringPointId);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26de'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'2024-01-02T04:00:00.000+00:00'", "'2024-01-03T04:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
            ]);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>()
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26de"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-02-04T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            skip: 2,
            take: 1,
            int.MaxValue).ToListAsync();

        Assert.Single(results);
        Assert.Equal(4, results[0].PeriodStart.ToDateTimeOffset().Hour);
        Assert.Equal(4, results[0].PeriodEnd!.Value.ToDateTimeOffset().Hour);
    }
}
