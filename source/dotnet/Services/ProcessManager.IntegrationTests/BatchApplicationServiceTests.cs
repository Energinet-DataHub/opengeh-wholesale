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
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchExecutionStateDomainService;
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using FluentAssertions;
using Microsoft.Azure.Databricks.Client;
using Moq;
using NodaTime;
using ProcessManager.IntegrationTests.Fixtures;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

[Collection(nameof(ProcessManagerIntegrationTestHost))]
public sealed class BatchApplicationServiceTests
{
    private const long DummyJobId = 42;
    private const string DummyJobName = "CalculatorJob";
    private readonly Mock<IDatabricksWheelClient> _databricksWheelClientMock = new Mock<IDatabricksWheelClient>();
    private readonly Mock<IJobsWheelApi> _jobsApiMock = new Mock<IJobsWheelApi>();
    private readonly ProcessManagerDatabaseFixture _processManagerDatabaseFixture;

    public BatchApplicationServiceTests(ProcessManagerDatabaseFixture processManagerDatabaseFixture)
    {
        _processManagerDatabaseFixture = processManagerDatabaseFixture;
        var jobs = new List<Job>() { CreateJob(DummyJobId, DummyJobName) };
        _jobsApiMock.Setup(x => x.List(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);
        _jobsApiMock.Setup(x => x.GetWheel(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCalculatorJob(DummyJobId, DummyJobName));

        var runIdentifier = new RunIdentifier() { RunId = DummyJobId, NumberInJob = 1 };
        _jobsApiMock.Setup(
                x => x.RunNow(
                    It.IsAny<long>(),
                    It.IsAny<RunParameters>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(runIdentifier);

        _databricksWheelClientMock.Setup(x => x.Jobs).Returns(_jobsApiMock.Object);
    }

    [Theory]
    [InlineAutoMoqData(CalculationState.Pending, BatchExecutionState.Pending)]
    [InlineAutoMoqData(CalculationState.Running, BatchExecutionState.Executing)]
    [InlineAutoMoqData(CalculationState.Completed, BatchExecutionState.Completed)]
    [InlineAutoMoqData(CalculationState.Canceled, BatchExecutionState.Canceled)]
    [InlineAutoMoqData(CalculationState.Failed, BatchExecutionState.Failed)]
    public void MapStateSuccess(CalculationState calculationState, BatchExecutionState expectedBatchExecutionState, BatchExecutionStateDomainService sut)
    {
        // Act
        var actualBatchExecutionState = sut.MapState(calculationState);

        // Assert
        actualBatchExecutionState.Should().Be(expectedBatchExecutionState);
    }

    [Theory]
    [InlineAutoMoqData]
    public void MapStateException(BatchExecutionStateDomainService sut)
    {
        // Arrange
        const CalculationState unexpectedCalculationState = (CalculationState)99;

        // Act
        Action action = () => sut.MapState(unexpectedCalculationState);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("calculationState")
            .And.ActualValue.Should().Be(unexpectedCalculationState);
    }

    [Theory]
    [InlineAutoMoqData(BatchExecutionState.Pending, BatchExecutionState.Pending, 0)]
    [InlineAutoMoqData(BatchExecutionState.Executing, BatchExecutionState.Executing, 0)]
    [InlineAutoMoqData(BatchExecutionState.Completed, BatchExecutionState.Completed, 1)]
    [InlineAutoMoqData(BatchExecutionState.Failed, BatchExecutionState.Failed, 0)]
    [InlineAutoMoqData(BatchExecutionState.Canceled, BatchExecutionState.Created, 0)]

    public void HandleNewStateSuccess(BatchExecutionState state, BatchExecutionState expectedState, int expectedCompletedBatchesCount,  BatchExecutionStateDomainService sut)
    {
        // Arrange
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new GridAreaCode("123") },
            Instant.MinValue,
            Instant.FromUtc(2023, 1, 1, 0, 0),
            Instant.MinValue,
            DateTimeZone.Utc);
        var completedBatches = new List<Batch>();

        // Act
        sut.HandleNewState(state, batch, completedBatches);

        // Assert
        batch.ExecutionState.Should().Be(expectedState);
        completedBatches.Count.Should().Be(expectedCompletedBatchesCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public void HandleNewStateException(BatchExecutionStateDomainService sut)
    {
        // Arrange
        var state = (BatchExecutionState)99;
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new GridAreaCode("123") },
            Instant.MinValue,
            Instant.FromUtc(2023, 1, 1, 0, 0),
            Instant.MinValue,
            DateTimeZone.Utc);
        var completedBatches = new List<Batch>();

        // Act
        Action action = () => sut.HandleNewState(state, batch, completedBatches);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static Job CreateJob(long jobId, string jobName)
    {
        return new Job()
        {
            JobId = jobId,
            Settings =
                new JobSettings()
                {
                    Name = jobName,
                },
        };
    }

    private static WheelJob CreateCalculatorJob(long jobId, string jobName)
    {
        var calculatorJob = new WheelJob
        {
            JobId = jobId,
            Settings =
                new WheelJobSettings
                {
                    Name = jobName,
                    Tasks = new List<JobWheelTask>
                    {
                        new() { PythonWheelTask = new PythonWheelTask { Parameters = new List<string>() }, },
                    },
                },
        };
        return calculatorJob;
    }
}
