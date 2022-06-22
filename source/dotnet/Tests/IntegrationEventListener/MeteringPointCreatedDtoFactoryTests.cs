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

using System.ComponentModel;
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Core.TestCommon.FluentAssertionsExtensions;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Extensions;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Moq;
using Xunit;
using mpTypes = Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener
{
    public class MeteringPointCreatedDtoFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_InvalidEventMetadata_ThrowsException(
            Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointCreated meteringPointCreatedEvent)
        {
            // Arrange
            var integrationEventMetadata = It.IsAny<IntegrationEventMetadata?>();
            integrationEventContext
                .Setup(x => x.TryReadMetadata(out integrationEventMetadata))
                .Returns(false);
            var sut = new MeteringPointCreatedDtoFactory(integrationEventContext.Object);

            // Act & Assert
            sut.Invoking(x => x.Create(meteringPointCreatedEvent))
               .Should()
               .Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_HasEventMetadata_ReturnsValidDto(
            Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointCreated meteringPointCreatedEvent,
            IntegrationEventMetadata integrationEventMetadata)
        {
            // Arrange
            var outEventMetadata = integrationEventMetadata;

            integrationEventContext
                .Setup(x => x.TryReadMetadata(out outEventMetadata))
                .Returns(true);

            meteringPointCreatedEvent.GridAreaCode = Guid.NewGuid().ToString();

            var sut = new MeteringPointCreatedDtoFactory(integrationEventContext.Object);

            // Act
            var actual = sut.Create(meteringPointCreatedEvent);

            // Assert
            actual.MessageType.Should().Be(integrationEventMetadata.MessageType);
            actual.OperationTime.Should().Be(integrationEventMetadata.OperationTimestamp);
        }

        [Theory]
        [InlineAutoMoqData]
        public void MeteringPointCreatedIntegrationInboundMapper_WhenCalled_ShouldMapToMeteringPointCreatedEventWithCorrectValues(
            Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointCreated meteringPointCreatedEvent,
            IntegrationEventMetadata integrationEventMetadata)
        {
            var outEventMetadata = integrationEventMetadata;

            integrationEventContext
                .Setup(x => x.TryReadMetadata(out outEventMetadata))
                .Returns(true);

            var sut = new MeteringPointCreatedDtoFactory(integrationEventContext.Object);

            meteringPointCreatedEvent.GridAreaCode = Guid.NewGuid().ToString();
            meteringPointCreatedEvent.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));
            meteringPointCreatedEvent.MeteringPointType = mpTypes.MeteringPointType.MptConsumption;
            meteringPointCreatedEvent.SettlementMethod = mpTypes.SettlementMethod.SmFlex;
            meteringPointCreatedEvent.ConnectionState = mpTypes.ConnectionState.CsNew;

            // Act
            var actual = sut.Create(meteringPointCreatedEvent);

            // Assert
            actual.Should().NotContainNullsOrEmptyEnumerables();
            actual.MeteringPointId.Should().Be(meteringPointCreatedEvent.GsrnNumber);
            actual.EffectiveDate.Should().Be(meteringPointCreatedEvent.EffectiveDate.ToInstant());
            actual.GridAreaLinkId.Should().Be(meteringPointCreatedEvent.GridAreaCode);
            actual.SettlementMethod.Should().Be(SettlementMethod.Flex);
            actual.ConnectionState.Should().Be(ConnectionState.New);
            actual.MeteringPointType.Should().Be(MeteringPointType.Consumption);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow(MeteringPointCreatedDtoFactory sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Create(null!));
        }

        [Theory]
        [InlineData(mpTypes.SettlementMethod.SmFlex, SettlementMethod.Flex)]
        [InlineData(mpTypes.SettlementMethod.SmNonprofiled, SettlementMethod.NonProfiled)]
        [InlineData(mpTypes.SettlementMethod.SmProfiled, SettlementMethod.Profiled)]
        [InlineData(mpTypes.SettlementMethod.SmNull, null)]
        public void MapSettlementMethod_WhenCalled_ShouldMapCorrectly(mpTypes.SettlementMethod protoSettlementMethod, SettlementMethod? expectedSettlementMethod)
        {
            var actual =
                MeteringPointCreatedDtoFactory.MapSettlementMethod(protoSettlementMethod);

            actual.Should().Be(expectedSettlementMethod);
        }

        [Fact]
        public void MapSettlementMethod_WhenCalledWithInvalidEnum_Throws()
        {
            Assert.Throws<InvalidEnumArgumentException>(
                () => MeteringPointCreatedDtoFactory.MapSettlementMethod((mpTypes.SettlementMethod)9999));
        }

        [Theory]
        [InlineData(mpTypes.ConnectionState.CsNew, ConnectionState.New)]
        public void MapConnectionState_WhenCalled_ShouldMapCorrectly(
            mpTypes.ConnectionState protoConnectionState,
            ConnectionState expectedConnectionState)
        {
            var actual = MeteringPointCreatedDtoFactory.MapConnectionState(protoConnectionState);

            actual.Should().Be(expectedConnectionState);
        }

        [Fact]
        public void MapConnectionState_WhenCalledWithInvalidEnum_Throws()
        {
            Assert.Throws<InvalidEnumArgumentException>(
                () => MeteringPointCreatedDtoFactory.MapConnectionState((mpTypes.ConnectionState)9999));
        }

        [Theory]
        [InlineData(mpTypes.MeteringPointType.MptAnalysis, MeteringPointType.Analysis)]
        [InlineData(mpTypes.MeteringPointType.MptConsumption, MeteringPointType.Consumption)]
        [InlineData(mpTypes.MeteringPointType.MptConsumptionFromGrid, MeteringPointType.ConsumptionFromGrid)]
        [InlineData(mpTypes.MeteringPointType.MptElectricalHeating, MeteringPointType.ElectricalHeating)]
        [InlineData(mpTypes.MeteringPointType.MptExchange, MeteringPointType.Exchange)]
        [InlineData(mpTypes.MeteringPointType.MptExchangeReactiveEnergy, MeteringPointType.ExchangeReactiveEnergy)]
        [InlineData(mpTypes.MeteringPointType.MptInternalUse, MeteringPointType.InternalUse)]
        [InlineData(mpTypes.MeteringPointType.MptNetConsumption, MeteringPointType.NetConsumption)]
        [InlineData(mpTypes.MeteringPointType.MptNetFromGrid, MeteringPointType.NetFromGrid)]
        [InlineData(mpTypes.MeteringPointType.MptNetProduction, MeteringPointType.NetProduction)]
        [InlineData(mpTypes.MeteringPointType.MptNetToGrid, MeteringPointType.NetToGrid)]
        [InlineData(mpTypes.MeteringPointType.MptOtherConsumption, MeteringPointType.OtherConsumption)]
        [InlineData(mpTypes.MeteringPointType.MptOtherProduction, MeteringPointType.OtherProduction)]
        [InlineData(mpTypes.MeteringPointType.MptOwnProduction, MeteringPointType.OwnProduction)]
        [InlineData(mpTypes.MeteringPointType.MptProduction, MeteringPointType.Production)]
        [InlineData(mpTypes.MeteringPointType.MptSupplyToGrid, MeteringPointType.SupplyToGrid)]
        [InlineData(mpTypes.MeteringPointType.MptSurplusProductionGroup, MeteringPointType.SurplusProductionGroup)]
        [InlineData(mpTypes.MeteringPointType.MptTotalConsumption, MeteringPointType.TotalConsumption)]
        [InlineData(mpTypes.MeteringPointType.MptVeproduction, MeteringPointType.VeProduction)]
        [InlineData(mpTypes.MeteringPointType.MptWholesaleServices, MeteringPointType.WholesaleService)]
        public void MapMeteringPointType_WhenCalled_ShouldMapCorrectly(
            mpTypes.MeteringPointType protoMeteringType,
            MeteringPointType expectedMeteringPointType)
        {
            var actual =
                MeteringPointCreatedDtoFactory.MapMeteringPointType(protoMeteringType);

            actual.Should().Be(expectedMeteringPointType);
        }
    }
}
