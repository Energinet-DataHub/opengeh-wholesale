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
using Energinet.DataHub.Core.TestCommon.FluentAssertionsExtensions;
using Energinet.DataHub.EnergySupplying.IntegrationEvents;
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
    private const string ExpectedEffectiveDate = "2022-07-04T08:05:30Z";

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenCalled_ShouldMapCorrectValues(
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
        var energySupplierChangedEvent = CreateEnergySupplierChangedEvent(
            accountingpointId,
            gsrnNumber,
            energySupplierGln,
            ExpectedEffectiveDate,
            id);

        // Act
        var actual = sut.Create(energySupplierChangedEvent);

        // Assert
        actual.Should().NotContainNullsOrEmptyEnumerables();
        actual.MeteringPointId.Should().Be(accountingpointId);
        actual.GsrnNumber.Should().Be(gsrnNumber);
        actual.EnergySupplierGln.Should().Be(energySupplierGln);
        actual.Id.Should().Be(id);
        actual.EffectiveDate.Should().Be(InstantPattern.General.Parse(ExpectedEffectiveDate).Value);
        actual.CorrelationId.Should().Be(correlationContextCorrelationId);
        actual.MessageType.Should().Be(expectedMessageType);
        actual.OperationTime.Should().Be(metadata.OperationTimestamp);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task MessageTypeValue_MatchesContract_WithCalculator(
        string accountingpointId,
        string gsrnNumber,
        string energySupplierGln,
        string id,
        [Frozen] Mock<IIntegrationEventContext> integrationEventContext,
        EnergySupplierChangedDtoFactory sut)
    {
        // Arrange
        await using var stream =
            EmbeddedResources.GetStream("IntegrationEventListener.MarketParticipant.energy-supplier-changed.json");
        var expectedMessageType = await ContractComplianceTestHelper.GetRequiredMessageTypeAsync(stream);

        var integrationEventMetadata = new IntegrationEventMetadata(
            expectedMessageType,
            Instant.MinValue,
            "D72AEBD6-068F-46A7-A5AA-EE9DF675A163");

        integrationEventContext
            .Setup(context => context.ReadMetadata())
            .Returns(integrationEventMetadata);

        var energySupplierChangedEvent = CreateEnergySupplierChangedEvent(
            accountingpointId,
            gsrnNumber,
            energySupplierGln,
            ExpectedEffectiveDate,
            id);

        // Act
        var actual = sut.Create(energySupplierChangedEvent);

        // Assert
        actual.MessageType.Should().Be(expectedMessageType);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_HasNoEventMetadata_ThrowsInvalidOperationException(
        IntegrationEventContext integrationEventContext,
        ICorrelationContext correlationContext,
        string accountingpointId,
        string gsrnNumber,
        string energySupplierGln,
        string id)
    {
        // Arrange
        integrationEventContext.SetMetadata(null!, Instant.FromUtc(2022, 1, 1, 22, 10), null!);

        var energySupplierChangedEvent = CreateEnergySupplierChangedEvent(
            accountingpointId,
            gsrnNumber,
            energySupplierGln,
            ExpectedEffectiveDate,
            id);

        var sut = new EnergySupplierChangedDtoFactory(correlationContext, integrationEventContext);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sut.Create(energySupplierChangedEvent));
    }

    private static EnergySupplierChanged CreateEnergySupplierChangedEvent(
        string accountingpointId,
        string gsrnNumber,
        string energySupplierGln,
        string effectiveDate,
        string id)
    {
        return new EnergySupplierChanged
        {
            AccountingpointId = accountingpointId,
            GsrnNumber = gsrnNumber,
            EffectiveDate = effectiveDate,
            EnergySupplierGln = energySupplierGln,
            Id = id,
        };
    }
}
