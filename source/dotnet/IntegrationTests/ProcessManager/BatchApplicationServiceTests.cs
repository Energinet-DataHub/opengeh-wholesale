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

using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using FluentAssertions;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

[Collection("ProcessManagerIntegrationTest")]
public sealed class BatchApplicationServiceTests
{
    [Fact]
    public async Task When_RunIsCanceled_Then_BatchIsRetried()
    {
        // Arrange
        using var host = await ProcessManagerIntegrationTestHost.InitializeAsync();

        await using var scope = host.BeginScope();
        var appService = scope.ServiceProvider.GetRequiredService<IBatchApplicationService>();
        var repository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

        var gridAreaCode = CreateRandomGridAreaCode();
        var batchRequest = new BatchRequestDto(WholesaleProcessType.BalanceFixing, new[] { gridAreaCode });

        await appService.CreateAsync(batchRequest);
        await appService.StartPendingAsync();

        // Assert 1: Verify that batch is now executing.
        var executing = await repository.GetExecutingAsync();
        var createdBatch = executing.Single(x => x.GridAreaCodes.Contains(new GridAreaCode(gridAreaCode)));

        // Act
        using var cancelHost = await ProcessManagerIntegrationTestHost.InitializeAsync(ConfigureDatabricksClientToCancel);
        await using var cancelScope = cancelHost.BeginScope();
        var target = cancelScope.ServiceProvider.GetRequiredService<IBatchApplicationService>();
        await target.UpdateExecutionStateAsync();

        // Assert 2: Verify that batch is back to pending.
        var pending = await repository.GetPendingAsync();
        var updatedBatch = pending.Single();
        updatedBatch.Id.Should().BeEquivalentTo(createdBatch.Id);
        updatedBatch.GridAreaCodes.Should().ContainSingle(code => code.Code == gridAreaCode);
    }

    [Fact]
    public async Task When_RunIsCompleted_Then_BatchIsCompleted()
    {
        // Arrange
        using var host = await ProcessManagerIntegrationTestHost.InitializeAsync();

        await using var scope = host.BeginScope();
        var target = scope.ServiceProvider.GetRequiredService<IBatchApplicationService>();
        var repository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

        var gridAreaCode = CreateRandomGridAreaCode();
        var batchRequest = new BatchRequestDto(WholesaleProcessType.BalanceFixing, new[] { gridAreaCode });

        await target.CreateAsync(batchRequest);
        await target.StartPendingAsync();

        // Assert 1: Verify that batch is now executing.
        var executing = await repository.GetExecutingAsync();
        var createdBatch = executing.Single(x => x.GridAreaCodes.Contains(new GridAreaCode(gridAreaCode)));

        // Act
        await target.UpdateExecutionStateAsync();

        // Assert 2: Verify that batch is completed.
        var pending = await repository.GetCompletedAsync();
        var updatedBatch = pending.Single(x => x.Id == createdBatch.Id);
        updatedBatch.GridAreaCodes.Should().ContainSingle(code => code.Code == gridAreaCode);
    }

    private static string CreateRandomGridAreaCode()
    {
        return Random.Shared.Next(999).ToString("000");
    }

    private static void ConfigureDatabricksClientToCancel(IServiceCollection serviceCollection)
    {
        var canceledRun = new Run { State = new RunState { ResultState = RunResultState.CANCELED } };

        var mockedJobsApi = new Mock<IJobsWheelApi>();
        mockedJobsApi
            .Setup(x => x.RunsGet(It.IsAny<long>(), default))
            .ReturnsAsync(canceledRun);

        var mockedDatabricksClient = new Mock<DatabricksWheelClient>();
        mockedDatabricksClient
            .Setup(x => x.Jobs)
            .Returns(mockedJobsApi.Object);

        serviceCollection.Replace(ServiceDescriptor.Singleton(mockedDatabricksClient.Object));
    }
}
