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

using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Factories.WholesaleServices;
using FluentAssertions;
using FluentAssertions.Execution;
using NodaTime;
using Xunit;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;
using QuantityUnit = Energinet.DataHub.Wholesale.Common.Interfaces.Models.QuantityUnit;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;
using WholesaleQuantity = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Factories.WholesaleServices;

public class WholesaleServiceRequestAcceptedMessageFactoryTests
{
    private readonly string _gridArea = "543";
    private readonly string _energySupplier = "1234567891234";
    private readonly string _chargeOwner = "1234567891999";
    private readonly Instant _periodStart = Instant.FromUtc(2020, 12, 31, 23, 0);
    private readonly Instant _periodEnd = Instant.FromUtc(2021, 1, 1, 23, 0);
    private static readonly Instant _defaultTime = Instant.FromUtc(2022, 5, 1, 1, 0);

    public static IEnumerable<object[]> QuantityQualitySets()
    {
        return new object[][]
        {
            [new[] { WholesaleQuantity.Missing }],
            [new[] { WholesaleQuantity.Measured }],
            [new[] { WholesaleQuantity.Estimated, WholesaleQuantity.Calculated }],
            [new[] { WholesaleQuantity.Estimated, WholesaleQuantity.Calculated, WholesaleQuantity.Missing }],
        };
    }

    [Fact]
    public void Create_WhenSendingMonthlyWholesaleResult_CreatesCorrectAcceptedEdiMessage()
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(WholesaleServicesRequestAccepted);
        const string expectedReferenceId = "123456789";
        var wholesaleService = CreateWholesaleServices(
            meteringPointType: null,
            settlementMethod: null,
            resolution: Resolution.Month);

        // Act
        var actual = WholesaleServiceRequestAcceptedMessageFactory.Create(wholesaleService, expectedReferenceId);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.ApplicationProperties.Should().ContainKey("ReferenceId");
        actual.ApplicationProperties["ReferenceId"].ToString().Should().Be(expectedReferenceId);
        actual.Subject.Should().Be(expectedAcceptedSubject);

        var responseBody = WholesaleServicesRequestAccepted.Parser.ParseFrom(actual.Body);
        var series = responseBody?.Series.FirstOrDefault();
        series.Should().NotBeNull();
        series!.GridArea.Should().Be(_gridArea);
        series.HasMeteringPointType.Should().Be(false);
        series.HasSettlementMethod.Should().Be(false);
        series.Resolution.Should().Be(WholesaleServicesRequestSeries.Types.Resolution.Monthly);
        series.TimeSeriesPoints.Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(QuantityQualitySets))]
    public void Create_DifferentSetsOfQualities_CreatesCorrectAcceptedEdiMessage(WholesaleQuantity[] quantityQualities)
    {
        // Arrange
        const string expectedReferenceId = "123456789";
        var expectedQuantityQualities = quantityQualities.ToList();
        var wholesaleService = CreateWholesaleServices(
            quantityQualities: expectedQuantityQualities);

        // Act
        var actual = WholesaleServiceRequestAcceptedMessageFactory.Create(wholesaleService, expectedReferenceId);

        // Assert
        actual.Should().NotBeNull();
        var responseBody = WholesaleServicesRequestAccepted.Parser.ParseFrom(actual.Body);
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

    private IReadOnlyCollection<CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.WholesaleServices> CreateWholesaleServices(
        IReadOnlyCollection<WholesaleQuantity>? quantityQualities = null,
        MeteringPointType? meteringPointType = null,
        SettlementMethod? settlementMethod = null,
        Resolution resolution = Resolution.Month,
        CalculationType calculationType = CalculationType.WholesaleFixing)
    {
        quantityQualities ??= new List<WholesaleQuantity> { WholesaleQuantity.Estimated };

        return [
            new(
                new Period(
                    _periodStart,
                    _periodEnd),
                _gridArea,
                _energySupplier,
                "FaQ-s0-t4",
                ChargeType.Tariff,
                _chargeOwner,
                resolution,
                QuantityUnit.Kwh,
                meteringPointType,
                settlementMethod,
                Currency.DKK,
                calculationType,
                new WholesaleTimeSeriesPoint[]
                {
                    new(_defaultTime.ToDateTimeOffset(), 2, quantityQualities, 2, 4),
                    new(_defaultTime.ToDateTimeOffset(), 3, quantityQualities, 2, 6),
                    new(_defaultTime.ToDateTimeOffset(), 3, quantityQualities, 3, 9),
                },
                1),
        ];
    }
}
