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
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2;

public class SettlementReportMeteringPointMasterDataRepositoryTests : TestBase<SettlementReportMeteringPointMasterDataRepository>, IClassFixture<MigrationsFreeDatabricksSqlStatementApiFixture>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportMeteringPointMasterDataRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
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
        var calculationsClientMock = new Mock<ICalculationsClient>();
        calculationsClientMock.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(new CalculationDto(null, Guid.Empty, DateTimeOffset.Now, DateTimeOffset.Now, "a", "b", DateTimeOffset.Now, DateTimeOffset.Now, CalculationState.Completed, true, [], CalculationType.Aggregation, Guid.Empty, 1, CalculationOrchestrationState.Calculated));
        Fixture.Inject<ISettlementReportMeteringPointMasterDataQueries>(new SettlementReportMeteringPointMasterDataQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor(),
            calculationsClientMock.Object));
    }

    [Fact]
    public async Task Count_ValidFilterNoEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d2'", "'WholesaleFixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8297670583196'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d2'", "'WholesaleFixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8497670583196'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d2"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"));

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_ValidFilterWithEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8597670583196'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "'8697670583197'"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "8597670583196",
                "da-DK"));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Count_ValidFilterWithNoEnergySupplierInFilterAndAlsoNullInData_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d4'", "'WholesaleFixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d4'", "'WholesaleFixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"));

        actual.Should().Be(2);
    }

    [Fact]
    public async Task Count_ValidFilterWithEnergySupplierInFilterButNullInData_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointMasterDataViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.METERING_POINT_MASTER_DATA_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d5'", "'WholesaleFixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d5'", "'WholesaleFixing'", "'15cba911-b91e-4782-bed4-f0d2841829ec'", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'406'", "'407'", "'consumption'", "'flex'", "null"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d5"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "8397670583196",
                "da-DK"));

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_LINK_PERIODS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'consumption'", "'tariff'", "'40000'", "'6392825108998'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-31T02:00:00.000+00:00'", "'404'", "'8397670583196'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'consumption'", "'tariff'", "'40000'", "'6392825108999'", "46", "'2024-01-02T03:00:00.000+00:00'", "'2024-01-31T03:00:00.000+00:00'", "'404'", "'8397670583196'"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'WholesaleFixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ed'", "'consumption'", "'tariff'", "'40000'", "'6392825108910'", "46", "'2024-01-02T04:00:00.000+00:00'", "'2024-01-31T04:00:00.000+00:00'", "'404'", "'8397670583196'"],
            ]);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId>()
                {
                    {
                        "404", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-02-04T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            skip: 2,
            take: 1).ToListAsync();

        Assert.Single(results);
        Assert.Equal(4, results[0].PeriodStart.ToDateTimeOffset().Hour);
    }
}
