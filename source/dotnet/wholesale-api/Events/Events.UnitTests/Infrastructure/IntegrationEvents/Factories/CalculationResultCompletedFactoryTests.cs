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
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Test.Core;
using Xunit;
using QuantityQuality =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Factories;

public class CalculationResultCompletedFactoryTests
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
        CalculationResultCompletedFactory sut)
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
        CalculationResultCompletedFactory sut)
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
        CalculationResultCompletedFactory sut)
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
        CalculationResultCompletedFactory sut)
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
            Energinet.DataHub.Wholesale.Common.Interfaces.Models.ProcessType.Aggregation,
            _periodStart,
            _periodEnd,
            _fromGridArea);
    }

    private CalculationResultCompleted CreateExpected(EnergyResult energyResult)
    {
        var calculationResultCompleted = new CalculationResultCompleted
        {
            BatchId = energyResult.BatchId.ToString(),
            Resolution = Resolution.Quarter,
            ProcessType = ProcessType.Aggregation,
            QuantityUnit = QuantityUnit.Kwh,
            PeriodStartUtc = energyResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = energyResult.PeriodEnd.ToTimestamp(),
            TimeSeriesType = Contracts.Events.TimeSeriesType.FlexConsumption,
            FromGridAreaCode = energyResult.FromGridArea,
        };
        calculationResultCompleted.TimeSeriesPoints.AddRange(
            energyResult.TimeSeriesPoints.Select(
                p => new TimeSeriesPoint
                {
                    Time = p.Time.ToTimestamp(),
                    Quantity = p.Quantity,
                    QuantityQuality = Contracts.Events.QuantityQuality.Estimated,
                }));

        if (energyResult.EnergySupplierId == null && energyResult.BalanceResponsibleId == null)
        {
            calculationResultCompleted.AggregationPerGridarea = new AggregationPerGridArea { GridAreaCode = energyResult.GridArea };
        }
        else if (energyResult.BalanceResponsibleId != null && energyResult.EnergySupplierId != null)
        {
            calculationResultCompleted.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
                new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    EnergySupplierGlnOrEic = energyResult.EnergySupplierId,
                    BalanceResponsiblePartyGlnOrEic = energyResult.BalanceResponsibleId,
                };
        }
        else if (energyResult.BalanceResponsibleId == null && energyResult.EnergySupplierId != null)
        {
            calculationResultCompleted.AggregationPerEnergysupplierPerGridarea =
                new AggregationPerEnergySupplierPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    EnergySupplierGlnOrEic = energyResult.EnergySupplierId,
                };
        }
        else
        {
            calculationResultCompleted.AggregationPerBalanceresponsiblepartyPerGridarea =
                new AggregationPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = energyResult.GridArea,
                    BalanceResponsiblePartyGlnOrEic = energyResult.BalanceResponsibleId,
                };
        }

        return calculationResultCompleted;
    }
}
