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

using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Moq.Protected;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Hosts;

public class ServiceCollectionConfigurator
{
    private interface IHttpResponseMessage
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
