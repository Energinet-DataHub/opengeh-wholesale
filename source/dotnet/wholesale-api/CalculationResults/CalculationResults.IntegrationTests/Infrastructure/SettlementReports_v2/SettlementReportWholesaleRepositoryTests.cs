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
public class SettlementReportWholesaleRepositoryTests : TestBase<SettlementReportWholesaleRepository>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportWholesaleRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;

        _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SettlementReportSchemaName =
            databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME;

        Fixture.Inject<ISettlementReportDatabricksContext>(new SettlementReportDatabricksContext(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor()));
    }

    [Fact]
    public async Task Count_ValidFilter_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'403'", "'8397670583196'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
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
                DateTimeOffset.Parse("2024-01-03T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null));

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'404'", "'8397670583196'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ea'", "'404'", "'8397670583196'", "'2024-01-02T03:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'404'", "'8397670583196'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
            ]);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>()
                {
                    {
                        "404", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-02T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-03T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            skip: 2,
            take: 1).ToListAsync();

        Assert.Single(results);
        Assert.Equal(4, results[0].StartDateTime.ToDateTimeOffset().Hour);
    }

    [Fact]
    public async Task Get_NullableValues_ReturnsNull()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            [
                ["'f9af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'404'", "'8397670583196'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "NULL", "'kWh'", "'DKK'", "NULL", "NULL", "NULL", "'tariff'", "'40000'", "'6392825108998'", "0"],
                ["'f9af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ea'", "'404'", "'8397670583196'", "'2024-01-02T03:00:00.000+00:00'", "'PT1H'", "'consumption'", "NULL", "'kWh'", "'DKK'", "NULL", "NULL", "NULL", "'tariff'", "'40000'", "'6392825108998'", "0"],
                ["'f9af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'404'", "'8397670583196'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "NULL", "'kWh'", "'DKK'", "NULL", "NULL", "NULL", "'tariff'", "'40000'", "'6392825108998'", "0"],
            ]);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>()
                {
                    {
                        "404", new CalculationId(Guid.Parse("f9af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-02T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-03T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            skip: 2,
            take: 1).ToListAsync();

        Assert.Single(results);
        Assert.Equal(4, results[0].StartDateTime.ToDateTimeOffset().Hour);
        Assert.Null(results[0].SettlementMethod);
        Assert.Null(results[0].Amount);
        Assert.Null(results[0].Price);
        Assert.Null(results[0].Quantity);
    }

    [Theory]
    [InlineData("8397670583199", 1)]
    [InlineData(null, 3)]
    public async Task Get_ValidFilter_FiltersCorrectlyOnEnergySupplier(string? energySupplier, int expected)
    {
        // arrange
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            [
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'405'", "'8397670583196'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ea'", "'405'", "'8397670583197'", "'2024-01-02T03:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
                ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'405'", "'8397670583198'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
            ]);

        if (energySupplier is not null)
        {
            await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
                _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
                [
                    ["'f8af5e30-3c65-439e-8fd0-1da0c40a26d3'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'405'", "'8397670583199'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
                ]);
        }

        // act
        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("f8af5e30-3c65-439e-8fd0-1da0c40a26d3"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                energySupplier,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.DataHubAdministrator, null),
            skip: 0,
            take: int.MaxValue).ToListAsync();

        // assert
        Assert.Equal(expected, actual.Count);
    }

    [Theory]
    [InlineData("8397670583104", 1, MarketRole.EnergySupplier, null)]
    [InlineData(null, 3, MarketRole.EnergySupplier, null)]
    [InlineData("8397670583119", 3, MarketRole.GridAccessProvider, "7397670583109")]
    [InlineData(null, 3, MarketRole.GridAccessProvider, "7397670583119")]
    [InlineData("8397670583129", 2, MarketRole.SystemOperator, "7397670583129")]
    [InlineData(null, 1, MarketRole.SystemOperator, "7397670583139")]
    public async Task Count_ValidFilter_FiltersCorrectlyOnEnergySupplier(string? energySupplier, int expected, MarketRole marketRole, string? chargeOwner)
    {
        var calculationId = Guid.NewGuid();
        // arrange
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
            [
                [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9eb'", "'406'", "'8397670583101'", "'2024-01-02T02:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
                [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ea'", "'406'", "'8397670583102'", "'2024-01-02T03:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
                [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'406'", "'8397670583103'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
            ]);

        if (energySupplier is not null)
        {
            await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
                _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
                [
                    [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ec'", "'406'", "'8397670583104'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", "'6392825108998'", "0"],
                ]);
        }

        if (marketRole is MarketRole.SystemOperator or MarketRole.GridAccessProvider)
        {
            await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
                _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
                [
                    [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ed'", "'406'", "'8397670583105'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", $"'{chargeOwner}'", "0"],
                    [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ee'", "'406'", "'8397670583106'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", $"'{chargeOwner}'", "1"],
                    [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9ef'", "'406'", "'8397670583107'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", $"'{chargeOwner}'", "1"],
                ]);

            if (energySupplier is not null)
            {
                await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportWholesaleViewColumns>(
                    _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_V1_VIEW_NAME,
                    [
                        [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9cc'", "'406'", $"'{energySupplier}'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", $"'{chargeOwner}'", "0"],
                        [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9dc'", "'406'", $"'{energySupplier}'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", $"'{chargeOwner}'", "1"],
                        [$"'{calculationId}'", "'wholesale_fixing'", "'15cba911-b91e-4786-bed4-f0d28418a9fc'", "'406'", $"'{energySupplier}'", "'2024-01-02T04:00:00.000+00:00'", "'PT1H'", "'consumption'", "'flex'", "'kWh'", "'DKK'", "46.543", "0.712345", "32.123456", "'tariff'", "'40000'", $"'{chargeOwner}'", "0"],
                    ]);
            }
        }

        // act
        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "406", new CalculationId(calculationId)
                    },
                },
                DateTimeOffset.Parse("2024-01-01T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                energySupplier,
                "da-DK"),
            new SettlementReportRequestedByActor(marketRole, marketRole is MarketRole.SystemOperator or MarketRole.GridAccessProvider ? chargeOwner : null));

        // assert
        Assert.Equal(expected, actual);
    }
}
