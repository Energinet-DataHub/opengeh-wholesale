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

using AutoFixture;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Extensions;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EventProviders
{
    public class WholesaleResultEventProviderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenMultipleResults_ReturnsOneEventPerResult(
            WholesaleResult[] wholesaleResults,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
            [Frozen(Matching.ImplementedInterfaces)] AmountPerChargeResultProducedV1Factory amountPerChargeResultProducedV1Factory,
            [Frozen(Matching.ImplementedInterfaces)] MonthlyAmountPerChargeResultProducedV1Factory monthlyAmountPerChargeResultProducedV1Factory,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
            [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueriesMock,
            WholesaleResultEventProvider sut)
        {
            // Arrange
            var expectedEventsPerResult = 1;
            var expectedEventsCount = wholesaleResults.Length * expectedEventsPerResult;

            var fixture = new Fixture();
            var wholesaleFixingBatch = fixture.ForConstructorOn<CompletedBatch>()
                .SetParameter("processType").To(ProcessType.WholesaleFixing)
                .Create();

            wholesaleResultQueriesMock
                .Setup(queries => queries.GetAsync(wholesaleFixingBatch.Id))
                .Returns(wholesaleResults.ToAsyncEnumerable());

            // Act
            var actualIntegrationEvents = await sut.GetAsync(wholesaleFixingBatch).ToListAsync();

            // Assert
            actualIntegrationEvents.Should().HaveCount(expectedEventsCount);
        }
    }
}
