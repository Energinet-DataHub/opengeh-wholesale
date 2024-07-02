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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using Xunit;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;
using Resolution =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.RequestCalculationResult;

public class WholesaleServicesQueriesCsvTests : TestBase<WholesaleServicesQueries>,
    IClassFixture<MigrationsFreeDatabricksSqlStatementApiFixture>
{
    private const string EnergySupplierOne = "5790001095390";
    private const string EnergySupplierTwo = "5790000701278";
    private const string EnergySupplierThree = "5790001687137";

    private const string ChargeOwnerOne = "5790000432752";
    private const string ChargeOwnerTwo = "5790000392551";

    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _fixture;

    public WholesaleServicesQueriesCsvTests(MigrationsFreeDatabricksSqlStatementApiFixture fixture)
    {
        Fixture.Inject(fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(fixture.GetDatabricksExecutor());
        _fixture = fixture;
    }

    [Fact]
    public async Task Energy_supplier_with_amount_per_charge_and_wholesale_fixing()
    {
        await ClearAndAddDatabricksDataAsync();

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
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();
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

        using var assertionScope = new AssertionScope();
    }

    [Fact]
    public async Task Query_parameters_all_assigned_values_with_latest_correction()
    {
        await ClearAndAddDatabricksDataAsync();

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
            Period: totalPeriod);

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
    public async Task Charge_owner_for_specific_grid_area_and_latest_correction()
    {
        await ClearAndAddDatabricksDataAsync();

        var totalPeriod = new Period(
            Instant.FromUtc(2021, 12, 31, 23, 0),
            Instant.FromUtc(2022, 1, 31, 23, 0));

        var parameters = new WholesaleServicesQueryParameters(
            AmountType: AmountType.AmountPerCharge,
            GridAreaCodes: ["804", "584"],
            EnergySupplierId: null,
            ChargeOwnerId: "5790000432752",
            ChargeTypes: [],
            CalculationType: null,
            Period: totalPeriod);

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
    public async Task Charge_type_for_specific_calculation_type_and_grid_areas()
    {
        await ClearAndAddDatabricksDataAsync();

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
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => (ats.GridArea, ats.EnergySupplierId, ats.ChargeOwnerId, ats.ChargeType, ats.ChargeCode,
                ats.AmountType, ats.Resolution, ats.MeteringPointType, ats.SettlementMethod,
                ats.CalculationType, ats.Version, ats.TimeSeriesPoints.Count))
            .Should()
            .BeEquivalentTo([
                ("543", "5790000701278", "5790000610976", ChargeType.Subscription, "AB1025", AmountType.MonthlyAmountPerCharge, Resolution.Month, (MeteringPointType?)null, (SettlementMethod?)null, CalculationType.SecondCorrectionSettlement, 3, 1),

                ("533", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                ("533", "5790001095390", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                ("543", "5790000701278", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                ("543", "5790001095390", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                ("543", "5790001687137", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
                ("804", "5790001687137", "5790000432752", ChargeType.Tariff, "40000", AmountType.MonthlyAmountPerCharge, Resolution.Month, null, null, CalculationType.SecondCorrectionSettlement, 3, 1),
            ]);
    }

    private async Task ClearAndAddDatabricksDataAsync()
    {
        await _fixture.DatabricksSchemaManager.DropSchemaAsync();
        await _fixture.DatabricksSchemaManager.CreateSchemaAsync();

        const string wholesaleOutputWholesaleResultsCsv = "wholesale_output.wholesale_results.csv";
        var wholesaleTestFile = Path.Combine("TestData", wholesaleOutputWholesaleResultsCsv);

        const string basisDataCalculationsCsv = "basis_data.calculations.csv";
        var basisDataTestFile = Path.Combine("TestData", basisDataCalculationsCsv);

        await _fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.CALCULATIONS_TABLE_NAME,
            BasisDataCalculationsTableSchemaDefinition.SchemaDefinition,
            basisDataTestFile);

        await _fixture.DatabricksSchemaManager.InsertFromCsvFileAsync(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_TABLE_NAME,
            WholesaleResultsTableSchemaDefinition.SchemaDefinition,
            wholesaleTestFile);
    }
}
