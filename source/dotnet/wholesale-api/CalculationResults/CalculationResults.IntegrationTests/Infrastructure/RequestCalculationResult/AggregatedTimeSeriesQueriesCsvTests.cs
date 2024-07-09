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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
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
    public async Task Given_EnergySupplierAcrossGridAreas_Then_RelevantDataFromRelevantGridAreasReturned()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.FlexConsumption, TimeSeriesType.NonProfiledConsumption],
            GridAreaCodes: [],
            EnergySupplierId: EnergySupplierThree,
            BalanceResponsibleId: null,
            CalculationType: CalculationType.BalanceFixing,
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
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 5, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 16),
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
    public async Task Given_EnergySupplierAndGridArea_Then_RelevantDataFromSpecifiedGridAreaReturned()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.FlexConsumption, TimeSeriesType.Production],
            GridAreaCodes: ["804"],
            EnergySupplierId: EnergySupplierOne,
            BalanceResponsibleId: null,
            CalculationType: CalculationType.BalanceFixing,
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
    public async Task Given_EnergySupplierAndBalanceResponsibleAndGridArea_Then_DataFilteredCorrectlyReturned()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.FlexConsumption, TimeSeriesType.Production],
            GridAreaCodes: ["804"],
            EnergySupplierId: EnergySupplierOne,
            BalanceResponsibleId: BalanceResponsibleOne,
            CalculationType: CalculationType.BalanceFixing,
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
    public async Task Given_BalanceResponsibleAndGridArea_Then_RelevantDataFromGridAreaReturned()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.FlexConsumption],
            GridAreaCodes: ["804"],
            EnergySupplierId: null,
            BalanceResponsibleId: BalanceResponsibleOne,
            CalculationType: CalculationType.BalanceFixing,
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
    public async Task Given_GridArea_Then_GridOperatorDataForGridAreaReturned()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.Production],
            GridAreaCodes: ["804"],
            EnergySupplierId: null,
            BalanceResponsibleId: null,
            CalculationType: CalculationType.BalanceFixing,
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

    [Fact]
    public async Task Given_FullQueryParametersForAggregation_Then_DataFromNewestVersionsReturned()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.NonProfiledConsumption],
            GridAreaCodes: ["543"],
            EnergySupplierId: EnergySupplierThree,
            BalanceResponsibleId: BalanceResponsibleOne,
            CalculationType: CalculationType.Aggregation,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.Version))
            .Should()
            .BeEquivalentTo([
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 2, 23, 0), 9),
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 2, 23, 0), Instant.FromUtc(2022, 1, 4, 23, 0), 10),
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 4, 23, 0), Instant.FromUtc(2022, 1, 6, 23, 0), 11),
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 6, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), 9),
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
    public async Task Given_EnergySupplierAndBalanceResponsibleWithLatestCorrection_Then_DataFromNewestCorrectionsReturned()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.NonProfiledConsumption],
            GridAreaCodes: [],
            EnergySupplierId: EnergySupplierThree,
            BalanceResponsibleId: BalanceResponsibleOne,
            CalculationType: null,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.CalculationType, ats.Version))
            .Should()
            .BeEquivalentTo([
                ("543", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), CalculationType.SecondCorrectionSettlement, 3),
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), CalculationType.ThirdCorrectionSettlement, 2),
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
    public async Task
        Given_NoEnergySupplierAndBalanceResponsibleAndGridArea_Then_IdenticalToRequestsForEachGridAreaIndividually()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.NonProfiledConsumption],
            GridAreaCodes: [],
            EnergySupplierId: null,
            BalanceResponsibleId: null,
            CalculationType: CalculationType.Aggregation,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        var eachGridAreaIndividually = new List<AggregatedTimeSeries>();
        foreach (var parametersForGridArea in new[] { "543", "584", "804", }
                     .Select(gridArea => parameters with { GridAreaCodes = [gridArea] }))
        {
            eachGridAreaIndividually.AddRange(await Sut.GetAsync(parametersForGridArea).ToListAsync());
        }

        actual.Should().NotBeEmpty().And.BeEquivalentTo(eachGridAreaIndividually);
    }

    [Fact]
    public async Task Given_EnergySupplierOnlyHaveDataForHalfOfThePeriod_Then_DataReturnedWithModifiedPeriod()
    {
        /*
         Business case example:
         When a new Energy Supplier is being made responsible for a metering point in the middle of the month,
         and they do not yet have a metering point in the grid area from the beginning of the month.
         The result is that the Energy Supplier will only have results for the last half of the month.
        */

        await ClearAndAddDatabricksDataAsync();
        await RemoveDataForEnergySupplierInTimespan("5790002617263", Instant.FromUtc(2022, 1, 4, 0, 0), null);

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.NonProfiledConsumption],
            GridAreaCodes: [],
            EnergySupplierId: "5790002617263",
            BalanceResponsibleId: null,
            CalculationType: CalculationType.SecondCorrectionSettlement,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.CalculationType,
                ats.Version))
            .Should()
            .BeEquivalentTo([
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2022, 1, 4, 1, 0), Instant.FromUtc(2022, 1, 8, 23, 0), CalculationType.SecondCorrectionSettlement, 3),
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
                .And.OnlyHaveUniqueItems(etsp => etsp.Time);
        });
    }

    [Fact]
    public async Task Given_EnergySupplierWithAHoleInData_Then_DataReturnedInOneChunkWithAHole()
    {
        await ClearAndAddDatabricksDataAsync();
        await RemoveDataForEnergySupplierInTimespan(
            "5790002617263",
            Instant.FromUtc(2022, 1, 5, 0, 0),
            Instant.FromUtc(2022, 1, 3, 0, 0));

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.NonProfiledConsumption],
            GridAreaCodes: [],
            EnergySupplierId: "5790002617263",
            BalanceResponsibleId: null,
            CalculationType: CalculationType.ThirdCorrectionSettlement,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.CalculationType,
                ats.Version))
            .Should()
            .BeEquivalentTo([
                ("804", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), CalculationType.ThirdCorrectionSettlement, 2),
            ]);

        actual.Should().AllSatisfy(ats =>
        {
            ats.TimeSeriesPoints.Should().AllSatisfy(etsp =>
            {
                ((object)etsp.Time).Should().Match<DateTimeOffset>(time =>
                    time <= new DateTimeOffset(2022, 1, 3, 0, 0, 0, TimeSpan.Zero)
                    || time > new DateTimeOffset(2022, 1, 5, 0, 0, 0, TimeSpan.Zero));
            });
        });
    }

    [Fact]
    public async Task Given_BalanceResponsibleWithLatestCorrectionButNoCorrectionData_Then_NoDataReturned()
    {
        await ClearAndAddDatabricksDataAsync();
        await RemoveDataForCorrections([]);

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.NonProfiledConsumption, TimeSeriesType.FlexConsumption, TimeSeriesType.Production],
            GridAreaCodes: [],
            EnergySupplierId: null,
            BalanceResponsibleId: BalanceResponsibleOne,
            CalculationType: null,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_EnergySupplierAndBalanceResponsibleWithLatestCorrectionButOnlyOneGridAreaWithCorrectionData_Then_DataReturnedForGridArea()
    {
        await ClearAndAddDatabricksDataAsync();
        await RemoveDataForCorrections(["804", "543"]);

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 8, 23, 0));

        var parameters = new AggregatedTimeSeriesQueryParameters(
            TimeSeriesTypes: [TimeSeriesType.NonProfiledConsumption, TimeSeriesType.FlexConsumption, TimeSeriesType.Production],
            GridAreaCodes: [],
            EnergySupplierId: EnergySupplierTwo,
            BalanceResponsibleId: BalanceResponsibleOne,
            CalculationType: null,
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => (ats.GridArea, ats.TimeSeriesType, ats.PeriodStart, ats.PeriodEnd, ats.CalculationType, ats.Version))
            .Should()
            .BeEquivalentTo([
                ("584", TimeSeriesType.FlexConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), CalculationType.SecondCorrectionSettlement, 3),
                ("584", TimeSeriesType.NonProfiledConsumption, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 8, 23, 0), CalculationType.SecondCorrectionSettlement, 3),
            ]);
    }

    private async Task ClearAndAddDatabricksDataAsync()
    {
        await _fixture.DatabricksSchemaManager.DropSchemaAsync();
        await _fixture.DatabricksSchemaManager.CreateSchemaAsync();

        const string basisDataCalculationsCsv = "basis_data.calculations.csv";
        var basisDataTestFile = Path.Combine("TestData", basisDataCalculationsCsv);

        await _fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.CALCULATIONS_TABLE_NAME,
            BasisDataCalculationsTableSchemaDefinition.SchemaDefinition,
            basisDataTestFile);

        foreach (var index in new[] { 0, 1, 2 })
        {
            var wholesaleOutputEnergyResultsCsv = $"wholesale_output.energy_results_{index}.csv";
            var energyTestFile = Path.Combine("TestData", wholesaleOutputEnergyResultsCsv);

            await _fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
                _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.ENERGY_RESULTS_TABLE_NAME,
                EnergyResultsTableSchemaDefinition.SchemaDefinition,
                energyTestFile);
        }
    }

    private async Task RemoveDataForEnergySupplierInTimespan(string energySupplierId, Instant before, Instant? after)
    {
        var statement = new DeleteEnergySupplierStatement(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value,
            energySupplierId,
            before,
            after);

        await _fixture.GetDatabricksExecutor().ExecuteStatementAsync(statement, Format.JsonArray).ToListAsync();
    }

    private async Task RemoveDataForCorrections(IReadOnlyCollection<string> gridAreasToRemoveFrom)
    {
        var statement = new DeleteCorrectionsStatement(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value,
            gridAreasToRemoveFrom);

        await _fixture.GetDatabricksExecutor().ExecuteStatementAsync(statement, Format.JsonArray).ToListAsync();
    }

    private class DeleteEnergySupplierStatement(
        DeltaTableOptions deltaTableOptions,
        string energySupplierId,
        Instant before,
        Instant? after) : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly string _energySupplierId = energySupplierId;
        private readonly Instant _before = before;
        private readonly Instant? _after = after;

        protected override string GetSqlStatement()
        {
            return $"""
                    DELETE FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME}
                    WHERE {EnergyResultColumnNames.EnergySupplierId} = '{_energySupplierId}'
                    AND {EnergyResultColumnNames.Time} <= '{_before}'
                    {(_after is not null ? $"AND {EnergyResultColumnNames.Time} > '{_after}'" : string.Empty)}
                    """;
        }
    }

    private class DeleteCorrectionsStatement(
        DeltaTableOptions deltaTableOptions,
        IReadOnlyCollection<string> gridAreasToRemoveFrom) : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly IReadOnlyCollection<string> _gridAreasToRemoveFrom = gridAreasToRemoveFrom;

        protected override string GetSqlStatement()
        {
            return $"""
                    DELETE FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME}
                    WHERE ({EnergyResultColumnNames.CalculationType} = '{DeltaTableCalculationType.FirstCorrectionSettlement}'
                    OR {EnergyResultColumnNames.CalculationType} = '{DeltaTableCalculationType.SecondCorrectionSettlement}'
                    OR {EnergyResultColumnNames.CalculationType} = '{DeltaTableCalculationType.ThirdCorrectionSettlement}')
                    {(_gridAreasToRemoveFrom.Any() ? $"AND {EnergyResultColumnNames.GridArea} IN ({string.Join(", ", _gridAreasToRemoveFrom.Select(ga => $"'{ga}'"))})" : string.Empty)}
                    """;
        }
    }
}
