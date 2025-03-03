// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
//
// using Energinet.DataHub.Core.App.Common.Abstractions.Users;
// using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
// using Energinet.DataHub.Wholesale.Calculations.Application.Model;
// using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
// using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.Calculations;
// using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
// using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
// using Energinet.DataHub.Wholesale.Orchestrations.Extensions.Options;
// using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
// using Energinet.DataHub.Wholesale.Orchestrations.Functions.ScheduleCalculation;
// using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
// using FluentAssertions;
// using Microsoft.DurableTask;
// using Microsoft.DurableTask.Client;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Moq;
// using NodaTime;
//
// namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.ScheduleCalculations;
//
// public class CalculationsSchedulerHandlerTests : IClassFixture<CalculationSchedulerFixture>, IAsyncLifetime
// {
//     public CalculationsSchedulerHandlerTests(CalculationSchedulerFixture fixture)
//     {
//         Fixture = fixture;
//     }
//
//     private CalculationSchedulerFixture Fixture { get; }
//
//     public async Task InitializeAsync()
//     {
//         await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
//
//         // Clean up existing calculations in database
//         await dbContext.Calculations.ExecuteDeleteAsync();
//     }
//
//     public Task DisposeAsync()
//     {
//         return Task.CompletedTask;
//     }
//
//     [Theory]
//     [InlineAutoMoqData]
//     public async Task Given_MultipleCalculationsWithOneReadyToStart_When_StartScheduledCalculationsAsync_Then_TheCorrectCalculationIsStarted(
//         Mock<DurableTaskClient> durableTaskClient,
//         Mock<IClock> clock,
//         Mock<ILogger<CalculationSchedulerHandler>> schedulerLogger,
//         Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions,
//         Mock<IUserContext<FrontendUser>> userContext)
//     {
//         // Arrange
//         var scheduledToRunAt = Instant.FromUtc(2024, 08, 19, 13, 37);
//
//         // => Setup clock to return the "scheduledToRunAt" value, which means that the expectedCalculation should start now
//         clock.Setup(c => c.GetCurrentInstant())
//             .Returns(scheduledToRunAt);
//
//         var expectedCalculation = CreateCalculation(scheduledToRunAt);
//
//         var futureCalculation = CreateCalculation(scheduledToRunAt.Plus(Duration.FromMilliseconds(1)));
//
//         var alreadyStartedCalculation = CreateCalculation(scheduledToRunAt);
//         alreadyStartedCalculation.MarkAsStarted();
//
//         var canceledCalculation = CreateCalculation(scheduledToRunAt);
//         canceledCalculation.MarkAsCanceled(Guid.NewGuid());
//
//         await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
//         {
//             var repository = new CalculationRepository(writeDbContext);
//
//             await repository.AddAsync(expectedCalculation);
//             await repository.AddAsync(futureCalculation);
//             await repository.AddAsync(alreadyStartedCalculation);
//             await repository.AddAsync(canceledCalculation);
//
//             await writeDbContext.SaveChangesAsync();
//         }
//
//         await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
//
//         var expectedInstanceId = expectedCalculation.OrchestrationInstanceId;
//
//         // => Setup that the CalculationStarter's call to GetInstanceAsync() returns null
//         // which indicates that an orchestration has not already been started for the calculation
//         durableTaskClient
//             .Setup(c => c.GetInstanceAsync(
//                 It.Is<string>(id => id == expectedInstanceId.Id),
//                 It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync((OrchestrationMetadata?)null);
//
//         // => Setup that ScheduleNewOrchestrationInstanceAsync can be called with
//         // the expected calculation id and instance id
//         durableTaskClient
//             .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
//                 It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
//                 It.Is<CalculationOrchestrationInput>(i => i.CalculationId.Id == expectedCalculation.Id),
//                 It.Is<StartOrchestrationOptions>(o => o.InstanceId == expectedInstanceId.Id),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(expectedInstanceId.Id);
//
//         var calculationSchedulerHandler = new CalculationSchedulerHandler(
//             schedulerLogger.Object,
//             calculationOrchestrationMonitorOptions.Object,
//             clock.Object,
//             userContext.Object,
//             new CalculationRepository(dbContext),
//             new UnitOfWork(dbContext));
//
//         // Act
//         await calculationSchedulerHandler.StartScheduledCalculationsAsync(durableTaskClient.Object);
//
//         // Assert
//         // => Verify that GetInstanceAsync was called once to check if the calculation was already started
//         durableTaskClient.Verify(
//             c => c.GetInstanceAsync(
//                 It.Is<string>(id => id == expectedInstanceId.Id),
//                 It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
//                 It.IsAny<CancellationToken>()),
//             Times.Exactly(1));
//
//         // => Verify that ScheduleNewOrchestrationInstanceAsync was called once to start the calculation
//         durableTaskClient
//             .Verify(
//                 c => c.ScheduleNewOrchestrationInstanceAsync(
//                     It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
//                     It.Is<CalculationOrchestrationInput>(i => i.CalculationId.Id == expectedCalculation.Id),
//                     It.Is<StartOrchestrationOptions>(o => o.InstanceId == expectedInstanceId.Id),
//                     It.IsAny<CancellationToken>()),
//                 Times.Exactly(1));
//
//         // => Verify that no other calls were made to the durable task client, to ensure no other calculations were started
//         durableTaskClient.VerifyNoOtherCalls();
//
//         // => Verify that the scheduler never logs an error (since the calculation should be started successfully)
//         schedulerLogger.Verify(
//             l => l.Log(
//                 It.Is<LogLevel>(level => level == LogLevel.Error),
//                 It.IsAny<EventId>(),
//                 It.IsAny<It.IsAnyType>(),
//                 It.IsAny<InvalidOperationException>(),
//                 It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
//             Times.Never);
//     }
//
//     [Theory]
//     [InlineAutoMoqData]
//     public async Task Given_CalculationScheduledInFuture_When_CurrentTimeReachesScheduledTime_Then_CalculationIsStartedWithoutErrors(
//         Mock<DurableTaskClient> durableTaskClient,
//         Mock<IClock> clock,
//         Mock<ILogger<CalculationSchedulerHandler>> schedulerLogger,
//         Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions,
//         Mock<IUserContext<FrontendUser>> userContext)
//     {
//         // Arrange
//         var now = Instant.FromUtc(2024, 08, 19, 13, 37);
//         var scheduledToRunAt = now.Plus(Duration.FromSeconds(1));
//
//         // => Setup clock to return the "now" value first, and then the scheduledToRunAt value next
//         // This means that we have to call StartScheduledCalculationsAsync twice to get to the point where
//         // the calculation should be started
//         clock.SetupSequence(c => c.GetCurrentInstant())
//             .Returns(now)
//             .Returns(scheduledToRunAt);
//
//         var expectedCalculation = CreateCalculation(scheduledToRunAt);
//         await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
//         {
//             var repository = new CalculationRepository(writeDbContext);
//             await repository.AddAsync(expectedCalculation);
//             await writeDbContext.SaveChangesAsync();
//         }
//
//         await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
//
//         var expectedInstanceId = expectedCalculation.OrchestrationInstanceId;
//
//         // => Setup (and verify later) that the CalculationStarter calls GetInstanceAsync() only once
//         durableTaskClient
//             .Setup(c => c.GetInstanceAsync(
//                 It.Is<string>(id => id == expectedInstanceId.Id),
//                 It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync((OrchestrationMetadata?)null);
//
//         // => Setup and verify that ScheduleNewOrchestrationInstanceAsync is called only once with
//         // the expected calculation id
//         durableTaskClient
//             .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
//                 It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
//                 It.Is<CalculationOrchestrationInput>(i => i.CalculationId.Id == expectedCalculation.Id),
//                 It.Is<StartOrchestrationOptions>(o => o.InstanceId == expectedInstanceId.Id),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(expectedInstanceId.Id);
//
//         var calculationScheduler = new CalculationSchedulerHandler(
//             schedulerLogger.Object,
//             calculationOrchestrationMonitorOptions.Object,
//             clock.Object,
//             userContext.Object,
//             new CalculationRepository(dbContext),
//             new UnitOfWork(dbContext));
//
//         // Act
//         // => First run of StartScheduledCalculationsAsync should not start the calculation since clock is before the scheduled time
//         await calculationScheduler.StartScheduledCalculationsAsync(durableTaskClient.Object);
//
//         // => Second run of StartScheduledCalculationsAsync should start the calculation since clock is now equal to the scheduled time
//         await calculationScheduler.StartScheduledCalculationsAsync(durableTaskClient.Object);
//
//         // Assert
//         // => Verify that GetInstanceAsync was called once to check if the calculation was already started
//         durableTaskClient
//             .Verify(
//                 c => c.GetInstanceAsync(
//                     It.Is<string>(id => id == expectedInstanceId.Id),
//                     It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
//                     It.IsAny<CancellationToken>()),
//                 Times.Exactly(1));
//
//         // => Verify that ScheduleNewOrchestrationInstanceAsync was called once to start the calculation
//         durableTaskClient
//             .Verify(
//                 c => c.ScheduleNewOrchestrationInstanceAsync(
//                     It.Is<TaskName>(taskName => taskName == "CalculationOrchestration"),
//                     It.Is<CalculationOrchestrationInput>(i => i.CalculationId.Id == expectedCalculation.Id),
//                     It.Is<StartOrchestrationOptions>(o => o.InstanceId == expectedInstanceId.Id),
//                     It.IsAny<CancellationToken>()),
//                 Times.Exactly(1));
//
//         // => Verify that no other calls were made to the durable task client
//         durableTaskClient.VerifyNoOtherCalls();
//
//         // => Verify that the scheduler never logs an error (since the calculation should be started successfully)
//         schedulerLogger.Verify(
//             l => l.Log(
//                 It.Is<LogLevel>(level => level == LogLevel.Error),
//                 It.IsAny<EventId>(),
//                 It.IsAny<It.IsAnyType>(),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
//             Times.Never);
//     }
//
//     [Theory]
//     [InlineAutoMoqData]
//     public async Task Given_ScheduledCalculationAlreadyStarted_When_StartScheduledCalculationsAsync_Then_AnErrorIsLogged(
//         Mock<DurableTaskClient> durableTaskClient,
//         Mock<IClock> clock,
//         Mock<ILogger<CalculationSchedulerHandler>> schedulerLogger,
//         Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions,
//         Mock<IUserContext<FrontendUser>> userContext)
//     {
//         // Arrange
//         var scheduledToRunAt = Instant.FromUtc(2024, 08, 19, 13, 37);
//
//         // => Setup clock to return the "scheduledToRunAt" value, which means that the expectedCalculation should start now
//         clock.Setup(c => c.GetCurrentInstant())
//             .Returns(scheduledToRunAt);
//
//         var alreadyRunningCalculation = CreateCalculation(scheduledToRunAt);
//
//         await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
//         {
//             var repository = new CalculationRepository(writeDbContext);
//             await repository.AddAsync(alreadyRunningCalculation);
//             await writeDbContext.SaveChangesAsync();
//         }
//
//         await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
//
//         var calculationId = alreadyRunningCalculation.Id;
//         var alreadyRunningInstanceId = alreadyRunningCalculation.OrchestrationInstanceId;
//
//         // => Setup that the CalculationStarter's call to GetInstanceAsync() returns an OrchestrationMetadata object
//         // which indicates that an orchestration has already been started for the calculation
//         durableTaskClient
//             .Setup(c => c.GetInstanceAsync(
//                 It.Is<string>(id => id == alreadyRunningInstanceId.Id),
//                 It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new OrchestrationMetadata(string.Empty, string.Empty));
//
//         var calculationSchedulerHandler = new CalculationSchedulerHandler(
//             schedulerLogger.Object,
//             calculationOrchestrationMonitorOptions.Object,
//             clock.Object,
//             userContext.Object,
//             new CalculationRepository(dbContext),
//             new UnitOfWork(dbContext));
//
//         // Act
//         await calculationSchedulerHandler.StartScheduledCalculationsAsync(durableTaskClient.Object);
//
//         // Assert
//         // => Verify that GetInstanceAsync was called once to check if the calculation was already started
//         durableTaskClient
//             .Verify(
//                 c => c.GetInstanceAsync(
//                     It.Is<string>(id => id == alreadyRunningInstanceId.Id),
//                     It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
//                     It.IsAny<CancellationToken>()),
//                 Times.Exactly(1));
//
//         // => Verify that no other calls were made to the durable task client (since the calculation should not be started)
//         durableTaskClient.VerifyNoOtherCalls();
//
//         // => Verify that the scheduler logs an error (since the calculation should not be started)
//         schedulerLogger.Verify(
//             l => l.Log(
//                 It.Is<LogLevel>(level => level == LogLevel.Error),
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((message, type) =>
//                     message.ToString()!.Contains($"Failed to start calculation with id = {calculationId} " +
//                                                  $"and orchestration instance id = {alreadyRunningInstanceId.Id}")),
//                 It.IsAny<InvalidOperationException>(),
//                 It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
//             Times.Exactly(1));
//     }
//
//     [Theory]
//     [InlineAutoMoqData]
//     public async Task Given_ScheduledCalculation_When_CancelScheduledCalculationAsync_ThenCalculationIsCanceled(
//         Mock<DurableTaskClient> durableTaskClient,
//         Mock<IClock> clock,
//         Mock<ILogger<CalculationSchedulerHandler>> schedulerLogger,
//         Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions,
//         Mock<IUserContext<FrontendUser>> userContext)
//     {
//         // Arrange
//         var scheduledToRunAt = Instant.FromUtc(2024, 08, 19, 13, 37);
//         var expectedCanceledByUserId = Guid.NewGuid();
//
//         userContext
//             .Setup(u => u.CurrentUser)
//             .Returns(new FrontendUser(
//                 expectedCanceledByUserId,
//                 true,
//                 new FrontendActor(
//                     Guid.NewGuid(),
//                     "1",
//                     FrontendActorMarketRole.DataHubAdministrator,
//                     [])));
//
//         var scheduledCalculation = CreateCalculation(scheduledToRunAt);
//         await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
//         {
//             var repository = new CalculationRepository(writeDbContext);
//             await repository.AddAsync(scheduledCalculation);
//             await writeDbContext.SaveChangesAsync();
//         }
//
//         var orchestrationInstanceId = scheduledCalculation.OrchestrationInstanceId;
//         await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
//         var calculationSchedulerHandler = new CalculationSchedulerHandler(
//             schedulerLogger.Object,
//             calculationOrchestrationMonitorOptions.Object,
//             clock.Object,
//             userContext.Object,
//             new CalculationRepository(dbContext),
//             new UnitOfWork(dbContext));
//
//         // => Setup that the CalculationSchedulerHandler's call to GetInstanceAsync() returns null
//         // which indicates that an orchestration has not yet been started for the calculation
//         durableTaskClient
//             .Setup(c => c.GetInstanceAsync(
//                 It.Is<string>(id => id == orchestrationInstanceId.Id),
//                 It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync((OrchestrationMetadata?)null);
//
//         // Act
//         await calculationSchedulerHandler.CancelScheduledCalculationAsync(
//             durableTaskClient.Object,
//             new CalculationId(scheduledCalculation.Id));
//
//         // Assert
//
//         // => Assert that the calculation is canceled in the database
//         await using var assertDbContext = Fixture.DatabaseManager.CreateDbContext();
//         var calculationRepository = new CalculationRepository(assertDbContext);
//         var canceledCalculation = await calculationRepository.GetAsync(scheduledCalculation.Id);
//         canceledCalculation.OrchestrationState.Should().Be(CalculationOrchestrationState.Canceled);
//         canceledCalculation.CanceledByUserId.Should().Be(expectedCanceledByUserId);
//
//         // => Verify that GetInstanceAsync was called once to check if the calculation was already started
//         durableTaskClient
//             .Verify(
//                 c => c.GetInstanceAsync(
//                     It.Is<string>(id => id == orchestrationInstanceId.Id),
//                     It.Is<bool>(getInputsAndOutputs => getInputsAndOutputs == false),
//                     It.IsAny<CancellationToken>()),
//                 Times.Exactly(1));
//
//         // => Verify that no other calls were made to the durable task client
//         durableTaskClient.VerifyNoOtherCalls();
//     }
//
//     [Theory]
//     [InlineAutoMoqData]
//     public async Task Given_AlreadyStartedCalculation_When_CancelScheduledCalculationAsync_ThenExceptionIsThrown(
//         Mock<DurableTaskClient> durableTaskClient,
//         Mock<IClock> clock,
//         Mock<ILogger<CalculationSchedulerHandler>> schedulerLogger,
//         Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions,
//         Mock<IUserContext<FrontendUser>> userContext)
//     {
//         // Arrange
//         var alreadyStartedCalculation = CreateCalculation(Instant.FromUtc(2024, 08, 19, 13, 37));
//         alreadyStartedCalculation.MarkAsStarted();
//         await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
//         {
//             var repository = new CalculationRepository(writeDbContext);
//             await repository.AddAsync(alreadyStartedCalculation);
//             await writeDbContext.SaveChangesAsync();
//         }
//
//         await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
//         var calculationSchedulerHandler = new CalculationSchedulerHandler(
//             schedulerLogger.Object,
//             calculationOrchestrationMonitorOptions.Object,
//             clock.Object,
//             userContext.Object,
//             new CalculationRepository(dbContext),
//             new UnitOfWork(dbContext));
//
//         // Act
//         var act = () => calculationSchedulerHandler.CancelScheduledCalculationAsync(
//             durableTaskClient.Object,
//             new CalculationId(alreadyStartedCalculation.Id));
//
//         // Assert
//         await act.Should().ThrowAsync<InvalidOperationException>();
//
//         // => Assert that the calculation is not canceled in the database
//         await using var assertDbContext = Fixture.DatabaseManager.CreateDbContext();
//         var calculationRepository = new CalculationRepository(assertDbContext);
//         var canceledCalculation = await calculationRepository.GetAsync(alreadyStartedCalculation.Id);
//         canceledCalculation.OrchestrationState.Should().Be(CalculationOrchestrationState.Started);
//         canceledCalculation.CanceledByUserId.Should().Be(null);
//     }
//
//     [Theory]
//     [InlineAutoMoqData]
//     public async Task Given_ScheduledCalculationWithAlreadyStartedOrchestration_When_CancelScheduledCalculationAsync_ThenExceptionIsThrown(
//         Mock<DurableTaskClient> durableTaskClient,
//         Mock<IClock> clock,
//         Mock<ILogger<CalculationSchedulerHandler>> schedulerLogger,
//         Mock<IOptions<CalculationOrchestrationMonitorOptions>> calculationOrchestrationMonitorOptions,
//         Mock<IUserContext<FrontendUser>> userContext)
//     {
//         // Arrange
//         var scheduledCalculation = CreateCalculation(Instant.FromUtc(2024, 08, 19, 13, 37));
//         await using (var writeDbContext = Fixture.DatabaseManager.CreateDbContext())
//         {
//             var repository = new CalculationRepository(writeDbContext);
//             await repository.AddAsync(scheduledCalculation);
//             await writeDbContext.SaveChangesAsync();
//         }
//
//         var alreadyStartedOrchestrationInstanceId = scheduledCalculation.OrchestrationInstanceId;
//
//         await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
//         var calculationSchedulerHandler = new CalculationSchedulerHandler(
//             schedulerLogger.Object,
//             calculationOrchestrationMonitorOptions.Object,
//             clock.Object,
//             userContext.Object,
//             new CalculationRepository(dbContext),
//             new UnitOfWork(dbContext));
//
//         // => Setup that the CalculationSchedulerHandler's call to GetInstanceAsync() returns an OrchestrationMetadata object
//         // which indicates that an orchestration has already been started for the calculation
//         durableTaskClient.Setup(c => c
//                 .GetInstanceAsync(
//                     It.Is<string>(instanceId => instanceId == alreadyStartedOrchestrationInstanceId.Id),
//                     It.IsAny<bool>(),
//                     It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new OrchestrationMetadata(string.Empty, string.Empty));
//
//         // Act
//         var act = () => calculationSchedulerHandler.CancelScheduledCalculationAsync(
//             durableTaskClient.Object,
//             new CalculationId(scheduledCalculation.Id));
//
//         // Assert
//         await act.Should().ThrowAsync<InvalidOperationException>();
//
//         // => Assert that the calculation is not canceled in the database
//         await using var assertDbContext = Fixture.DatabaseManager.CreateDbContext();
//         var calculationRepository = new CalculationRepository(assertDbContext);
//         var canceledCalculation = await calculationRepository.GetAsync(scheduledCalculation.Id);
//         canceledCalculation.OrchestrationState.Should().Be(CalculationOrchestrationState.Scheduled);
//         canceledCalculation.CanceledByUserId.Should().Be(null);
//
//         // => Verify that GetInstanceAsync was called once to check if the calculation was already started
//         durableTaskClient
//             .Verify(
//                 c => c.GetInstanceAsync(
//                     It.Is<string>(instanceId => instanceId == alreadyStartedOrchestrationInstanceId.Id),
//                     It.IsAny<bool>(),
//                     It.IsAny<CancellationToken>()),
//                 Times.Exactly(1));
//
//         // => Verify that no other calls were made to the durable task client
//         durableTaskClient.VerifyNoOtherCalls();
//     }
//
//     private Calculations.Application.Model.Calculations.Calculation CreateCalculation(Instant scheduledToRunAt)
//     {
//         var dummyInstant = Instant.FromUtc(2000, 01, 01, 00, 00);
//         return new Calculations.Application.Model.Calculations.Calculation(
//             dummyInstant,
//             CalculationType.BalanceFixing,
//             [new GridAreaCode("100")],
//             dummyInstant,
//             dummyInstant.Plus(Duration.FromDays(1)),
//             scheduledToRunAt,
//             DateTimeZone.Utc,
//             Guid.Empty,
//             1,
//             false);
//     }
// }
