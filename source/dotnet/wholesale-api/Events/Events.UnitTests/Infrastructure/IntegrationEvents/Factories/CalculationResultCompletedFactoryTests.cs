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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
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
using TimeSeriesPoint =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesPoint;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Factories;

public class CalculationResultCompletedFactoryTests
{
    private readonly Guid _calculationId = Guid.NewGuid();
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
        var calculationResult = CreateCalculationResult();
        calculationResult.SetPrivateProperty(r => r.EnergySupplierId, null);
        calculationResult.SetPrivateProperty(r => r.BalanceResponsibleId, null);

        var expected = CreateExpected(calculationResult);

        // Act
        var actual = sut.Create(calculationResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenForEnergySupplier_ReturnsExpectedObject(
        CalculationResultCompletedFactory sut)
    {
        // Arrange
        var calculationResult = CreateCalculationResult();
        calculationResult.SetPrivateProperty(r => r.BalanceResponsibleId, null);

        var expected = CreateExpected(calculationResult);

        // Act
        var actual = sut.Create(calculationResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenForBalanceResponsibleParty_ReturnsExpectedObject(
        CalculationResultCompletedFactory sut)
    {
        // Arrange
        var calculationResult = CreateCalculationResult();
        calculationResult.SetPrivateProperty(r => r.EnergySupplierId, null);

        var expected = CreateExpected(calculationResult);

        // Act
        var actual = sut.Create(calculationResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_WhenForEnergySupplierPerBalanceResponsibleParty_ReturnsExpectedObject(
        CalculationResultCompletedFactory sut)
    {
        // Arrange
        var calculationResult = CreateCalculationResult();
        var expected = CreateExpected(calculationResult);

        // Act
        var actual = sut.Create(calculationResult);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    private CalculationResult CreateCalculationResult()
    {
        return new CalculationResult(
            _id,
            _calculationId,
            _gridArea,
            TimeSeriesType.FlexConsumption,
            _energySupplierId,
            _balanceResponsibleId,
            new TimeSeriesPoint[]
            {
                new(new DateTime(2021, 1, 1), 1, QuantityQuality.Estimated),
                new(new DateTime(2021, 1, 1), 2, QuantityQuality.Estimated),
                new(new DateTime(2021, 1, 1), 3, QuantityQuality.Estimated),
            },
            Common.Models.ProcessType.Aggregation,
            _periodStart,
            _periodEnd,
            _fromGridArea);
    }

    private CalculationResultCompleted CreateExpected(CalculationResult calculationResult)
    {
        var calculationResultCompleted = new CalculationResultCompleted
        {
            CalculationId = calculationResult.CalculationId.ToString(),
            Resolution = Resolution.Quarter,
            ProcessType = ProcessType.Aggregation,
            QuantityUnit = QuantityUnit.Kwh,
            PeriodStartUtc = calculationResult.PeriodStart.ToTimestamp(),
            PeriodEndUtc = calculationResult.PeriodEnd.ToTimestamp(),
            TimeSeriesType = Contracts.Events.TimeSeriesType.FlexConsumption,
            FromGridAreaCode = calculationResult.FromGridArea,
        };
        calculationResultCompleted.TimeSeriesPoints.AddRange(
            calculationResult.TimeSeriesPoints.Select(
                p => new Contracts.Events.TimeSeriesPoint
                {
                    Time = p.Time.ToTimestamp(),
                    Quantity = p.Quantity,
                    QuantityQuality = Contracts.Events.QuantityQuality.Estimated,
                }));

        if (calculationResult.EnergySupplierId == null && calculationResult.BalanceResponsibleId == null)
        {
            calculationResultCompleted.AggregationPerGridarea = new AggregationPerGridArea { GridAreaCode = calculationResult.GridArea };
        }
        else if (calculationResult.BalanceResponsibleId != null && calculationResult.EnergySupplierId != null)
        {
            calculationResultCompleted.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea =
                new AggregationPerEnergySupplierPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = calculationResult.GridArea,
                    EnergySupplierGlnOrEic = calculationResult.EnergySupplierId,
                    BalanceResponsiblePartyGlnOrEic = calculationResult.BalanceResponsibleId,
                };
        }
        else if (calculationResult.BalanceResponsibleId == null && calculationResult.EnergySupplierId != null)
        {
            calculationResultCompleted.AggregationPerEnergysupplierPerGridarea =
                new AggregationPerEnergySupplierPerGridArea
                {
                    GridAreaCode = calculationResult.GridArea,
                    EnergySupplierGlnOrEic = calculationResult.EnergySupplierId,
                };
        }
        else
        {
            calculationResultCompleted.AggregationPerBalanceresponsiblepartyPerGridarea =
                new AggregationPerBalanceResponsiblePartyPerGridArea
                {
                    GridAreaCode = calculationResult.GridArea,
                    BalanceResponsiblePartyGlnOrEic = calculationResult.BalanceResponsibleId,
                };
        }

        return calculationResultCompleted;
    }
}
