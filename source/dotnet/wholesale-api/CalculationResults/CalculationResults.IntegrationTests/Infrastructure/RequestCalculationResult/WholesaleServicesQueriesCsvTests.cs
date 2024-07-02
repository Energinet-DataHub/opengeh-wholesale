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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
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
    private readonly MigrationsFreeDatabricksSqlStatementApiFixture _fixture;

    public WholesaleServicesQueriesCsvTests(MigrationsFreeDatabricksSqlStatementApiFixture fixture)
    {
        Fixture.Inject(fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(fixture.GetDatabricksExecutor());
        _fixture = fixture;
    }

    [Fact]
    public async Task Given_EnergySupplierWithAmountPerChargeAndWholesaleFixing_Then_CorrespondingDataReturned()
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
    public async Task Given_AllQueryParametersAssignedValuesWithLatestCorrection_Then_LatestCorrectionReturned()
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
    public async Task Given_ChargeOwnerForSpecificGridAreaAndLatestCorrection_Then_LatestCorrectionReturned()
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
    public async Task Given_ChargeTypeForSpecificCalculationTypeAndGridAreas_Then_CalculationTypeForChargeAndGridAreasReturned()
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
        await DoTheThing("5790001687137", Instant.FromUtc(2022, 1, 15, 0, 0), null);

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
            Period: totalPeriod);

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
    public async Task Given_EnergySupplierWithAHoleInData_Then_DataReturnedInOneChunkWithAHole()
    {
        await ClearAndAddDatabricksDataAsync();
        await DoTheThing("5790001687137", Instant.FromUtc(2022, 1, 20, 0, 0), Instant.FromUtc(2022, 1, 10, 0, 0));

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
            Period: totalPeriod);

        // Act
        var actual = await Sut.GetAsync(parameters).ToListAsync();

        using var assertionScope = new AssertionScope();
        actual.Select(ats => (ats.MeteringPointType, ats.SettlementMethod, ats.Period.Start,
                ats.Period.End, ats.TimeSeriesPoints.Count))
            .Should()
            .BeEquivalentTo([
                (MeteringPointType.Consumption, SettlementMethod.Flex, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 21),
                (MeteringPointType.Consumption, SettlementMethod.NonProfiled, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 21),
                (MeteringPointType.ElectricalHeating, (SettlementMethod?)null, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 21),
                (MeteringPointType.NetConsumption, (SettlementMethod?)null, Instant.FromUtc(2021, 12, 31, 23, 0), Instant.FromUtc(2022, 1, 31, 23, 0), 21),
            ]);

        actual.Should().AllSatisfy(ats =>
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

    private async Task DoTheThing(string energySupplierId, Instant before, Instant? after)
    {
        var statement = new DeleteStatement(
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value,
            energySupplierId,
            before,
            after);

        await _fixture.GetDatabricksExecutor().ExecuteStatementAsync(statement, Format.JsonArray).ToListAsync();
    }

    private class DeleteStatement(DeltaTableOptions deltaTableOptions, string energySupplierId, Instant before, Instant? after) : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly string _energySupplierId = energySupplierId;
        private readonly Instant _before = before;
        private readonly Instant? _after = after;

        protected override string GetSqlStatement()
        {
            return $"""
                    DELETE FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.WHOLESALE_RESULTS_TABLE_NAME}
                    WHERE {WholesaleResultColumnNames.EnergySupplierId} = '{_energySupplierId}'
                    AND {WholesaleResultColumnNames.Time} <= '{_before}'
                    {(_after is not null ? $"AND {WholesaleResultColumnNames.Time} > '{_after}'" : string.Empty)}
                    """;
        }
    }
}
