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
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
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

public class CalculationsSchedulerTests : IClassFixture<CalculationSchedulerFixture>, IAsyncLifetime
{
    public CalculationsSchedulerTests(CalculationSchedulerFixture fixture)
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
        Mock<DataConverter> dataConverter,
        Mock<IClock> clock,
        Mock<ILogger<CalculationScheduler>> schedulerLogger,
        Mock<ILogger<CalculationStarter>> starterLogger,
        Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions)
    {
        // Arrange
        var dateTimeZone = DateTimeZone.Utc;
        var scheduledToRunAt = Instant.FromUtc(2024, 08, 19, 13, 37);

        // => Setup clock to return the "now" value first, and then the scheduledToRunAt value next
        // This means that we have to call StartScheduledCalculationsAsync twice to get to the point where
        // the calculation should be started
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(scheduledToRunAt);

        Calculations.Application.Model.Calculations.Calculation expectedCalculation;
        await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
        {
            var repository = new CalculationRepository(writeDbContext);

            var dummyInstant = Instant.FromUtc(2000, 01, 01, 00, 00);

            expectedCalculation = new Calculations.Application.Model.Calculations.Calculation(
                dummyInstant,
                CalculationType.BalanceFixing,
                [new GridAreaCode("100")],
                dummyInstant,
                dummyInstant.Plus(Duration.FromDays(1)),
                scheduledToRunAt,
                dateTimeZone,
                Guid.Empty,
                1);

            var futureCalculation = new Calculations.Application.Model.Calculations.Calculation(
                dummyInstant,
                CalculationType.BalanceFixing,
                [new GridAreaCode("100")],
                dummyInstant,
                dummyInstant.Plus(Duration.FromDays(1)),
                scheduledToRunAt.Plus(Duration.FromMilliseconds(1)),
                dateTimeZone,
                Guid.Empty,
                1);

            var alreadyStartedCalculation = new Calculations.Application.Model.Calculations.Calculation(
                dummyInstant,
                CalculationType.BalanceFixing,
                [new GridAreaCode("100")],
                dummyInstant,
                dummyInstant.Plus(Duration.FromDays(1)),
                scheduledToRunAt.Plus(Duration.FromMilliseconds(1)),
                dateTimeZone,
                Guid.Empty,
                1);
            alreadyStartedCalculation.MarkAsStarted(new OrchestrationInstanceId("dummy-instance-id"));

            await repository.AddAsync(expectedCalculation);
            await repository.AddAsync(futureCalculation);
            await repository.AddAsync(alreadyStartedCalculation);

            await writeDbContext.SaveChangesAsync();
        }

        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();

        var expectedInstanceId = "instance-id-1";
        var serializedMetadataObject = "serialized-metadata-object";

        // => Setup that the CalculationStarter can call GetInstanceAsync() to get the CalculationMetadata object
        durableTaskClient
            .Setup(c => c.GetInstanceAsync(
                It.Is<string>(id => id == expectedInstanceId),
                It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == true),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrchestrationMetadata(string.Empty, string.Empty)
            {
                DataConverter = dataConverter.Object,
                SerializedCustomStatus = serializedMetadataObject,
            })
            .Verifiable(Times.Exactly(1));

        // => Setup that the CalculationStarter can call get custom status from the CalculationMetadata.ReadCustomStatusAs<>() (which uses .Deserialize())
        dataConverter.Setup(dc => dc.Deserialize(
                It.Is<string?>(s => s == serializedMetadataObject),
                It.IsAny<Type>()))
            .Returns(new CalculationMetadata { IsStarted = true, });

        // => Setup and verify that ScheduleNewOrchestrationInstanceAsync is called only once with
        // the expected calculation id
        durableTaskClient
            .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
                It.Is<CalculationOrchestrationInput>(i => i.CalculationId.Id == expectedCalculation.Id),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInstanceId)
            .Verifiable(Times.Exactly(1));

        var calculationScheduler = new CalculationScheduler(
            schedulerLogger.Object,
            clock.Object,
            new CalculationsClient(
                new CalculationRepository(dbContext),
                new CalculationDtoMapper(),
                new CalculationFactory(clock.Object, dateTimeZone),
                new UnitOfWork(dbContext)),
            new CalculationStarter(
                starterLogger.Object,
                clock.Object,
                calculationOrchestrationMonitorOptions.Object));

        // Act
        await calculationScheduler.StartScheduledCalculationsAsync(durableTaskClient.Object);

        // Assert
        durableTaskClient.Verify();
        durableTaskClient.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Given_CalculationScheduledInFuture_When_CurrentTimeReachesScheduledTime_Then_CalculationIsStartedWithoutErrors(
        Mock<DurableTaskClient> durableTaskClient,
        Mock<DataConverter> dataConverter,
        Mock<IClock> clock,
        Mock<ILogger<CalculationScheduler>> schedulerLogger,
        Mock<ILogger<CalculationStarter>> starterLogger,
        Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions)
    {
        // Arrange
        var dateTimeZone = DateTimeZone.Utc;
        var now = Instant.FromUtc(2024, 08, 19, 13, 37);
        var scheduledToRunAt = now.Plus(Duration.FromSeconds(1));

        // => Setup clock to return the "now" value first, and then the scheduledToRunAt value next
        // This means that we have to call StartScheduledCalculationsAsync twice to get to the point where
        // the calculation should be started
        clock.SetupSequence(c => c.GetCurrentInstant())
            .Returns(now)
            .Returns(scheduledToRunAt);

        Calculations.Application.Model.Calculations.Calculation scheduledCalculation;
        await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
        {
            var repository = new CalculationRepository(writeDbContext);

            var dummyInstant = Instant.FromUtc(2000, 01, 01, 00, 00);

            scheduledCalculation = new Calculations.Application.Model.Calculations.Calculation(
                dummyInstant,
                CalculationType.BalanceFixing,
                [new GridAreaCode("100")],
                dummyInstant,
                dummyInstant.Plus(Duration.FromDays(1)),
                scheduledToRunAt,
                dateTimeZone,
                Guid.Empty,
                1);

            await repository.AddAsync(scheduledCalculation);

            await writeDbContext.SaveChangesAsync();
        }

        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();

        var expectedInstanceId = "instance-id-1";
        var serializedMetadataObject = "serialized-metadata-object";

        // => Setup (and verify later) that the CalculationStarter calls GetInstanceAsync() only once
        durableTaskClient
            .Setup(c => c.GetInstanceAsync(
                It.Is<string>(id => id == expectedInstanceId),
                It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == true),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrchestrationMetadata(string.Empty, string.Empty)
            {
                DataConverter = dataConverter.Object,
                SerializedCustomStatus = serializedMetadataObject,
            })
            .Verifiable(Times.Exactly(1));

        // => Setup (and verify later) that the CalculationStarter calls ReadCustomStatusAs() (which uses Deserialize()) only once
        dataConverter.Setup(dc => dc.Deserialize(
                It.Is<string?>(s => s == serializedMetadataObject),
                It.IsAny<Type>()))
            .Returns(new CalculationMetadata { IsStarted = true, });

        // => Setup and verify that ScheduleNewOrchestrationInstanceAsync is called only once with
        // the expected calculation id
        durableTaskClient
            .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
                It.Is<CalculationOrchestrationInput>(i => i.CalculationId.Id == scheduledCalculation.Id),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInstanceId)
            .Verifiable(Times.Exactly(1));

        var calculationScheduler = new CalculationScheduler(
            schedulerLogger.Object,
            clock.Object,
            new CalculationsClient(
                new CalculationRepository(dbContext),
                new CalculationDtoMapper(),
                new CalculationFactory(clock.Object, dateTimeZone),
                new UnitOfWork(dbContext)),
            new CalculationStarter(
                starterLogger.Object,
                clock.Object,
                calculationOrchestrationMonitorOptions.Object));

        // Act
        await calculationScheduler.StartScheduledCalculationsAsync(durableTaskClient.Object);
        await calculationScheduler.StartScheduledCalculationsAsync(durableTaskClient.Object);

        // Assert
        durableTaskClient.Verify();
        durableTaskClient.VerifyNoOtherCalls();

        // => Verify that the scheduler never logs an error (since the calculation is started successfully)
        schedulerLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception?, string>>()),
            Times.Never);
        schedulerLogger.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Given_ScheduledCalculation_WhenMetadataIsNotAvailableImmediately_ThenTryAgainUntilAvailable(
        Mock<DurableTaskClient> durableTaskClient,
        Mock<DataConverter> dataConverter,
        Mock<IClock> clock,
        Mock<ILogger<CalculationScheduler>> schedulerLogger,
        Mock<ILogger<CalculationStarter>> starterLogger,
        Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions)
    {
        // Arrange
        var dateTimeZone = DateTimeZone.Utc;
        var scheduledToRunAt = Instant.FromUtc(2024, 08, 19, 13, 37);

        // => Setup clock to return the "now" value first, and then the scheduledToRunAt value next
        // This means that we have to call StartScheduledCalculationsAsync twice to get to the point where
        // the calculation should be started
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(scheduledToRunAt);

        await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
        {
            var repository = new CalculationRepository(writeDbContext);

            var dummyInstant = Instant.FromUtc(2000, 01, 01, 00, 00);

            var expectedCalculation = new Calculations.Application.Model.Calculations.Calculation(
                dummyInstant,
                CalculationType.BalanceFixing,
                [new GridAreaCode("100")],
                dummyInstant,
                dummyInstant.Plus(Duration.FromDays(1)),
                scheduledToRunAt,
                dateTimeZone,
                Guid.Empty,
                1);

            await repository.AddAsync(expectedCalculation);

            await writeDbContext.SaveChangesAsync();
        }

        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();

        var expectedInstanceId = "instance-id-1";

        durableTaskClient
            .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
                It.IsAny<CalculationOrchestrationInput>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInstanceId)
            .Verifiable(Times.Exactly(1));

        // => Setup (and verify later) that the CalculationStarter can handle that a custom status (the CalculationMetadata object) is not always available
        // The first call to Deserialize will return null, then expected calls after returns a CalculationMetadata object
        // with IsStarted set to false, then true
        var serializedMetadataObject = "serialized-metadata-object";
        dataConverter.SetupSequence(dc => dc.Deserialize(
                It.Is<string?>(s => s == serializedMetadataObject),
                It.IsAny<Type>()))
            .Returns((CalculationMetadata?)null)
            .Returns(new CalculationMetadata { IsStarted = false, })
            .Returns(new CalculationMetadata { IsStarted = true, });

        // => Setup (and verify later) that the CalculationStarter can handle that OrchestrationMetadata might not be available immediately
        // The first call to GetInstanceAsync will return null, then expected calls after that uses the
        // dataConverter to return a CalculationMetadata object
        durableTaskClient
            .SetupSequence(c => c.GetInstanceAsync(
                It.Is<string>(id => id == expectedInstanceId),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrchestrationMetadata?)null)
            .ReturnsAsync(new OrchestrationMetadata(string.Empty, string.Empty)
            {
                DataConverter = dataConverter.Object,
                SerializedCustomStatus = null,
            })
            .ReturnsAsync(new OrchestrationMetadata(string.Empty, string.Empty)
            {
                DataConverter = dataConverter.Object,
                SerializedCustomStatus = serializedMetadataObject,
            })
            .ReturnsAsync(new OrchestrationMetadata(string.Empty, string.Empty)
            {
                DataConverter = dataConverter.Object,
                SerializedCustomStatus = serializedMetadataObject,
            })
            .ReturnsAsync(new OrchestrationMetadata(string.Empty, string.Empty)
            {
                DataConverter = dataConverter.Object,
                SerializedCustomStatus = serializedMetadataObject,
            });

        var calculationScheduler = new CalculationScheduler(
            schedulerLogger.Object,
            clock.Object,
            new CalculationsClient(
                new CalculationRepository(dbContext),
                new CalculationDtoMapper(),
                new CalculationFactory(clock.Object, dateTimeZone),
                new UnitOfWork(dbContext)),
            new CalculationStarter(
                starterLogger.Object,
                clock.Object,
                calculationOrchestrationMonitorOptions.Object));

        // Act
        await calculationScheduler.StartScheduledCalculationsAsync(durableTaskClient.Object);

        // Assert
        durableTaskClient.Verify();
        durableTaskClient.Verify(
            c => c.GetInstanceAsync(
                It.Is<string>(id => id == expectedInstanceId),
                It.Is<bool>(includeInputsAndOutputs => includeInputsAndOutputs == true),
                It.IsAny<CancellationToken>()),
            Times.Exactly(5));
        durableTaskClient.VerifyNoOtherCalls();

        dataConverter.Verify(
            dc => dc.Deserialize(
                It.Is<string?>(d => d == serializedMetadataObject),
                It.IsAny<Type>()),
            Times.Exactly(3));
        dataConverter.Verify(
            dc => dc.Deserialize<CalculationMetadata>(
                It.Is<string?>(d => d == serializedMetadataObject)),
            Times.Exactly(3));
        dataConverter.VerifyNoOtherCalls();
    }
}
