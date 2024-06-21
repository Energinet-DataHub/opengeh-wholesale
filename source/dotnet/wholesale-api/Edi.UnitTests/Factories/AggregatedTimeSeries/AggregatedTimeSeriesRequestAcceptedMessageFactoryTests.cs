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
using Energinet.DataHub.Wholesale.Edi.Factories.AggregatedTimeSeries;
using FluentAssertions;
using FluentAssertions.Execution;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Factories.AggregatedTimeSeries;

public class AggregatedTimeSeriesRequestAcceptedMessageFactoryTests
{
    private readonly string _gridArea = "543";
    private readonly Instant _periodStart = Instant.FromUtc(2020, 12, 31, 23, 0);
    private readonly Instant _periodEnd = Instant.FromUtc(2021, 1, 1, 23, 0);
    private readonly TimeSeriesType _timeSeriesType = TimeSeriesType.Production;

    public static IEnumerable<object[]> QuantityQualitySets()
    {
        return new[]
        {
            new object[] { new object[] { QuantityQuality.Missing } },
            new object[] { new object[] { QuantityQuality.Measured } },
            new object[] { new object[] { QuantityQuality.Estimated, QuantityQuality.Calculated } },
            new object[] { new object[] { QuantityQuality.Estimated, QuantityQuality.Calculated, QuantityQuality.Missing } },
        };
    }

    [Fact]
    public void Create_WithCalculationResultFromTotalProductionPerGridArea_CreatesCorrectAcceptedEdiMessage()
    {
        // Arrange
        var expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);
        var expectedReferenceId = "123456789";
        var aggregatedTimeSeries = CreateAggregatedTimeSeries();

        // Act
        var actual = AggregatedTimeSeriesRequestAcceptedMessageFactory.Create(aggregatedTimeSeries, expectedReferenceId);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.ApplicationProperties.ContainsKey("ReferenceId").Should().BeTrue();
        actual.ApplicationProperties["ReferenceId"].ToString().Should().Be(expectedReferenceId);
        actual.Subject.Should().Be(expectedAcceptedSubject);

        var responseBody = AggregatedTimeSeriesRequestAccepted.Parser.ParseFrom(actual.Body);
        var series = responseBody?.Series.FirstOrDefault();
        series.Should().NotBeNull();
        series!.GridArea.Should().Be(_gridArea);
        series.TimeSeriesType.Should().Be(Energinet.DataHub.Edi.Responses.TimeSeriesType.Production);
        series.HasSettlementVersion.Should().BeFalse();

        var timeSeriesOrdered = series.TimeSeriesPoints.OrderBy(ts => ts.Time).ToList();
        var earliestTimestamp = timeSeriesOrdered.First();
        var latestTimestamp = timeSeriesOrdered.Last();

        var periodStartTimestamp = new Timestamp() { Seconds = _periodStart.ToUnixTimeSeconds() };
        var periodEndTimestamp = new Timestamp() { Seconds = _periodEnd.ToUnixTimeSeconds() };
        earliestTimestamp.Time.Should().BeGreaterThanOrEqualTo(periodStartTimestamp)
            .And.BeLessThan(periodEndTimestamp);
        latestTimestamp.Time.Should().BeLessThan(periodEndTimestamp)
            .And.BeGreaterOrEqualTo(earliestTimestamp.Time);

        series.TimeSeriesPoints.Count.Should().Be(aggregatedTimeSeries.First().TimeSeriesPoints.Length);
    }

    [Fact]
    public void Create_CalculationResultWithSettlement_CreatesAcceptedEdiMessageWithSettlementVersion()
    {
        // Arrange
        var expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);
        var expectedReferenceId = "123456789";
        var aggregatedTimeSeries = CreateAggregatedTimeSeries(calculationType: CalculationType.FirstCorrectionSettlement);

        // Act
        var actual = AggregatedTimeSeriesRequestAcceptedMessageFactory.Create(aggregatedTimeSeries, expectedReferenceId);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.ApplicationProperties.ContainsKey("ReferenceId").Should().BeTrue();
        actual.ApplicationProperties["ReferenceId"].ToString().Should().Be(expectedReferenceId);
        actual.Subject.Should().Be(expectedAcceptedSubject);

        var responseBody = AggregatedTimeSeriesRequestAccepted.Parser.ParseFrom(actual.Body);
        var series = responseBody?.Series.FirstOrDefault();
        series.Should().NotBeNull();
        series!.SettlementVersion.Should().Be(SettlementVersion.FirstCorrection);
    }

    [Theory]
    [MemberData(nameof(QuantityQualitySets))]
    public void Create_DifferentSetsOfQualities_CreatesCorrectAcceptedEdiMessage(QuantityQuality[] quantityQualities)
    {
        // Arrange
        const string expectedReferenceId = "123456789";
        var expectedQuantityQualities = quantityQualities.ToList();
        var aggregatedTimeSeries = CreateAggregatedTimeSeries(expectedQuantityQualities);

        // Act
        var actual = AggregatedTimeSeriesRequestAcceptedMessageFactory.Create(aggregatedTimeSeries, expectedReferenceId);

        // Assert
        actual.Should().NotBeNull();
        var responseBody = AggregatedTimeSeriesRequestAccepted.Parser.ParseFrom(actual.Body);
        responseBody.Series.Should().ContainSingle();
        responseBody.Series.Single().TimeSeriesPoints.Should().HaveCount(3);
        responseBody.Series.Single().TimeSeriesPoints.Select(p => p.QuantityQualities).Should().AllSatisfy(
            qqs =>
            {
                qqs.Should().HaveCount(expectedQuantityQualities.Count);
                qqs.Select(qq => qq.ToString())
                    .Should()
                    .Contain(expectedQuantityQualities.Select(qq => qq.ToString()));
            });
    }

    private IReadOnlyCollection<CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.AggregatedTimeSeries> CreateAggregatedTimeSeries(
        IReadOnlyCollection<QuantityQuality>? quantityQualities = null,
        CalculationType calculationType = CalculationType.Aggregation)
    {
        quantityQualities ??= new List<QuantityQuality> { QuantityQuality.Estimated };

        var aggregatedTimeSeries = new CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.AggregatedTimeSeries(
            _gridArea,
            new EnergyTimeSeriesPoint[]
            {
                new(new DateTime(2021, 1, 1, 0, 15, 0), 1, quantityQualities),
                new(new DateTime(2021, 1, 1, 0, 30, 0), 2, quantityQualities),
                new(new DateTime(2021, 1, 1, 0, 45, 0), 3, quantityQualities),
            },
            _timeSeriesType,
            calculationType,
            DateTimeOffset.Parse("2022-01-01T00:00Z").ToInstant(),
            DateTimeOffset.Parse("2022-01-01T00:45Z").ToInstant(),
            Resolution.Quarter,
            1);

        return new List<CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.AggregatedTimeSeries>()
        {
            aggregatedTimeSeries,
        };
    }
}
