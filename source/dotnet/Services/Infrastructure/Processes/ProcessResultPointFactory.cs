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

using System.Text.Json;
using Energinet.DataHub.Wholesale.Application.ProcessResult;

namespace Energinet.DataHub.Wholesale.Infrastructure.Processes;

public class ProcessResultPointFactory : IProcessResultPointFactory
{
    /// <summary>
    /// Creates a List of ProcessResultPoint.
    /// </summary>
    /// <param name="resultStream">The stream must be a .json file in the 'json newline' format.</param>
    public async Task<List<ProcessResultPoint>> GetPointsFromJsonStreamAsync(Stream resultStream)
    {
        var list = new List<ProcessResultPoint>();

        var streamer = new StreamReader(resultStream);

        var nextline = await streamer.ReadLineAsync().ConfigureAwait(false);
        while (nextline != null)
        {
            var dto = JsonSerializer.Deserialize<ProcessResultPoint>(
                nextline,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto != null)
                list.Add(dto);

            nextline = await streamer.ReadLineAsync().ConfigureAwait(false);
        }

        return list;
    }
}
