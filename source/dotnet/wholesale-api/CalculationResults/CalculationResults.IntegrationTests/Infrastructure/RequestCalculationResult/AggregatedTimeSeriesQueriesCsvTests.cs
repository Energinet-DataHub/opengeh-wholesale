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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public class AggregatedTimeSeriesQueriesCsvTests : TestBase<AggregatedTimeSeriesQueries>,
    IClassFixture<MigrationsFreeDatabricksSqlStatementApiFixture>
{
    private const string EnergySupplierOne = "5790002617263";
    private const string EnergySupplierTwo = "5790000701414";
    private const string EnergySupplierThree = "5790001687137";

    private const string BalanceResponsibleOne = "5790000701414";
    private const string BalanceResponsibleTwo = "5790001964597";

    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _fixture;

    public AggregatedTimeSeriesQueriesCsvTests(MigrationsFreeDatabricksSqlStatementApiFixture fixture)
    {
        Fixture.Inject(fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(fixture.GetDatabricksExecutor());
        _fixture = fixture;
    }

    [Fact]
    public async Task Energy_supplier_across_grid_areas()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 9, 0, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.FlexConsumption, TimeSeriesType.NonProfiledConsumption],
            GridAreaCodes: [],
            EnergySupplierId: EnergySupplierThree,
            BalanceResponsibleId: null,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(18);
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.Version))
            .Should()
            .BeEquivalentTo([
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 1, 23, 0), 7),
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 1, 23, 0), Instant.FromUtc(2022, 1, 3, 23, 0), 8),
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 3, 23, 0), Instant.FromUtc(2022, 1, 5, 23, 0), 8),
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 6),
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 7, 23, 0), 8),
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 7, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 6),
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 1, 23, 0), 7),
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 1, 23, 0), Instant.FromUtc(2022, 1, 3, 23, 0), 8),
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 3, 23, 0), Instant.FromUtc(2022, 1, 5, 23, 0), 8),
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 6),
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 7, 23, 0), 8),
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 7, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 6),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 1, 23, 0), 7),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 1, 23, 0), Instant.FromUtc(2022, 1, 3, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 3, 23, 0), Instant.FromUtc(2022, 1, 5, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 6),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 7, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 7, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 6),
            ]);

        actual.Should().AllSatisfy(ats =>
        {
            ats.TimeSeriesPoints.Should()
                .AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeGreaterOrEqualTo(ats.PeriodStart))
                .And.AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeLessThan(ats.PeriodEnd))
                .And.AllSatisfy(etsp =>
                {
                    etsp.Time.Minute.Should().Be(0);
                    etsp.Time.Second.Should().Be(0);
                })
                .And.HaveCount((int)ats.PeriodEnd.Minus(ats.PeriodStart).TotalHours)
                .And.OnlyHaveUniqueItems(etsp => etsp.Time);
        });
    }

    [Fact]
    public async Task Energy_supplier_in_single_grid_area()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 9, 0, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.FlexConsumption, TimeSeriesType.Production],
            GridAreaCodes: ["804"],
            EnergySupplierId: EnergySupplierOne,
            BalanceResponsibleId: null,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(12);
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.Version))
            .Should()
            .BeEquivalentTo([
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 6),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 7, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 6),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 1, 23, 0), Instant.FromUtc(2022, 1, 3, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 7, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 3, 23, 0), Instant.FromUtc(2022, 1, 5, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 1, 23, 0), 7),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 6),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 7, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 6),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 1, 23, 0), Instant.FromUtc(2022, 1, 3, 23, 0), 8),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 7, 23, 0), 8),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 3, 23, 0), Instant.FromUtc(2022, 1, 5, 23, 0), 8),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 1, 23, 0), 7),
            ]);

        actual.Should().AllSatisfy(ats =>
        {
            ats.TimeSeriesPoints.Should()
                .AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeGreaterOrEqualTo(ats.PeriodStart))
                .And.AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeLessThan(ats.PeriodEnd))
                .And.AllSatisfy(etsp =>
                {
                    etsp.Time.Minute.Should().Be(0);
                    etsp.Time.Second.Should().Be(0);
                })
                .And.HaveCount((int)ats.PeriodEnd.Minus(ats.PeriodStart).TotalHours)
                .And.OnlyHaveUniqueItems(etsp => etsp.Time);
        });
    }

    [Fact]
    public async Task Energy_supplier_and_balance_responsible()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 9, 0, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.FlexConsumption, TimeSeriesType.Production],
            GridAreaCodes: ["804"],
            EnergySupplierId: EnergySupplierOne,
            BalanceResponsibleId: BalanceResponsibleOne,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(6);
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.Version))
            .Should()
            .BeEquivalentTo([
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 6),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 7, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 6),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 1, 23, 0), Instant.FromUtc(2022, 1, 3, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 7, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 3, 23, 0), Instant.FromUtc(2022, 1, 5, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 1, 23, 0), 7),
            ]);

        actual.Should().AllSatisfy(ats =>
        {
            ats.TimeSeriesPoints.Should()
                .AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeGreaterOrEqualTo(ats.PeriodStart))
                .And.AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeLessThan(ats.PeriodEnd))
                .And.AllSatisfy(etsp =>
                {
                    etsp.Time.Minute.Should().Be(0);
                    etsp.Time.Second.Should().Be(0);
                })
                .And.HaveCount((int)ats.PeriodEnd.Minus(ats.PeriodStart).TotalHours)
                .And.OnlyHaveUniqueItems(etsp => etsp.Time);
        });
    }

    [Fact]
    public async Task Balance_responsible_without_energy_supplier_and_grid_area()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 9, 0, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.FlexConsumption],
            GridAreaCodes: ["804"],
            EnergySupplierId: null,
            BalanceResponsibleId: BalanceResponsibleOne,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(6);
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.Version))
            .OrderBy(t => t.PeriodStart)
            .Should()
            .BeEquivalentTo([
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 1, 23, 0), 7),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 1, 23, 0), Instant.FromUtc(2022, 1, 3, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 3, 23, 0), Instant.FromUtc(2022, 1, 5, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 6),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 7, 23, 0), 8),
                ("804", TimeSeriesType.FlexConsumption, Instant.FromUtc(2022, 1, 7, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 6),
            ]);

        actual.Should().AllSatisfy(ats =>
        {
            ats.TimeSeriesPoints.Should()
                .AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeGreaterOrEqualTo(ats.PeriodStart))
                .And.AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeLessThan(ats.PeriodEnd))
                .And.AllSatisfy(etsp =>
                {
                    etsp.Time.Minute.Should().Be(0);
                    etsp.Time.Second.Should().Be(0);
                })
                .And.HaveCount((int)ats.PeriodEnd.Minus(ats.PeriodStart).TotalHours)
                .And.OnlyHaveUniqueItems(etsp => etsp.Time);
        });
    }

    [Fact]
    public async Task Grid_owner_for_grid_area()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 9, 0, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.Production],
            GridAreaCodes: ["804"],
            EnergySupplierId: null,
            BalanceResponsibleId: null,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(6);
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.Version))
            .Should()
            .BeEquivalentTo([
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 6),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 7, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 6),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 1, 23, 0), Instant.FromUtc(2022, 1, 3, 23, 0), 8),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 7, 23, 0), 8),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2022, 1, 3, 23, 0), Instant.FromUtc(2022, 1, 5, 23, 0), 8),
                ("804", TimeSeriesType.Production, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 1, 23, 0), 7),
            ]);

        actual.Should().AllSatisfy(ats =>
        {
            ats.TimeSeriesPoints.Should()
                .AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeGreaterOrEqualTo(ats.PeriodStart))
                .And.AllSatisfy(etsp => etsp.Time.ToInstant().Should().BeLessThan(ats.PeriodEnd))
                .And.AllSatisfy(etsp =>
                {
                    etsp.Time.Minute.Should().Be(0);
                    etsp.Time.Second.Should().Be(0);
                })
                .And.HaveCount((int)ats.PeriodEnd.Minus(ats.PeriodStart).TotalHours)
                .And.OnlyHaveUniqueItems(etsp => etsp.Time);
        });
    }

    private async Task ClearAndAddDatabricksDataAsync()
    {
        await _fixture.DatabricksSchemaManager.DropSchemaAsync();
        await _fixture.DatabricksSchemaManager.CreateSchemaAsync();

        const string wholesaleOutputEnergyResultsCsv = "wholesale_output.energy_results.csv";
        var energyTestFile = Path.Combine("TestData", wholesaleOutputEnergyResultsCsv);

        const string basisDataCalculationsCsv = "basis_data.calculations.csv";
        var basisDataTestFile = Path.Combine("TestData", basisDataCalculationsCsv);

        await _fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.CALCULATIONS_TABLE_NAME,
            BasisDataCalculationsTableSchemaDefinition.SchemaDefinition,
            basisDataTestFile);

        await _fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME,
            EnergyResultsTableSchemaDefinition.SchemaDefinition,
            energyTestFile);
    }
}
