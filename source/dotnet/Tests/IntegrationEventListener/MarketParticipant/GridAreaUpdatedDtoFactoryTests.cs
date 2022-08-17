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

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener.MarketParticipant;

public class GridAreaUpdatedDtoFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void MessageTypeValue_MatchesContract_WithCalculator(
        GridAreaUpdatedIntegrationEvent anyGridAreaUpdatedIntegrationEvent,
        IntegrationEventMetadata anyMetadata,
        [Frozen] Mock<IIntegrationEventContext> integrationEventContext,
        GridAreaUpdatedDtoFactory sut)
    {
        // Arrange
        using var stream = EmbeddedResources.GetStream("IntegrationEventListener.MarketParticipant.grid-area-updated.json");
        var jsonObject = JsonNode.Parse(stream)!.AsObject()["bodyFields"]!;
        var expectedMessageType = ((JsonArray)jsonObject).First(x => x!["name"]!
            .GetValue<string?>() == "MessageType")!["value"]!
            .GetValue<string?>();
        integrationEventContext.Setup(context => context.ReadMetadata()).Returns(anyMetadata);

        // Assert
        var actual = sut.Create(anyGridAreaUpdatedIntegrationEvent);
        actual.MessageType.Should().Be(expectedMessageType);
    }
}
