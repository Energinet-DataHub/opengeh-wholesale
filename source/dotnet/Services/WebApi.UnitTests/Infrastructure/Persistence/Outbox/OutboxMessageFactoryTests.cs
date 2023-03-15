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

using System.Text;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Energinet.DataHub.Wholesale.Infrastructure.ServiceBus;
using FluentAssertions;
using Google.Protobuf;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.Persistence.Outbox;

public class OutboxMessageFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void CreateFrom_ReturnServiceBusMessage(
        [Frozen] Mock<ICalculationResultReadyIntegrationEventFactory> calculationResultReadyIntegrationEventFactoryMock,
        [Frozen] Mock<IServiceBusMessageFactory> serviceBusMessageFactoryMock,
        [Frozen] Mock<IJsonSerializer> jsonSerializerMock,
        [Frozen] Mock<IClock> clockMock,
        OutboxMessageFactory sut,
        ProcessStepResult processStepResult,
        ProcessCompletedEventDto processCompletedEventDto,
        CalculationResultCompleted calculationResultCompleted,
        ServiceBusMessage serviceBusMessage,
        string energySupplierGln)
    {
        // Arrange
        var instance = SystemClock.Instance.GetCurrentInstant();
        var serialized = new JsonSerializer().Serialize(serviceBusMessage);
        calculationResultReadyIntegrationEventFactoryMock.Setup(x => x.CreateCalculationResultCompletedForGridArea(processStepResult, processCompletedEventDto))
            .Returns(calculationResultCompleted);
        serviceBusMessageFactoryMock.Setup(x => x.CreateProcessCompleted(calculationResultCompleted.ToByteArray(), processCompletedEventDto.ProcessType.ToString()))
            .Returns(serviceBusMessage);
        jsonSerializerMock.Setup(x => x.Serialize(It.IsAny<ServiceBusMessage>())).Returns(serialized);
        clockMock.Setup(x => x.GetCurrentInstant()).Returns(instance);

        // Act
        var actual = sut.CreateMessageCalculationResultForEnergySupplier(processStepResult, processCompletedEventDto, energySupplierGln);

        // Assert
        var expected = new OutboxMessage(CalculationResultCompleted.BalanceFixingEventName, Encoding.UTF8.GetBytes(serialized), instance);
        actual.Should().BeEquivalentTo(expected, x => x.Excluding(y => y.Id));
    }
}
