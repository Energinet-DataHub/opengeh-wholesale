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
using Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.AggregatedTimeSeriesRequest;

public sealed class RequestedByActorRoleValidationRuleTest
{
    [Theory]
    [InlineData(DataHubNames.ActorRole.MeteredDataResponsible)]
    [InlineData(DataHubNames.ActorRole.EnergySupplier)]
    [InlineData(DataHubNames.ActorRole.BalanceResponsibleParty)]
    public async Task ValidateAsync_WhenRequestingWithValidActorRole_ReturnsEmptyErrorListAsync(string actorRole)
    {
        // Arrange
        var request = new DataHub.Edi.Requests.AggregatedTimeSeriesRequest { RequestedForActorRole = actorRole };
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
        var request = new DataHub.Edi.Requests.AggregatedTimeSeriesRequest { RequestedForActorRole = actorRole };
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
        var request = new DataHub.Edi.Requests.AggregatedTimeSeriesRequest { RequestedForActorRole = "GridOperator" };
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
