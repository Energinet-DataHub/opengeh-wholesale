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
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.EDI.Validation;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

public class AggregatedTimeSeriesMessageFactoryTests
{
    private readonly Guid _batchId = Guid.NewGuid();
    private readonly Guid _id = Guid.NewGuid();
    private readonly string _gridArea = "543";
    private readonly string _energySupplierId = "es_id";
    private readonly string _balanceResponsibleId = "br_id";
    private readonly string _fromGridArea = "123";
    private readonly Instant _periodStart = SystemClock.Instance.GetCurrentInstant();
    private readonly Instant _periodEnd = SystemClock.Instance.GetCurrentInstant();
    private readonly TimeSeriesType _timeSeriesType = TimeSeriesType.Production;

    [Fact]
    public void Create_WithCalculationResultFromTotalProductionPerGridArea_CreatesAcceptedEdiMessage()
    {
        // Arrange
        var expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);
        var expectedReferenceId = "123456789";
        var energyResult = CreateEnergyResult();
        var sut = new AggregatedTimeSeriesMessageFactory();

        // Act
        var response = sut.Create(energyResult, expectedReferenceId);

        // Assert
        response.Should().NotBeNull();
        response.ApplicationProperties.ContainsKey("ReferenceId").Should().BeTrue();
        response.ApplicationProperties["ReferenceId"].ToString().Should().Be(expectedReferenceId);
        response.Subject.Should().Be(expectedAcceptedSubject);

        var responseBody = AggregatedTimeSeriesRequestAccepted.Parser.ParseFrom(response.Body);
        responseBody.GridArea.Should().Be(_gridArea);
        responseBody.TimeSeriesType.Should().Be(Energinet.DataHub.Edi.Responses.TimeSeriesType.Production);
        responseBody.Period.StartOfPeriod.Should().Be(new Timestamp() { Seconds = _periodStart.ToUnixTimeSeconds() });
        responseBody.Period.EndOfPeriod.Should().Be(new Timestamp() { Seconds = _periodEnd.ToUnixTimeSeconds() });
        responseBody.TimeSeriesPoints.Count.Should().Be(energyResult.TimeSeriesPoints.Length);
    }

    [Fact]
    public void Create_WithNoCalculationResult_CreatesRejectMessage()
    {
        // Arrange
        var expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = "123456789";
        var sut = new AggregatedTimeSeriesMessageFactory();

        // Act
        var response = sut.Create(null, expectedReferenceId);

        // Assert
        response.Should().NotBeNull();
        response.ApplicationProperties.ContainsKey("ReferenceId").Should().BeTrue();
        response.ApplicationProperties["ReferenceId"].ToString().Should().Be(expectedReferenceId);
        response.Subject.Should().Be(expectedAcceptedSubject);

        var responseBody = AggregatedTimeSeriesRequestRejected.Parser.ParseFrom(response.Body);
        responseBody.RejectReasons.Should().ContainSingle();
        responseBody.RejectReasons[0].ErrorCode.Should().Be(ValidationError.NoDataFound.ErrorCode);
    }

    private EnergyResult CreateEnergyResult()
    {
        return new EnergyResult(
            _id,
            _batchId,
            _gridArea,
            _timeSeriesType,
            _energySupplierId,
            _balanceResponsibleId,
            new EnergyTimeSeriesPoint[]
            {
                new(new DateTime(2021, 1, 1), 1, CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Estimated),
                new(new DateTime(2021, 1, 1), 2, CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Estimated),
                new(new DateTime(2021, 1, 1), 3, CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality.Estimated),
            },
            ProcessType.Aggregation,
            _periodStart,
            _periodEnd,
            _fromGridArea);
    }
}
