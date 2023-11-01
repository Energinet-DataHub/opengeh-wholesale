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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.Contracts.IntegrationEvents;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Test.Core;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Factories;

public class EnergyResultProducedV1FactoryTests
{
    private readonly Guid _batchId = Guid.NewGuid();
    private readonly Guid _id = Guid.NewGuid();
    private readonly string _gridArea = "543";
    private readonly string _energySupplierId = "es_id";
    private readonly string _balanceResponsibleId = "br_id";
    private readonly string _fromGridArea = "123";
    private readonly Instant _periodStart = SystemClock.Instance.GetCurrentInstant();
    private readonly Instant _periodEnd = SystemClock.Instance.GetCurrentInstant();

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenForTotalGridArea_ReturnsExpectedObject(
        EnergyResultProducedV1Factory sut)
    {
        // Arrange
        var energyResult = CreateEnergyResult();
        energyResult.SetPrivateProperty(r => r.EnergySupplierId, null);
        energyResult.SetPrivateProperty(r => r.BalanceResponsibleId, null);

        var expected = CreateExpected(energyResult);

        // Act
        var actual = sut.Create(energyResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenForEnergySupplier_ReturnsExpectedObject(
        EnergyResultProducedV1Factory sut)
    {
        // Arrange
        var energyResult = CreateEnergyResult();
        energyResult.SetPrivateProperty(r => r.BalanceResponsibleId, null);

        var expected = CreateExpected(energyResult);

        // Act
        var actual = sut.Create(energyResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenForBalanceResponsibleParty_ReturnsExpectedObject(
        EnergyResultProducedV1Factory sut)
    {
        // Arrange
        var energyResult = CreateEnergyResult();
        energyResult.SetPrivateProperty(r => r.EnergySupplierId, null);

        var expected = CreateExpected(energyResult);

        // Act
        var actual = sut.Create(energyResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenForEnergySupplierPerBalanceResponsibleParty_ReturnsExpectedObject(
        EnergyResultProducedV1Factory sut)
    {
        // Arrange
        var energyResult = CreateEnergyResult();
        var expected = CreateExpected(energyResult);

        // Act
        var actual = sut.Create(energyResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    private EnergyResult CreateEnergyResult()
    {
        var quantityQualities = new List<QuantityQuality> { QuantityQuality.Estimated };

        return new EnergyResult(
            _id,
            _batchId,
            _gridArea,
            TimeSeriesType.FlexConsumption,
            _energySupplierId,
            _balanceResponsibleId,
            new EnergyTimeSeriesPoint[]
            {
                new(new DateTime(2021, 1, 1), 1, quantityQualities),
                new(new DateTime(2021, 1, 1), 2, quantityQualities),
                new(new DateTime(2021, 1, 1), 3, quantityQualities),
            },
            ProcessType.Aggregation,
            _periodStart,
            _periodEnd,
            _fromGridArea);
    }

    private static EnergyResultProducedV1 CreateExpected(EnergyResult energyResult)
    {
        var energyResultProducedV1 = new EnergyResultProducedV1
        {
            CalculationId = energyResult.BatchId.ToString(),
            Resolution = EnergyResultProducedV1.Types.Resolution.Quarter,
            CalculationType = EnergyResultProducedV1.Types.CalculationType.Aggregation,
            QuantityUnit = EnergyResultProducedV1.Types.QuantityUnit.Kwh,
            PeriodStartUtc = energyResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = energyResult.PeriodEnd.ToTimestamp(),
            TimeSeriesType = EnergyResultProducedV1.Types.TimeSeriesType.FlexConsumption,
            FromGridAreaCode = energyResult.FromGridArea,
        };
        energyResultProducedV1.TimeSeriesPoints.AddRange(
            energyResult.TimeSeriesPoints.Select(
                p => new EnergyResultProducedV1.Types.TimeSeriesPoint
                {
                    Time = p.Time.ToTimestamp(),
                    Quantity = p.Quantity,
                    QuantityQuality = EnergyResultProducedV1.Types.QuantityQuality.Estimated,
                }));

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId == null)
        {
            energyResultProducedV1.AggregationPerGridarea = new EnergyResultProducedV1.Types.AggregationPerGridArea { GridAreaCode = energyResult.GridArea };
        }
        else if (energyResult.BalanceResponsibleId != null && energyResult.EnergySupplierId != null)
        {
            energyResultProducedV1.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
                new EnergyResultProducedV1.Types.AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    EnergySupplierId = energyResult.EnergySupplierId,
                    BalanceResponsibleId = energyResult.BalanceResponsibleId,
                };
        }
        else if (energyResult.BalanceResponsibleId == null && energyResult.EnergySupplierId != null)
        {
            energyResultProducedV1.AggregationPerEnergysupplierPerGridarea =
                new EnergyResultProducedV1.Types.AggregationPerEnergySupplierPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    EnergySupplierId = energyResult.EnergySupplierId,
                };
        }
        else
        {
            energyResultProducedV1.AggregationPerBalanceresponsiblepartyPerGridarea =
                new EnergyResultProducedV1.Types.AggregationPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    BalanceResponsibleId = energyResult.BalanceResponsibleId,
                };
        }

        return energyResultProducedV1;
    }
}
