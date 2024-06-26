﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2;

public class SettlementReportMonthlyAmountRepositoryTests : TestBase<SettlementReportMonthlyAmountRepository>, IClassFixture<MigrationsFreeDatabricksSqlStatementApiFixture>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportMonthlyAmountRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;

        var mockedOptions = new Mock<IOptions<DeltaTableOptions>>();
        mockedOptions.Setup(x => x.Value).Returns(new DeltaTableOptions
        {
            SettlementReportSchemaName = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME,
            SCHEMA_NAME = _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME,
        });

        Fixture.Inject(mockedOptions);
        Fixture.Inject(_databricksSqlStatementApiFixture.GetDatabricksExecutor());
        Fixture.Inject<ISettlementReportMonthlyAmountQueries>(new SettlementReportMonthlyAmountQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor()));
    }

    [Fact]
    public async Task Count_ValidFilterNoEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd2-1da0c40a26de'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'kWh'", "'DKK'", "18.012345", "'tariff'", "'123'", "8397670583197" ],
                ["'f8af5e30-3c65-439e-8fd2-1da0c40a26de'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "'subscription'", "'122'", "8397670583197" ],
                ["'f8af5e30-3c65-439e-8fd2-1da0c40a26de'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "NULL", "NULL", "NULL" ],
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
            new SettlementReportRequestInputActorInfo(MarketRole.EnergySupplier, null));

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_ValidFilterWithEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'kWh'", "'DKK'", "18.012345", "'tariff'", "'123'", "8397670583197" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583192", "'2024-01-02T04:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "'subscription'", "'122'", "8397670583197" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583196", "'2024-01-02T04:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "NULL", "NULL", "NULL" ],
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
            new SettlementReportRequestInputActorInfo(MarketRole.EnergySupplier, null));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMonthlyAmountViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e1'", "'405'", "8397670583196", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'kWh'", "'DKK'", "18.012345", "'tariff'", "'123'", "8397670583197" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e2'", "'405'", "8397670583192", "'2024-01-02T04:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "'subscription'", "'122'", "8397670583197" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e3'", "'405'", "8397670583195", "'2024-01-02T06:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "'subscription'", "'122'", "8397670583197" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e4'", "'405'", "8397670583195", "'2024-01-02T07:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "null", "null", "null" ],
                ["'f8af5e30-3c65-439e-8fd4-1da0c40a26d4'", "'first_correction_settlement'", "'15cba911-b91e-4782-bed4-f0d2841829e4'", "'405'", "8397670583195", "'2024-01-02T07:00:00.000+00:00'", "'P1D'", "'pcs'", "'DKK'", "18.012346", "'subscription'", "'122'", "8397670583197" ],
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
            new SettlementReportRequestInputActorInfo(MarketRole.EnergySupplier, null),
            skip: 3,
            take: 1).ToListAsync();

        Assert.Single(results);
        Assert.Equal(7, results[0].StartDateTime.ToDateTimeOffset().Hour);
        Assert.Equal("8397670583195", results[0].EnergySupplierId);
    }
}
