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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using FluentAssertions;
using Google.Protobuf;
using Moq;
using NodaTime;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.EventPublishers;

public class CalculationResultCompletedToIntegrationEventFactoryTests
{
    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_ReturnAnIntegrationEventDtoWithTheCorrectValues(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        [Frozen] Mock<IClock> clockMock,
        CalculationResultCompleted calculationResultCompleted,
        CalculationResult calculationResult,
        CalculationResultIntegrationEventFactory sut)
    {
        // Arrange
        calculationResult.SetPrivateProperty(r => r.BalanceResponsibleId, null);
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForEnergySupplier(calculationResult, calculationResult.EnergySupplierId!))
            .Returns(calculationResultCompleted);

        var instant = SystemClock.Instance.GetCurrentInstant();
        clockMock.Setup(x => x.GetCurrentInstant())
            .Returns(instant);

        // Act
        var actual = sut.CreateForEnergySupplier(calculationResult);

        // Assert
        actual.MessageName.Should().Be(CalculationResultCompleted.MessageName);
        actual.Message.ToByteArray().Should().BeEquivalentTo(calculationResultCompleted.ToByteArray());
        actual.OperationTimeStamp.Should().Be(instant);
        actual.MessageVersion.Should().Be(CalculationResultCompleted.MessageVersion);
        actual.EventIdentification.Should().Be(calculationResult.Id);
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForBalanceResponsibleParty(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        CalculationResult calculationResult,
        CalculationResultIntegrationEventFactory sut)
    {
        // Arrange
        calculationResult.SetPrivateProperty(r => r.EnergySupplierId, null);
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForBalanceResponsibleParty(calculationResult, calculationResult.BalanceResponsibleId!))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForBalanceResponsibleParty(calculationResult);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(x => x.CreateForBalanceResponsibleParty(calculationResult, calculationResult.BalanceResponsibleId!));
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForGridArea(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        CalculationResult calculationResult,
        CalculationResultIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForGridArea(calculationResult))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForTotalGridArea(calculationResult);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(x => x.CreateForGridArea(calculationResult));
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForEnergySupplierByBalanceResponsibleParty(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        CalculationResult calculationResult,
        CalculationResultIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForEnergySupplierByBalanceResponsibleParty(calculationResult, calculationResult.EnergySupplierId!, calculationResult.BalanceResponsibleId!))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForEnergySupplierByBalanceResponsibleParty(calculationResult);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(
            x => x.CreateForEnergySupplierByBalanceResponsibleParty(calculationResult, calculationResult.EnergySupplierId!, calculationResult.BalanceResponsibleId!));
    }
}
