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

using Energinet.DataHub.Edi.Requests;
using Energinet.DataHub.Wholesale.EDI.Factories;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

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
        var calculationIds = new[] { Guid.NewGuid() };
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request, calculationIds);

        // Assert
        using var assertionScope = new AssertionScope();
        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        actual.CalculationIds.Should().Equal(calculationIds);
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
        var calculationIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request, calculationIds);

        // Assert
        using var assertionScope = new AssertionScope();
        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        actual.CalculationIds.Should().Equal(calculationIds);
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
        var calculationIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var request = CreateRequest(
            gridAreaCode: gridAreaCode,
            energySupplier: energySupplier,
            balanceResponsible: balanceResponsibleId);

        // Act
        var actual = AggregatedTimeSeriesRequestFactory.Parse(request, calculationIds);

        // Assert
        using var assertionScope = new AssertionScope();
        var aggregationLevel = actual.AggregationPerRoleAndGridArea;
        actual.CalculationIds.Should().Equal(calculationIds);
        aggregationLevel.BalanceResponsibleId.Should().BeNull();
        aggregationLevel.EnergySupplierId.Should().Be(energySupplier);
        aggregationLevel.GridAreaCode.Should().Be(gridAreaCode);
    }

    private AggregatedTimeSeriesRequest CreateRequest(
        string? gridAreaCode = null,
        string? energySupplier = null,
        string? balanceResponsible = null)
    {
        var request = new AggregatedTimeSeriesRequest()
        {
            // Required
            Period = new Period()
            {
                Start = "2022-12-30T23:00:00Z",
                End = "2022-12-31T23:00:00Z",
            },
            MeteringPointType = "E18",
            RequestedByActorId = "1234567891234",
            RequestedByActorRole = "DDQ",
            BusinessReason = "D04",

            // Optional
            SettlementMethod = "D01",
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
