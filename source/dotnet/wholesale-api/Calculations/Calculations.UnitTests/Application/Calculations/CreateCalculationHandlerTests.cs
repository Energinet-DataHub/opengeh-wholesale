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
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Test.Core;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Application.Calculations;

[UnitTest]
public class CreateCalculationHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task Handle_AddsCalculationToRepository(
        [Frozen] Mock<ICalculationFactory> calculationFactoryMock,
        [Frozen] Mock<ICalculationRepository> calculationRepositoryMock,
        CreateCalculationHandler sut)
    {
        // Arrange
        var calculationCommand = CreateCalculationCommand();
        var calculation = CreateCalculationFromCommand(calculationCommand);
        calculationFactoryMock.Setup(x => x.Create(calculation.ProcessType, calculationCommand.GridAreaCodes, calculationCommand.StartDate, calculationCommand.EndDate, calculationCommand.CreatedByUserId))
            .Returns(calculation);

        // Act
        var actual = await sut.HandleAsync(calculationCommand);

        // Assert
        calculation.Id.Should().Be(actual);
        calculationRepositoryMock.Verify(x => x.AddAsync(calculation));
    }

    private static CreateCalculationCommand CreateCalculationCommand()
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new CreateCalculationCommand(
            ProcessType.BalanceFixing,
            new List<string> { "805" },
            period.PeriodStart.ToDateTimeOffset(),
            period.PeriodEnd.ToDateTimeOffset(),
            Guid.NewGuid());
    }

    private static Calculation CreateCalculationFromCommand(CreateCalculationCommand command)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            ProcessType.BalanceFixing,
            command.GridAreaCodes.Select(x => new GridAreaCode(x)).ToList(),
            command.StartDate.ToInstant(),
            command.EndDate.ToInstant(),
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone,
            command.CreatedByUserId);
    }
}
