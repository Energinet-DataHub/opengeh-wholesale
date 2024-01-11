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
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Test.Core;
using Xunit;
using EnergyResultProduced = Energinet.DataHub.Wholesale.Contracts.IntegrationEvents.EnergyResultProducedV2;
using EnergyResultProducedFactory = Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories.EnergyResultProducedV2Factory;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.EnergyResultProducedV2.Factories;

public class EnergyResultProducedV2FactoryTests
{
    private readonly Guid _batchId = Guid.NewGuid();
    private readonly Guid _id = Guid.NewGuid();
    private readonly string _gridArea = "543";
    private readonly string _energySupplierId = "es_id";
    private readonly string _balanceResponsibleId = "br_id";
    private readonly string _fromGridArea = "123";
    private readonly Instant _periodStart = SystemClock.Instance.GetCurrentInstant();
    private readonly Instant _periodEnd = SystemClock.Instance.GetCurrentInstant();
    private readonly string _version = DateTime.Now.Ticks.ToString();

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenForTotalGridArea_ReturnsExpectedObject(
        EnergyResultProducedFactory sut)
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
        EnergyResultProducedFactory sut)
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
        EnergyResultProducedFactory sut)
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
        EnergyResultProducedFactory sut)
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
        var quantityQualities = new Collection<QuantityQuality> { QuantityQuality.Estimated, QuantityQuality.Calculated };

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
            _fromGridArea,
            _version);
    }

    private static EnergyResultProduced CreateExpected(EnergyResult energyResult)
    {
        var energyResultProduced = new EnergyResultProduced
        {
            CalculationId = energyResult.BatchId.ToString(),
            Resolution = EnergyResultProduced.Types.Resolution.Quarter,
            CalculationType = EnergyResultProduced.Types.CalculationType.Aggregation,
            QuantityUnit = EnergyResultProduced.Types.QuantityUnit.Kwh,
            PeriodStartUtc = energyResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = energyResult.PeriodEnd.ToTimestamp(),
            TimeSeriesType = EnergyResultProduced.Types.TimeSeriesType.FlexConsumption,
            FromGridAreaCode = energyResult.FromGridArea,
        };
        energyResultProduced.TimeSeriesPoints.AddRange(
            energyResult.TimeSeriesPoints.Select(p =>
            {
                var timeSeriesPoint = new EnergyResultProduced.Types.TimeSeriesPoint
                {
                    Time = p.Time.ToTimestamp(),
                    Quantity = p.Quantity,
                };
                timeSeriesPoint.QuantityQualities.Add(EnergyResultProduced.Types.QuantityQuality.Estimated);
                timeSeriesPoint.QuantityQualities.Add(EnergyResultProduced.Types.QuantityQuality.Calculated);
                return timeSeriesPoint;
            }));

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId == null)
        {
            energyResultProduced.AggregationPerGridarea = new EnergyResultProduced.Types.AggregationPerGridArea { GridAreaCode = energyResult.GridArea };
        }
        else if (energyResult.BalanceResponsibleId != null && energyResult.EnergySupplierId != null)
        {
            energyResultProduced.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
                new EnergyResultProduced.Types.AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    EnergySupplierId = energyResult.EnergySupplierId,
                    BalanceResponsibleId = energyResult.BalanceResponsibleId,
                };
        }
        else if (energyResult.BalanceResponsibleId == null && energyResult.EnergySupplierId != null)
        {
            energyResultProduced.AggregationPerEnergysupplierPerGridarea =
                new EnergyResultProduced.Types.AggregationPerEnergySupplierPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    EnergySupplierId = energyResult.EnergySupplierId,
                };
        }
        else
        {
            energyResultProduced.AggregationPerBalanceresponsiblepartyPerGridarea =
                new EnergyResultProduced.Types.AggregationPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    BalanceResponsibleId = energyResult.BalanceResponsibleId,
                };
        }

        return energyResultProduced;
    }
}
