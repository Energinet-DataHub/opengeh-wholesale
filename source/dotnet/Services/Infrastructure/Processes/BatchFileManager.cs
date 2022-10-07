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

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

// TODO BJARKE: Split file zipping into separate class
// TODO BJARKE: The directory paths must match the directory used by Databricks (see calculator.py).
public class BatchFileManager : IBatchFileManager
{
    private const string ZipFilename = "Basis Data.zip";
    private (string Directory, string Extension) GetResultDirectory(Guid batchId, GridAreaCode gridAreaCode) => ($"results/batch_id={batchId}/grid_area={gridAreaCode}/", ".json");

    private (string Directory, string Extension) GetTimeSeriesHourBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode) => ($"results/basis-data/batch_id={batchId}/time-series-hour/grid_area={gridAreaCode}/", ".csv");

    private (string Directory, string Extension) GetTimeSeriesQuarterBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode) => ($"results/basis-data/batch_id={batchId}/time-series-quarter/grid_area={gridAreaCode}/", ".csv");

    private (string Directory, string Extension) GetMasterBasisDataDirectory(Guid batchId, GridAreaCode gridAreaCode) => ($"results/master-basis-data/batch_id={batchId}/grid_area={gridAreaCode}/", ".csv");

    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;
    private readonly List<Func<Guid, GridAreaCode, (string Directory, string Extension)>> _fileIdentifierProviders;

    // TODO BJARKE: Create in DI with correct path
    private readonly BlobContainerClient _blobContainerClient;

    public BatchFileManager(DataLakeFileSystemClient dataLakeFileSystemClient, BlobContainerClient blobContainerClient)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
        _blobContainerClient = blobContainerClient;
        _fileIdentifierProviders = new List<Func<Guid, GridAreaCode, (string Directory, string Extension)>>
        {
            GetResultDirectory,
            GetTimeSeriesHourBasisDataDirectory,
            GetTimeSeriesQuarterBasisDataDirectory,
            GetMasterBasisDataDirectory,
        };
    }

    // TODO BJARKE: Remove result file (including in naming)?
    public async Task ZipBasisDataAndResultAsync(Batch completedBatch)
    {
        foreach (var gridAreaCode in completedBatch.GridAreaCodes)
        {
            await ZipBasisDataAndResultAsync(completedBatch.Id, gridAreaCode).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Create a zip archive for a process.
    /// </summary>
    private async Task ZipBasisDataAndResultAsync(Guid batchId, GridAreaCode gridAreaCode)
    {
        var tempDirectoryPath = CreateTempDirectory();
        try
        {
            foreach (var fileIdentifierProvider in _fileIdentifierProviders)
                await CreateTempInputFileAsync(fileIdentifierProvider(batchId, gridAreaCode), tempDirectoryPath).ConfigureAwait(false);

            ZipFile.CreateFromDirectory(tempDirectoryPath, ZipFilename);
            var zipFilePath = Path.Combine(Path.GetTempPath(), ZipFilename);

            await UploadFileAsync(zipFilePath);
        }
        finally
        {
            Directory.Delete(tempDirectoryPath);
        }
    }

    private static string CreateTempDirectory()
    {
        // Files must be collocated in an (empty) folder from which the zip archive can be created
        var tempDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectoryPath);
        return tempDirectoryPath;
    }

    private async Task CreateTempInputFileAsync((string Directory, string Extension) fileIdentifier, string tempDirectoryPath)
    {
        var tempFilePath = Path.GetTempFileName();
        var inputStream = await GetFileStreamAsync(fileIdentifier).ConfigureAwait(false);
        await using (inputStream.ConfigureAwait(false))
        {
            await using var outputStream = new FileStream(tempFilePath, FileMode.Append);
            await inputStream.CopyToAsync(outputStream).ConfigureAwait(false);
        }

        File.Move(tempFilePath, tempDirectoryPath);
    }

    private async Task<Stream> GetFileStreamAsync((string Directory, string Extension) fileIdentifier)
    {
        var (directory, extension) = fileIdentifier;
        var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(directory);

        DataLakeFileClient? file = null;
        await foreach (var pathItem in directoryClient.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == extension)
            {
                file = _dataLakeFileSystemClient.GetFileClient(pathItem.Name);
                break;
            }
        }

        if (file == null)
            throw new InvalidOperationException($"Blob for process was not found in '{directory}*{extension}'.");

        return await file.OpenReadAsync().ConfigureAwait(false);
    }

    // TODO BJARKE: Move to new class
    private async Task UploadFileAsync(string localFilePath)
    {
        var fileName = Path.GetFileName(localFilePath);
        var blobClient = _blobContainerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(localFilePath, true);
    }
}
