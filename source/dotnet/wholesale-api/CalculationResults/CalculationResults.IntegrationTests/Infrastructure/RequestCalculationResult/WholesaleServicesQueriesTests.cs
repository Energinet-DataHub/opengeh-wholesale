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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using JetBrains.Annotations;
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

    public static IEnumerable<object[]> GetWholesaleServicesProperties()
    {
        var values = Enum.GetValues<WholesaleServicesProperty>();

        return values.Select(v => new object[] { v }).ToArray();
    }

    [Fact]
    public async Task GetAsync_WhenRequestingCalculationPeriod_ReturnsCorrectData()
    {
        // Arrange
        var calculationPeriod = CreateCalculationPeriods().Calculation1Period1;
        var amountType = AmountType.MonthlyAmountPerCharge;

        var package = new WholesaleServicesPackage(
            CalculationPeriod: calculationPeriod,
            Points:
            [
                calculationPeriod.Period.Start,
                calculationPeriod.Period.Start.Plus(Duration.FromHours(1)),
            ],
            amountType,
            Resolution.Month,
            "111",
            "1136552000028",
            "2276543210000",
            "3333",
            ChargeType.Fee,
            CalculationType.SecondCorrectionSettlement);

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints([package]); // A package creates 1 sql row per point (9 rows total in this case)
        await InsertData(rows);

        var query = CreateQueryParameters([calculationPeriod], amountType);

        // Act
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, package));
    }

    [Fact]
    public async Task GetAsync_WhenRequesting3CalculationPeriods_Returns3WholesaleServices()
    {
        // Arrange
        var calculationPeriods = CreateCalculationPeriods();

        List<WholesaleServicesPackage> packages = [
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriods.Calculation1Period1,
                Points: [
                    calculationPeriods.Calculation1Period1.Period.Start,
                    calculationPeriods.Calculation1Period1.Period.Start.Plus(Duration.FromHours(1)),
                    calculationPeriods.Calculation1Period1.Period.Start.Plus(Duration.FromHours(2)),
                ]),
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriods.Calculation2,
                Points: [
                    calculationPeriods.Calculation2.Period.Start,
                    calculationPeriods.Calculation2.Period.Start.Plus(Duration.FromHours(1))
                ]),
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriods.Calculation1Period2,
                Points: [
                    calculationPeriods.Calculation1Period2.Period.Start,
                    calculationPeriods.Calculation1Period2.Period.Start.Plus(Duration.FromHours(1)),
                    calculationPeriods.Calculation1Period2.Period.Start.Plus(Duration.FromHours(2)),
                    calculationPeriods.Calculation1Period2.Period.Start.Plus(Duration.FromDays(1)),
                ]),
        ];

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints(packages); // A package creates 1 sql row per point (9 rows total in this case)
        await InsertData(rows);

        var query = CreateQueryParameters([calculationPeriods.Calculation1Period1, calculationPeriods.Calculation2, calculationPeriods.Calculation1Period2]);

        // Act
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(3);
        foreach (var expectedPackage in packages)
            actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, expectedPackage));
    }

    [Fact]
    public async Task GetAsync_WhenRequest2GridAreas_Returns2WholesaleServices()
    {
        // Arrange
        string[] gridAreaCodesFilter = ["101", "103"];
        var calculationPeriod = CreateCalculationPeriods().Calculation1Period1;

        List<WholesaleServicesPackage> packages = [
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(1)),
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(2)),
                ],
                GridArea: "101"),
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                ],
                GridArea: "102"),
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(4)),
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(7)),
                ],
                GridArea: "103"),
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromDays(1)),
                    calculationPeriod.Period.Start.Plus(Duration.FromDays(2)),
                ],
                GridArea: "104"),
        ];

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints(packages); // A package creates 1 sql row per point (10 rows total in this case)
        await InsertData(rows);
        var query = CreateQueryParameters([calculationPeriod], gridAreaCodes: gridAreaCodesFilter);

        // Act
        string[] expectedGridAreaCodes = ["101", "103"];
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        foreach (var expectedGridAreaCode in expectedGridAreaCodes)
        {
            actual.Should().ContainSingle(actualPackage => actualPackage.GridArea == expectedGridAreaCode);
        }
    }

    [Fact]
    public async Task GetAsync_WhenSameDataIn4DifferentGridAreas_Returns4WholesaleServices()
    {
        // Arrange
        var calculationPeriod = CreateCalculationPeriods().Calculation1Period1;

        List<WholesaleServicesPackage> packages = [
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(1)),
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(2)),
                ],
                GridArea: "101"),
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                ],
                GridArea: "102"),
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(4)),
                    calculationPeriod.Period.Start.Plus(Duration.FromHours(7)),
                ],
                GridArea: "103"),
            new WholesaleServicesPackage(
                CalculationPeriod: calculationPeriod,
                Points: [
                    calculationPeriod.Period.Start,
                    calculationPeriod.Period.Start.Plus(Duration.FromDays(1)),
                    calculationPeriod.Period.Start.Plus(Duration.FromDays(2)),
                ],
                GridArea: "104"),
        ];

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints(packages); // A package creates 1 sql row per point (10 rows total in this case)
        await InsertData(rows);

        var query = CreateQueryParameters([calculationPeriod]);

        // Act
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(4);
        foreach (var expectedPackage in packages)
            actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, expectedPackage));
    }

    [Theory]
    [MemberData(nameof(GetWholesaleServicesProperties))]
    public async Task GetAsync_WhenSameDataExceptOneProperty_WholesaleServicesGroupedCorrectlyIn3(WholesaleServicesProperty propertyToDiffer)
    {
        // Arrange
        var calculationPeriods = CreateCalculationPeriods();

        var calculationPeriod = calculationPeriods.Calculation2;
        var resolution = Resolution.Day;
        var gridArea = "999";
        var energySupplierId = "2236552000028";
        var chargeOwnerId = "9876543210000";
        var chargeCode = "4000";
        var chargeType = ChargeType.Tariff;

        List<WholesaleServicesPackage> packages = [
            new WholesaleServicesPackage(
                Points: [
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation2 : calculationPeriod).Period.Start,
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation2 : calculationPeriod).Period.Start.Plus(Duration.FromHours(1)),
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation2 : calculationPeriod).Period.Start.Plus(Duration.FromHours(2)),
                ],
                CalculationPeriod: propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation2 : calculationPeriod,
                Resolution: propertyToDiffer == WholesaleServicesProperty.Resolution ? Resolution.Month : resolution,
                GridArea: propertyToDiffer == WholesaleServicesProperty.GridArea ? "101" : gridArea,
                EnergySupplierId: propertyToDiffer == WholesaleServicesProperty.EnergySupplierId ? "9999999999991" : energySupplierId,
                ChargeOwnerId: propertyToDiffer == WholesaleServicesProperty.ChargeOwnerId ? "8888888888881" : chargeOwnerId,
                ChargeCode: propertyToDiffer == WholesaleServicesProperty.ChargeCode ? "9991" : chargeCode,
                ChargeType: propertyToDiffer == WholesaleServicesProperty.ChargeType ? ChargeType.Fee : chargeType),
            new WholesaleServicesPackage(
                Points: [
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation3 : calculationPeriod).Period.Start,
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation3 : calculationPeriod).Period.Start.Plus(Duration.FromHours(3)),
                ],
                CalculationPeriod: propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation3 : calculationPeriod,
                Resolution: propertyToDiffer == WholesaleServicesProperty.Resolution ? Resolution.Hour : resolution,
                GridArea: propertyToDiffer == WholesaleServicesProperty.GridArea ? "102" : gridArea,
                EnergySupplierId: propertyToDiffer == WholesaleServicesProperty.EnergySupplierId ? "9999999999992" : energySupplierId,
                ChargeOwnerId: propertyToDiffer == WholesaleServicesProperty.ChargeOwnerId ? "8888888888882" : chargeOwnerId,
                ChargeCode: propertyToDiffer == WholesaleServicesProperty.ChargeCode ? "9992" : chargeCode,
                ChargeType: propertyToDiffer == WholesaleServicesProperty.ChargeType ? ChargeType.Tariff : chargeType),
            new WholesaleServicesPackage(
                Points: [
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation4 : calculationPeriod).Period.Start,
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation4 : calculationPeriod).Period.Start.Plus(Duration.FromHours(4)),
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation4 : calculationPeriod).Period.Start.Plus(Duration.FromHours(7)),
                    (propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation4 : calculationPeriod).Period.Start.Plus(Duration.FromHours(12)),
                ],
                CalculationPeriod: propertyToDiffer == WholesaleServicesProperty.CalculationPeriod ? calculationPeriods.Calculation4 : calculationPeriod,
                Resolution: propertyToDiffer == WholesaleServicesProperty.Resolution ? Resolution.Day : resolution,
                GridArea: propertyToDiffer == WholesaleServicesProperty.GridArea ? "103" : gridArea,
                EnergySupplierId: propertyToDiffer == WholesaleServicesProperty.EnergySupplierId ? "9999999999993" : energySupplierId,
                ChargeOwnerId: propertyToDiffer == WholesaleServicesProperty.ChargeOwnerId ? "8888888888883" : chargeOwnerId,
                ChargeCode: propertyToDiffer == WholesaleServicesProperty.ChargeCode ? "9993" : chargeCode,
                ChargeType: propertyToDiffer == WholesaleServicesProperty.ChargeType ? ChargeType.Subscription : chargeType),
        ];

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints(packages); // A package creates 1 sql row per point (8 rows total in this case)
        await InsertData(rows);

        var query = CreateQueryParameters([calculationPeriods.Calculation2, calculationPeriods.Calculation3, calculationPeriods.Calculation4]);

        // Act
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(3);
        foreach (var expectedPackage in packages)
            actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, expectedPackage));
    }

    [Fact]
    public async Task GetAsync_WhenUsingAllQueryFilters_Returns1MatchingWholesaleServices()
    {
        // Arrange
        var calculationPeriods = CreateCalculationPeriods();
        var expectedCalculationPeriod = calculationPeriods.Calculation1Period1;
        var expectedAmountType = AmountType.AmountPerCharge;
        var expectedGridAreaCode = "911";
        var expectedEnergySupplierId = "8886552000028";
        var expectedChargeOwnerId = "9996543210000";
        var expectedChargeCode = "8000";
        var expectedChargeType = ChargeType.Subscription;

        var match = CreatePackageForFilter(
            expectedCalculationPeriod,
            expectedAmountType,
            expectedGridAreaCode,
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            expectedChargeCode,
            expectedChargeType);

        var matchExpectPeriod = CreatePackageForFilter(
            calculationPeriods.Calculation2,
            expectedAmountType,
            expectedGridAreaCode,
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            expectedChargeCode,
            expectedChargeType);

        var matchExceptAmountType = CreatePackageForFilter(
            expectedCalculationPeriod,
            AmountType.MonthlyAmountPerCharge,
            expectedGridAreaCode,
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            expectedChargeCode,
            expectedChargeType);

        var matchExceptGridArea = CreatePackageForFilter(
            expectedCalculationPeriod,
            expectedAmountType,
            "111",
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            expectedChargeCode,
            expectedChargeType);

        var matchExceptEnergySupplierId = CreatePackageForFilter(
            expectedCalculationPeriod,
            expectedAmountType,
            expectedGridAreaCode,
            "2226552000028",
            expectedChargeOwnerId,
            expectedChargeCode,
            expectedChargeType);

        var matchExceptChargeOwnerId = CreatePackageForFilter(
            expectedCalculationPeriod,
            expectedAmountType,
            expectedGridAreaCode,
            expectedEnergySupplierId,
            "3336543210000",
            expectedChargeCode,
            expectedChargeType);

        var matchExceptChargeCode = CreatePackageForFilter(
            expectedCalculationPeriod,
            expectedAmountType,
            expectedGridAreaCode,
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            "9000",
            expectedChargeType);

        var matchExceptChargeType = CreatePackageForFilter(
            expectedCalculationPeriod,
            expectedAmountType,
            expectedGridAreaCode,
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            expectedChargeCode,
            ChargeType.Fee);

        var noMatch = CreatePackageForFilter(calculationPeriods.Calculation2);

        List<WholesaleServicesPackage> packages = [
            match,
            matchExpectPeriod,
            matchExceptAmountType,
            matchExceptGridArea,
            matchExceptEnergySupplierId,
            matchExceptChargeOwnerId,
            matchExceptChargeCode,
            matchExceptChargeType,
            noMatch,
        ];

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints(packages);
        await InsertData(rows);

        var query = CreateQueryParameters(
            [expectedCalculationPeriod],
            expectedAmountType,
            [expectedGridAreaCode],
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            (expectedChargeCode, expectedChargeType));

        // Act
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, match));
    }

    [Fact]
    public async Task GetAsync_WhenFilteringBy2DifferentChargeTypes_Returns2MatchingWholesaleServices()
    {
        // Arrange
        var calculationPeriod = CreateCalculationPeriods().Calculation1Period1;
        var expectedChargeCode1 = "8000";
        var expectedChargeType1 = ChargeType.Subscription;
        var expectedChargeCode2 = "3000";
        var expectedChargeType2 = ChargeType.Fee;

        var match1 = CreatePackageForFilter(
            calculationPeriod,
            chargeCode: expectedChargeCode1,
            chargeType: expectedChargeType1);

        var match2 = CreatePackageForFilter(
            calculationPeriod,
            chargeCode: expectedChargeCode2,
            chargeType: expectedChargeType2);

        var match1ExceptChargeCode = CreatePackageForFilter(
            calculationPeriod,
            chargeCode: expectedChargeCode2,
            chargeType: expectedChargeType1);

        var match1ExceptChargeType = CreatePackageForFilter(
            calculationPeriod,
            chargeCode: expectedChargeCode1,
            chargeType: expectedChargeType2);

        var match2ExceptChargeCode = CreatePackageForFilter(
            calculationPeriod,
            chargeCode: expectedChargeCode1,
            chargeType: expectedChargeType2);

        var match2ExceptChargeType = CreatePackageForFilter(
            calculationPeriod,
            chargeCode: expectedChargeCode2,
            chargeType: expectedChargeType1);

        var noMatch = CreatePackageForFilter(
            calculationPeriod,
            chargeCode: "9000",
            chargeType: ChargeType.Tariff);

        List<WholesaleServicesPackage> packages = [
            match1,
            match2,
            match1ExceptChargeCode,
            match1ExceptChargeType,
            match2ExceptChargeCode,
            match2ExceptChargeType,
            noMatch,
        ];

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints(packages);
        await InsertData(rows);

        var query = CreateQueryParameters(
            [calculationPeriod],
            chargeTypes: [
                (expectedChargeCode1, expectedChargeType1),
                (expectedChargeCode2, expectedChargeType2)
            ]);

        // Act
        var actual = await Sut.GetAsync(query).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(2);
        actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, match1));
        actual.Should().ContainSingle(actualPackage => PackagesAreEqual(actualPackage, match2));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task AnyAsync_WithNoQueryFilters_MatchesCorrectly(bool shouldMatch, bool addNonMatchingDataPoints)
    {
        // Arrange
        var calculationPeriods = CreateCalculationPeriods();

        var package = new WholesaleServicesPackage(
            calculationPeriods.Calculation1Period1,
            [
                calculationPeriods.Calculation1Period1.Period.Start
            ]);

        var packages = new List<WholesaleServicesPackage>
        {
            package,
        };

        if (addNonMatchingDataPoints)
        {
            packages.Add(new WholesaleServicesPackage(
                calculationPeriods.Calculation2,
                [
                    calculationPeriods.Calculation2.Period.Start
                ]));

            packages.Add(new WholesaleServicesPackage(
                calculationPeriods.Calculation3,
                [
                    calculationPeriods.Calculation3.Period.Start
                ]));
        }

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints(packages);
        await InsertData(rows);

        var query = CreateQueryParameters([shouldMatch ? package.CalculationPeriod : calculationPeriods.Calculation1Period2]);

        // Act
        var actual = await Sut.AnyAsync(query);

        if (shouldMatch)
            actual.Should().BeTrue();
        else
            actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task AnyAsync_WithQueryFilters_MatchesCorrectly(bool shouldMatch, bool addNonMatchingDataPoints)
    {
        // Arrange
        var calculationPeriods = CreateCalculationPeriods();
        var expectedCalculationPeriod = calculationPeriods.Calculation1Period1;
        var expectedResolution = Resolution.Hour;
        var expectedAmountType = AmountType.AmountPerCharge;
        var expectedGridAreaCode = "911";
        var expectedEnergySupplierId = "8886552000028";
        var expectedChargeOwnerId = "9996543210000";
        var expectedChargeCode = "8000";
        var expectedChargeType = ChargeType.Subscription;

        var package1 = new WholesaleServicesPackage(
            expectedCalculationPeriod,
            [
                calculationPeriods.Calculation1Period1.Period.Start
            ],
            expectedAmountType,
            expectedResolution,
            expectedGridAreaCode,
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            expectedChargeCode,
            expectedChargeType);

        var package2 = new WholesaleServicesPackage(
            expectedCalculationPeriod,
            [
                calculationPeriods.Calculation1Period2.Period.Start
            ],
            expectedAmountType,
            expectedResolution,
            expectedGridAreaCode,
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            expectedChargeCode,
            expectedChargeType);

        var packages = new List<WholesaleServicesPackage>
        {
            package1,
            package2,
        };

        if (addNonMatchingDataPoints)
        {
            packages.Add(new WholesaleServicesPackage(
                calculationPeriods.Calculation2,
                [
                    calculationPeriods.Calculation2.Period.Start
                ]));

            packages.Add(new WholesaleServicesPackage(
                calculationPeriods.Calculation3,
                [
                    calculationPeriods.Calculation3.Period.Start
                ]));
        }

        var rows = ExtractSqlRowsFromPackagesAndTheirPoints(packages);
        await InsertData(rows);

        var query = CreateQueryParameters(
            [
                shouldMatch ? package1.CalculationPeriod : calculationPeriods.Calculation4,
                shouldMatch ? package2.CalculationPeriod : calculationPeriods.Calculation4,
            ],
            expectedAmountType,
            [expectedGridAreaCode],
            expectedEnergySupplierId,
            expectedChargeOwnerId,
            (expectedChargeCode, expectedChargeType));

        // Act
        var actual = await Sut.AnyAsync(query);

        if (shouldMatch)
            actual.Should().BeTrue();
        else
            actual.Should().BeFalse();
    }

    private WholesaleServicesPackage CreatePackageForFilter(
        CalculationForPeriod calculationPeriod,
        AmountType amountType = AmountType.AmountPerCharge,
        string gridArea = "100",
        string energySupplierId = "2236552000028",
        string chargeOwnerId = "9876543210000",
        string chargeCode = "4000",
        ChargeType chargeType = ChargeType.Tariff)
    {
        return new WholesaleServicesPackage(
            calculationPeriod,
            [calculationPeriod.Period.Start],
            amountType,
            amountType == AmountType.TotalMonthlyAmount ? Resolution.Month : Resolution.Day,
            gridArea,
            energySupplierId,
            chargeOwnerId,
            chargeCode,
            chargeType);
    }

    private bool PackagesAreEqual(WholesaleServices actualPackage, WholesaleServicesPackage expectedPackage)
    {
        var actual = new PackageForComparision(
            actualPackage.Period,
            actualPackage.Version,
            actualPackage.GridArea,
            actualPackage.EnergySupplierId,
            actualPackage.ChargeOwnerId,
            actualPackage.ChargeCode,
            actualPackage.ChargeType,
            actualPackage.AmountType,
            actualPackage.Resolution,
            actualPackage.CalculationType);

        var expected = new PackageForComparision(
            expectedPackage.CalculationPeriod.Period,
            expectedPackage.CalculationPeriod.CalculationVersion,
            expectedPackage.GridArea,
            expectedPackage.EnergySupplierId,
            expectedPackage.ChargeOwnerId,
            expectedPackage.ChargeCode,
            expectedPackage.ChargeType,
            expectedPackage.AmountType,
            expectedPackage.Resolution,
            expectedPackage.CalculationType);

        var actualPoints = actualPackage.TimeSeriesPoints.Select(p => p.Time.ToInstant()).ToList();
        var expectedPoints = expectedPackage.Points;

        return actual == expected && actualPoints.SequenceEqual(expectedPoints);
    }

    private List<IReadOnlyCollection<string?>> ExtractSqlRowsFromPackagesAndTheirPoints(List<WholesaleServicesPackage> packages)
    {
        return packages
            .SelectMany(p =>
                p.Points.Select(time =>
                    CreateRow(
                        p.CalculationPeriod,
                        time,
                        p.AmountType,
                        p.Resolution,
                        p.GridArea,
                        p.EnergySupplierId,
                        p.ChargeOwnerId,
                        p.ChargeType,
                        p.ChargeCode,
                        p.CalculationType)))
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
        AmountType amountType,
        Resolution resolution,
        string gridArea,
        string energySupplierId,
        string chargeOwnerId,
        ChargeType chargeType,
        string chargeCode,
        CalculationType calculationType)
    {
        return WholesaleResultDeltaTableHelper.CreateRowValues(
            calculationPeriod.CalculationId.ToString(),
            time: time.ToString(),
            amountType: AmountTypeMapper.ToDeltaTableValue(amountType),
            resolution: ResolutionMapper.ToDeltaTableValue(resolution),
            gridArea: gridArea,
            energySupplierId: energySupplierId,
            chargeOwnerId: chargeOwnerId,
            chargeType: ChargeTypeMapper.ToDeltaTableValue(chargeType),
            chargeCode: chargeCode,
            calculationType: CalculationTypeMapper.ToDeltaTableValue(calculationType));
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

        var calculation2 = new CalculationForPeriod(
            new Period(
                calculation1Period1.Period.End,
                Duration.FromDays(7)),
            Guid.NewGuid(),
            2);

        var calculation1Period2 = new CalculationForPeriod(
            new Period(
                calculation2.Period.End,
                totalPeriod.End),
            calculation1Id,
            calculation1Version);

        var calculation3 = calculation2 with { CalculationId = Guid.NewGuid(), CalculationVersion = 3 };
        var calculation4 = calculation2 with { CalculationId = Guid.NewGuid(), CalculationVersion = 4 };

        return new CalculationPeriods(calculation1Period1, calculation2, calculation1Period2, calculation3, calculation4);
    }

    private WholesaleServicesQueryParameters CreateQueryParameters(
        IReadOnlyCollection<CalculationForPeriod> calculations,
        AmountType amountType = AmountType.AmountPerCharge,
        string[]? gridAreaCodes = null,
        string? energySupplierId = null,
        string? chargeOwnerId = null,
        params (string? ChargeCode, ChargeType? ChargeType)[] chargeTypes)
    {
        return new WholesaleServicesQueryParameters(
            amountType,
            gridAreaCodes ?? [],
            energySupplierId,
            chargeOwnerId,
            chargeTypes.ToList(),
            calculations);
    }

    private record CalculationPeriods(
        CalculationForPeriod Calculation1Period1,
        CalculationForPeriod Calculation2,
        CalculationForPeriod Calculation1Period2,
        CalculationForPeriod Calculation3,
        CalculationForPeriod Calculation4);

    private record WholesaleServicesPackage(
        CalculationForPeriod CalculationPeriod,
        List<Instant> Points,
        AmountType AmountType = AmountType.AmountPerCharge,
        Resolution Resolution = Resolution.Day,
        string GridArea = "100",
        string EnergySupplierId = "2236552000028",
        string ChargeOwnerId = "9876543210000",
        string ChargeCode = "4000",
        ChargeType ChargeType = ChargeType.Tariff,
        CalculationType CalculationType = CalculationType.WholesaleFixing);

    // Properties are used by implicit comparision
    private record PackageForComparision(
        [UsedImplicitly] Period Period,
        [UsedImplicitly] long CalculationVersion,
        [UsedImplicitly] string GridArea,
        [UsedImplicitly] string EnergySupplierId,
        [UsedImplicitly] string ChargeOwnerId,
        [UsedImplicitly] string ChargeCode,
        [UsedImplicitly] ChargeType ChargeType,
        [UsedImplicitly] AmountType AmountType,
        [UsedImplicitly] Resolution Resolution,
        [UsedImplicitly] CalculationType CalculationType);

    public enum WholesaleServicesProperty
    {
        CalculationPeriod,
        GridArea,
        EnergySupplierId,
        ChargeOwnerId,
        ChargeCode,
        ChargeType,
        Resolution,
    }
}
