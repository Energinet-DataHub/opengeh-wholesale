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
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Batches;
using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Moq.Protected;
using NodaTime;
using Test.Core;
using Xunit;

// TODO: Tests:
// - Creates zip without missing files
// - Logs error when files are missing
namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

// TODO: Why collection as a string? Why even a collection?
[Collection("ProcessManagerIntegrationTest")]
public sealed class BasisDataApplicationServiceTests
{
    public class ServiceCollectionConfigurator
    {
        public interface IHttpResponseMessage
        {
            Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        }

        private readonly List<Batch> _batches = new();
        private (Batch Batch, string ZipFileName)? _withBasisDataFilesForBatch;

        public ServiceCollectionConfigurator WithBatchInDatabase(Batch batch)
        {
            _batches.Add(batch);
            return this;
        }

        public ServiceCollectionConfigurator WithBasisDataFilesInCalculationStorage(Batch batch, string zipFileName)
        {
            _withBasisDataFilesForBatch = (batch, zipFileName);
            return this;
        }

        public void Configure(IServiceCollection serviceCollection)
        {
            // TODO: Use database
            serviceCollection.AddScoped(_ =>
                Moq.Mock.Of<IBatchRepository>(repo =>
                    repo.GetAsync(_withBasisDataFilesForBatch!.Value.Batch.Id) ==
                    Task.FromResult(_withBasisDataFilesForBatch!.Value.Batch)));

            if (_withBasisDataFilesForBatch != null)
            {
                var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
                serviceCollection.Replace(ServiceDescriptor.Singleton(dataLakeFileSystemClientMock.Object));

                var mockMessageHandler = new Mock<HttpMessageHandler>();
                serviceCollection.Replace(ServiceDescriptor.Singleton(new HttpClient(mockMessageHandler.Object)));

                // Mock batch basis files
                foreach (var gridAreaCode in _withBasisDataFilesForBatch.Value.Batch.GridAreaCodes)
                {
                    var fileDescriptorProviders = new List<Func<Guid, GridAreaCode, (string Directory, string Extension, string EntryPath)>>
                    {
                        BatchFileManager.GetResultDirectory,
                        BatchFileManager.GetTimeSeriesHourBasisDataDirectory,
                        BatchFileManager.GetTimeSeriesQuarterBasisDataDirectory,
                        BatchFileManager.GetMasterBasisDataDirectory,
                    };
                    foreach (var descriptorProvider in fileDescriptorProviders)
                    {
                        var (directory, extension, zipEntryPath) =
                            descriptorProvider(_withBasisDataFilesForBatch.Value.Batch.Id, gridAreaCode);

                        var response = new Mock<Response<bool>>();
                        var dataLakeDirectoryClient = new Mock<DataLakeDirectoryClient>();

                        dataLakeFileSystemClientMock
                            .Setup(client => client.GetDirectoryClient(directory))
                            .Returns(dataLakeDirectoryClient.Object);

                        dataLakeDirectoryClient
                            .Setup(client => client.ExistsAsync(default))
                            .ReturnsAsync(response.Object);

                        response
                            .Setup(r => r.Value)
                            .Returns(true);

                        var pathItemName = $"foo{extension}";
                        var pathItem = DataLakeModelFactory
                            .PathItem(pathItemName, false, DateTimeOffset.Now, ETag.All, 10, "owner", "group", "permissions");
                        var page = Page<PathItem>.FromValues(new[] { pathItem }, null, Moq.Mock.Of<Response>());
                        var asyncPageable = AsyncPageable<PathItem>.FromPages(new[] { page });
                        dataLakeDirectoryClient
                            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
                            .Returns(asyncPageable);

                        var dataLakeFileClientMock = new Mock<DataLakeFileClient>();
                        dataLakeFileSystemClientMock
                            .Setup(client => client.GetFileClient(pathItemName))
                            .Returns(dataLakeFileClientMock.Object);

                        var encodedDirectory = UrlEncoder.Create().Encode(directory);
                        var uriString = $"http://foo.bar?directory={encodedDirectory}";
                        dataLakeFileClientMock
                            .Setup(client => client.Uri)
                            .Returns(new Uri(uriString));

                        // Mock HttpClient for fetching basis data files
                        // TODO: Make length match content length above
                        var memoryStream = new MemoryStream(
                            Encoding.UTF8.GetBytes(
                                $"The '{extension}' file from directory '{directory}'"));
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        mockMessageHandler.Protected().As<IHttpResponseMessage>()
                            .Setup(message => message.SendAsync(
                                    It.Is<HttpRequestMessage>(requestMessage => requestMessage.RequestUri!.AbsoluteUri.Contains(encodedDirectory)),
                                    It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = new StreamContent(memoryStream),
                            });
                    }
                }

                // Mock zip file
                var zipFileClient = new Mock<DataLakeFileClient>();
                zipFileClient
                    .Setup(client => client.OpenWriteAsync(false, null, default))
                    .ReturnsAsync(() => File.OpenWrite(_withBasisDataFilesForBatch.Value.ZipFileName));
                dataLakeFileSystemClientMock
                    .Setup(client => client.GetFileClient(BatchFileManager.GetZipFileName(_withBasisDataFilesForBatch.Value.Batch)))
                    .Returns(zipFileClient.Object);
            }
        }
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task When_BatchIsCompleted_Then_CalculationFilesAreZipped(BatchCompletedEventDto batchCompletedEvent)
    {
        // Arrange
        var batch = CreateBatch(batchCompletedEvent);
        var serviceCollectionConfigurator = new ServiceCollectionConfigurator();
        var zipFileName = Path.GetTempFileName();

        using var host = await ProcessManagerIntegrationTestHost.CreateAsync(collection =>
            serviceCollectionConfigurator
                .WithBatchInDatabase(batch)
                .WithBasisDataFilesInCalculationStorage(batch, zipFileName)
                .Configure(collection));

        await using var scope = host.BeginScope();
        var sut = scope.ServiceProvider.GetRequiredService<IBasisDataApplicationService>();

        // Act
        await sut.ZipBasisDataAsync(batchCompletedEvent);

        // Assert
        // TODO: assert expected content and all files
        var zipExtractDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(zipFileName, zipExtractDirectory);
        var (directory, extension, zipEntryPath) = BatchFileManager.GetResultDirectory(batch.Id, batch.GridAreaCodes.Single());
        File.Exists(Path.Combine(zipExtractDirectory, zipEntryPath)).Should().BeTrue();
    }

    private static Batch CreateBatch(BatchCompletedEventDto batchCompletedEvent)
    {
        var gridAreaCode = new GridAreaCode("805");
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new[] { gridAreaCode },
            SystemClock.Instance.GetCurrentInstant(),
            SystemClock.Instance.GetCurrentInstant(),
            SystemClock.Instance);
        batch.SetPrivateProperty(b => b.Id, batchCompletedEvent.BatchId);
        return batch;
    }

    // [Theory]
    // [InlineAutoMoqData]
    private async Task When_BatchIsCompleted_Then_CalculationFilesAreZipped_DEPRECATED(BatchCompletedEventDto batchCompletedEvent)
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

        var zipBlobName = BatchFileManager.GetZipFileName(batch);
        var zipFilePath = Path.GetTempFileName();
        var zipFileClient = new Mock<DataLakeFileClient>();
        zipFileClient
            .Setup(client => client.OpenWriteAsync(false, null, default))
            .ReturnsAsync(() => File.OpenWrite(zipFilePath));
        dataLakeFileSystemClient
            .Setup(client => client.GetFileClient(zipBlobName))
            .Returns(zipFileClient.Object);

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
