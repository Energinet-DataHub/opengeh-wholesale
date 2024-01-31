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

using System.Collections.ObjectModel;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Common;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Mappers;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;
using GridLossResultProducedV1Factory = Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories.GridLossResultProducedV1Factory;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.GridLossResultProducedV1.Factories;

public class GridLossResultProducedV1FactoryTests
{
    private readonly Guid _batchId = Guid.NewGuid();
    private readonly Guid _id = Guid.NewGuid();
    private readonly string _gridArea = "543";
    private readonly Instant _periodStart = SystemClock.Instance.GetCurrentInstant();
    private readonly Instant _periodEnd = SystemClock.Instance.GetCurrentInstant();

    [Theory]
    [InlineAutoMoqData(TimeSeriesType.NegativeGridLoss)]
    [InlineAutoMoqData(TimeSeriesType.PositiveGridLoss)]
    public void Create_WhenNegativeOrPositiveGridLoss_ReturnsExpectedObject(
        TimeSeriesType timeSeriesType,
        GridLossResultProducedV1Factory sut)
    {
        // Arrange
        var energyResult = CreateEnergyResult(timeSeriesType);
        var expected = CreateExpected(energyResult);

        // Act
        var actual = sut.Create(energyResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    private EnergyResult CreateEnergyResult(TimeSeriesType timeSeriesType)
    {
        var quantityQualities = new Collection<QuantityQuality> { QuantityQuality.Estimated, QuantityQuality.Calculated };

        return new EnergyResult(
            _id,
            _batchId,
            _gridArea,
            timeSeriesType,
            null,
            null,
            new EnergyTimeSeriesPoint[]
            {
                new(new DateTime(2021, 1, 1), 1, quantityQualities),
                new(new DateTime(2021, 1, 1), 2, quantityQualities),
                new(new DateTime(2021, 1, 1), 3, quantityQualities),
            },
            ProcessType.Aggregation,
            _periodStart,
            _periodEnd,
            null,
            1);
    }

    private static Contracts.IntegrationEvents.GridLossResultProducedV1 CreateExpected(EnergyResult energyResult)
    {
        var gridLossResultProduced = new Contracts.IntegrationEvents.GridLossResultProducedV1
        {
            CalculationId = energyResult.CalculationId.ToString(),
            Resolution = Contracts.IntegrationEvents.GridLossResultProducedV1.Types.Resolution.Quarter,
            QuantityUnit = Contracts.IntegrationEvents.GridLossResultProducedV1.Types.QuantityUnit.Kwh,
            MeteringPointId = energyResult.MeteringPointId,
            MeteringPointType = GridLossMeteringPointTypeMapper.MapFromTimeSeriesType(energyResult.TimeSeriesType),
            PeriodStartUtc = energyResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = energyResult.PeriodEnd.ToTimestamp(),
        };
        gridLossResultProduced.TimeSeriesPoints.AddRange(
            energyResult.TimeSeriesPoints.Select(p =>
            {
                var timeSeriesPoint = new Contracts.IntegrationEvents.GridLossResultProducedV1.Types.TimeSeriesPoint
                {
                    Time = p.Time.ToTimestamp(),
                    Quantity = p.Quantity,
                };
                return timeSeriesPoint;
            }));

        return gridLossResultProduced;
    }
}
