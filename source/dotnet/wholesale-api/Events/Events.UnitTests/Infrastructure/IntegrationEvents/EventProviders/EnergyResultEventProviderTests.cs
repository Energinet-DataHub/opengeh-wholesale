﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Collections.ObjectModel;
using AutoFixture;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EventProviders;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;
using CalculationType = Energinet.DataHub.Wholesale.Common.Interfaces.Models.CalculationType;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EventProviders;

public class EnergyResultEventProviderTests
{
    private readonly Guid _calculationId = Guid.NewGuid();
    private readonly string _gridArea = "543";
    private readonly string? _energySupplierId = null;
    private readonly string? _balanceResponsibleId = null;
    private readonly string? _fromGridArea = null;
    private readonly Instant _periodStart = Instant.FromUtc(2021, 1, 2, 23, 0);
    private readonly Instant _periodEnd = Instant.FromUtc(2021, 1, 3, 23, 0);
    private readonly Resolution _resolution = Resolution.Quarter;

    public EnergyResultEventProviderTests()
    {
    }

    [Theory]
    [InlineAutoMoqData(TimeSeriesType.NegativeGridLoss)]
    [InlineAutoMoqData(TimeSeriesType.PositiveGridLoss)]
    public async Task GetAsync_WhenNegativeOrPositiveGridLoss_ReturnsExactlyOneGridLossResultProducedV1Event(
        TimeSeriesType positiveOrNegativeGridLoss,
        [Frozen] Mock<IEnergyResultQueries> energyResultQueriesMock,
        GridLossResultProducedV1Factory gridLossResultProducedV1Factory)
    {
        // Arrange
        const string expectedEventName = Contracts.IntegrationEvents.GridLossResultProducedV1.EventName;
        var energyResult = CreateEnergyResult(positiveOrNegativeGridLoss);
        var energyResults = new[] { energyResult };
        var sut = new EnergyResultEventProvider(
            energyResultQueriesMock.Object,
            gridLossResultProducedV1Factory);

        energyResultQueriesMock
            .Setup(mock => mock.GetAsync(_calculationId))
            .Returns(energyResults.ToAsyncEnumerable());

        // Act
        var actualIntegrationEvents = await sut.GetAsync(_calculationId).ToListAsync();

        // Assert
        actualIntegrationEvents.Where(e => e.EventName == expectedEventName).Should().ContainSingle();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetAsync_WhenNotNegativeOrPositiveGridLoss_ReturnsNoGridLossResultProducedV1Event(
        [Frozen] Mock<IEnergyResultQueries> energyResultQueriesMock,
        GridLossResultProducedV1Factory gridLossResultProducedV1Factory)
    {
        foreach (var timeSeriesType in Enum.GetValues(typeof(TimeSeriesType)).Cast<TimeSeriesType>())
        {
            // Arrange
            if (timeSeriesType is TimeSeriesType.NegativeGridLoss or TimeSeriesType.PositiveGridLoss)
                continue;

            const string gridLossEventName = Contracts.IntegrationEvents.GridLossResultProducedV1.EventName;
            var energyResult = CreateEnergyResult(timeSeriesType);
            var energyResults = new[] { energyResult };
            var sut = new EnergyResultEventProvider(
                energyResultQueriesMock.Object,
                gridLossResultProducedV1Factory);

            energyResultQueriesMock
                .Setup(mock => mock.GetAsync(_calculationId))
                .Returns(energyResults.ToAsyncEnumerable());

            // Act
            var actualIntegrationEvents = await sut.GetAsync(_calculationId).ToListAsync();

            // Assert
            actualIntegrationEvents.Where(e => e.EventName == gridLossEventName).Should().BeEmpty();
        }
    }

    private EnergyResult CreateEnergyResult(TimeSeriesType timeSeriesType)
    {
        var quantityQualities = new Collection<QuantityQuality> { QuantityQuality.Estimated };
        var meteringPointType = timeSeriesType is TimeSeriesType.NegativeGridLoss or TimeSeriesType.PositiveGridLoss
            ? "123"
            : null;
        return new EnergyResult(
            Guid.NewGuid(),
            _calculationId,
            _gridArea,
            timeSeriesType,
            _energySupplierId,
            _balanceResponsibleId,
            new EnergyTimeSeriesPoint[]
            {
                new(new DateTime(2021, 1, 1), 1, quantityQualities),
                new(new DateTime(2021, 1, 1), 2, quantityQualities),
                new(new DateTime(2021, 1, 1), 3, quantityQualities),
            },
            CalculationType.Aggregation,
            _periodStart,
            _periodEnd,
            _fromGridArea,
            meteringPointType,
            _resolution,
            1);
    }
}
