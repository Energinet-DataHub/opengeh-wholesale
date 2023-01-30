﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.DataLake;

namespace Energinet.DataHub.Wholesale.Infrastructure;

public class JsonNewlineSerializer : IJsonNewlineSerializer
{
    public async Task<List<T>> DeserializeAsync<T>(Stream resultStream)
    {
        var list = new List<T>();

        var streamer = new StreamReader(resultStream);

        var nextline = await streamer.ReadLineAsync().ConfigureAwait(false);
        while (nextline != null)
        {
            var item = JsonSerializer.Deserialize<T>(
                nextline,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (item != null)
                list.Add(item);

            nextline = await streamer.ReadLineAsync().ConfigureAwait(false);
        }

        return list;
    }
}
