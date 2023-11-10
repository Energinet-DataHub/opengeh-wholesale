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

using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.EDI.Factories;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

public class AggregatedTimeSeriesRequestAcceptedMessageFactoryTests
{
    private readonly Guid _batchId = Guid.NewGuid();
    private readonly Guid _id = Guid.NewGuid();
    private readonly string _gridArea = "543";
    private readonly string _energySupplierId = "es_id";
    private readonly string _balanceResponsibleId = "br_id";
    private readonly string _fromGridArea = "123";
    private readonly Instant _periodStart = Instant.FromUtc(2020, 12, 31, 23, 0);
    private readonly Instant _periodEnd = Instant.FromUtc(2021, 1, 1, 23, 0);
    private readonly TimeSeriesType _timeSeriesType = TimeSeriesType.Production;

    [Fact]
    public void Create_WithCalculationResultFromTotalProductionPerGridArea_CreatesCorrectAcceptedEdiMessage()
    {
        // Arrange
        var expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);
        var expectedReferenceId = "123456789";
        var energyResult = CreateEnergyResult();

        // Act
        var response = AggregatedTimeSeriesRequestAcceptedMessageFactory.Create(energyResult, expectedReferenceId);

        // Assert
        response.Should().NotBeNull();
        response.ApplicationProperties.ContainsKey("ReferenceId").Should().BeTrue();
        response.ApplicationProperties["ReferenceId"].ToString().Should().Be(expectedReferenceId);
        response.Subject.Should().Be(expectedAcceptedSubject);

        var responseBody = AggregatedTimeSeriesRequestAccepted.Parser.ParseFrom(response.Body);
        responseBody.GridArea.Should().Be(_gridArea);
        responseBody.TimeSeriesType.Should().Be(Energinet.DataHub.Edi.Responses.TimeSeriesType.Production);

        var timeSeriesOrdered = responseBody.TimeSeriesPoints.OrderBy(ts => ts.Time).ToList();
        var earliestTimestamp = timeSeriesOrdered.First();
        var latestTimestamp = timeSeriesOrdered.Last();

        var periodStartTimestamp = new Timestamp() { Seconds = _periodStart.ToUnixTimeSeconds() };
        var periodEndTimestamp = new Timestamp() { Seconds = _periodEnd.ToUnixTimeSeconds() };
        earliestTimestamp.Time.Should().BeGreaterThanOrEqualTo(periodStartTimestamp)
            .And.BeLessThan(periodEndTimestamp);
        latestTimestamp.Time.Should().BeLessThan(periodEndTimestamp)
            .And.BeGreaterOrEqualTo(earliestTimestamp.Time);

        responseBody.TimeSeriesPoints.Count.Should().Be(energyResult.TimeSeriesPoints.Length);
    }

    private EnergyResult CreateEnergyResult()
    {
        var quantityQualities = new List<QuantityQuality> { QuantityQuality.Estimated };

        return new EnergyResult(
            _id,
            _batchId,
            _gridArea,
            _timeSeriesType,
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
}
