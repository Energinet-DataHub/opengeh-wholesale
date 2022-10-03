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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixture.Database;
using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Azure.Management.EventHub.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

[Collection("ProcessManagerIntegrationTest")]
public sealed class BatchApplicationServiceTests
{
    private const long DummyJobId = 42;
    private const string DummyJobName = "CalculatorJob";
    private const string GridAreaCode = "321";
    private readonly Mock<IJobsWheelApi> _jobsApiMock = new Mock<IJobsWheelApi>();
    private readonly Mock<IDatabricksWheelClient> _databricksWheelClientMock = new Mock<IDatabricksWheelClient>();
    private BatchRequestDto _batchRequest = new BatchRequestDto(WholesaleProcessType.BalanceFixing, new[] { GridAreaCode }, DateTimeOffset.Now, DateTimeOffset.Now);

    [Fact]
    public async Task When_RunIsCompleted_Then_BatchIsCompleted()
    {
        Setup();
        var pendingRun = new Run { State = new RunState { LifeCycleState = RunLifeCycleState.PENDING, ResultState = RunResultState.SUCCESS } };

        _jobsApiMock
            .Setup(x => x.RunsGet(It.IsAny<long>(), default))
            .ReturnsAsync(pendingRun);

        var foo = (IServiceCollection collection) =>
        {
            collection.Replace(ServiceDescriptor.Singleton(_databricksWheelClientMock.Object));
        };
        using var host = await ProcessManagerIntegrationTestHost.CreateAsync(foo);

        await using var scope = host.BeginScope();
        var target = scope.ServiceProvider.GetRequiredService<IBatchApplicationService>();
        var repository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

        await target.CreateAsync(_batchRequest);
        await target.StartPendingAsync();
        await target.UpdateExecutionStateAsync();

        // // Assert 1: Verify that batch is now pending.
        var pending = await repository.GetPendingAsync();
        var createdBatch = pending.Single(x => x.GridAreaCodes.Contains(new GridAreaCode(GridAreaCode)));
        var runningRun = new Run { State = new RunState { LifeCycleState = RunLifeCycleState.RUNNING, ResultState = RunResultState.SUCCESS } };

        _jobsApiMock
            .Setup(x => x.RunsGet(It.IsAny<long>(), default))
            .ReturnsAsync(runningRun);

        // Act
        await target.UpdateExecutionStateAsync();

        // Assert 2: Verify that batch is now running.
        var running = await repository.GetExecutingAsync();
        var runningBatch = running.Single(x => x.GridAreaCodes.Contains(new GridAreaCode(GridAreaCode)));

        var completedRun = new Run { State = new RunState { LifeCycleState = RunLifeCycleState.TERMINATED, ResultState = RunResultState.SUCCESS } };

        _jobsApiMock
            .Setup(x => x.RunsGet(It.IsAny<long>(), default))
            .ReturnsAsync(completedRun);

        // Act
        await target.UpdateExecutionStateAsync();

        // Assert 2: Verify that batch is completed.
        var completed = await repository.GetCompletedAsync();
        var updatedBatch = completed.Single(x => x.Id == createdBatch.Id);
        updatedBatch.GridAreaCodes.Should().ContainSingle(code => code.Code == GridAreaCode);
    }

    private void Setup()
    {
        var jobs = new List<Job>() { CreateJob(DummyJobId, DummyJobName) };
        _jobsApiMock.Setup(x => x.List(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);
        _jobsApiMock.Setup(x => x.GetWheel(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCalculatorJob(DummyJobId, DummyJobName));
        var runIdentifier = new RunIdentifier() { RunId = 22, NumberInJob = 1 };
        _jobsApiMock.Setup(
                x => x.RunNow(
                    It.IsAny<long>(),
                    It.IsAny<RunParameters>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(runIdentifier);

        _databricksWheelClientMock.Setup(x => x.Jobs).Returns(_jobsApiMock.Object);
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
