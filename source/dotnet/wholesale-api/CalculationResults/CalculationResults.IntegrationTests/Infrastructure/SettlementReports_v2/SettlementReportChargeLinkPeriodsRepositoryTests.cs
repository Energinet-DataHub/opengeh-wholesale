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
public class SettlementReportChargeLinkPeriodsRepositoryTests : TestBase<SettlementReportChargeLinkPeriodsRepository>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportChargeLinkPeriodsRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
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
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_LINK_PERIODS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'consumption'", "'tariff'", "'40000'", "'6392825108998'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'8397670583196'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ec'", "'consumption'", "'tariff'", "'40000'", "'6392825108998'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'8397670583191'", "0"],
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
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null));

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_ValidFilterWithEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_LINK_PERIODS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'consumption'", "'tariff'", "'40000'", "'6392825108998'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'403'", "'8397670583196'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9e2'", "'consumption'", "'tariff'", "'40000'", "'6392825108998'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'403'", "'8397670583191'", "0"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "403", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "8397670583191",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Count_ValidFilterGridAccessProviderNoEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_LINK_PERIODS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'consumption'", "'tariff'", "'40000'", "'8397670583196'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108998'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ec'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108988'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ed'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108988'", "1"],
                ["'f8af5e30-3c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ee'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108987'", "1"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-2da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, "8397670583191"));

        Assert.Equal(3, actual);
    }

    [Fact]
    public async Task Count_ValidFilterSystemOperatorNoEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_LINK_PERIODS_V1_VIEW_NAME,
            [
                ["'f8af6d30-3c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'consumption'", "'tariff'", "'40000'", "'8397670583196'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108998'", "0"],
                ["'f8af6d30-3c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ec'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108988'", "1"],
                ["'f8af6d30-3c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ed'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108988'", "0"],
                ["'f8af6d30-3c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ee'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108987'", "1"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af6d30-3c65-439e-8fd0-2da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.SystemOperator, "8397670583191"));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Count_ValidFilterSystemOperatorWithEnergySupplier_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_LINK_PERIODS_V1_VIEW_NAME,
            [
                ["'f8af5e21-4c65-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4782-bed4-f0d2841829eb'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108998'", "0"],
                ["'f8af5e21-4c66-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ec'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108988'", "0"],
                ["'f8af5e21-4c66-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ed'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108998'", "1"],
                ["'f8af5e21-4c66-439e-8fd0-2da0c40a26d4'", "'wholesale_fixing'", "'15cba911-b91e-4784-bed4-f0d2841839ee'", "'consumption'", "'tariff'", "'40000'", "'8397670583191'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-03T02:00:00.000+00:00'", "'405'", "'6392825108988'", "1"],
            ]);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e21-4c66-439e-8fd0-2da0c40a26d4"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "6392825108988",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.SystemOperator, "8397670583191"));

        Assert.Equal(1, actual);
    }

    [Theory]
    [InlineData("f8af5e30-3c65-439e-8fd0-1da0c40a26d3", "2024-01-01T00:00:00.000+00:00", "2024-01-04T00:00:00.000+00:00", 1, new[] { "15cba911-b91e-4786-bed4-f0d28418a9eb" })]
    [InlineData("f8af5e30-3c65-439e-8fd0-2da0c40a26d3", "2024-01-01T00:00:00.000+00:00", "2024-02-04T00:00:00.000+00:00", 2, new[] { "15cba911-b91e-4786-bed4-f0d28418a9eb", "16cba911-b91e-4786-bed4-f0d28418a9ec" })]
    [InlineData("f8af5e30-3c65-439e-8fd0-3da0c40a26d3", "2024-01-01T00:00:00.000+00:00", "2024-03-04T00:00:00.000+00:00", 3, new[] { "15cba911-b91e-4786-bed4-f0d28418a9eb", "16cba911-b91e-4786-bed4-f0d28418a9ec", "17cba911-b91e-4786-bed4-f0d28418a9ed" })]
    [InlineData("f8af5e30-3c65-439e-8fd0-4da0c40a26d3", "2024-02-01T00:00:00.000+00:00", "2024-02-14T00:00:00.000+00:00", 1, new[] { "16cba911-b91e-4786-bed4-f0d28418a9ec" })]
    [InlineData("f8af5e30-3c65-439e-8fd0-5da0c40a26d3", "2024-02-05T00:00:00.000+00:00", "2024-02-14T00:00:00.000+00:00", 1, new[] { "16cba911-b91e-4786-bed4-f0d28418a9ec" })]
    [InlineData("f8af5e30-3c65-439e-8fd0-6da0c40a26d3", "2024-02-05T00:00:00.000+00:00", "2024-03-01T00:00:00.000+00:00", 1, new[] { "16cba911-b91e-4786-bed4-f0d28418a9ec" })]
    [InlineData("f8af5e30-3c65-439e-8fd0-7da0c40a26d3", "2024-02-05T00:00:00.000+00:00", "2024-03-16T00:00:00.000+00:00", 2, new[] { "16cba911-b91e-4786-bed4-f0d28418a9ec", "17cba911-b91e-4786-bed4-f0d28418a9ed" })]
    public async Task Get_OverlappingDateWithFilter_ReturnsExpectedRows(string calcId, string startDate, string endDate, int returnCount, string[] expectedMeteringPointIds)
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_LINK_PERIODS_V1_VIEW_NAME,
            [
                [$"'{calcId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'consumption'", "'tariff'", "'40000'", "'6392825108998'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-31T02:00:00.000+00:00'", "'404'", "'8397670583196'", "0"],
                [$"'{calcId}'", "'wholesale_fixing'", "'16cba911-b91e-4786-bed4-f0d28418a9ec'", "'consumption'", "'tariff'", "'40000'", "'6392825108999'", "46", "'2024-02-02T03:00:00.000+00:00'", "'2024-02-28T03:00:00.000+00:00'", "'404'", "'8397670583196'", "0"],
                [$"'{calcId}'", "'wholesale_fixing'", "'17cba911-b91e-4786-bed4-f0d28418a9ed'", "'consumption'", "'tariff'", "'40000'", "'6392825108910'", "46", "'2024-03-02T04:00:00.000+00:00'", "'2024-03-31T04:00:00.000+00:00'", "'404'", "'8397670583196'", "0"],
            ]);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>()
                {
                    {
                        "404", new CalculationId(Guid.Parse(calcId))
                    },
                },
                DateTimeOffset.Parse(startDate),
                DateTimeOffset.Parse(endDate),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            skip: 0,
            take: int.MaxValue).ToListAsync();

        Assert.Equal(returnCount, results.Count);
        expectedMeteringPointIds.Should().Equal(results.Select(x => x.MeteringPointId).ToList());
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargeLinkPeriodsViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_LINK_PERIODS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'consumption'", "'tariff'", "'40000'", "'6392825108998'", "46", "'2024-01-02T02:00:00.000+00:00'", "'2024-01-31T02:00:00.000+00:00'", "'404'", "'8397670583196'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'consumption'", "'tariff'", "'40000'", "'6392825108999'", "46", "'2024-01-02T03:00:00.000+00:00'", "'2024-01-31T03:00:00.000+00:00'", "'404'", "'8397670583196'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ed'", "'consumption'", "'tariff'", "'40000'", "'6392825108910'", "46", "'2024-01-02T04:00:00.000+00:00'", "'2024-01-31T04:00:00.000+00:00'", "'404'", "'8397670583196'", "0"],
            ]);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>()
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
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            skip: 2,
            take: 1).ToListAsync();

        Assert.Single(results);
        Assert.Equal(4, results[0].PeriodStart.ToDateTimeOffset().Hour);
    }
}
