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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.ScheduleCalculation;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.ScheduleCalculations;

public class CalculationsSchedulerHandlerTests : IClassFixture<CalculationSchedulerFixture>, IAsyncLifetime
{
    public CalculationsSchedulerHandlerTests(CalculationSchedulerFixture fixture)
    {
        Fixture = fixture;
    }

    private CalculationSchedulerFixture Fixture { get; }

    public async Task InitializeAsync()
    {
        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();

        // Clean up existing calculations in database
        await dbContext.Calculations.ExecuteDeleteAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Given_MultipleCalculationsWithOneReadyToStart_When_StartScheduledCalculationsAsync_Then_TheCorrectCalculationIsStarted(
        Mock<DurableTaskClient> durableTaskClient,
        Mock<IClock> clock,
        Mock<ILogger<CalculationSchedulerHandler>> schedulerLogger,
        Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions)
    {
        // Arrange
        var scheduledToRunAt = Instant.FromUtc(2024, 08, 19, 13, 37);

        // => Setup clock to return the "scheduledToRunAt" value, which means that the expectedCalculation should start now
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(scheduledToRunAt);

        var expectedCalculation = CreateCalculation(scheduledToRunAt);

        var futureCalculation = CreateCalculation(scheduledToRunAt.Plus(Duration.FromMilliseconds(1)));

        var alreadyStartedCalculation = CreateCalculation(scheduledToRunAt);
        alreadyStartedCalculation.MarkAsStarted();

        await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
        {
            var repository = new CalculationRepository(writeDbContext);

            await repository.AddAsync(expectedCalculation);
            await repository.AddAsync(futureCalculation);
            await repository.AddAsync(alreadyStartedCalculation);

            await writeDbContext.SaveChangesAsync();
        }

        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();

        var expectedInstanceId = expectedCalculation.OrchestrationInstanceId;

        // => Setup that the CalculationStarter's call to GetInstanceAsync() returns null
        // which indicates that an orchestration has not already been started for the calculation
        durableTaskClient
            .Setup(c => c.GetInstanceAsync(
                It.Is<string>(id => id == expectedInstanceId.Id),
                It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrchestrationMetadata?)null)
            .Verifiable(Times.Exactly(1));

        // => Setup and verify that ScheduleNewOrchestrationInstanceAsync is called only once with
        // the expected calculation id and instance id
        durableTaskClient
            .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
                It.Is<CalculationOrchestrationInput>(i => i.CalculationId.Id == expectedCalculation.Id),
                It.Is<StartOrchestrationOptions>(o => o.InstanceId == expectedInstanceId.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInstanceId.Id)
            .Verifiable(Times.Exactly(1));

        var calculationSchedulerHandler = new CalculationSchedulerHandler(
            schedulerLogger.Object,
            calculationOrchestrationMonitorOptions.Object,
            clock.Object,
            new CalculationRepository(dbContext));

        // Act
        await calculationSchedulerHandler.StartScheduledCalculationsAsync(durableTaskClient.Object);

        // Assert
        // => Verify the durableTaskClient setups in the Arrange section
        durableTaskClient.Verify();
        durableTaskClient.VerifyNoOtherCalls();

        // => Verify that the scheduler never logs an error (since the calculation should be started successfully)
        schedulerLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception?, string>>()),
            Times.Never);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Given_CalculationScheduledInFuture_When_CurrentTimeReachesScheduledTime_Then_CalculationIsStartedWithoutErrors(
        Mock<DurableTaskClient> durableTaskClient,
        Mock<IClock> clock,
        Mock<ILogger<CalculationSchedulerHandler>> schedulerLogger,
        Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions)
    {
        // Arrange
        var now = Instant.FromUtc(2024, 08, 19, 13, 37);
        var scheduledToRunAt = now.Plus(Duration.FromSeconds(1));

        // => Setup clock to return the "now" value first, and then the scheduledToRunAt value next
        // This means that we have to call StartScheduledCalculationsAsync twice to get to the point where
        // the calculation should be started
        clock.SetupSequence(c => c.GetCurrentInstant())
            .Returns(now)
            .Returns(scheduledToRunAt);

        var expectedCalculation = CreateCalculation(scheduledToRunAt);
        await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
        {
            var repository = new CalculationRepository(writeDbContext);
            await repository.AddAsync(expectedCalculation);
            await writeDbContext.SaveChangesAsync();
        }

        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();

        var expectedInstanceId = expectedCalculation.OrchestrationInstanceId;

        // => Setup (and verify later) that the CalculationStarter calls GetInstanceAsync() only once
        durableTaskClient
            .Setup(c => c.GetInstanceAsync(
                It.Is<string>(id => id == expectedInstanceId.Id),
                It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrchestrationMetadata?)null)
            .Verifiable(Times.Exactly(1));

        // => Setup and verify that ScheduleNewOrchestrationInstanceAsync is called only once with
        // the expected calculation id
        durableTaskClient
            .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
                It.Is<CalculationOrchestrationInput>(i => i.CalculationId.Id == expectedCalculation.Id),
                It.Is<StartOrchestrationOptions>(o => o.InstanceId == expectedInstanceId.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInstanceId.Id)
            .Verifiable(Times.Exactly(1));

        var calculationScheduler = new CalculationSchedulerHandler(
            schedulerLogger.Object,
            calculationOrchestrationMonitorOptions.Object,
            clock.Object,
            new CalculationRepository(dbContext));

        // Act
        // => First run of StartScheduledCalculationsAsync should not start the calculation since clock is before the scheduled time
        await calculationScheduler.StartScheduledCalculationsAsync(durableTaskClient.Object);

        // => Second run of StartScheduledCalculationsAsync should start the calculation since clock is now equal to the scheduled time
        await calculationScheduler.StartScheduledCalculationsAsync(durableTaskClient.Object);

        // Assert
        // => Verify the durableTaskClient setups in the Arrange section
        durableTaskClient.Verify();
        durableTaskClient.VerifyNoOtherCalls();

        // => Verify that the scheduler never logs an error (since the calculation should be started successfully)
        schedulerLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception?, string>>()),
            Times.Never);
    }

    private Calculations.Application.Model.Calculations.Calculation CreateCalculation(Instant scheduledToRunAt)
    {
        var dummyInstant = Instant.FromUtc(2000, 01, 01, 00, 00);
        return new Calculations.Application.Model.Calculations.Calculation(
            dummyInstant,
            CalculationType.BalanceFixing,
            [new GridAreaCode("100")],
            dummyInstant,
            dummyInstant.Plus(Duration.FromDays(1)),
            scheduledToRunAt,
            DateTimeZone.Utc,
            Guid.Empty,
            1);
    }
}