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
using Azure;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NodaTime;
using Test.Core;
using Xunit;

// TODO: Tests:
// - Creates zip without missing files
// - Logs error when files are missing
namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

// TODO: Why collection as a string?
[Collection("ProcessManagerIntegrationTest")]
public sealed class BasisDataApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task When_BatchIsCompleted_Then_CalculationFilesAreZipped(BatchCompletedEventDto batchCompletedEvent)
    {
        // Arrange
        var gridAreaCode = new GridAreaCode("805");
        var batch = new Batch(ProcessType.BalanceFixing, new[] { gridAreaCode }, SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance);
        batch.SetPrivateProperty(b => b.Id, batchCompletedEvent.BatchId);

        var repo = new Mock<IBatchRepository>();
        repo.Setup(repository => repository.GetAsync(batchCompletedEvent.BatchId)).ReturnsAsync(batch);

        var response = new Mock<Response<bool>>();
        response
            .Setup(r => r.Value)
            .Returns(true);

        var dataLakeDirectoryClient = new Mock<DataLakeDirectoryClient>();
        dataLakeDirectoryClient
            .Setup(client => client.ExistsAsync(CancellationToken.None))
            .ReturnsAsync(response.Object);

        var dataLakeFileSystemClient = new Mock<DataLakeFileSystemClient>();
        var resultsDirectoryName = $"results/batch_id={batchCompletedEvent.BatchId}/grid_area={gridAreaCode.Code}/";
        dataLakeFileSystemClient
            .Setup(client => client.GetDirectoryClient(It.IsAny<string>() /*resultsDirectoryName*/))
            .Returns(dataLakeDirectoryClient.Object);

        var zipBlobName = BatchFileManager.GetZipBlobName(batch);
        var zipFilePath = Path.GetTempFileName();
        await using (var stream = File.OpenWrite(zipFilePath))
        {
            var zipFileClient = new Mock<DataLakeFileClient>();
            zipFileClient
                .Setup(client => client.OpenWriteAsync(false, null, default))
                .ReturnsAsync(stream);
            dataLakeFileSystemClient
                .Setup(client => client.GetFileClient(zipBlobName))
                .Returns(zipFileClient.Object);
        }

        void ServiceCollectionAction(IServiceCollection collection)
        {
            collection.AddScoped(_ => repo.Object);
            collection.AddScoped(_ => dataLakeFileSystemClient.Object);

            collection.AddSingleton(_ =>
            {
                var body = new MemoryStream();

                var content = new Mock<HttpContent>();
                content.Setup(httpContent => httpContent.ReadAsStreamAsync()).ReturnsAsync(body);

                var responseMessage = new Mock<HttpResponseMessage>();
                responseMessage.Setup(message => message.Content).Returns(content.Object);

                var client = new Mock<HttpClient>();
                client
                    .Setup(httpClient => httpClient.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(responseMessage.Object);

                return client.Object;
            });
        }

        using var host = await ProcessManagerIntegrationTestHost.CreateAsync(ServiceCollectionAction);

        // TODO: Create all calculation files (basis data and result)
        var (directory, extension, masterDataEntryPath) = BatchFileManager.GetMasterBasisDataDirectory(batchCompletedEvent.BatchId, gridAreaCode);
        // var path = Path.Combine(directory, $"master-basis-data{extension}");
        // var fileClient = await dataLakeFileSystemClient.CreateFileAsync(path);
        // await fileClient.Value.UploadAsync("master basis data content");
        await using var scope = host.BeginScope();
        var sut = scope.ServiceProvider.GetRequiredService<IBasisDataApplicationService>();

        // Act
        await sut.ZipBasisDataAsync(batchCompletedEvent);

        // Assert
        // await using (var stream = File.OpenWrite(zipFilePath))
        // {
        //     await zipFileClient.ReadToAsync(stream);
        // }
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(zipFilePath, dir);
        File.Exists(Path.Combine(dir, masterDataEntryPath)).Should().BeTrue();
        // TODO: assert expected content
    }
}
