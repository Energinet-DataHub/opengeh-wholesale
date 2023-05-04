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
using Energinet.DataHub.Wholesale.Batches.Infrastructure;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchAggregate;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.GridAreaAggregate;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.BatchAggregate;

[UnitTest]
public class BatchTests
{
    [Fact]
    public void Ctor_CreatesImmutableGridAreaCodes()
    {
        // Arrange
        var someGridAreaCodes = new List<GridAreaCode> { new("004"), new("805") };
        var sut = new BatchBuilder().WithGridAreaCodes(someGridAreaCodes).Build();

        // Act
        var unexpectedGridAreaCode = new GridAreaCode("777");
        someGridAreaCodes.Add(unexpectedGridAreaCode);

        // Assert
        sut.GridAreaCodes.Should().NotContain(unexpectedGridAreaCode);
    }

    [Fact]
    public void Ctor_WhenNoGridAreaCodes_ThrowsBusinessValidationException()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var emptyGridAreaCodes = new List<GridAreaCode>();
        var actual = Assert.Throws<BusinessValidationException>(() => new Batch(
            ProcessType.BalanceFixing,
            emptyGridAreaCodes,
            Instant.FromDateTimeOffset(DateTimeOffset.Now),
            Instant.FromDateTimeOffset(DateTimeOffset.Now),
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!));
        actual.Message.Should().Contain("Batch must contain at least one grid area code");
    }

    [Fact]
    public void Ctor_SetsExecutionTimeEndToNull()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.ExecutionTimeEnd.Should().BeNull();
    }

    [Fact]
    public void Ctor_ExecutionTimeStartNotNull()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.ExecutionTimeStart.Should().NotBeNull();
    }

    [Fact]
    public void GetResolution_DoesNotThrowExceptionForAllProcessTypes()
    {
        // Arrange
        foreach (var processType in Enum.GetValues(typeof(ProcessType)))
        {
            var sut = new BatchBuilder().WithProcessType((ProcessType)processType).Build();

            // Act & Assert
            sut.GetResolution();
        }
    }

    [Fact]
    public void GetQuantityUnit_DoesNotThrowExceptionForAllProcessTypes()
    {
        // Arrange
        foreach (var processType in Enum.GetValues(typeof(ProcessType)))
        {
            var sut = new BatchBuilder().WithProcessType((ProcessType)processType).Build();

            // Act & Assert - Remember to add new [InlineAutoMoqData (...,...)] for new process types in other tests
            sut.GetQuantityUnit();
        }
    }

    [Theory]
    [InlineAutoMoqData(Contracts.ProcessType.BalanceFixing, "PT15M")]
    [InlineAutoMoqData(Contracts.ProcessType.Aggregation, "PT15M")]
    public void GetResolution_ReturnsExpectedIso8601Duration(ProcessType processType, string expectedIso8601Duration)
    {
        // Arrange
        var sut = new BatchBuilder().WithProcessType(processType).Build();

        // Act
        var actual = sut.GetResolution();

        // Assert
        actual.Should().Be(expectedIso8601Duration);
    }

    [Theory]
    [InlineAutoMoqData(ProcessType.BalanceFixing, "kWh")]
    [InlineAutoMoqData(ProcessType.Aggregation, "kWh")]
    public void GetQuantityUnit_ReturnsExpectedIso8601Duration(ProcessType processType, string expectedQuantityUnit)
    {
        // Arrange
        var sut = new BatchBuilder().WithProcessType(processType).Build();

        // Act
        var actual = sut.GetQuantityUnit();

        // Assert
        actual.Should().Be(expectedQuantityUnit);
    }

    [Fact]
    public void MarkAsCompleted_WhenComplete_ThrowsBusinessValidationException()
    {
        var sut = new BatchBuilder().WithStateCompleted().Build();
        Assert.Throws<BusinessValidationException>(() => sut.MarkAsCompleted(It.IsAny<Instant>()));
    }

    [Theory]
    [InlineData("2023-01-31T23:00:00.0001Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T22:59:59Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T22:59:59.9999999Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T23:00:00Z", "Asia/Tokyo")]
    public void Ctor_WhenPeriodEndIsNotMidnight_ThrowsBusinessValidationException(string periodEndString, string timeZoneId)
    {
        // Arrange
        var periodEnd = DateTimeOffset.Parse(periodEndString).ToInstant();
        var gridAreaCode = new GridAreaCode("113");

        // Act
        var actual = Assert.Throws<BusinessValidationException>(() => new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { gridAreaCode },
            Instant.MinValue,
            periodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId)!));

        // Assert
        actual.Message.Should().ContainAll("period", "end");
    }

    [Theory]
    [InlineData("2023-01-31T22:59:00.9999999Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T23:00:00.9999999Z", "Europe/Copenhagen")]
    [InlineData("2023-01-31T23:00:00Z", "America/Cayman")]
    public void Ctor_WhenPeriodStartIsNotMidnight_ThrowsBusinessValidationException(string periodStartString, string timeZoneId)
    {
        // Arrange
        var startPeriod = DateTimeOffset.Parse(periodStartString).ToInstant();
        var gridAreaCode = new GridAreaCode("113");

        // Act
        var actual = Assert.Throws<BusinessValidationException>(() => new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { gridAreaCode },
            startPeriod,
            Instant.FromDateTimeOffset(new DateTimeOffset(2023, 02, 01, 23, 0, 0, new TimeSpan(0))),
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId)!));

        // Assert
        actual.Message.Should().Contain($"The period start '{startPeriod.ToString()}' must be midnight.");
    }

    [Fact]
    public void MarkAsCompleted_WhenExecuting_CompletesBatch()
    {
        // Arrange
        var sut = new BatchBuilder().WithStateExecuting().Build();
        var executionTimeEndGreaterThanStart = sut.ExecutionTimeStart.Plus(Duration.FromDays(2));

        // Act
        sut.MarkAsCompleted(executionTimeEndGreaterThanStart);

        // Assert
        sut.ExecutionState.Should().Be(BatchExecutionState.Completed);
    }

    [Fact]
    public void MarkAsCompleted_SetsExecutionTimeEnd()
    {
        // Arrange
        var sut = new BatchBuilder().WithStateExecuting().Build();
        var executionTimeEndGreaterThanStart = sut.ExecutionTimeStart.Plus(Duration.FromDays(2));

        // Act
        sut.MarkAsCompleted(executionTimeEndGreaterThanStart);

        // Assert
        sut.ExecutionTimeEnd.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsCompleted_WhenExecutionTimeEndLessThanStart_ThrowsBusinessValidationException()
    {
        // Arrange
        var sut = new BatchBuilder().WithStateExecuting().Build();
        var executionTimeEndLessThanStart = sut.ExecutionTimeStart.Minus(Duration.FromDays(2));

        // Act
        var actual = Assert.Throws<BusinessValidationException>(() => sut.MarkAsCompleted(executionTimeEndLessThanStart));

        // Assert
        sut.ExecutionTimeEnd.Should().BeNull();
        actual.Message.Should().Contain("cannot be before execution time start");
    }

    [Fact]
    public void MarkAsExecuting_WhenExecuting_ThrowsBusinessValidationException()
    {
        var sut = new BatchBuilder().WithStateExecuting().Build();
        Assert.Throws<BusinessValidationException>(() => sut.MarkAsExecuting());
    }

    [Fact]
    public void MarkAsExecuting_WhenComplete_ThrowsBusinessValidationException()
    {
        var sut = new BatchBuilder().WithStateCompleted().Build();
        Assert.Throws<BusinessValidationException>(() => sut.MarkAsExecuting());
    }

    [Fact]
    public void MarkAsExecuting_WhenPending_ExecutesBatch()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.MarkAsExecuting();
        sut.ExecutionState.Should().Be(BatchExecutionState.Executing);
    }

    [Fact]
    public void MarkAsExecuting_ExecutionTimeIsSetToNull()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.MarkAsExecuting();
        sut.ExecutionTimeEnd.Should().BeNull();
    }

    [Fact]
    public void Reset_WhenSubmitted_SetsStateCreated()
    {
        var sut = new BatchBuilder().WithStateSubmitted().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(BatchExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenPending_SetsStateCreated()
    {
        var sut = new BatchBuilder().WithStatePending().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(BatchExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenExecuting_SetsStateCreated()
    {
        var sut = new BatchBuilder().WithStateExecuting().Build();
        sut.Reset();
        sut.ExecutionState.Should().Be(BatchExecutionState.Created);
    }

    [Fact]
    public void Reset_WhenCompleted_ThrowsBusinessValidationException()
    {
        var sut = new BatchBuilder().WithStateCompleted().Build();
        Assert.Throws<BusinessValidationException>(() => sut.Reset());
    }
}
