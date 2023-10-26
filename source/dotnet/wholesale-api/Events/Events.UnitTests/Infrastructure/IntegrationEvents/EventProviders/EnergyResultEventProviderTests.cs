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
using Energinet.DataHub.Wholesale.Events.Application.Communication;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EventProviders
{
    public class EnergyResultEventProviderTests
    {
        /// <summary>
        /// Each result might return multiple events, if we currently support multiple versions of an event.
        /// </summary>
        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenTwoEventResults_ReturnsFourIntegrationEvents(
            CompletedBatch completedBatch,
            EnergyResult energyResult,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
            [Frozen(Matching.ImplementedInterfaces)] CalculationResultCompletedFactory calculationResultCompletedFactory,
            [Frozen(Matching.ImplementedInterfaces)] EnergyResultProducedV1Factory energyResultProducedV1Factory,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
            [Frozen] Mock<IEnergyResultQueries> energyResultQueriesMock,
            EnergyResultEventProvider sut)
        {
            // Arrange
            energyResultQueriesMock
                .Setup(queries => queries.GetAsync(completedBatch.Id))
                .Returns(AsAsyncEnumerable(energyResult, energyResult));

            // Act
            var actualIntegrationEvents = await sut.GetAsync(completedBatch).ToListAsync();

            // Assert
            actualIntegrationEvents.Should().HaveCount(4);
        }

        private async IAsyncEnumerable<T> AsAsyncEnumerable<T>(params T[] items)
        {
            foreach (var item in items)
                yield return item;
            await Task.Delay(0);
        }
    }
}
