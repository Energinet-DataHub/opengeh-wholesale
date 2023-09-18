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
using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents;
using Google.Protobuf.WellKnownTypes;
using Moq;
using NodaTime;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesPoint = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesPoint;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.InboxEvents;

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
        var response = sut.Create(new List<EnergyResult> { energyResult }, expectedReferenceId, isRejected: false);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.ApplicationProperties.ContainsKey("ReferenceId"));
        Assert.Equal(expectedReferenceId, response.ApplicationProperties["ReferenceId"].ToString());
        Assert.Equal(expectedAcceptedSubject, response.Subject);
        var responseBody = AggregatedTimeSeriesRequestAccepted.Parser.ParseFrom(response.Body);
        Assert.All(responseBody.Series, serie =>
        {
            Assert.Equal(_gridArea, serie.GridArea);
            Assert.Equal(Energinet.DataHub.Edi.Responses.TimeSeriesType.Production, serie.TimeSeriesType);
            Assert.Equal(new Timestamp() { Seconds = _periodStart.ToUnixTimeSeconds() }, serie.Period.StartOfPeriod);
            Assert.Equal(new Timestamp() { Seconds = _periodEnd.ToUnixTimeSeconds() }, serie.Period.EndOfPeriod);
            Assert.Equal(energyResult.TimeSeriesPoints.Length, serie.TimeSeriesPoints.Count);

            var expected = CreateExpectedTimeSeries(energyResult.TimeSeriesPoints.ToList());
            for (var i = 0; i < energyResult.TimeSeriesPoints.Length; i++)
            {
                Assert.Equal(expected[i].Time, serie.TimeSeriesPoints[i].Time);
                Assert.Equal(expected[i].Quantity, serie.TimeSeriesPoints[i].Quantity);
                Assert.Equal(expected[i].QuantityQuality, serie.TimeSeriesPoints[i].QuantityQuality);
            }
        });
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

    private List<Energinet.DataHub.Edi.Responses.TimeSeriesPoint> CreateExpectedTimeSeries(List<TimeSeriesPoint> timeSeriesPoint)
    {
        var expected = new List<Energinet.DataHub.Edi.Responses.TimeSeriesPoint>();
        expected.AddRange(timeSeriesPoint.Select(point => new Energinet.DataHub.Edi.Responses.TimeSeriesPoint()
        {
            Time = new Timestamp() { Seconds = point.Time.ToUnixTimeSeconds() },
            Quantity =
                new DecimalValue()
                {
                    Units = decimal.ToInt64(point.Quantity),
                    Nanos = decimal.ToInt32((point.Quantity - decimal.ToInt64(point.Quantity)) * 1_000_000_000),
                },
            QuantityQuality = point.Quality == QuantityQuality.Estimated
                ? Energinet.DataHub.Edi.Responses.QuantityQuality.Estimated
                : Energinet.DataHub.Edi.Responses.QuantityQuality.Measured,
        }));

        return expected;
    }
}
