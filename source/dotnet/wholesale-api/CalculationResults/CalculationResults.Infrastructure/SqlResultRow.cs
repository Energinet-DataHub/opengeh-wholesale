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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure;

public record SqlResultRow
{
    private readonly TableChunk _chunk;

    private readonly int _index;

    public SqlResultRow(TableChunk chunk, int index)
    {
        _chunk = chunk;
        _index = index;
    }

    ////public string this[string column] => _chunk[_index, column];

    public IDictionary<string, object?> ToDic()
    {
        var dic = new Dictionary<string, object?>();
        foreach (var columnName in _chunk.ColumnNames)
        {
            dic.Add(columnName, _chunk[_index, columnName]);
        }

        return dic;
    }
}
