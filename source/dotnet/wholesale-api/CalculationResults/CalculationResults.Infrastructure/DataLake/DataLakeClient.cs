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

using Azure.Storage.Files.DataLake;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.DataLake;

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;

/// <inheritdoc />
public sealed class DataLakeClient : IDataLakeClient
{
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;

    public DataLakeClient(DataLakeFileSystemClient dataLakeFileSystemClient)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
    }

    /// <inheritdoc />
    public async Task<string> FindFileAsync(string directory, string extension)
    {
        var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(directory);
        var directoryExists = await directoryClient.ExistsAsync().ConfigureAwait(false);
        if (!directoryExists.Value)
            throw new InvalidOperationException($"No directory was found on path: {directory}");

        await foreach (var pathItem in directoryClient.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == extension)
                return pathItem.Name;
        }

        throw new Exception($"No Data Lake file with extension '{extension}' was found in directory '{directory}'");
    }

    /// <inheritdoc />
    public Task<Stream> GetWriteableFileStreamAsync(string filename)
    {
        var dataLakeFileClient = _dataLakeFileSystemClient.GetFileClient(filename);
        return dataLakeFileClient.OpenWriteAsync(false);
    }

    /// <inheritdoc />
    public Task<Stream> GetReadableFileStreamAsync(string filename)
    {
        var dataLakeFileClient = _dataLakeFileSystemClient.GetFileClient(filename);
        return dataLakeFileClient.OpenReadAsync(false);
    }
}
