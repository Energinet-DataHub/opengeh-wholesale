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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EventProviders
{
    public class EnergyResultEventProviderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenMultipleResults_ReturnsTwoEventsPerResult(
            CompletedBatch completedBatch,
            EnergyResult[] energyResults,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
            [Frozen(Matching.ImplementedInterfaces)] CalculationResultCompletedFactory calculationResultCompletedFactory,
            [Frozen(Matching.ImplementedInterfaces)] EnergyResultProducedV1Factory energyResultProducedV1Factory,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
            [Frozen] Mock<IEnergyResultQueries> energyResultQueriesMock,
            EnergyResultEventProvider sut)
        {
            // Arrange
            var expectedEventsPerResult = 2;
            var expectedEventsCount = energyResults.Length * expectedEventsPerResult;

            energyResultQueriesMock
                .Setup(queries => queries.GetAsync(completedBatch.Id))
                .Returns(energyResults.ToAsyncEnumerable());

            // Act
            var actualIntegrationEvents = await sut.GetAsync(completedBatch).ToListAsync();

            // Assert
            actualIntegrationEvents.Should().HaveCount(expectedEventsCount);
        }
    }
}
