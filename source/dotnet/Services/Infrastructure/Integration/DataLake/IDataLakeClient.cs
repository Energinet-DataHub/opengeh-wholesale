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

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;

/// <summary>
/// Client for accessing data lake.
/// </summary>
public interface IDataLakeClient
{
    /// <summary>
    /// Search for a file by a given extension in a blob directory.
    /// </summary>
    /// <param name="directory">The directory in which to search.</param>
    /// <param name="extension">The extension to search for.</param>
    /// <returns>The first file with matching file extension. If the file cannot be found, an exception is thrown</returns>
    Task<Stream> FindAndOpenFileAsync(string directory, string extension);

    /// <summary>
    /// Gets a writeable stream to the file specified.
    /// </summary>
    /// <param name="filename">The name of the file.</param>
    /// <returns>A writeable stream to the file.</returns>
    Task<Stream> GetWriteableFileStreamAsync(string filename);

    /// <summary>
    /// Gets a readable stream to the file specified.
    /// </summary>
    /// <param name="filename">The name of the file.</param>
    /// <returns>A readable stream to the file</returns>
    Task<Stream> GetReadableFileStreamAsync(string filename);
}
