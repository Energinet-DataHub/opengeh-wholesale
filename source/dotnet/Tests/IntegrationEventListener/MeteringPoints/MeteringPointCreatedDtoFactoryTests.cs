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
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Core.TestCommon.FluentAssertionsExtensions;
using Energinet.DataHub.MeteringPoints.IntegrationEvents.Contracts;
using Energinet.DataHub.Wholesale.IntegrationEventListener.Extensions;
using Energinet.DataHub.Wholesale.IntegrationEventListener.MeteringPoints;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;
using mpTypes = Energinet.DataHub.MeteringPoints.IntegrationEvents.Contracts.MeteringPointCreated.Types;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener.MeteringPoints
{
    [UnitTest]
    public class MeteringPointCreatedDtoFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task MessageTypeValue_MatchesContract_WithCalculator(
            [Frozen] Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointCreated meteringPointCreatedEvent,
            IntegrationEventMetadata integrationEventMetadata,
            MeteringPointCreatedDtoFactory sut)
        {
            // Arrange
            await using var stream = EmbeddedResources.GetStream("IntegrationEventListener.MeteringPoints.metering-point-created.json");
            var expectedMessageType = await ContractComplianceTestHelper.GetRequiredMessageTypeAsync(stream);

            integrationEventContext.Setup(context => context.ReadMetadata()).Returns(integrationEventMetadata);

            // Act
            var actual = sut.Create(meteringPointCreatedEvent);

            // Assert
            actual.MessageType.Should().Be(expectedMessageType);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_HasEventMetadata_ReturnsValidDto(
            Mock<ICorrelationContext> correlationContext,
            Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointCreated meteringPointCreatedEvent,
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

            meteringPointCreatedEvent.GridAreaCode = Guid.NewGuid().ToString();

            var sut = new MeteringPointCreatedDtoFactory(correlationContext.Object, integrationEventContext.Object);

            // Act
            var actual = sut.Create(meteringPointCreatedEvent);

            // Assert
            actual.CorrelationId.Should().Be(correlationId.ToString());
            actual.MessageType.Should().Be(integrationEventMetadata.MessageType);
            actual.OperationTime.Should().Be(integrationEventMetadata.OperationTimestamp);
        }

        [Theory]
        [InlineAutoMoqData]
        public void MeteringPointCreatedIntegrationInboundMapper_WhenCalled_ShouldMapToMeteringPointCreatedEventWithCorrectValues(
            Mock<ICorrelationContext> correlationContext,
            Mock<IIntegrationEventContext> integrationEventContext,
            MeteringPointCreated meteringPointCreatedEvent,
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

            var sut = new MeteringPointCreatedDtoFactory(correlationContext.Object, integrationEventContext.Object);

            meteringPointCreatedEvent.GridAreaCode = Guid.NewGuid().ToString();
            meteringPointCreatedEvent.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));
            meteringPointCreatedEvent.MeteringPointType = mpTypes.MeteringPointType.MptConsumption;
            meteringPointCreatedEvent.SettlementMethod = mpTypes.SettlementMethod.SmFlex;
            meteringPointCreatedEvent.ConnectionState = mpTypes.ConnectionState.CsNew;

            // Act
            var actual = sut.Create(meteringPointCreatedEvent);

            // Assert
            actual.Should().NotContainNullsOrEmptyEnumerables();
            actual.GsrnNumber.Should().Be(meteringPointCreatedEvent.GsrnNumber);
            actual.EffectiveDate.Should().Be(meteringPointCreatedEvent.EffectiveDate.ToInstant());
            actual.GridAreaLinkId.Should().Be(meteringPointCreatedEvent.GridAreaCode);
            actual.SettlementMethod.Should().Be(SettlementMethod.Flex);
            actual.ConnectionState.Should().Be(ConnectionState.New);
            actual.MeteringPointType.Should().Be(MeteringPointType.Consumption);
            actual.MeteringPointId.Should().Be(meteringPointCreatedEvent.MeteringPointId);
        }

        [Theory]
        [InlineData(mpTypes.SettlementMethod.SmFlex, SettlementMethod.Flex)]
        [InlineData(mpTypes.SettlementMethod.SmNonprofiled, SettlementMethod.NonProfiled)]
        [InlineData(mpTypes.SettlementMethod.SmProfiled, SettlementMethod.Profiled)]
        [InlineData(mpTypes.SettlementMethod.SmNull, null)]
        public void MapSettlementMethod_WhenCalled_ShouldMapCorrectly(mpTypes.SettlementMethod protoSettlementMethod, SettlementMethod? expectedSettlementMethod)
        {
            // Arrange
            var meteringPointCreated = CreateValidMeteringPointCreated();
            meteringPointCreated.SettlementMethod = protoSettlementMethod;
            var sut = CreateTarget();

            // Act
            var actual = sut.Create(meteringPointCreated);

            // Assert
            actual.SettlementMethod.Should().Be(expectedSettlementMethod);
        }

        [Fact]
        public void MapSettlementMethod_WhenCalledWithInvalidEnum_Throws()
        {
            // Arrange
            const mpTypes.SettlementMethod invalidValue = (mpTypes.SettlementMethod)100;
            var meteringPointCreated = CreateValidMeteringPointCreated();
            meteringPointCreated.SettlementMethod = invalidValue;
            var sut = CreateTarget();

            // Act + Assert
            sut
                .Invoking(s => s.Create(meteringPointCreated))
                .Should()
                .Throw<InvalidEnumArgumentException>();
        }

        [Theory]
        [InlineData(mpTypes.ConnectionState.CsNew, ConnectionState.New)]
        public void MapConnectionState_WhenCalled_ShouldMapCorrectly(
            mpTypes.ConnectionState protoConnectionState,
            ConnectionState expectedConnectionState)
        {
            // Arrange
            var meteringPointCreated = CreateValidMeteringPointCreated();
            meteringPointCreated.ConnectionState = protoConnectionState;
            var sut = CreateTarget();

            // Act
            var actual = sut.Create(meteringPointCreated);

            // Assert
            actual.ConnectionState.Should().Be(expectedConnectionState);
        }

        [Fact]
        public void MapConnectionState_WhenCalledWithInvalidEnum_Throws()
        {
            // Arrange
            const mpTypes.ConnectionState invalidValue = (mpTypes.ConnectionState)100;
            var meteringPointCreated = CreateValidMeteringPointCreated();
            meteringPointCreated.ConnectionState = invalidValue;

            var sut = CreateTarget();

            // Act + Assert
            sut
                .Invoking(s => s.Create(meteringPointCreated))
                .Should()
                .Throw<InvalidEnumArgumentException>();
        }

        [Theory]
        [InlineData(mpTypes.MeteringPointType.MptAnalysis, MeteringPointType.Analysis)]
        [InlineData(mpTypes.MeteringPointType.MptConsumption, MeteringPointType.Consumption)]
        [InlineData(mpTypes.MeteringPointType.MptConsumptionFromGrid, MeteringPointType.ConsumptionFromGrid)]
        [InlineData(mpTypes.MeteringPointType.MptElectricalHeating, MeteringPointType.ElectricalHeating)]
        [InlineData(mpTypes.MeteringPointType.MptExchange, MeteringPointType.Exchange)]
        [InlineData(mpTypes.MeteringPointType.MptExchangeReactiveEnergy, MeteringPointType.ExchangeReactiveEnergy)]
        [InlineData(mpTypes.MeteringPointType.MptGridLossCorrection, MeteringPointType.GridLossCorrection)]
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
            // Arrange
            var meteringPointCreated = CreateValidMeteringPointCreated();
            meteringPointCreated.MeteringPointType = protoMeteringType;
            var sut = CreateTarget();

            // Act
            var actual = sut.Create(meteringPointCreated);

            // Assert
            actual.MeteringPointType.Should().Be(expectedMeteringPointType);
        }

        [Fact]
        public void MapMeteringPointType_WhenCalledWithInvalidEnum_Throws()
        {
            // Arrange
            const mpTypes.MeteringPointType invalidValue = (mpTypes.MeteringPointType)100;
            var meteringPointCreated = CreateValidMeteringPointCreated();
            meteringPointCreated.MeteringPointType = invalidValue;
            var sut = CreateTarget();

            // Act + Assert
            sut
                .Invoking(s => s.Create(meteringPointCreated))
                .Should()
                .Throw<InvalidEnumArgumentException>();
        }

        [Theory]
        [InlineData(mpTypes.MeterReadingPeriodicity.MrpHourly, Resolution.Hourly)]
        [InlineData(mpTypes.MeterReadingPeriodicity.MrpQuarterly, Resolution.Quaterly)]
        public void MapResolutionType_WhenCalled_ShouldMapCorrectly(
            mpTypes.MeterReadingPeriodicity protoPeriodicity,
            Resolution expectedResolution)
        {
            // Arrange
            var meteringPointCreated = CreateValidMeteringPointCreated();
            meteringPointCreated.MeterReadingPeriodicity = protoPeriodicity;
            var sut = CreateTarget();

            // Act
            var actual = sut.Create(meteringPointCreated);

            // Assert
            actual.Resolution.Should().Be(expectedResolution);
        }

        [Fact]
        public void MapResolutionType_WhenCalledWithInvalidEnum_Throws()
        {
            // Arrange
            const mpTypes.MeterReadingPeriodicity invalidValue = (mpTypes.MeterReadingPeriodicity)100;
            var meteringPointCreated = CreateValidMeteringPointCreated();
            meteringPointCreated.MeterReadingPeriodicity = invalidValue;
            var sut = CreateTarget();

            // Act + Assert
            sut
                .Invoking(s => s.Create(meteringPointCreated))
                .Should()
                .Throw<InvalidEnumArgumentException>();
        }

        private static MeteringPointCreatedDtoFactory CreateTarget()
        {
            var correlationContext = new Mock<ICorrelationContext>();
            var integrationEventContext = new Mock<IIntegrationEventContext>();
            var outEventMetadata = new IntegrationEventMetadata(
                "fake_value",
                Instant.FromDateTimeUtc(DateTime.UtcNow),
                "BB2E6420-EC56-4462-9499-47F2D7B30DBB");

            integrationEventContext
                .Setup(x => x.ReadMetadata())
                .Returns(outEventMetadata);

            return new MeteringPointCreatedDtoFactory(correlationContext.Object, integrationEventContext.Object);
        }

        private static MeteringPointCreated CreateValidMeteringPointCreated()
        {
            return new MeteringPointCreated
            {
                GridAreaCode = "3448BFAD-6783-4502-95FC-D57E09228D72",
                EffectiveDate = Timestamp.FromDateTime(DateTime.UtcNow),
            };
        }
    }
}
