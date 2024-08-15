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
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationAggregate;

public class CalculationFactoryTests
{
    private readonly DateTimeOffset _startDate = DateTimeOffset.Parse("2021-12-31T23:00Z");
    private readonly DateTimeOffset _endDate = DateTimeOffset.Parse("2022-01-31T23:00Z");
    private readonly DateTimeOffset _scheduledAt = DateTimeOffset.Parse("2024-08-15T13:37Z");
    private readonly DateTimeZone _timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;
    private readonly List<string> _someGridAreasIds = ["004", "805"];

    [Fact]
    public void Create_ReturnsCalculationWithCorrectPeriod()
    {
        // Arrange
        var sut = new CalculationFactory(SystemClock.Instance, _timeZone);

        // Act
        var calculation = sut.Create(CalculationType.BalanceFixing, _someGridAreasIds, _startDate, _endDate, _scheduledAt, Guid.NewGuid());

        // Assert
        calculation.PeriodStart.Should().Be(Instant.FromDateTimeOffset(_startDate));
        calculation.PeriodEnd.Should().Be(Instant.FromDateTimeOffset(_endDate));
    }

    [Fact]
    public void Create_ReturnsCalculationWithCorrectScheduledAt()
    {
        // Arrange
        var sut = new CalculationFactory(SystemClock.Instance, _timeZone);

        // Act
        var calculation = sut.Create(CalculationType.BalanceFixing, _someGridAreasIds, _startDate, _endDate, _scheduledAt, Guid.NewGuid());

        // Assert
        calculation.ScheduledAt.Should().Be(Instant.FromDateTimeOffset(_scheduledAt));
    }

    [Fact]
    public void Create_ReturnsCalculationWithCorrectGridAreas()
    {
        // Arrange
        var sut = new CalculationFactory(SystemClock.Instance, _timeZone);

        // Act
        var calculation = sut.Create(CalculationType.BalanceFixing, _someGridAreasIds, _startDate, _endDate, _scheduledAt, Guid.NewGuid());

        // Assert
        calculation.GridAreaCodes.Select(x => x.Code).Should().Contain(_someGridAreasIds);
        calculation.GridAreaCodes.Count.Should().Be(_someGridAreasIds.Count);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Create_ReturnsCalculationWithExpectedExecutionTimeStart([Frozen] Mock<IClock> clockMock)
    {
        // Arrange
        var sut = new CalculationFactory(clockMock.Object, _timeZone);

        // Act
        var calculation = sut.Create(CalculationType.BalanceFixing, _someGridAreasIds, _startDate, _endDate, _scheduledAt, Guid.NewGuid());

        // Assert
        calculation.ExecutionTimeStart.Should().Be(Instant.FromDateTimeOffset(_scheduledAt));
    }

    [Theory]
    [AutoMoqData]
    public void Create_ReturnsCalculationWithCorrectVersion([Frozen] Mock<IClock> clockMock)
    {
        // Arrange
        var instant = SystemClock.Instance.GetCurrentInstant();
        var expected = instant.ToDateTimeUtc().Ticks;
        clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(instant);
        var sut = new CalculationFactory(clockMock.Object, _timeZone);

        // Act
        var actual = sut.Create(CalculationType.BalanceFixing, _someGridAreasIds, _startDate, _endDate, _scheduledAt, Guid.NewGuid()).Version;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Create_WhenAnotherCalculationCreatedFirst_ReturnsCalculationWithHigherVersion()
    {
        // Arrange
        var sut = new CalculationFactory(SystemClock.Instance, _timeZone);
        var earlierCalculation = sut.Create(CalculationType.BalanceFixing, _someGridAreasIds, _startDate, _endDate, _scheduledAt, Guid.NewGuid());
        var earlierVersion = earlierCalculation.Version;

        // Act
        var actual = sut.Create(CalculationType.BalanceFixing, _someGridAreasIds, _startDate, _endDate, _scheduledAt, Guid.NewGuid()).Version;

        // Assert
        actual.Should().BeGreaterThan(earlierVersion);
    }
}
