//  Copyright 2020 Energinet DataHub A/S
//
//  Licensed under the Apache License, Version 2.0 (the "License2");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Azure.Storage.Files.DataLake;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.DataLake;

public class DataLakeRepositoryBase
{
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;

    public DataLakeRepositoryBase(DataLakeFileSystemClient dataLakeFileSystemClient)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
    }

    /// <summary>
    /// Search for a file by a given extension in a blob directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="extension"></param>
    /// <returns>The first file with matching file extension. If no directory was found, return null</returns>
    public async Task<DataLakeFileClient> GetDataLakeFileClientAsync(string directory, string extension)
    {
        var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(directory);
        var directoryExists = await directoryClient.ExistsAsync().ConfigureAwait(false);
        if (!directoryExists.Value)
            throw new InvalidOperationException($"No directory was found on path: {directory}");

        await foreach (var pathItem in directoryClient.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == extension)
                return _dataLakeFileSystemClient.GetFileClient(pathItem.Name);
        }

        throw new Exception($"No Data Lake file with extension '{extension}' was found in directory '{directory}'");
    }
}
