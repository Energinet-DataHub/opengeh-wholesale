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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public sealed class WholesaleServicesQueriesTests : TestBase<WholesaleServicesQueries>, IClassFixture<DatabricksSqlStatementApiFixture>
{
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public WholesaleServicesQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        Fixture.Inject(fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(fixture.GetDatabricksExecutor());
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAsync_WhenRequestWithNoFilters_Returns3CorrectPackages()
    {
        // Arrange
        var calculationPeriods = CreateCalculationPeriods();

        List<Package> packages = [
            new Package(
                CalculationPeriod: calculationPeriods.Calculation1Period1,
                Points: [
                    calculationPeriods.Calculation1Period1.Period.Start,
                    calculationPeriods.Calculation1Period1.Period.Start.Plus(Duration.FromHours(1)),
                    calculationPeriods.Calculation1Period1.Period.Start.Plus(Duration.FromHours(2)),
                ]),
            new Package(
                CalculationPeriod: calculationPeriods.Calculation2Period,
                Points: [
                    calculationPeriods.Calculation2Period.Period.Start,
                    calculationPeriods.Calculation2Period.Period.Start.Plus(Duration.FromHours(1))
                ]),
            new Package(
                CalculationPeriod: calculationPeriods.Calculation1Period2,
                Points: [
                    calculationPeriods.Calculation1Period2.Period.Start,
                    calculationPeriods.Calculation1Period2.Period.Start.Plus(Duration.FromHours(1)),
                    calculationPeriods.Calculation1Period2.Period.Start.Plus(Duration.FromHours(2)),
                    calculationPeriods.Calculation1Period2.Period.Start.Plus(Duration.FromDays(1)),
                ]),
        ];

        var rows = ExtractSqlRowsFromPackageAggregations(packages);
        await InsertData(rows);

        var query = CreateQueryParameters(calculationPeriods.All);

        // Act
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(3);
        foreach (var expectedPackage in packages)
            actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, expectedPackage));
    }

    [Fact]
    public async Task GetAsync_WhenSameDataIn4DifferentGridAreas_Returns4CorrectPackages()
    {
        // Arrange
        var calculationPeriod = CreateCalculationPeriods().Calculation1Period1;
        List<Package> packages = [
            new Package(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(1)),
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(2)),
                ],
                GridArea: "101"),
            new Package(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                ],
                GridArea: "102"),
            new Package(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(4)),
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(7)),
                ],
                GridArea: "103"),
            new Package(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromDays(1)),
                    calculationPeriod.Period.Start.Plus(Duration.FromDays(2)),
                ],
                GridArea: "104"),
        ];

        var rows = ExtractSqlRowsFromPackageAggregations(packages);
        await InsertData(rows);

        var query = CreateQueryParameters([calculationPeriod]);

        // Act
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(4);
        foreach (var expectedPackage in packages)
            actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, expectedPackage));
    }

    private bool PackagesAreEqual(WholesaleServices actualPackage, Package expectedPackage)
    {
        var actual = new PackageForComparision(
            actualPackage.Period,
            actualPackage.Version,
            actualPackage.GridArea,
            actualPackage.EnergySupplierId,
            actualPackage.ChargeOwnerId,
            actualPackage.ChargeCode,
            actualPackage.ChargeType);

        var expected = new PackageForComparision(
            expectedPackage.CalculationPeriod.Period,
            expectedPackage.CalculationPeriod.CalculationVersion,
            expectedPackage.GridArea,
            expectedPackage.EnergySupplierId,
            expectedPackage.ChargeOwnerId,
            expectedPackage.ChargeCode,
            expectedPackage.ChargeType);

        var actualPoints = actualPackage.TimeSeriesPoints.Select(p => p.Time.ToInstant()).ToList();
        var expectedPoints = expectedPackage.Points;

        return actual == expected && actualPoints.SequenceEqual(expectedPoints);
    }

    private List<IReadOnlyCollection<string?>> ExtractSqlRowsFromPackageAggregations(List<Package> packages)
    {
        return packages
            .SelectMany(p =>
                p.Points.Select(time =>
                    CreateRow(
                        p.CalculationPeriod,
                        time,
                        p.GridArea,
                        p.EnergySupplierId,
                        p.ChargeOwnerId,
                        p.ChargeType,
                        p.ChargeCode)))
            .ToList();
    }

    private async Task InsertData(List<IReadOnlyCollection<string?>> rows)
    {
        await _fixture.DatabricksSchemaManager.EmptyAsync(_fixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_TABLE_NAME);
        await _fixture.DatabricksSchemaManager.InsertAsync<WholesaleResultColumnNames>(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_TABLE_NAME,
            rows);
    }

    private IReadOnlyCollection<string?> CreateRow(
        CalculationForPeriod calculationPeriod,
        Instant time,
        string gridArea,
        string energySupplierId,
        string chargeOwnerId,
        ChargeType chargeType,
        string chargeCode)
    {
        return WholesaleResultDeltaTableHelper.CreateRowValues(
            calculationPeriod.CalculationId.ToString(),
            time: time.ToString(),
            gridArea: gridArea,
            energySupplierId: energySupplierId,
            chargeOwnerId: chargeOwnerId,
            chargeType: ChargeTypeMapper.ToDeltaTableValue(chargeType),
            chargeCode: chargeCode);
    }

    private CalculationPeriods CreateCalculationPeriods()
    {
        var totalPeriod = new Period(
            Instant.FromUtc(2024, 1, 1, 0, 0),
            Instant.FromUtc(2024, 1, 25, 0, 0));

        var calculation1Id = Guid.NewGuid();
        var calculation1Version = 1;
        var calculation1Period1 = new CalculationForPeriod(
            new Period(
                totalPeriod.Start,
                Duration.FromDays(4)),
            calculation1Id,
            calculation1Version);

        var calculation2Period = new CalculationForPeriod(
            new Period(
                calculation1Period1.Period.End,
                Duration.FromDays(7)),
            Guid.NewGuid(),
            2);

        var calculation1Period2 = new CalculationForPeriod(
            new Period(
                calculation2Period.Period.End,
                totalPeriod.End),
            calculation1Id,
            calculation1Version);

        return new CalculationPeriods(calculation1Period1, calculation2Period, calculation1Period2);
    }

    private WholesaleServicesQueryParameters CreateQueryParameters(
        IReadOnlyCollection<CalculationForPeriod> calculations,
        Resolution? resolution = null,
        string? gridArea = null,
        string? energySupplierId = null,
        string? chargeOwnerId = null,
        List<(string? ChargeCode, ChargeType? ChargeType)>? chargeTypes = null)
    {
        return new WholesaleServicesQueryParameters(
            resolution,
            gridArea,
            energySupplierId,
            chargeOwnerId,
            chargeTypes,
            calculations);
    }

    private record CalculationPeriods(
        CalculationForPeriod Calculation1Period1,
        CalculationForPeriod Calculation2Period,
        CalculationForPeriod Calculation1Period2)
    {
        public IReadOnlyCollection<CalculationForPeriod> All =>
        [
            Calculation1Period1,
            Calculation2Period,
            Calculation1Period2,
        ];
    }

    private record Package(
        CalculationForPeriod CalculationPeriod,
        List<Instant> Points,
        string GridArea = "100",
        string EnergySupplierId = "2236552000028",
        string ChargeOwnerId = "9876543210000",
        string ChargeCode = "4000",
        ChargeType ChargeType = ChargeType.Tariff);

    private record PackageForComparision(
        Period Period,
        long CalculationVersion,
        string GridArea,
        string EnergySupplierId,
        string ChargeOwnerId,
        string ChargeCode,
        ChargeType ChargeType);
}
