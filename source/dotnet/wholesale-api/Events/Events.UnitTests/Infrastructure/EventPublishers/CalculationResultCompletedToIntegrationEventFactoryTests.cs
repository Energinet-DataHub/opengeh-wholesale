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
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing.Model;
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
        CalculationResultCompleted calculationResultCompleted,
        CalculationResult calculationResult,
        BatchGridAreaInfo batchGridAreaInfo,
        string energySupplierGln,
        CalculationResultCompletedToIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForEnergySupplier(calculationResult, batchGridAreaInfo, energySupplierGln))
            .Returns(calculationResultCompleted);

        var instant = SystemClock.Instance.GetCurrentInstant();
        clockMock.Setup(x => x.GetCurrentInstant())
            .Returns(instant);

        // Act
        var actual = sut.CreateForEnergySupplier(calculationResult, batchGridAreaInfo, energySupplierGln);

        // Assert
        Assert.Equal(CalculationResultCompleted.MessageType, actual.MessageType);
        Assert.Equal(MessageExtensions.ToByteArray(calculationResultCompleted), actual.EventData);
        Assert.Equal(instant, actual.CreationDate);
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForBalanceResponsibleParty(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        CalculationResult calculationResult,
        BatchGridAreaInfo batchGridAreaInfo,
        string energySupplierGln,
        CalculationResultCompletedToIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForBalanceResponsibleParty(calculationResult, batchGridAreaInfo, energySupplierGln))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForBalanceResponsibleParty(calculationResult, batchGridAreaInfo, energySupplierGln);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(x => x.CreateForBalanceResponsibleParty(calculationResult, batchGridAreaInfo, energySupplierGln));
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForGridArea(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        CalculationResult calculationResult,
        BatchGridAreaInfo batchGridAreaInfo,
        CalculationResultCompletedToIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForGridArea(calculationResult, batchGridAreaInfo))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForTotalGridArea(calculationResult, batchGridAreaInfo);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(x => x.CreateForGridArea(calculationResult, batchGridAreaInfo));
    }

    [Theory]
    [AutoMoqData]
    public void CalculationResultCompletedToIntegrationEventFactory_CallsCreateForEnergySupplierByBalanceResponsibleParty(
        [Frozen] Mock<ICalculationResultCompletedIntegrationEventFactory> calculationResultCompletedIntegrationEventFactoryMock,
        CalculationResultCompleted calculationResultCompleted,
        CalculationResult calculationResult,
        BatchGridAreaInfo batchGridAreaInfo,
        string energySupplierGln,
        string brpGln,
        CalculationResultCompletedToIntegrationEventFactory sut)
    {
        // Arrange
        calculationResultCompletedIntegrationEventFactoryMock
            .Setup(x => x.CreateForEnergySupplierByBalanceResponsibleParty(calculationResult, batchGridAreaInfo, energySupplierGln, brpGln))
            .Returns(calculationResultCompleted);

        // Act
        sut.CreateForEnergySupplierByBalanceResponsibleParty(calculationResult, batchGridAreaInfo, energySupplierGln, brpGln);

        // Assert
        calculationResultCompletedIntegrationEventFactoryMock.Verify(x => x.CreateForEnergySupplierByBalanceResponsibleParty(calculationResult, batchGridAreaInfo, energySupplierGln, brpGln));
    }
}
