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

using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Factories.AggregatedTimeSeries;
using Energinet.DataHub.Wholesale.Edi.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;
using Period = Energinet.DataHub.Edi.Requests.Period;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Factories.AggregatedTimeSeries;

public class AggregatedTimeSeriesRequestFactoryTests
{
    [Fact]
    public void Parse_WhenGridAreaIsEmpty_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891237";
        var request = CreateRequest(
            gridAreaCodes: [],
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId,
            meteringPointType: DataHubNames.MeteringPointType.Production);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().ContainSingle().And.Contain(TimeSeriesType.Production);
        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCodes.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenEnergySupplierIsNull_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = null as string;
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCodes: [gridAreaCode],
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().ContainSingle().And.Contain(TimeSeriesType.Production);
        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().BeNull();
        aggregationLevel.GridAreaCodes.Should().Equal(gridAreaCode);
    }

    [Fact]
    public void Parse_WhenBalanceResponsibleIsNull_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = null as string;
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCodes: [gridAreaCode],
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().ContainSingle().And.Contain(TimeSeriesType.Production);
        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().BeNull();
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCodes.Should().Equal(gridAreaCode);
    }

    [Fact]
    public void Parse_WhenMeteringPointTypeIsNullForEnergySupplier_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCodes: [gridAreaCode],
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId,
            meteringPointType: null,
            settlementMethod: string.Empty);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().BeEquivalentTo([
            TimeSeriesType.Production,
            TimeSeriesType.FlexConsumption,
            TimeSeriesType.NonProfiledConsumption
        ]);

        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCodes.Should().Equal(gridAreaCode);
    }

    [Fact]
    public void
        Parse_WhenMeteringPointTypeIsNullForEnergySupplierWithNonProfiledSettlementMethod_IgnoreSettlementMethod()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCodes: [gridAreaCode],
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId,
            meteringPointType: null,
            settlementMethod: DataHubNames.SettlementMethod.NonProfiled);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().BeEquivalentTo([
            TimeSeriesType.Production,
            TimeSeriesType.NonProfiledConsumption,
            TimeSeriesType.FlexConsumption
        ]);

        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCodes.Should().Equal(gridAreaCode);
    }

    [Fact]
    public void Parse_WhenMeteringPointTypeIsNullForBalanceResponsibleParty_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCodes: [gridAreaCode],
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId,
            meteringPointType: null,
            settlementMethod: string.Empty);

        request.RequestedForActorRole = DataHubNames.ActorRole.BalanceResponsibleParty;

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().BeEquivalentTo([
            TimeSeriesType.Production,
            TimeSeriesType.FlexConsumption,
            TimeSeriesType.NonProfiledConsumption
        ]);

        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCodes.Should().Equal(gridAreaCode);
    }

    [Fact]
    public void Parse_WhenMeteringPointTypeIsNullForMeteredDataResponsible_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCodes: [gridAreaCode],
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId,
            meteringPointType: null,
            settlementMethod: string.Empty);

        request.RequestedForActorRole = DataHubNames.ActorRole.MeteredDataResponsible;

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes
            .Should()
            .BeEquivalentTo([
                TimeSeriesType.Production,
                TimeSeriesType.FlexConsumption,
                TimeSeriesType.NonProfiledConsumption,
                TimeSeriesType.TotalConsumption,
                TimeSeriesType.NetExchangePerGa
            ]);

        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCodes.Should().Equal(gridAreaCode);
    }

    private AggregatedTimeSeriesRequest CreateRequest(
        IReadOnlyCollection<string> gridAreaCodes,
        string? energySupplier,
        string? balanceResponsible,
        string? meteringPointType = DataHubNames.MeteringPointType.Production,
        string? settlementMethod = DataHubNames.SettlementMethod.Flex)
    {
        var request = new AggregatedTimeSeriesRequest()
        {
            // Required
            Period = new Period()
            {
                Start = "2022-12-30T23:00:00Z",
                End = "2022-12-31T23:00:00Z",
            },
            RequestedForActorNumber = "1234567891234",
            RequestedForActorRole = DataHubNames.ActorRole.EnergySupplier,
            BusinessReason = DataHubNames.BusinessReason.BalanceFixing,

            // Optional
            SettlementMethod = settlementMethod,
        };

        if (meteringPointType is not null)
        {
            request.MeteringPointType = meteringPointType;
        }

        if (gridAreaCodes.Count > 0)
        {
            request.GridAreaCodes.AddRange(gridAreaCodes);
        }

        if (energySupplier is not null)
        {
            request.EnergySupplierId = energySupplier;
        }

        if (balanceResponsible is not null)
        {
            request.BalanceResponsibleId = balanceResponsible;
        }

        return request;
    }
}
