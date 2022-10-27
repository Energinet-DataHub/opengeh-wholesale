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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener.MarketParticipant;

[UnitTest]
public class GridAreaUpdatedDtoFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task MessageTypeValue_MatchesContract_WithCalculator(
        GridAreaUpdatedIntegrationEvent anyGridAreaUpdatedIntegrationEvent,
        IntegrationEventMetadata anyMetadata,
        [Frozen] Mock<IIntegrationEventContext> integrationEventContext,
        GridAreaUpdatedDtoFactory sut)
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("IntegrationEventListener.MarketParticipant.grid-area-updated.json");
        var expectedMessageType = await ContractComplianceTestHelper.GetRequiredMessageTypeAsync(stream);

        integrationEventContext.Setup(context => context.ReadMetadata()).Returns(anyMetadata);

        // Act
        var actual = sut.Create(anyGridAreaUpdatedIntegrationEvent);

        // Assert
        actual.MessageType.Should().Be(expectedMessageType);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenCalled_ShouldMapCorrectValues(
        Guid id,
        Guid gridAreaId,
        string name,
        string code,
        PriceAreaCode priceAreaCode,
        Guid gridAreaLinkId,
        string correlationContextCorrelationId,
        IntegrationEventMetadata metadata,
        [Frozen] Mock<IIntegrationEventContext> integrationEventContext,
        [Frozen] Mock<ICorrelationContext> correlationContext,
        GridAreaUpdatedDtoFactory sut)
    {
        // Arrange
        const string expectedMessageType = "GridAreaUpdated";

        integrationEventContext.Setup(context => context.ReadMetadata()).Returns(metadata);
        correlationContext.Setup(context => context.Id).Returns(correlationContextCorrelationId);
        var energySupplierEvent = new GridAreaUpdatedIntegrationEvent(id, gridAreaId, name, code, priceAreaCode, gridAreaLinkId);

        // Act
        var actual = sut.Create(energySupplierEvent);

        // Assert
        actual.Should().NotContainNullsOrEmptyEnumerables();
        actual.GridAreaCode.Should().Be(code);
        actual.GridAreaId.Should().Be(gridAreaId);
        actual.GridAreaLinkId.Should().Be(gridAreaLinkId);
        actual.CorrelationId.Should().Be(correlationContextCorrelationId);
        actual.MessageType.Should().Be(expectedMessageType);
        actual.OperationTime.Should().Be(metadata.OperationTimestamp);
    }
}
