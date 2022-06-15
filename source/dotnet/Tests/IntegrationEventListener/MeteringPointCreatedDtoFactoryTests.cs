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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.MeteringPoints;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Factories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener
{
    public class MeteringPointCreatedDtoFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Test(
            Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            // Arrange
            var integrationEventMetadata = It.IsAny<IntegrationEventMetadata?>();
            integrationEventContext
                .Setup(x => x.TryReadMetadata(out integrationEventMetadata))
                .Returns(false);
            var sut = new MeteringPointCreatedDtoFactory(integrationEventContext.Object);

            // Act & Assert
            sut.Invoking(x => x.Create(meteringPointCreatedEvent))
                .Should().Throw<InvalidOperationException>();
        }
    }
}
