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
public class SettlementReportMonthlyAmountRepositoryTests : TestBase<SettlementReportMonthlyAmountRepository>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportMonthlyAmountRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
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
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd2-1da0c40a26de'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'kWh'", "18.012345", "'tariff'", "'123'", "8397670583197", "0" ],
                ["'f8af5e30-3c65-439e-8fd2-1da0c40a26de'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
                ["'f8af5e30-3c65-439e-8fd2-1da0c40a26de'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "NULL", "NULL", "NULL", "NULL" ],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd2-1da0c40a26de"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.FirstCorrectionSettlement,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null));

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_ValidFilterWithEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'kWh'", "18.012345", "'tariff'", "'123'", "8397670583197", "0" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583192", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "NULL", "NULL", "NULL", "NULL" ],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd4-1da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.FirstCorrectionSettlement,
                "8397670583196",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Count_ValidFilterAsGridAccessProviderWithoutEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8bf5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'kWh'", "18.012345", "'tariff'", "'123'", "8397670583197", "0" ],
                ["'f8bf5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583192", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "1" ],
                ["'f8bf5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "1" ],
                ["'f8bf5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e4'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "NULL", "NULL", "NULL", "NULL" ],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8bf5e30-3c65-439e-8fd4-1da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.FirstCorrectionSettlement,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, "8397670583197"));

        Assert.Equal(3, actual);
    }

    [Fact]
    public async Task Count_ValidFilterAsGridAccessProviderWithEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f9af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583192", "'2024-01-02T02:00:00.000+00:00'", "'kWh'", "18.012345", "'tariff'", "'123'", "8397670583197", "0" ],
                ["'f9af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583192", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "1" ],
                ["'f9af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "NULL", "NULL", "NULL", "NULL" ],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f9af5e30-3c65-439e-8fd4-1da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.FirstCorrectionSettlement,
                "8397670583192",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, "8397670583197"));

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_ValidFilterAsSystemOperatorWithoutEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f2bf5e31-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'kWh'", "18.012345", "'tariff'", "'123'", "8397670583197", "1" ],
                ["'f2bf5e31-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583192", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
                ["'f2bf5e31-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
                ["'f2bf5e31-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e4'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "NULL", "NULL", "NULL", "NULL" ],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f2bf5e31-3c65-439e-8fd4-1da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.FirstCorrectionSettlement,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.SystemOperator, "8397670583197"));

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_ValidFilterAsSystemOperatorWithEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f7bf5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'kWh'", "18.012345", "'tariff'", "'123'", "8397670583197", "1" ],
                ["'f7bf5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583192", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
                ["'f7bf5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
                ["'f7bf5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e4'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "NULL", "NULL", "NULL", "NULL" ],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f7bf5e30-3c65-439e-8fd4-1da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.FirstCorrectionSettlement,
                "8397670583196",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, "8397670583197"));

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'kWh'", "18.012345", "'tariff'", "'123'", "8397670583197", "0" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583192", "'2024-01-02T04:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583195", "'2024-01-02T06:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e4'", "'405'", "8397670583195", "'2024-01-02T07:00:00.000+00:00'", "'pcs'", "18.012346", "null", "null", "null", "null" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e5'", "'405'", "8397670583195", "'2024-01-02T07:00:00.000+00:00'", "'pcs'", "18.012346", "'subscription'", "'122'", "8397670583197", "0" ],
            ]);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>()
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd4-1da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-02-04T00:00:00.000+00:00"),
                CalculationType.FirstCorrectionSettlement,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null),
            skip: 3,
            take: 1).ToListAsync();

        Assert.Single(results);
        Assert.Equal(7, results[0].StartDateTime.ToDateTimeOffset().Hour);
        Assert.Equal("8397670583195", results[0].EnergySupplierId);
    }
}
