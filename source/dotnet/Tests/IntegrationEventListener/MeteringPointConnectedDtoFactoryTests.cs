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

using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Core.TestCommon.FluentAssertionsExtensions;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Extensions;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener
{
    public class MeteringPointConnectedDtoFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_HasEventMetadata_ReturnsValidDto(
            Mock<ICorrelationContext> correlationContext,
            Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointConnected meteringPointConnectedEvent,
            IntegrationEventMetadata integrationEventMetadata,
            Guid correlationId)
        {
            // Arrange
            correlationContext
                .Setup(x => x.Id)
                .Returns(correlationId.ToString());

            integrationEventContext
                .Setup(x => x.ReadMetadata())
                .Returns(integrationEventMetadata);

            var sut = new MeteringPointConnectedDtoFactory(correlationContext.Object, integrationEventContext.Object);

            // Act
            var actual = sut.Create(meteringPointConnectedEvent);

            // Assert
            actual.CorrelationId.Should().Be(correlationId.ToString());
            actual.MessageType.Should().Be(integrationEventMetadata.MessageType);
            actual.OperationTime.Should().Be(integrationEventMetadata.OperationTimestamp);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_WhenCalled_ShouldMapToMeteringPointConnectedEventWithCorrectValues(
            Mock<ICorrelationContext> correlationContext,
            Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointConnected meteringPointConnectedEvent,
            IntegrationEventMetadata integrationEventMetadata,
            Guid correlationId)
        {
            // Arrange
            correlationContext
                .Setup(x => x.Id)
                .Returns(correlationId.ToString());

            integrationEventContext
                .Setup(x => x.ReadMetadata())
                .Returns(integrationEventMetadata);

            var sut = new MeteringPointConnectedDtoFactory(correlationContext.Object, integrationEventContext.Object);

            meteringPointConnectedEvent.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));

            // Act
            var actual = sut.Create(meteringPointConnectedEvent);

            // Assert
            actual.Should().NotContainNullsOrEmptyEnumerables();
            actual.GsrnNumber.Should().Be(meteringPointConnectedEvent.GsrnNumber);
            actual.EffectiveDate.Should().Be(meteringPointConnectedEvent.EffectiveDate.ToInstant());
        }
    }
}
