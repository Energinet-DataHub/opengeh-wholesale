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
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Zip;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

// TODO BJARKE: The directory paths must match the directory used by Databricks (see calculator.py).
public class BatchFileManager : IBatchFileManager
{
    private const string ZipFilename = "Basis Data.zip";

    private (string Directory, string Extension) GetResultDirectory(Guid batchId, GridAreaCode gridAreaCode) => ($"results/batch_id={batchId}/grid_area={gridAreaCode}/", ".json");

    private (string Directory, string Extension) GetTimeSeriesHourBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode) => ($"results/basis-data/batch_id={batchId}/time-series-hour/grid_area={gridAreaCode}/", ".csv");

    private (string Directory, string Extension) GetTimeSeriesQuarterBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode) => ($"results/basis-data/batch_id={batchId}/time-series-quarter/grid_area={gridAreaCode}/", ".csv");

    private (string Directory, string Extension) GetMasterBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode) => ($"results/master-basis-data/batch_id={batchId}/grid_area={gridAreaCode}/", ".csv");

    private string GetZipBlobName(Guid batchId) => $"results/zip/batch_id={batchId}";

    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;
    private readonly List<Func<Guid, GridAreaCode, (string Directory, string Extension)>> _fileIdentifierProviders;

    private readonly BlobContainerClient _blobContainerClient;
    private readonly IWebFilesZipper _webFilesZipper;

    public BatchFileManager(DataLakeFileSystemClient dataLakeFileSystemClient, BlobContainerClient blobContainerClient, IWebFilesZipper webFilesZipper)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
        _blobContainerClient = blobContainerClient;
        _webFilesZipper = webFilesZipper;
        _fileIdentifierProviders = new List<Func<Guid, GridAreaCode, (string Directory, string Extension)>>
        {
            GetResultDirectory,
            GetTimeSeriesHourBasisDataDirectory,
            GetTimeSeriesQuarterBasisDataDirectory,
            GetMasterBasisDataDirectory,
        };
    }

    public async Task CreateBasisDataZipAsync(Batch completedBatch)
    {
        var batchBasisFileUrls = GetBatchBasisFileUrlsAsync(completedBatch);

        var zipBlobName = GetZipBlobName(completedBatch.Id);
        var zipStream = await GetWriteStreamAsync(zipBlobName).ConfigureAwait(false);
        await using (zipStream)
            await _webFilesZipper.ZipAsync(batchBasisFileUrls, zipStream).ConfigureAwait(false);
    }

    private async Task<IEnumerable<Uri>> GetBatchBasisFileUrlsAsync(Batch batch)
    {
        var basisDataFileUrls = new List<Uri>();

        foreach (var gridAreaCode in batch.GridAreaCodes)
        {
            var gridAreaFileUrls = await GetProcessBasisFileUrlsAsync(batch.Id, gridAreaCode).ConfigureAwait(false);
            basisDataFileUrls.AddRange(gridAreaFileUrls);
        }

        return basisDataFileUrls;
    }

    private async Task<List<Uri>> GetProcessBasisFileUrlsAsync(Guid batchId, GridAreaCode gridAreaCode)
    {
        var processDataFilesUrls = new List<Uri>();

        foreach (var fileIdentifierProvider in _fileIdentifierProviders)
        {
            var (directory, extension) = fileIdentifierProvider(batchId, gridAreaCode);
            var processDataFileUrl = await SearchBlobUrlAsync(directory, extension).ConfigureAwait(false);
            if (processDataFileUrl != null)
            {
                processDataFilesUrls.Add(processDataFileUrl);
            }
            else
            {
                // TODO BJARKE: Log error instead
                throw new InvalidOperationException($"Blob for process was not found in '{directory}*{extension}'.");
            }
        }

        return processDataFilesUrls;
    }

    /// <summary>
    /// Search for a file by a given extension in a blob directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="extension"></param>
    /// <returns>The first file with matching file extension.</returns>
    private async Task<Uri?> SearchBlobUrlAsync(string directory, string extension)
    {
        var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(directory);

        await foreach (var pathItem in directoryClient.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == extension)
            {
                var fileClient = _dataLakeFileSystemClient.GetFileClient(pathItem.Name);
                return fileClient.Uri;
            }
        }

        return null;
    }

    private async Task<Stream> GetWriteStreamAsync(string blobName)
    {
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenWriteAsync(false).ConfigureAwait(false);
    }
}
