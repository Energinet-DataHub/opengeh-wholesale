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

namespace Energinet.DataHub.Wholesale.Infrastructure.BasisData;

public interface IStreamZipper
{
    /// <summary>
    /// Create a zip file containing the remove files.
    /// </summary>
    /// <param name="inputFiles">
    /// Tuples with streamed source file and the name to use in the zip archive.
    /// <paramref name="inputFiles.EntryPath"/> is the relative path in the zip archive.
    /// </param>
    /// <param name="zipFileStream">The local file system path of the output zip file.</param>
    Task ZipAsync(IEnumerable<(Stream FileStream, string EntryPath)> inputFiles, Stream zipFileStream);
}
