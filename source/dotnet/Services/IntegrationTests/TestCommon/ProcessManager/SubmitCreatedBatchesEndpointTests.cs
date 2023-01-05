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
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.FunctionApp;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Function;
using Energinet.DataHub.Wholesale.ProcessManager.Endpoints;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.ProcessManager;

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

        [Fact]
        public async Task When_PendingBatchCreated_Then_BatchAndProcessCompletedEventsArePublished()
        {
            // Arrange
            var gridAreaCode = CreateGridAreaCode();
            await CreateAndSavePendingBatch(gridAreaCode);

            using var eventualBatchCompletedEvent = await Fixture
                .BatchCompletedListener
                .ListenForMessageAsync<BatchCompletedEventDto>(_ => true);
            using var eventualProcessCompletedEvent = await Fixture
                .SendDataAvailableWhenProcessCompletedListener
                .ListenForMessageAsync<ProcessCompletedEventDto>(e => e.GridAreaCode == gridAreaCode);

            // Act: The sut endpoint is timer triggered, thus there are nothing to invoke here

            // Assert: Await timer triggered endpoints has executed before actually asserting
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(SubmitCreatedBatchesEndpoint));

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

        [Fact]
        public async Task When_PendingBatchCreated_Then_BatchIsCompleted()
        {
            // Arrange
            var gridAreaCode = CreateGridAreaCode();
            var batchId = await CreateAndSavePendingBatch(gridAreaCode);
            Fixture.HostManager.ClearHostLog();

            // Act: The sut endpoint is timer triggered, thus there are nothing to invoke here

            // Assert: Await timer triggered endpoints has executed before actually asserting
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(SubmitCreatedBatchesEndpoint));

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
            var periodStart = DateTimeOffset.Parse("2021-12-31T23:00Z").ToInstant();
            var periodEnd = DateTimeOffset.Parse("2022-01-31T22:59:59.999Z").ToInstant();

            var pendingBatch = new Batch(
                ProcessType.BalanceFixing,
                new List<GridAreaCode> { new(gridAreaCode) },
                periodStart,
                periodEnd,
                SystemClock.Instance.GetCurrentInstant());

            await dbContext.Batches.AddAsync(pendingBatch);
            await dbContext.SaveChangesAsync();
            return pendingBatch.Id;
        }
    }
}
