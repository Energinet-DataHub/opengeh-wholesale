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
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Infrastructure.EventPublishers;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.EventPublishers;

public class IntegrationEventTypeMapperTests
{
    [Fact]
    public void GetEventName_WhenEventType_ReturnsEventName()
    {
        // Arrange
        var sut = new IntegrationEventTypeMapper(new Dictionary<Type, string>
        {
            { typeof(CalculationResultCompleted), CalculationResultCompleted.BalanceFixingEventName },
        });

        const string expected = CalculationResultCompleted.BalanceFixingEventName;
        var eventType = typeof(CalculationResultCompleted);

        // Act
        var actual = sut.GetMessageType(eventType);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [AutoData]
    public void GetMessageType_WhenGettingNotExisting_ThrowsException(IntegrationEventTypeMapper sut)
    {
        // Arrange
        var eventType = typeof(CalculationResultCompleted);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => sut.GetMessageType(eventType));
    }

    [Theory]
    [AutoData]
    public void Ctor_WhenAddingExistingEventType_ThrowsException(
        string eventName,
        string otherEventName,
        Type type)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new IntegrationEventTypeMapper(new Dictionary<Type, string>
        {
            { type, eventName },
            { type, otherEventName },
        }));
    }

    [Theory]
    [AutoData]
    public void Ctor_WhenAddingExistingEventName_ThrowsException(
        string eventName,
        Type type,
        Type otherType)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new IntegrationEventTypeMapper(new Dictionary<Type, string>
        {
            { type, eventName },
            { otherType, eventName },
        }));
    }
}
