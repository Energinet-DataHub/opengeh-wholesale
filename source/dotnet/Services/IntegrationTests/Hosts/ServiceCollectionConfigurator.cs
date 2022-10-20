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

using System.Text;
using System.Web;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Hosts;

/// <summary>
/// Extension methods to configure mocks for external dependencies of the host applications.
///
/// This allows for integration testing of the hosts without the problems with
/// long waiting times to provision external resources and the related stability problems.
///
/// Uses a builder-like pattern.
/// </summary>
public class ServiceCollectionConfigurator
{
    private (Batch Batch, string ZipFileName)? _withBasisDataFilesForBatch;

    /// <summary>
    /// Configure the service collection to support the methods of the <see cref="BatchFileManager"/>
    /// for the given batch. The batch must exist in the database.
    /// </summary>
    public ServiceCollectionConfigurator WithBatchFileManagerForBatch(Batch batch, string zipFileName)
    {
        _withBasisDataFilesForBatch = (batch, zipFileName);
        return this;
    }

    /// <summary>
    /// Execute the configuration of the service collection according to configuration
    /// specified by calling the other methods.
    /// </summary>
    public void Configure(IServiceCollection serviceCollection)
    {
        if (_withBasisDataFilesForBatch != null)
            ConfigureBasisDataFilesForBatch(serviceCollection);
    }

    /// <summary>
    /// Add the configuration needed to use the <see cref="BatchFileManager"/>
    /// for the <see cref="Batch"/> specified in <see cref="WithBatchFileManagerForBatch"/>.
    /// </summary>
    private void ConfigureBasisDataFilesForBatch(IServiceCollection serviceCollection)
    {
        var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        serviceCollection.Replace(ServiceDescriptor.Singleton(dataLakeFileSystemClientMock.Object));

        // Mock batch basis data files for each process (grid area)
        foreach (var gridAreaCode in _withBasisDataFilesForBatch!.Value.Batch.GridAreaCodes)
        {
            var fileDescriptorProviders =
                new List<Func<Guid, GridAreaCode, (string Directory, string Extension, string EntryPath)>>
                {
                    BatchFileManager.GetResultDirectory,
                    BatchFileManager.GetTimeSeriesHourBasisDataDirectory,
                    BatchFileManager.GetTimeSeriesQuarterBasisDataDirectory,
                    BatchFileManager.GetMasterBasisDataDirectory,
                };

            // Mock each basis data files for the process
            foreach (var descriptorProvider in fileDescriptorProviders)
            {
                var (directory, _, entryPath) =
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

                var basisDataBuffer = Encoding.UTF8.GetBytes(directory);

                // Configure the Data Lake directory client to get paths in directory
                var pathItemName = Path.GetFileName(entryPath);
                var pathItem = DataLakeModelFactory
                    .PathItem(pathItemName, false, DateTimeOffset.Now, ETag.All, basisDataBuffer.Length, "owner", "group", "permissions");
                var page = Page<PathItem>.FromValues(new[] { pathItem }, null, Moq.Mock.Of<Response>());
                var asyncPageable = AsyncPageable<PathItem>.FromPages(new[] { page });
                dataLakeDirectoryClient
                    .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
                    .Returns(asyncPageable);

                var dataLakeFileClientMock = new Mock<DataLakeFileClient>();
                dataLakeFileSystemClientMock
                    .Setup(client => client.GetFileClient(pathItemName))
                    .Returns(dataLakeFileClientMock.Object);

                var encodedDirectory = HttpUtility.UrlEncode(directory);
                var uriString = $"https://foo.bar?directory={encodedDirectory}";
                dataLakeFileClientMock
                    .Setup(client => client.Uri)
                    .Returns(new Uri(uriString));

                var str = "stringValue";
                // Enable the IStreamZipper to access the basis data files
                // The files are represented by mocked names and in-memory streams
                var memoryStream = new MemoryStream(basisDataBuffer);
                var response1 = Response.FromValue<FileDownloadInfo>(
                    DataLakeModelFactory.FileDownloadInfo(
                    memoryStream.Length,
                    memoryStream,
                    null,
                    DataLakeModelFactory.FileDownloadDetails(
                        DateTimeOffset.Now,
                        new Dictionary<string,
                            string>(),
                        str,
                        ETag.All,
                        str,
                        str,
                        str,
                        str,
                        DateTimeOffset.Now,
                        str,
                        str,
                        str,
                        new Uri("http://stuff.com"),
                        CopyStatus.Success,
                        DataLakeLeaseDuration.Fixed,
                        DataLakeLeaseState.Available,
                        DataLakeLeaseStatus.Locked,
                        str,
                        false,
                        str,
                        basisDataBuffer)),
                    null!);
                dataLakeFileClientMock
                    .Setup(client => client.ReadAsync())
                    .ReturnsAsync(() => response1);
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
