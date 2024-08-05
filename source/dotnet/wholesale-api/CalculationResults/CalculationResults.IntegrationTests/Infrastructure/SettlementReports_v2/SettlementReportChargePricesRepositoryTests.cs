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
public class SettlementReportChargePricesRepositoryTests : TestBase<SettlementReportChargePriceRepository>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    private readonly string[][] _chargePriceRows =
    [
        ["'08d5d29f-ceac-4ab5-a1d4-98db8479fa6c'", "'wholesale_fixing'", "'66'", "'tariff'", "'40000'", "'5790001330552'", "'PT1H'", "'false'", "'2023-01-31T23:00:00.000'", "ARRAY(STRUCT('2023-01-31T23:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T00:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T01:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T02:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T03:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T04:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T05:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T06:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T07:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T08:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T09:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T10:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T11:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T12:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T13:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T14:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T15:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T16:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T17:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T18:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T19:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T20:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T21:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-01T22:00:00.000Z' AS time,0.756998 AS price))", "'804'", "'5790001662233'"],
        ["'08d5d29f-ceac-4ab5-a1d4-98db8479fa6c'", "'wholesale_fixing'", "'66'", "'tariff'", "'40000'", "'5790001330552'", "'PT1H'", "'false'", "'2023-02-01T23:00:00.000'", "ARRAY(STRUCT('2023-02-01T23:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T00:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T01:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T02:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T03:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T04:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T05:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T06:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T07:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T08:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T09:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T10:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T11:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T12:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T13:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T14:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T15:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T16:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T17:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T18:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T19:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T20:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T21:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-02T22:00:00.000Z' AS time,0.756998 AS price))", "'804'", "'5790001662233'"],
        ["'08d5d29f-ceac-4ab5-a1d4-98db8479fa6c'", "'wholesale_fixing'", "'66'", "'tariff'", "'40000'", "'5790001330552'", "'PT1H'", "'false'", "'2023-02-02T23:00:00.000'", "ARRAY(STRUCT('2023-02-02T23:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T00:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T01:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T02:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T03:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T04:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T05:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T06:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T07:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T08:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T09:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T10:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T11:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T12:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T13:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T14:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T15:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T16:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T17:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T18:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T19:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T20:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T21:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-03T22:00:00.000Z' AS time,0.756998 AS price))", "'805'", "'5790001662233'"],
        ["'08d5d29f-ceac-4ab5-a1d4-98db8479fa6c'", "'wholesale_fixing'", "'66'", "'tariff'", "'40000'", "'5790001330552'", "'PT1H'", "'false'", "'2023-02-03T23:00:00.000'", "ARRAY(STRUCT('2023-02-03T23:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T00:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T01:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T02:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T03:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T04:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T05:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T06:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T07:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T08:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T09:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T10:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T11:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T12:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T13:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T14:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T15:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T16:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T17:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T18:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T19:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T20:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T21:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T22:00:00.000Z' AS time,0.756998 AS price))", "'805'", "'5790001662233'"],
        ["'08d5d29f-ceac-4ab5-a1d4-98db8479fa6c'", "'wholesale_fixing'", "'66'", "'tariff'", "'40000'", "'5790001330552'", "'PT1H'", "'true'", "'2023-02-05T23:00:00.000'", "ARRAY(STRUCT('2023-02-03T23:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T00:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T01:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T02:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T03:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T04:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T05:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T06:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T07:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T08:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T09:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T10:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T11:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T12:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T13:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T14:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T15:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T16:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T17:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T18:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T19:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T20:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T21:00:00.000Z' AS time,0.756998 AS price),STRUCT('2023-02-04T22:00:00.000Z' AS time,0.756998 AS price))", "'805'", "'5790001662233'"],
    ];

    public SettlementReportChargePricesRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
    {
        _databricksSqlStatementApiFixture = databricksSqlStatementApiFixture;

        _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SettlementReportSchemaName =
            databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.SCHEMA_NAME;

        Fixture.Inject(_databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(_databricksSqlStatementApiFixture.GetDatabricksExecutor());

        Fixture.Inject<ISettlementReportDatabricksContext>(new SettlementReportDatabricksContext(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor()));
    }

    [Fact]
    public async Task Count_WrongGridArea_ReturnsEmpty()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager
            .EmptyAsync(_databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargePriceViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME,
            _chargePriceRows);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "800", new CalculationId(Guid.Parse("08d5d29f-ceac-4ab5-a1d4-98db8479fa6c"))
                    },
                },
                DateTimeOffset.Parse("2023-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null));

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task Count_ValidFilterGridArea_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager
            .EmptyAsync(_databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargePriceViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME,
            _chargePriceRows);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "804", new CalculationId(Guid.Parse("08d5d29f-ceac-4ab5-a1d4-98db8479fa6c"))
                    },
                },
                DateTimeOffset.Parse("2023-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                "5790001662233",
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null));

        Assert.Equal(2, actual);
    }

    [Fact]
    public async Task Count_ValidFilterGridAreaWithTax_ReturnsCount()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager
            .EmptyAsync(_databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargePriceViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME,
            _chargePriceRows);

        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "805", new CalculationId(Guid.Parse("08d5d29f-ceac-4ab5-a1d4-98db8479fa6c"))
                    },
                },
                DateTimeOffset.Parse("2023-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T02:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, "5790001330552"));

        Assert.Equal(3, actual);
    }

    [Fact]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager
            .EmptyAsync(_databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargePriceViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME,
            _chargePriceRows);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>()
                {
                    {
                        "805", new CalculationId(Guid.Parse("08d5d29f-ceac-4ab5-a1d4-98db8479fa6c"))
                    },
                },
                DateTimeOffset.Parse("2023-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-02-04T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null),
            skip: 1,
            take: 2).ToListAsync();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task Get_TakeMoreThanAvailable_ReturnsFewerRows()
    {
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager
            .EmptyAsync(_databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME);

        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportChargePriceViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.CHARGE_PRICES_V1_VIEW_NAME,
            _chargePriceRows);

        var results = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>()
                {
                    {
                        "804", new CalculationId(Guid.Parse("08d5d29f-ceac-4ab5-a1d4-98db8479fa6c"))
                    },
                },
                DateTimeOffset.Parse("2023-01-01T02:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-02-04T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null),
            skip: 1,
            take: 2).ToListAsync();

        Assert.Single(results);
    }
}
