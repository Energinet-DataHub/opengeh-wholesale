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

using System.IO.Compression;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using Energinet.DataHub.Wholesale.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.Fixture.Database;
using Energinet.DataHub.Wholesale.IntegrationTests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

[Collection(nameof(ProcessManagerIntegrationTestHost))]
public sealed class SettlementReportApplicationServiceTests
{
    private readonly ProcessManagerDatabaseFixture _processManagerDatabaseFixture;

    public SettlementReportApplicationServiceTests(ProcessManagerDatabaseFixture processManagerDatabaseFixture)
    {
        _processManagerDatabaseFixture = processManagerDatabaseFixture;
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task When_BatchIsCompleted_Then_CalculationFilesAreZipped(BatchCompletedEventDto batchCompletedEvent)
    {
        // Arrange
        var batch = CreateBatch(batchCompletedEvent);
        var serviceCollectionConfigurator = new ServiceCollectionConfigurator();
        var zipFileName = Path.GetTempFileName();

        using var host = await ProcessManagerIntegrationTestHost.CreateAsync(_processManagerDatabaseFixture.DatabaseManager.ConnectionString, collection =>
            serviceCollectionConfigurator
                .WithBatchFileManagerForBatch(batch, zipFileName)
                .Configure(collection));

        await using var scope = host.BeginScope();
        await AddBatchToDatabase(scope, batch);
        var sut = scope.ServiceProvider.GetRequiredService<ISettlementReportApplicationService>();

        // Act
        await sut.CreateSettlementReportAsync(batchCompletedEvent);

        // Assert
        var zipExtractDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(zipFileName, zipExtractDirectory);

        var (resultDir, _, resultPath) = ProcessStepResultRepository.GetResultFileSpecification(batch.Id, batch.GridAreaCodes.Single());
        File.Exists(Path.Combine(zipExtractDirectory, resultPath)).Should().BeTrue();
        var resultContent = File.ReadLines(Path.Combine(zipExtractDirectory, resultPath)).First();
        resultContent.Should().BeEquivalentTo(resultDir);

        var (masterDataDir, _, masterDataPath) = SettlementReportRepository.GetMasterBasisDataFileSpecification(batch.Id, batch.GridAreaCodes.Single());
        File.Exists(Path.Combine(zipExtractDirectory, masterDataPath)).Should().BeTrue();
        var masterDataContent = File.ReadLines(Path.Combine(zipExtractDirectory, masterDataPath)).First();
        masterDataContent.Should().BeEquivalentTo(masterDataDir);

        var (quarterDir, _, quarterPath) = SettlementReportRepository.GetTimeSeriesQuarterBasisDataFileSpecification(batch.Id, batch.GridAreaCodes.Single());
        File.Exists(Path.Combine(zipExtractDirectory, quarterPath)).Should().BeTrue();
        var quarterContent = File.ReadLines(Path.Combine(zipExtractDirectory, quarterPath)).First();
        quarterContent.Should().BeEquivalentTo(quarterDir);

        var (hourDir, _, hourPath) = SettlementReportRepository.GetTimeSeriesHourBasisDataFileSpecification(batch.Id, batch.GridAreaCodes.Single());
        File.Exists(Path.Combine(zipExtractDirectory, hourPath)).Should().BeTrue();
        var hourContent = File.ReadLines(Path.Combine(zipExtractDirectory, hourPath)).First();
        hourContent.Should().BeEquivalentTo(hourDir);
    }

    private static async Task AddBatchToDatabase(AsyncServiceScope scope, Batch batch)
    {
        var batchRepository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();
        await batchRepository.AddAsync(batch);
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.CommitAsync();
    }

    private static Batch CreateBatch(BatchCompletedEventDto batchCompletedEvent)
    {
        var gridAreaCode = new GridAreaCode("805");
        var period = Periods.January_EuropeCopenhagen_Instant;
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { gridAreaCode },
            period.PeriodStart,
            period.PeriodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone);
        batch.SetPrivateProperty(b => b.Id, batchCompletedEvent.BatchId);
        return batch;
    }
}
