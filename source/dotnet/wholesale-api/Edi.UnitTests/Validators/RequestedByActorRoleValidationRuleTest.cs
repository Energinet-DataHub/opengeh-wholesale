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

using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeries.Rules;
using FluentAssertions;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators;

public sealed class RequestedByActorRoleValidationRuleTest
{
    [Theory]
    [InlineData(ActorRoleCode.MeteredDataResponsible)]
    [InlineData(ActorRoleCode.EnergySupplier)]
    [InlineData(ActorRoleCode.BalanceResponsibleParty)]
    public async Task ValidateAsync_WhenRequestingWithValidActorRole_ReturnsEmptyErrorListAsync(string actorRole)
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest { RequestedByActorRole = actorRole };
        var rule = new RequestedByActorRoleValidationRule();

        // Act
        var result = await rule.ValidateAsync(request);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("DLG")]
    [InlineData("TSO")]
    [InlineData("FOO")]
    public async Task ValidateAsync_WhenRequestingWithUnexpectedActorRole_ReturnsEmptyErrorListAsync(string actorRole)
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest { RequestedByActorRole = actorRole };
        var rule = new RequestedByActorRoleValidationRule();

        // Act
        var result = await rule.ValidateAsync(request);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WhenRequestingWithDdmActorRole_ReturnsDdmShouldRequestAsMdrErrorAsync()
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest { RequestedByActorRole = "DDM" };
        var rule = new RequestedByActorRoleValidationRule();

        // Act
        var result = await rule.ValidateAsync(request);

        // Assert
        result.Should().ContainSingle();
        result
            .Single()
            .Message
            .Should()
            .Be(
                "Rollen skal være MDR når der anmodes om beregnede energitidsserier / Role must be MDR when requesting aggregated measure data");

        result.Single().ErrorCode.Should().Be("D02");
    }
}
