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
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.EnergySupplying.IntegrationEvents;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener.MarketParticipant;

[UnitTest]
public class EnergySupplierChangedDtoFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Create_Map_Values_Correct(
        string accountingpointId,
        string gsrnNumber,
        string energySupplierGln,
        string id,
        string correlationContextCorrelationId,
        IntegrationEventMetadata metadata,
        [Frozen] Mock<IIntegrationEventContext> integrationEventContext,
        [Frozen] Mock<ICorrelationContext> correlationContext,
        EnergySupplierChangedDtoFactory sut)
    {
        // Arrange
        const string expectedMessageType = "EnergySupplierChanged";

        integrationEventContext.Setup(context => context.ReadMetadata()).Returns(metadata);
        correlationContext.Setup(context => context.Id).Returns(correlationContextCorrelationId);
        var expectedEffectiveDate = "2022-07-04T08:05:30Z";
        var energySupplierEvent = new EnergySupplierChanged
        {
            AccountingpointId = accountingpointId,
            GsrnNumber = gsrnNumber,
            EffectiveDate = expectedEffectiveDate,
            EnergySupplierGln = energySupplierGln,
            Id = id,
        };

        // Act
        var actual = sut.Create(energySupplierEvent);

        // Assert
        actual.AccountingpointId.Should().Be(accountingpointId);
        actual.GsrnNumber.Should().Be(gsrnNumber);
        actual.EnergySupplierGln.Should().Be(energySupplierGln);
        actual.Id.Should().Be(id);
        actual.EffectiveDate.Should().Be(InstantPattern.General.Parse(expectedEffectiveDate).Value);
        actual.CorrelationId.Should().Be(correlationContextCorrelationId);
        actual.MessageType.Should().Be(expectedMessageType);
        actual.OperationTime.Should().Be(metadata.OperationTimestamp);
    }
}
