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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Calculations;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Period = Energinet.DataHub.Wholesale.Edi.Models.Period;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Calculations;

public class CompletedCalculationRetrieverTest
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetLatestCompletedCalculationForRequestAsync_WithLatestCorrectionWhenCalculationIsThirdCorrectionSettlement_ReturnsCalculationsAsync(
        [Frozen] Mock<LatestCalculationsForPeriod> latestCalculationsForPeriod,
        [Frozen] Mock<ILogger<CompletedCalculationRetriever>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        calculationsClient
            .Setup(x => x.SearchAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CalculationState>(),
                It.IsAny<Instant>(),
                It.IsAny<Instant>(),
                CalculationType.ThirdCorrectionSettlement))
            .ReturnsAsync(new List<CalculationDto>
            {
                CalculationDtoBuilder.CalculationDto()
                    .WithCalculationType(CalculationType.ThirdCorrectionSettlement)
                    .WithPeriodStart(startOfPeriodFilter)
                    .WithPeriodEnd(endOfPeriodFilter)
                    .Build(),
            });

        var request = CreateRequestParameters(startOfPeriodFilter, endOfPeriodFilter);

        var sut = new CompletedCalculationRetriever(latestCalculationsForPeriod.Object, calculationsClient.Object, logger.Object);

        // Act
        var actual = await sut.GetLatestCompletedCalculationsForPeriodAsync(request.GridAreaCodes, request.Period, request.RequestedCalculationType);

        // Assert
        actual.Should().HaveCount(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetLatestCorrectionAsync_WithLatestCorrectionWhenCalculationIsSecondCorrectionSettlement_ReturnsCalculationsAsync(
        [Frozen] Mock<LatestCalculationsForPeriod> latestCalculationsForPeriod,
        [Frozen] Mock<ILogger<CompletedCalculationRetriever>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        calculationsClient
            .Setup(x => x.SearchAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CalculationState>(),
                It.IsAny<Instant>(),
                It.IsAny<Instant>(),
                CalculationType.SecondCorrectionSettlement))
            .ReturnsAsync(new List<CalculationDto>
            {
                CalculationDtoBuilder.CalculationDto()
                    .WithCalculationType(CalculationType.SecondCorrectionSettlement)
                    .WithPeriodStart(startOfPeriodFilter)
                    .WithPeriodEnd(endOfPeriodFilter)
                    .Build(),
            });

        var request = CreateRequestParameters(startOfPeriodFilter, endOfPeriodFilter);

        var sut = new CompletedCalculationRetriever(latestCalculationsForPeriod.Object, calculationsClient.Object, logger.Object);

        // Act
        var actual = await sut.GetLatestCompletedCalculationsForPeriodAsync(request.GridAreaCodes, request.Period, request.RequestedCalculationType);

        // Assert
        actual.Should().HaveCount(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetLatestCorrectionAsync_WithLatestCorrectionWhenCalculationIsFirstCorrectionSettlement_ReturnsCalculationsAsync(
        [Frozen] Mock<LatestCalculationsForPeriod> latestCalculationsForPeriod,
        [Frozen] Mock<ILogger<CompletedCalculationRetriever>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        calculationsClient
            .Setup(x => x.SearchAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CalculationState>(),
                It.IsAny<Instant>(),
                It.IsAny<Instant>(),
                CalculationType.FirstCorrectionSettlement))
            .ReturnsAsync(new List<CalculationDto>
            {
                CalculationDtoBuilder.CalculationDto()
                    .WithCalculationType(CalculationType.FirstCorrectionSettlement)
                    .WithPeriodStart(startOfPeriodFilter)
                    .WithPeriodEnd(endOfPeriodFilter)
                    .Build(),
            });

        var request = CreateRequestParameters(startOfPeriodFilter, endOfPeriodFilter);

        var sut = new CompletedCalculationRetriever(latestCalculationsForPeriod.Object, calculationsClient.Object, logger.Object);

        // Act
        var actual = await sut.GetLatestCompletedCalculationsForPeriodAsync(request.GridAreaCodes, request.Period, request.RequestedCalculationType);

        // Assert
        actual.Should().HaveCount(1);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetLatestCompletedCalculationForRequestAsync_WhenNoCorrectionsExists_ReturnsNoResultAsync(
        [Frozen] Mock<LatestCalculationsForPeriod> latestCalculationsForPeriod,
        [Frozen] Mock<ILogger<CompletedCalculationRetriever>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var request = CreateRequestParameters(startOfPeriodFilter, endOfPeriodFilter);

        var sut = new CompletedCalculationRetriever(latestCalculationsForPeriod.Object, calculationsClient.Object, logger.Object);

        // Act
        var actual = await sut.GetLatestCompletedCalculationsForPeriodAsync(request.GridAreaCodes, request.Period, request.RequestedCalculationType);

        // Assert
        actual.Should().HaveCount(0);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetLatestCorrectionAsync_WithLatestCorrectionWhenFirstCorrectionSettlementCalculationAndSecondCorrectionSettlementCalculation_ReturnsCalculationsForSecondCorrectionAsync(
        [Frozen] Mock<LatestCalculationsForPeriod> latestCalculationsForPeriod,
        [Frozen] Mock<ILogger<CompletedCalculationRetriever>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var calculationWithFirstCorrection = CalculationDtoBuilder.CalculationDto()
            .WithCalculationType(CalculationType.FirstCorrectionSettlement)
            .WithPeriodStart(startOfPeriodFilter)
            .WithPeriodEnd(endOfPeriodFilter)
            .Build();
        var calculationWithSecondCorrection = CalculationDtoBuilder.CalculationDto()
            .WithCalculationType(CalculationType.FirstCorrectionSettlement)
            .WithPeriodStart(startOfPeriodFilter)
            .WithPeriodEnd(endOfPeriodFilter)
            .Build();

        calculationsClient
            .Setup(x => x.SearchAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CalculationState>(),
                It.IsAny<Instant>(),
                It.IsAny<Instant>(),
                CalculationType.FirstCorrectionSettlement))
            .ReturnsAsync(new List<CalculationDto>
            {
                calculationWithFirstCorrection,
            });
        calculationsClient
            .Setup(x => x.SearchAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CalculationState>(),
                It.IsAny<Instant>(),
                It.IsAny<Instant>(),
                CalculationType.SecondCorrectionSettlement))
            .ReturnsAsync(new List<CalculationDto>
            {
                calculationWithSecondCorrection,
            });

        var request = CreateRequestParameters(startOfPeriodFilter, endOfPeriodFilter);

        var sut = new CompletedCalculationRetriever(latestCalculationsForPeriod.Object, calculationsClient.Object, logger.Object);

        // Act
        var actual = await sut.GetLatestCompletedCalculationsForPeriodAsync(request.GridAreaCodes, request.Period, request.RequestedCalculationType);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.Should().ContainSingle(x => x.CalculationId == calculationWithSecondCorrection.CalculationId);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetLatestCorrectionAsync_WithSecondCorrectionWhenSecondCorrectionSettlementCalculationAndThirdCorrectionSettlementCalculation_ReturnsCalculationsForSecondCorrectionAsync(
        [Frozen] Mock<LatestCalculationsForPeriod> latestCalculationsForPeriod,
        [Frozen] Mock<ILogger<CompletedCalculationRetriever>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        var startOfPeriodFilter = Instant.FromUtc(2022, 1, 1, 0, 0);
        var endOfPeriodFilter = Instant.FromUtc(2022, 1, 2, 0, 0);

        var calculationWithSecondCorrection = CalculationDtoBuilder.CalculationDto()
            .WithCalculationType(CalculationType.SecondCorrectionSettlement)
            .WithPeriodStart(startOfPeriodFilter)
            .WithPeriodEnd(endOfPeriodFilter)
            .Build();
        var calculationWithThirdCorrection = CalculationDtoBuilder.CalculationDto()
            .WithCalculationType(CalculationType.ThirdCorrectionSettlement)
            .WithPeriodStart(startOfPeriodFilter)
            .WithPeriodEnd(endOfPeriodFilter)
            .Build();

        calculationsClient
            .Setup(x => x.SearchAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CalculationState>(),
                It.IsAny<Instant>(),
                It.IsAny<Instant>(),
                CalculationType.SecondCorrectionSettlement))
            .ReturnsAsync(new List<CalculationDto>
            {
                calculationWithSecondCorrection,
            });
        calculationsClient
            .Setup(x => x.SearchAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CalculationState>(),
                It.IsAny<Instant>(),
                It.IsAny<Instant>(),
                CalculationType.ThirdCorrectionSettlement))
            .ReturnsAsync(new List<CalculationDto>
            {
                calculationWithThirdCorrection,
            });

        var request = CreateRequestParameters(startOfPeriodFilter, endOfPeriodFilter, RequestedCalculationType.SecondCorrection);

        var sut = new CompletedCalculationRetriever(latestCalculationsForPeriod.Object, calculationsClient.Object, logger.Object);

        // Act
        var actual = await sut.GetLatestCompletedCalculationsForPeriodAsync(request.GridAreaCodes, request.Period, request.RequestedCalculationType);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().HaveCount(1);
        actual.Should().ContainSingle(x => x.CalculationId == calculationWithSecondCorrection.CalculationId);
    }

    private static (string[] GridAreaCodes, Period Period, RequestedCalculationType RequestedCalculationType) CreateRequestParameters(
        Instant startOfPeriodFilter,
        Instant endOfPeriodFilter,
        RequestedCalculationType? requestedCalculationType = null)
    {
        return (
            ["543"],
            new Period(startOfPeriodFilter, endOfPeriodFilter),
            requestedCalculationType ?? RequestedCalculationType.LatestCorrection);
    }
}
