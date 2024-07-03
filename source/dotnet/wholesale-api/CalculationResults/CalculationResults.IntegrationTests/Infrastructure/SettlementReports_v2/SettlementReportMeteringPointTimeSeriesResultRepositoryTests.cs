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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2;

public class SettlementReportMeteringPointTimeSeriesResultRepositoryTests : TestBase<SettlementReportMeteringPointTimeSeriesResultRepository>, IClassFixture<MigrationsFreeDatabricksSqlStatementApiFixture>
{
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _databricksSqlStatementApiFixture;

    public SettlementReportMeteringPointTimeSeriesResultRepositoryTests(MigrationsFreeDatabricksSqlStatementApiFixture databricksSqlStatementApiFixture)
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
        Fixture.Inject<ISettlementReportMeteringPointTimeSeriesResultQueries>(new SettlementReportMeteringPointTimeSeriesResultQueries(
            mockedOptions.Object,
            _databricksSqlStatementApiFixture.GetDatabricksExecutor()));
    }

    [Fact(Skip = "Perf Test")]
    public async Task Count_ValidFilter_ReturnsCount()
    {
        // arrange
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointTimeSeriesViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME,
            [
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000004'", "'exchange'", "'PT15M'", "'404'", "'8442359392711'", "'2022-01-10T03:15:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
            ]);

        // act
        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "404", new CalculationId(Guid.Parse("c50f82a9-8d90-4b44-9387-a51cc059a17a"))
                    },
                },
                DateTimeOffset.Parse("2022-01-10T03:00:00.000+00:00"),
                DateTimeOffset.Parse("2022-01-10T04:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            Resolution.Quarter);

        // assert
        Assert.Equal(1, actual);
    }

    [Fact(Skip = "Perf Test")]
    public async Task Get_SkipTake_ReturnsExpectedRows()
    {
        // arrange
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointTimeSeriesViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME,
            [
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000004'", "'exchange'", "'PT15M'", "'405'", "'8442359392712'", "'2024-01-02T02:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000005'", "'exchange'", "'PT15M'", "'405'", "'8442359392712'", "'2024-01-02T03:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000006'", "'exchange'", "'PT15M'", "'405'", "'8442359392712'", "'2024-01-02T04:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
            ]);

        // act
        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "405", new CalculationId(Guid.Parse("c50f82a9-8d90-4b44-9387-a51cc059a17a"))
                    },
                },
                DateTimeOffset.Parse("2024-01-02T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-03T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            Resolution.Quarter,
            skip: 2,
            take: 1).ToListAsync();

        // assert
        Assert.Single(actual);
        Assert.Equal(4, actual[0].StartDateTime.ToDateTimeOffset().Hour);
    }

    [Theory(Skip = "Perf Test")]
    [InlineData("8442359392717", 1)]
    [InlineData(null, 3)]
    public async Task Get_ValidFilter_FiltersCorrectlyOnEnergySupplier(string? energySupplier, int expected)
    {
        // arrange
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointTimeSeriesViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME,
            [
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000004'", "'exchange'", "'PT15M'", "'406'", "'8442359392714'", "'2024-01-02T02:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000005'", "'exchange'", "'PT15M'", "'406'", "'8442359392715'", "'2024-01-02T03:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000006'", "'exchange'", "'PT15M'", "'406'", "'8442359392716'", "'2024-01-02T04:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
            ]);

        if (energySupplier is not null)
        {
            await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointTimeSeriesViewColumns>(
                _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME,
                [
                    ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000007'", "'exchange'", "'PT15M'", "'406'", $"'{energySupplier}'", "'2024-01-02T04:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
                ]);
        }

        // act
        var actual = await Sut.GetAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "406", new CalculationId(Guid.Parse("c50f82a9-8d90-4b44-9387-a51cc059a17a"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                energySupplier,
                "da-DK"),
            Resolution.Quarter,
            skip: 0,
            take: int.MaxValue).ToListAsync();

        // assert
        Assert.Equal(expected, actual.Count);
    }

    [Theory(Skip = "Perf Test")]
    [InlineData("8442359392721", 1)]
    [InlineData(null, 3)]
    public async Task Count_ValidFilter_FiltersCorrectlyOnEnergySupplier(string? energySupplier, int expected)
    {
        // arrange
        await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointTimeSeriesViewColumns>(
            _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME,
            [
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000004'", "'exchange'", "'PT15M'", "'407'", "'8442359392718'", "'2024-01-02T02:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000005'", "'exchange'", "'PT15M'", "'407'", "'8442359392719'", "'2024-01-02T03:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
                ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000006'", "'exchange'", "'PT15M'", "'407'", "'8442359392720'", "'2024-01-02T04:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
            ]);

        if (energySupplier is not null)
        {
            await _databricksSqlStatementApiFixture.DatabricksSchemaManager.InsertAsync<SettlementReportMeteringPointTimeSeriesViewColumns>(
                _databricksSqlStatementApiFixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME,
                [
                    ["'c50f82a9-8d90-4b44-9387-a51cc059a17a'", "'400000000000000007'", "'exchange'", "'PT15M'", "'407'", $"'{energySupplier}'", "'2024-01-02T04:00:00.000+00:00'", "ARRAY(STRUCT('2023-01-01 12:00:00' AS observation_time, 123.45 AS quantity), STRUCT('2023-01-02 13:00:00' AS observation_time, 678.90 AS quantity))"],
                ]);
        }

        // act
        var actual = await Sut.CountAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "407", new CalculationId(Guid.Parse("c50f82a9-8d90-4b44-9387-a51cc059a17a"))
                    },
                },
                DateTimeOffset.Parse("2024-01-01T00:00:00.000+00:00"),
                DateTimeOffset.Parse("2024-01-04T00:00:00.000+00:00"),
                CalculationType.WholesaleFixing,
                energySupplier,
                "da-DK"),
            Resolution.Quarter);

        // assert
        Assert.Equal(expected, actual);
    }
}
