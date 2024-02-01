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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Application.Calculations;

public class CreateCalculationHandlerTests
{
    private readonly CreateCalculationCommand _defaultCreateCalculationCommand;

    public CreateCalculationHandlerTests()
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        var periodStart = period.PeriodStart.ToDateTimeOffset();
        var periodEnd = period.PeriodEnd.ToDateTimeOffset();
        var gridAreaCodes = new List<string> { "805" };
        _defaultCreateCalculationCommand = CreateCalculationCommand(CalculationType.Aggregation, periodStart, periodEnd, gridAreaCodes);
    }

    [Theory]
    [InlineAutoMoqData(CalculationType.BalanceFixing)]
    [InlineAutoMoqData(CalculationType.Aggregation)]
    [InlineAutoMoqData(CalculationType.WholesaleFixing)]
    [InlineAutoMoqData(CalculationType.FirstCorrectionSettlement)]
    [InlineAutoMoqData(CalculationType.SecondCorrectionSettlement)]
    [InlineAutoMoqData(CalculationType.ThirdCorrectionSettlement)]
    public async Task Handle_AddsCalculationToRepository(
        CalculationType calculationType,
        [Frozen] Mock<ICalculationFactory> calculationFactoryMock,
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        CreateCalculationHandler sut)
    {
        // Arrange
        var calculationCommand = _defaultCreateCalculationCommand with { CalculationType = calculationType };
        var calculation = CreateCalculationFromCommand(calculationCommand);
        calculationFactoryMock.Setup(x => x.Create(calculation.CalculationType, calculationCommand.GridAreaCodes, calculationCommand.StartDate, calculationCommand.EndDate, calculationCommand.CreatedByUserId))
            .Returns(calculation);

        // Act
        var actual = await sut.HandleAsync(calculationCommand);

        // Assert
        calculation.Id.Should().Be(actual);
        calculationRepositoryMock.Verify(x => x.AddAsync(calculation));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Handle_LogsExpectedMessage(
        [Frozen] Mock<ILogger<CreateCalculationHandler>> loggerMock,
        [Frozen] Mock<ICalculationFactory> calculationFactoryMock,
        CreateCalculationHandler sut)
    {
        // Arrange
        const string expectedLogMessage = $"Calculation created with id {LoggingConstants.CalculationId}";
        var calculation = CreateCalculationFromCommand(_defaultCreateCalculationCommand);
        calculationFactoryMock.Setup(x => x.Create(calculation.CalculationType, _defaultCreateCalculationCommand.GridAreaCodes, _defaultCreateCalculationCommand.StartDate, _defaultCreateCalculationCommand.EndDate, _defaultCreateCalculationCommand.CreatedByUserId))
            .Returns(calculation);

        // Act
        await sut.HandleAsync(_defaultCreateCalculationCommand);

        // Assert
        loggerMock.ShouldBeCalledWith(LogLevel.Information, expectedLogMessage);
    }

    [Theory]
    [InlineData(CalculationType.WholesaleFixing)]
    [InlineData(CalculationType.FirstCorrectionSettlement)]
    [InlineData(CalculationType.SecondCorrectionSettlement)]
    [InlineData(CalculationType.ThirdCorrectionSettlement)]
    public void Handle_WhenPeriodNotOneMonthForCertainCalculationTypes_ThrowBusinessValidationException(CalculationType calculationType)
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse("2021-12-31T23:00Z");
        var periodEnd = DateTimeOffset.Parse("2022-01-30T23:00Z");
        var calculationCommand = _defaultCreateCalculationCommand with { StartDate = periodStart, EndDate = periodEnd, CalculationType = calculationType };

        // Act
        var actual = () => CreateCalculationFromCommand(calculationCommand);

        // Assert
        actual.Should().Throw<BusinessValidationException>();
    }

    [Theory]
    [InlineData("2021-12-31T23:00Z", "2022-01-31T22:00Z")]
    [InlineData("2021-12-31T22:00Z", "2022-01-31T23:00Z")]
    public void Handle_WhenPeriodIsNotMidnight_ThrowBusinessValidationException(
        string periodStartString,
        string periodEndString)
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse(periodStartString);
        var periodEnd = DateTimeOffset.Parse(periodEndString);
        var calculationCommand = _defaultCreateCalculationCommand with { StartDate = periodStart, EndDate = periodEnd };

        // Act
        var actual = () => CreateCalculationFromCommand(calculationCommand);

        // Assert
        actual.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void Handle_WhenPeriodStartIsLaterThePeriodEnd_ThrowBusinessValidationException()
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse("2022-12-31T23:00Z");
        var periodEnd = DateTimeOffset.Parse("2022-01-31T23:00Z");
        var calculationCommand = _defaultCreateCalculationCommand with { StartDate = periodStart, EndDate = periodEnd };

        // Act
        var actual = () => CreateCalculationFromCommand(calculationCommand);

        // Assert
        actual.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void Handle_WhenNoGridAreaCodes_ThrowBusinessValidationException()
    {
        // Arrange
        var calculationCommand = _defaultCreateCalculationCommand with { GridAreaCodes = new List<string>() };

        // Act
        var actual = () => CreateCalculationFromCommand(calculationCommand);

        // Assert
        actual.Should().Throw<BusinessValidationException>();
    }

    private static CreateCalculationCommand CreateCalculationCommand(CalculationType calculationType, DateTimeOffset periodStart, DateTimeOffset periodEnd, IEnumerable<string> gridAreaCodes)
    {
        return new CreateCalculationCommand(
            calculationType,
            gridAreaCodes,
            periodStart,
            periodEnd,
            Guid.NewGuid());
    }

    private static Calculation CreateCalculationFromCommand(CreateCalculationCommand command)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            command.CalculationType,
            command.GridAreaCodes.Select(x => new GridAreaCode(x)).ToList(),
            command.StartDate.ToInstant(),
            command.EndDate.ToInstant(),
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone,
            command.CreatedByUserId,
            SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Ticks);
    }
}
