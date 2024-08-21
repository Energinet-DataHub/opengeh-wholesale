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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;
using Resolution =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public class WholesaleServicesQueriesCsvTests
{
    public class WholesaleServicesQueriesCsvTestsWithSharedData
        : TestBase<WholesaleServicesQueries>, IClassFixture<MigrationsFreeDatabricksSqlStatementApiFixture>, IAsyncLifetime
    {
        private readonly MigrationsFreeDatabricksSqlStatementApiFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;

        public WholesaleServicesQueriesCsvTestsWithSharedData(
            MigrationsFreeDatabricksSqlStatementApiFixture fixture,
            ITestOutputHelper testOutputHelper)
        {
            Fixture.Inject(fixture.DatabricksSchemaManager.DeltaTableOptions);
            Fixture.Inject(fixture.GetDatabricksExecutor());
            Fixture.Inject(new WholesaleServicesQuerySnippetProviderFactory([new AmountsPerChargeWholesaleServicesDatabricksContract(), new MonthlyAmountsPerChargeWholesaleServicesDatabricksContract(), new TotalMonthlyAmountWholesaleServicesDatabricksContract()]));
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
        }

        public async Task InitializeAsync()
        {
            if (!_fixture.DataIsInitialized)
            {
                await ClearAndAddDatabricksDataAsync(_fixture, _testOutputHelper);
                _fixture.DataIsInitialized = true;
            }
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Given_EnergySupplierWithAmountPerChargeAndWholesaleFixing_Then_CorrespondingDataReturned()
        {
            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.AmountPerCharge,
                GridAreaCodes: [],
                EnergySupplierId: "5790000701278",
                ChargeOwnerId: null,
                ChargeTypes: [],
                CalculationType: CalculationType.WholesaleFixing,
                Period: totalPeriod,
                RequestedForEnergySupplier: true,
                RequestedForActorNumber: "5790000701278");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                    ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod,
                    ats.CalculationType, ats.Version, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    ("543", "5790000701278", "5790000610976", ChargeType.Tariff, "NT1009", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB1009", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790001089023", ChargeType.Tariff, "NT15003", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000392551", ChargeType.Tariff, "SEF3 NT-01", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB1012", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Tariff, "NT1007", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790001089023", ChargeType.Subscription, "AB15001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Tariff, "NT10001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000392551", ChargeType.Subscription, "SEF2 E-50", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB1010", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB1032", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("804", "5790000701278", "8100000000047", ChargeType.Tariff, "4300", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000392551", ChargeType.Tariff, "SEF2 NT-01", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "45013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB1013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "EA-004", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("804", "5790000701278", "8100000000047", ChargeType.Subscription, "4310", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB1027", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB1025", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000392551", ChargeType.Subscription, "SEF3 E-50", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB10001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000432752", ChargeType.Tariff, "45013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000392551", ChargeType.Tariff, "SEF NT-02", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790001089023", ChargeType.Tariff, "NT15001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("533", "5790000701278", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790001089023", ChargeType.Tariff, "NT15004", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "45013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.WholesaleFixing, 24, 31),

                    ("543", "5790000701278", "5790000610976", ChargeType.Tariff, "NT1009", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.NetFromGrid, null, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.NetFromGrid, null, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.NetFromGrid, null, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.NetFromGrid, null, CalculationType.WholesaleFixing, 24, 31),

                    ("543", "5790000701278", "5790000610976", ChargeType.Tariff, "NT1010", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.OwnProduction, (SettlementMethod?)null, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000610976", ChargeType.Tariff, "NT1008", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.OwnProduction, null, CalculationType.WholesaleFixing, 24, 31),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "42030", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.OwnProduction, null, CalculationType.WholesaleFixing, 24, 31),
                ]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task
            Given_EnergySupplierAndChargeOwnerWithTotalMonthlyAmountAndSecondCorrection_Then_CorrespondingDataReturned(
                bool isEnergySupplier)
        {
            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.TotalMonthlyAmount,
                GridAreaCodes: [],
                EnergySupplierId: "5790000701278",
                ChargeOwnerId: "5790000610976",
                ChargeTypes: [],
                CalculationType: CalculationType.SecondCorrectionSettlement,
                Period: totalPeriod,
                RequestedForEnergySupplier: isEnergySupplier,
                RequestedForActorNumber: isEnergySupplier ? "5790000701278" : "5790000610976");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                    ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod,
                    ats.CalculationType, ats.Version, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    ("543", "5790000701278", "5790000610976", (ChargeType?)null, (string?)null, AmountType.TotalMonthlyAmount, Resolution.Month, (MeteringPointType?)null, (SettlementMethod?)null, CalculationType.SecondCorrectionSettlement, 4, 1)
                ]);
        }

        [Fact]
        public async Task Given_EnergySupplierWithTotalMonthlyAmountAndSecondCorrection_Then_CorrespondingDataReturned()
        {
            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.TotalMonthlyAmount,
                GridAreaCodes: [],
                EnergySupplierId: "5790000701278",
                ChargeOwnerId: null,
                ChargeTypes: [],
                CalculationType: CalculationType.SecondCorrectionSettlement,
                Period: totalPeriod,
                RequestedForEnergySupplier: true,
                RequestedForActorNumber: "5790000701278");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                    ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod,
                    ats.CalculationType, ats.Version, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    ("533", "5790000701278", (string?)null, (ChargeType?)null, (string?)null, AmountType.TotalMonthlyAmount, Resolution.Month, (MeteringPointType?)null, (SettlementMethod?)null, CalculationType.SecondCorrectionSettlement, 3, 1),
                    ("543", "5790000701278", null, null, null, AmountType.TotalMonthlyAmount, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 4, 1),
                    ("584", "5790000701278", null, null, null, AmountType.TotalMonthlyAmount, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                    ("804", "5790000701278", null, null, null, AmountType.TotalMonthlyAmount, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                ]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Given_AllQueryParametersAssignedValuesWithLatestCorrection_Then_LatestCorrectionReturned(
            bool isEnergySupplier)
        {
            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.MonthlyAmountPerCharge,
                GridAreaCodes: ["804"],
                EnergySupplierId: "5790001687137",
                ChargeOwnerId: "5790000432752",
                ChargeTypes: [("EA-003", ChargeType.Tariff)],
                CalculationType: null, // This is how we denote 'latest correction'
                Period: totalPeriod,
                RequestedForEnergySupplier: isEnergySupplier,
                RequestedForActorNumber: isEnergySupplier ? "5790001687137" : "5790000432752");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                    ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod,
                    ats.CalculationType, ats.Version, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-003", AmountType.MonthlyAmountPerCharge, Resolution.Month, (MeteringPointType?)null, (SettlementMethod?)null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                ]);
        }

        [Fact]
        public async Task Given_SomeArbitraryQueryParameters_Then_AmountAndMonthlyAndTotalHaveCorrectPeriods()
        {
            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            // Amount per charge
            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.AmountPerCharge,
                GridAreaCodes: ["804"],
                EnergySupplierId: "5790001687137",
                ChargeOwnerId: "5790000432752",
                ChargeTypes: [("EA-003", ChargeType.Tariff)],
                CalculationType: null, // This is how we denote 'latest correction'
                Period: totalPeriod,
                RequestedForEnergySupplier: true,
                RequestedForActorNumber: "5790001687137");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.Period.Start, ats.Period.End))
                .Distinct()
                .Should()
                .BeEquivalentTo([
                    (Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0))
                ]);

            // Monthly amount
            parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.MonthlyAmountPerCharge,
                GridAreaCodes: ["804"],
                EnergySupplierId: "5790001687137",
                ChargeOwnerId: "5790000432752",
                ChargeTypes: [("EA-003", ChargeType.Tariff)],
                CalculationType: null, // This is how we denote 'latest correction'
                Period: totalPeriod,
                RequestedForEnergySupplier: true,
                RequestedForActorNumber: "5790001687137");

            // Act
            actual = await Sut.GetAsync(parameters).ToListAsync();

            actual.Select(ats => (ats.Period.Start, ats.Period.End))
                .Distinct()
                .Should()
                .BeEquivalentTo([
                    (Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0))
                ]);

            // Total monthly amount
            parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.AmountPerCharge,
                GridAreaCodes: ["804"],
                EnergySupplierId: "5790001687137",
                ChargeOwnerId: "5790000432752",
                ChargeTypes: [],
                CalculationType: null, // This is how we denote 'latest correction'
                Period: totalPeriod,
                RequestedForEnergySupplier: true,
                RequestedForActorNumber: "5790001687137");

            // Act
            actual = await Sut.GetAsync(parameters).ToListAsync();

            actual.Select(ats => (ats.Period.Start, ats.Period.End))
                .Distinct()
                .Should()
                .BeEquivalentTo([
                    (Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0))
                ]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Given_ChargeOwnerForSpecificGridAreaAndLatestCorrection_Then_LatestCorrectionReturned(
            bool isEnergySupplier)
        {
            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            // The charge owner case isn't technically possible,
            // as an energy supplier must always provide an energy supplier.
            // But we keep the case for completeness.
            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.AmountPerCharge,
                GridAreaCodes: ["804", "584"],
                EnergySupplierId: null,
                ChargeOwnerId: "5790000432752",
                ChargeTypes: [],
                CalculationType: null,
                Period: totalPeriod,
                RequestedForEnergySupplier: isEnergySupplier,
                RequestedForActorNumber: isEnergySupplier ? "5790001687137" : "5790000432752");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();
            actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                    ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod,
                    ats.CalculationType, ats.Version, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "45013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),

                    ("584", "5790001095390", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790001095390", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790001095390", "5790000432752", ChargeType.Tariff, "45013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790001095390", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790001095390", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790001095390", "5790000432752", ChargeType.Tariff, "42030", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.OwnProduction, null, CalculationType.SecondCorrectionSettlement, 3, 31),

                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.Flex, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.Flex, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.Flex, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "45013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.Flex, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.Flex, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-002", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.Flex, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "45013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-002", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.ConsumptionFromGrid, (SettlementMethod?)null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-003", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.ElectricalHeating, null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.ElectricalHeating, null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.NetConsumption, null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.NetConsumption, null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.NetConsumption, null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.NetConsumption, null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "40010", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Production, null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "45012", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Production, null, CalculationType.ThirdCorrectionSettlement, 2, 31),
                ]);
        }

        [Fact]
        public async Task Given_ChargeTypeForSpecificCalculationTypeAndGridAreas_Then_CalculationTypeForChargeAndGridAreasReturned()
        {
            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.MonthlyAmountPerCharge,
                GridAreaCodes: ["804", "533", "543"],
                EnergySupplierId: null,
                ChargeOwnerId: null,
                ChargeTypes: [("40000", ChargeType.Tariff), ("AB1025", ChargeType.Subscription)],
                CalculationType: CalculationType.SecondCorrectionSettlement,
                Period: totalPeriod,
                false,
                "5790000432752");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                    ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod,
                    ats.CalculationType, ats.Version, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    ("533", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, (MeteringPointType?)null, (SettlementMethod?)null, CalculationType.SecondCorrectionSettlement, 3, 1),
                    ("533", "5790001095390", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                    ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 4, 1),
                    ("543", "5790001095390", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 4, 1),
                    ("543", "5790001687137", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 4, 1),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                ]);
        }

        [Fact]
        public async Task
            Given_ChargeOwnerRequestsWithoutChargeOwnerOrEnergySupplier_Then_DataReturnedContainsChargeOwnerChargesAndIsTaxCharges()
        {
            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.MonthlyAmountPerCharge,
                GridAreaCodes: ["804"],
                EnergySupplierId: null,
                ChargeOwnerId: null,
                ChargeTypes: [],
                CalculationType: null, // This is how we denote 'latest correction'
                Period: totalPeriod,
                RequestedForEnergySupplier: false,
                RequestedForActorNumber: "8100000000047");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                    ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod, ats.CalculationType,
                    ats.Version, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    // Tax charges for grid area
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.MonthlyAmountPerCharge, Resolution.Month, (MeteringPointType?)null, (SettlementMethod?)null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-002", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "EA-003", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    // Charge owners own charges
                    ("804", "5790001687137", "8100000000047", ChargeType.Tariff, "100", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790000701278", "8100000000047", ChargeType.Tariff, "4300", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790001687137", "8100000000047", ChargeType.Tariff, "4300", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790001687137", "8100000000047", ChargeType.Tariff, "Rabat-T", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790001687137", "8100000000047", ChargeType.Tariff, "Tarif_Ny", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790001687137", "8100000000047", ChargeType.Subscription, "100", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790000701278", "8100000000047", ChargeType.Subscription, "4310", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790001687137", "8100000000047", ChargeType.Subscription, "4310", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                    ("804", "5790001687137", "8100000000047", ChargeType.Subscription, "Abb Flex", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.ThirdCorrectionSettlement, 2, 1),
                ]);
        }
    }

    public class WholesaleServicesQueriesCsvTestsWithIndividualData
        : TestBase<WholesaleServicesQueries>, IClassFixture<MigrationsFreeDatabricksSqlStatementApiFixture>
    {
        private readonly MigrationsFreeDatabricksSqlStatementApiFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;

        public WholesaleServicesQueriesCsvTestsWithIndividualData(
            MigrationsFreeDatabricksSqlStatementApiFixture fixture,
            ITestOutputHelper testOutputHelper)
        {
            Fixture.Inject(fixture.DatabricksSchemaManager.DeltaTableOptions);
            Fixture.Inject(fixture.GetDatabricksExecutor());
            Fixture.Inject(new WholesaleServicesQuerySnippetProviderFactory([new AmountsPerChargeWholesaleServicesDatabricksContract(), new MonthlyAmountsPerChargeWholesaleServicesDatabricksContract(), new TotalMonthlyAmountWholesaleServicesDatabricksContract()]));
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Given_EnergySupplierOnlyHaveDataForHalfOfThePeriod_Then_DataReturnedWithModifiedPeriod(
            bool isEnergySupplier)
        {
            /*
             Business case example:
             When a new Energy Supplier is being made responsible for a metering point in the middle of the month,
             and they do not yet have a metering point in the grid area from the beginning of the month.
             The result is that the Energy Supplier will only have results for the last half of the month.
            */

            await ClearAndAddDatabricksDataAsync(_fixture, _testOutputHelper);
            await RemoveDataForEnergySupplierInTimespan(
                _fixture,
                _testOutputHelper,
                "5790001687137",
                Instant.FromUtc(2022, 1, 15, 0, 0),
                null);

            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.AmountPerCharge,
                GridAreaCodes: ["804"],
                EnergySupplierId: "5790001687137",
                ChargeOwnerId: "5790000432752",
                ChargeTypes: [("EA-001", ChargeType.Tariff)],
                CalculationType: CalculationType.SecondCorrectionSettlement,
                Period: totalPeriod,
                RequestedForEnergySupplier: isEnergySupplier,
                RequestedForActorNumber: isEnergySupplier ? "5790001687137" : "5790000432752");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.MeteringPointType, ats.SettlementMethod, ats.Period.Start,
                    ats.Period.End, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    (MeteringPointType.Consumption, SettlementMethod.Flex, Instant.FromUtc(2022, 1, 15, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 16),
                    (MeteringPointType.Consumption, SettlementMethod.NonProfiled, Instant.FromUtc(2022, 1, 15, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 16),
                    (MeteringPointType.ElectricalHeating, (SettlementMethod?)null, Instant.FromUtc(2022, 1, 15, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 16),
                    (MeteringPointType.NetConsumption, (SettlementMethod?)null, Instant.FromUtc(2022, 1, 15, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 16),
                ]);
        }

        [Fact]
        public async Task Given_EnergySupplierWithAHoleInData_Then_DataReturnedInTwoChunkWithoutAHole()
        {
            await ClearAndAddDatabricksDataAsync(_fixture, _testOutputHelper);
            await RemoveDataForEnergySupplierInTimespan(
                _fixture,
                _testOutputHelper,
                "5790001687137",
                Instant.FromUtc(2022, 1, 20, 0, 0),
                Instant.FromUtc(2022, 1, 10, 0, 0));

            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.AmountPerCharge,
                GridAreaCodes: ["804"],
                EnergySupplierId: "5790001687137",
                ChargeOwnerId: "5790000432752",
                ChargeTypes: [("EA-001", ChargeType.Tariff)],
                CalculationType: CalculationType.SecondCorrectionSettlement,
                Period: totalPeriod,
                RequestedForEnergySupplier: true,
                RequestedForActorNumber: "5790001687137");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.MeteringPointType, ats.SettlementMethod, ats.Period.Start,
                    ats.Period.End, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    (MeteringPointType.Consumption, SettlementMethod.Flex, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 10, 23, 0), 10),
                    (MeteringPointType.Consumption, SettlementMethod.Flex, Instant.FromUtc(2022, 1, 20, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 11),
                    (MeteringPointType.Consumption, SettlementMethod.NonProfiled, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 10, 23, 0), 10),
                    (MeteringPointType.Consumption, SettlementMethod.NonProfiled, Instant.FromUtc(2022, 1, 20, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 11),
                    (MeteringPointType.ElectricalHeating, (SettlementMethod?)null, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 10, 23, 0), 10),
                    (MeteringPointType.ElectricalHeating, (SettlementMethod?)null, Instant.FromUtc(2022, 1, 20, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 11),
                    (MeteringPointType.NetConsumption, (SettlementMethod?)null, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 10, 23, 0), 10),
                    (MeteringPointType.NetConsumption, (SettlementMethod?)null, Instant.FromUtc(2022, 1, 20, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 11),
                ]);

            // First chunk should have data up to 2022-01-10
            actual.Where(x => x.TimeSeriesPoints.First().Time == new DateTimeOffset(2021, 12, 31, 23, 0, 0, TimeSpan.Zero))
                .Should().AllSatisfy(ats =>
            {
                ats.TimeSeriesPoints.Select(wtsp => wtsp.Time).Should().Equal([
                    new DateTimeOffset(2021, 12, 31, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 1, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 2, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 3, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 4, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 5, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 6, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 7, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 8, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 9, 23, 0, 0, TimeSpan.Zero),
                ]);
            });

            // Second chunk should have data from 2022-01-20
            actual.Where(x => x.TimeSeriesPoints.First().Time != new DateTimeOffset(2021, 12, 31, 23, 0, 0, TimeSpan.Zero))
                .Should().AllSatisfy(ats =>
            {
                ats.TimeSeriesPoints.Select(wtsp => wtsp.Time).Should().Equal([
                    new DateTimeOffset(2022, 1, 20, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 21, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 22, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 23, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 24, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 25, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 26, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 27, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 28, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 29, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 30, 23, 0, 0, TimeSpan.Zero),
                ]);
            });
        }

        [Fact]
        public async Task Given_GridOwnerWithLatestCorrectionButNoCorrectionData_Then_NoDataReturned()
        {
            await ClearAndAddDatabricksDataAsync(_fixture, _testOutputHelper);
            await RemoveDataForCorrections(_fixture, _testOutputHelper, []);

            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.AmountPerCharge,
                GridAreaCodes: ["804", "584"],
                EnergySupplierId: null,
                ChargeOwnerId: null,
                ChargeTypes: [],
                CalculationType: null,
                Period: totalPeriod,
                RequestedForEnergySupplier: false,
                RequestedForActorNumber: "5790000432752");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task
            Given_EnergySupplierAndChargeOwnerWithLatestCorrectionButOnlyOneGridAreaWithCorrectionData_Then_DataReturnedForGridArea(
                bool isEnergySupplier)
        {
            await ClearAndAddDatabricksDataAsync(_fixture, _testOutputHelper);
            await RemoveDataForCorrections(_fixture, _testOutputHelper, ["804", "543"]);

            var totalPeriod = new Period(
                Instant.FromUtc(2021, 12, 31, 23, 0),
                Instant.FromUtc(2022, 1, 31, 23, 0));

            var parameters = new WholesaleServicesQueryParameters(
                AmountType: AmountType.AmountPerCharge,
                GridAreaCodes: ["543", "584"],
                EnergySupplierId: "5790000701278",
                ChargeOwnerId: "5790000432752",
                ChargeTypes: [],
                CalculationType: null,
                Period: totalPeriod,
                RequestedForEnergySupplier: isEnergySupplier,
                RequestedForActorNumber: isEnergySupplier ? "5790000701278" : "5790000432752");

            // Act
            var actual = await Sut.GetAsync(parameters).ToListAsync();

            using var assertionScope = new AssertionScope();
            actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                    ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod,
                    ats.CalculationType, ats.Version, ats.TimeSeriesPoints.Count))
                .Should()
                .BeEquivalentTo([
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "EA-001", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "41000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "45013", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                    ("584", "5790000701278", "5790000432752", ChargeType.Tariff, "42000", AmountType.AmountPerCharge, Resolution.Day, MeteringPointType.Consumption, SettlementMethod.NonProfiled, CalculationType.SecondCorrectionSettlement, 3, 31),
                ]);
        }
    }

    private static async Task ClearAndAddDatabricksDataAsync(
        MigrationsFreeDatabricksSqlStatementApiFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        using (new PerformanceLogger(testOutputHelper, "ClearAndAddDatabricksDataAsync"))
        {
            using (new PerformanceLogger(testOutputHelper, "Drop databricks schema"))
            {
                await fixture.DatabricksSchemaManager.DropSchemaAsync();
            }

            using (new PerformanceLogger(testOutputHelper, "Create databricks schema"))
            {
                await fixture.DatabricksSchemaManager.CreateSchemaAsync();
            }

            const string view1 = "wholesale_calculation_results.amounts_per_charge_v1.csv";
            var view1File = Path.Combine("TestData", view1);

            const string view2 = "wholesale_calculation_results.monthly_amounts_per_charge_v1.csv";
            var view2File = Path.Combine("TestData", view2);

            const string view3 = "wholesale_calculation_results.total_monthly_amounts_v1.csv";
            var view3File = Path.Combine("TestData", view3);

            using (new PerformanceLogger(testOutputHelper, "Insert amounts_per_charge in databricks"))
            {
                await fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
                    fixture.DatabricksSchemaManager.DeltaTableOptions.Value.AMOUNTS_PER_CHARGE_V1_VIEW_NAME,
                    AmountsPerChargeViewSchemaDefinition.SchemaDefinition,
                    view1File);
            }

            using (new PerformanceLogger(testOutputHelper, "Insert monthly_amounts_per_charge in databricks"))
            {
                await fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
                    fixture.DatabricksSchemaManager.DeltaTableOptions.Value.MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME,
                    MonthlyAmountsPerChargeViewSchemaDefinition.SchemaDefinition,
                    view2File);
            }

            using (new PerformanceLogger(testOutputHelper, "Insert total_monthly_amounts_per_charge in databricks"))
            {
                await fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
                    fixture.DatabricksSchemaManager.DeltaTableOptions.Value.TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME,
                    TotalMonthlyAmountsViewSchemaDefinition.SchemaDefinition,
                    view3File);
            }
        }
    }

    private static async Task RemoveDataForEnergySupplierInTimespan(
        MigrationsFreeDatabricksSqlStatementApiFixture fixture,
        ITestOutputHelper testOutputHelper,
        string energySupplierId,
        Instant before,
        Instant? after)
    {
        foreach (var amountType in Enum.GetValues<AmountType>())
        {
            var statement = new DeleteEnergySupplierStatement(
                fixture.DatabricksSchemaManager.DeltaTableOptions.Value,
                energySupplierId,
                before,
                after,
                amountType);

            using (new PerformanceLogger(testOutputHelper, $"Execute DeleteEnergySupplierStatement for {amountType} in databricks"))
            {
                await fixture.GetDatabricksExecutor().ExecuteStatementAsync(statement, Format.JsonArray).ToListAsync();
            }
        }
    }

    private static async Task RemoveDataForCorrections(
        MigrationsFreeDatabricksSqlStatementApiFixture fixture,
        ITestOutputHelper testOutputHelper,
        IReadOnlyCollection<string> gridAreasToRemoveFrom)
    {
        foreach (var amountType in Enum.GetValues<AmountType>())
        {
            var statement = new DeleteCorrectionsStatement(
                fixture.DatabricksSchemaManager.DeltaTableOptions.Value,
                gridAreasToRemoveFrom,
                amountType);

            using (new PerformanceLogger(testOutputHelper, $"Execute DeleteCorrectionsStatement for {amountType} in databricks"))
            {
                await fixture.GetDatabricksExecutor().ExecuteStatementAsync(statement, Format.JsonArray).ToListAsync();
            }
        }
    }

    private class DeleteEnergySupplierStatement(
        DeltaTableOptions deltaTableOptions,
        string energySupplierId,
        Instant before,
        Instant? after,
        AmountType amountType) : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly string _energySupplierId = energySupplierId;
        private readonly Instant _before = before;
        private readonly Instant? _after = after;

        private readonly IWholesaleServicesDatabricksContract _helper = amountType switch
        {
            AmountType.AmountPerCharge => new AmountsPerChargeWholesaleServicesDatabricksContract(),
            AmountType.MonthlyAmountPerCharge => new MonthlyAmountsPerChargeWholesaleServicesDatabricksContract(),
            AmountType.TotalMonthlyAmount => new TotalMonthlyAmountWholesaleServicesDatabricksContract(),
            _ => throw new ArgumentOutOfRangeException(nameof(amountType), amountType, null),
        };

        protected override string GetSqlStatement()
        {
            return $"""
                    DELETE FROM {_helper.GetSource(_deltaTableOptions)}
                    WHERE {_helper.GetEnergySupplierIdColumnName()} = '{_energySupplierId}'
                    AND {_helper.GetTimeColumnName()} <= '{_before}'
                    {(_after is not null ? $"AND {_helper.GetTimeColumnName()} > '{_after}'" : string.Empty)}
                    """;
        }
    }

    private class DeleteCorrectionsStatement(
        DeltaTableOptions deltaTableOptions,
        IReadOnlyCollection<string> gridAreasToRemoveFrom,
        AmountType amountType) : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly IReadOnlyCollection<string> _gridAreasToRemoveFrom = gridAreasToRemoveFrom;

        private readonly IWholesaleServicesDatabricksContract _helper = amountType switch
        {
            AmountType.AmountPerCharge => new AmountsPerChargeWholesaleServicesDatabricksContract(),
            AmountType.MonthlyAmountPerCharge => new MonthlyAmountsPerChargeWholesaleServicesDatabricksContract(),
            AmountType.TotalMonthlyAmount => new TotalMonthlyAmountWholesaleServicesDatabricksContract(),
            _ => throw new ArgumentOutOfRangeException(nameof(amountType), amountType, null),
        };

        protected override string GetSqlStatement()
        {
            return $"""
                    DELETE FROM {_helper.GetSource(_deltaTableOptions)}
                    WHERE ({_helper.GetCalculationTypeColumnName()} = '{DeltaTableCalculationType.FirstCorrectionSettlement}'
                    OR {_helper.GetCalculationTypeColumnName()} = '{DeltaTableCalculationType.SecondCorrectionSettlement}'
                    OR {_helper.GetCalculationTypeColumnName()} = '{DeltaTableCalculationType.ThirdCorrectionSettlement}')
                    {(_gridAreasToRemoveFrom.Any() ? $"AND {_helper.GetGridAreaCodeColumnName()} IN ({string.Join(", ", _gridAreasToRemoveFrom.Select(ga => $"'{ga}'"))})" : string.Empty)}
                    """;
        }
    }
}
