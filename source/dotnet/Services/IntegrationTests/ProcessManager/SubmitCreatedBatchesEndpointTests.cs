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

using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestHelpers;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Function;
using Energinet.DataHub.Wholesale.ProcessManager.Endpoints;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

public class SubmitCreatedBatchesEndpointTests
{
    [Collection(nameof(ProcessManagerFunctionAppCollectionFixture))]
    public class RunAsync : FunctionAppTestBase<ProcessManagerFunctionAppFixture>, IAsyncLifetime
    {
        public RunAsync(ProcessManagerFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        public Task InitializeAsync()
        {
            Fixture.BatchCompletedListener.Reset();
            return Task.CompletedTask;
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact(Skip = "Split into multiple tests when concepts are ready")]
        public async Task When_PendingBatchCreated_Then_BatchAndProcessCompletedEventsArePublished()
        {
            // Arrange
            var gridAreaCode = CreateGridAreaCode();
            var batchId = await CreateAndSavePendingBatch(gridAreaCode);

            using var eventualBatchCompletedEvent = await Fixture
                .BatchCompletedListener
                .ListenForMessageAsync<BatchCompletedEventDto>(b => b.BatchId == batchId);

            using var eventualProcessCompletedEvent = await Fixture
                .ProcessCompletedListener
                .ListenForMessageAsync<ProcessCompletedEventDto>(e => e.GridAreaCode == gridAreaCode);

            // Act: how to invoke create batch

            // Assert: Await timer triggered endpoints has executed before actually asserting
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(StartCalculationEndpoint));

            // clear log to ensure that initial run of UpdateBatchExecutionStateEndpoint does not count.
            Fixture.HostManager.ClearHostLog();
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(UpdateBatchExecutionStateEndpoint));

            // Assert: Batch and process completed events have been published
            var isBatchCompletedEventPublished = eventualBatchCompletedEvent
                .MessageAwaiter!
                .Wait(TimeSpan.FromSeconds(20));
            isBatchCompletedEventPublished.Should().BeTrue();

            var isProcessCompletedEventPublished = eventualProcessCompletedEvent
                .MessageAwaiter!
                .Wait(TimeSpan.FromSeconds(20));
            isProcessCompletedEventPublished.Should().BeTrue();
        }

        [Fact(Skip = "Split into multiple tests when concepts are ready")]
        public async Task When_PendingBatchCreated_Then_BatchIsCompleted()
        {
            // Arrange
            var gridAreaCode = CreateGridAreaCode();
            var batchId = await CreateAndSavePendingBatch(gridAreaCode);
            Fixture.HostManager.ClearHostLog();

            // Act: The sut endpoint is timer triggered, thus there are nothing to invoke here

            // Assert: Await timer triggered endpoints has executed before actually asserting
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(StartCalculationEndpoint));

            // clear log to ensure that initial run of UpdateBatchExecutionStateEndpoint does not count.
            Fixture.HostManager.ClearHostLog();
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(UpdateBatchExecutionStateEndpoint));

            // Assert: The pending batch is now complete
            await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
            var actualBatch = await dbContext.Batches.SingleAsync(b => b.Id == batchId);
            actualBatch.ExecutionState.Should().Be(BatchExecutionState.Completed);
        }

        private static readonly Random _generator = new();

        /// <summary>
        /// Create a grid area code with valid format.
        /// </summary>
        private static string CreateGridAreaCode() => _generator.Next(100, 1000).ToString();

        private async Task<Guid> CreateAndSavePendingBatch(string gridAreaCode)
        {
            await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
            var period = Periods.January_EuropeCopenhagen_Instant;

            var pendingBatch = new Batch(
                ProcessType.BalanceFixing,
                new List<GridAreaCode> { new(gridAreaCode) },
                period.PeriodStart,
                period.PeriodEnd,
                SystemClock.Instance.GetCurrentInstant(),
                period.DateTimeZone);

            await dbContext.Batches.AddAsync(pendingBatch);
            await dbContext.SaveChangesAsync();
            return pendingBatch.Id;
        }
    }
}
