﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Globalization;
using AutoFixture;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.CalculationResults;

public class WholesaleResultQueriesTests : TestBase<WholesaleResultQueries>, IClassFixture<DatabricksSqlStatementApiFixture>
{
    private const string CalculationId = "019703e7-98ee-45c1-b343-0cbf185a47d9";
    private const string HourlyTariffCalculationResultId = "12345678-98ee-45c1-b343-0cbf185a47d9";
    private const string MonthlyAmountTariffCalculationResultId = "23456789-98ee-45c1-b343-0cbf185a47d9";
    private const string DefaultHourlyAmount = "2.34567";
    private const string DefaultMonthlyAmount = "1.123456";

    private readonly DatabricksSqlStatementApiFixture _fixture;
    private readonly Mock<ICalculationsClient> _calculationsClientMock;

    public WholesaleResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
        _calculationsClientMock = Fixture.Freeze<Mock<ICalculationsClient>>();
        Fixture.Inject(_fixture.DatabricksSchemaManager.DeltaTableOptions);
        Fixture.Inject(_fixture.GetDatabricksExecutor());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_WhenCalculationHasHourlyAndMonthlyTariff_ReturnsExpectedWholesaleResult(CalculationDto calculation)
    {
        // Arrange
        await InsertHourlyTariffAndMonthlyAmountTariffRowsAsync();
        calculation = calculation with { CalculationId = Guid.Parse(CalculationId) };
        _calculationsClientMock
            .Setup(b => b.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync(calculation);

        // Act
        var actual = await Sut.GetAsync(calculation.CalculationId).ToListAsync();

        // Assert
        using var assertionScope = new AssertionScope();
        var actualHourlyAmount = actual.Single(row => row.Id.ToString() == HourlyTariffCalculationResultId);
        actualHourlyAmount.AmountType.Should().Be(AmountType.AmountPerCharge);
        actualHourlyAmount.Resolution.Should().Be(Resolution.Hour);
        actualHourlyAmount.TimeSeriesPoints.First().Amount.Should().Be(decimal.Parse(DefaultHourlyAmount, CultureInfo.InvariantCulture));

        var actualMonthlyAmount = actual.Single(row => row.Id.ToString() == MonthlyAmountTariffCalculationResultId);
        actualMonthlyAmount.AmountType.Should().Be(AmountType.MonthlyAmountPerCharge);
        actualMonthlyAmount.Resolution.Should().Be(Resolution.Month);
        actualMonthlyAmount.TimeSeriesPoints.First().Amount.Should().Be(decimal.Parse(DefaultMonthlyAmount, CultureInfo.InvariantCulture));
    }

    private async Task InsertHourlyTariffAndMonthlyAmountTariffRowsAsync()
    {
        var hourlyTariffRow = WholesaleResultDeltaTableHelper.CreateRowValues(
            calculationId: CalculationId,
            calculationResultId: HourlyTariffCalculationResultId,
            chargeType: "tariff",
            resolution: "PT1H",
            amountType: "amount_per_charge",
            meteringPointType: "production",
            settlementMethod: null,
            amount: DefaultHourlyAmount);

        var monthlyAmountTariffRow = WholesaleResultDeltaTableHelper.CreateRowValues(
            calculationId: CalculationId,
            calculationResultId: MonthlyAmountTariffCalculationResultId,
            chargeType: "tariff",
            resolution: "P1M",
            amountType: "monthly_amount_per_charge",
            meteringPointType: null,
            settlementMethod: null,
            amount: DefaultMonthlyAmount);

        var rows = new List<IReadOnlyCollection<string?>> { hourlyTariffRow, monthlyAmountTariffRow };
        var wholesaleResultTableName =
            _fixture.DatabricksSchemaManager.DeltaTableOptions.Value.WHOLESALE_RESULTS_TABLE_NAME;
        await _fixture.DatabricksSchemaManager.InsertAsync<WholesaleResultColumnNames>(wholesaleResultTableName, rows);
    }
}
