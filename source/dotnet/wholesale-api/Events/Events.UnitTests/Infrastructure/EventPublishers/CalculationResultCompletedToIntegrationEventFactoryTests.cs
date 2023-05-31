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
using Energinet.DataHub.Wholesale.Events.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Google.Protobuf;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.EventPublishers;

public class CalculationResultCompletedToIntegrationEventFactoryTests
{
    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_ReturnAnIntegrationEventDtoWithTheCorrectValues(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IIntegrationEventTypeMapper> integrationEventTypeMapperMock,
        CalculationResultCompleted calculationResultCompleted,
        ProcessStepResult processStepResult,
        ProcessCompletedEventDto processCompletedEventDto,
        string energySupplierGln,
        CalculationResultCompletedToIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForEnergySupplier(processStepResult, processCompletedEventDto, energySupplierGln))
            .Returns(calculationResultCompleted);

        var instant = SystemClock.Instance.GetCurrentInstant();
        clockMock.Setup(x => x.GetCurrentInstant())
            .Returns(instant);

        integrationEventTypeMapperMock
            .Setup(x => x.GetMessageType(calculationResultCompleted.GetType()))
            .Returns(typeof(CalculationResultCompleted).ToString);

        // Act
        var actual = sut.CreateForEnergySupplier(processStepResult, processCompletedEventDto, energySupplierGln);

        // Assert
        Assert.Equal(calculationResultCompleted.GetType().ToString(), actual.MessageType);
        Assert.Equal(MessageExtensions.ToByteArray(calculationResultCompleted), actual.EventData);
        Assert.Equal(instant, actual.CreationDate);
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForBalanceResponsibleParty(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        ProcessStepResult processStepResult,
        ProcessCompletedEventDto processCompletedEventDto,
        string energySupplierGln,
        CalculationResultCompletedToIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForBalanceResponsibleParty(processStepResult, processCompletedEventDto, energySupplierGln))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForBalanceResponsibleParty(processStepResult, processCompletedEventDto, energySupplierGln);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(x => x.CreateForBalanceResponsibleParty(processStepResult, processCompletedEventDto, energySupplierGln));
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForGridArea(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        ProcessStepResult processStepResult,
        ProcessCompletedEventDto processCompletedEventDto,
        CalculationResultCompletedToIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForGridArea(processStepResult, processCompletedEventDto))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForTotalGridArea(processStepResult, processCompletedEventDto);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(x => x.CreateForGridArea(processStepResult, processCompletedEventDto));
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForEnergySupplierByBalanceResponsibleParty(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        ProcessStepResult processStepResult,
        ProcessCompletedEventDto processCompletedEventDto,
        string energySupplierGln,
        string brpGln,
        CalculationResultCompletedToIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForEnergySupplierByBalanceResponsibleParty(processStepResult, processCompletedEventDto, energySupplierGln, brpGln))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForEnergySupplierByBalanceResponsibleParty(processStepResult, processCompletedEventDto, energySupplierGln, brpGln);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(x => x.CreateForEnergySupplierByBalanceResponsibleParty(processStepResult, processCompletedEventDto, energySupplierGln, brpGln));
    }
}
