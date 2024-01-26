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

using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.EDI.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;
using Period = Energinet.DataHub.Edi.Requests.Period;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Factories;

public class AggregatedTimeSeriesRequestFactoryTests
{
    [Fact]
    public void Parse_WhenGridAreaIsNull_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891237";
        var gridAreaCode = null as string;
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().ContainSingle().And.Contain(TimeSeriesType.Production);
        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCode.Should().BeNull();
    }

    [Fact]
    public void Parse_WhenEnergySupplierIsNull_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = null as string;
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
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
        aggregationLevel.GridAreaCode.Should().Be(gridAreaCode);
    }

    [Fact]
    public void Parse_WhenBalanceResponsibleIsNull_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = null as string;
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
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
        aggregationLevel.GridAreaCode.Should().Be(gridAreaCode);
    }

    [Fact]
    public void Parse_WhenMeteringPointTypeIsEmptyForEnergySupplier_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId,
            meteringPointType: string.Empty);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().BeEquivalentTo([TimeSeriesType.Production, TimeSeriesType.FlexConsumption]);

        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCode.Should().Be(gridAreaCode);
    }

    [Fact]
    public void Parse_WhenMeteringPointTypeIsEmptyForBalanceResponsibleParty_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId,
            meteringPointType: string.Empty);

        request.RequestedByActorRole = ActorRoleCode.BalanceResponsibleParty;

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes.Should().BeEquivalentTo([TimeSeriesType.Production, TimeSeriesType.FlexConsumption]);

        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCode.Should().Be(gridAreaCode);
    }

    [Fact]
    public void Parse_WhenMeteringPointTypeIsEmptyForMeteredDataResponsible_ExpectedParsing()
    {
        // Arrange
        var balanceResponsibleId = "1234567891234";
        var energySupplier = "1234567891234";
        var gridAreaCode = "303";
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId,
            meteringPointType: string.Empty);

        request.RequestedByActorRole = ActorRoleCode.MeteredDataResponsible;

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.TimeSeriesTypes
            .Should()
            .BeEquivalentTo([
                TimeSeriesType.Production,
                TimeSeriesType.FlexConsumption,
                TimeSeriesType.NetExchangePerGa
            ]);

        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        aggregationLevel.BalanceResponsibleId.Should().Be(balanceResponsibleId);
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCode.Should().Be(gridAreaCode);
    }

    private AggregatedTimeSeriesRequest CreateRequest(
        string? gridAreaCode = null,
        string? energySupplier = null,
        string? balanceResponsible = null,
        string meteringPointType = MeteringPointType.Production)
    {
        var request = new AggregatedTimeSeriesRequest()
        {
            // Required
            Period = new Period()
            {
                Start = "2022-12-30T23:00:00Z",
                End = "2022-12-31T23:00:00Z",
            },
            MeteringPointType = meteringPointType,
            RequestedByActorId = "1234567891234",
            RequestedByActorRole = ActorRoleCode.EnergySupplier,
            BusinessReason = BusinessReason.BalanceFixing,

            // Optional
            SettlementMethod = SettlementMethod.Flex,
        };

        if (gridAreaCode is not null)
        {
            request.GridAreaCode = gridAreaCode;
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
