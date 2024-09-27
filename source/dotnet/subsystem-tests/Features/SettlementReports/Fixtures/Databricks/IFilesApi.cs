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

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.SettlementReports.Fixtures.Databricks;

/// <summary>
/// Extend the Databricks Client with operations from the Files REST API: https://docs.databricks.com/api/azure/workspace/files
/// Inspired by the design of the Databricks Client library: https://github.com/Azure/azure-databricks-client
/// </summary>
public interface IFilesApi : IDisposable
{
    /// <summary>
    /// Wrapping a call to: https://docs.databricks.com/api/azure/workspace/files/getmetadata
    /// </summary>
    /// <param name="filePath">The absolute path of the file. Example: "/Volumes/my-catalog/my-schema/my-volume/directory/file.txt"</param>
    /// <param name="cancellationToken"></param>
    /// <returns>File information if we can get metadata for the file; otherwise throws an exception.</returns>
    Task<FileInfo> GetFileInfoAsync(string filePath, CancellationToken cancellationToken = default);
}
