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

using System.Collections.ObjectModel;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure;

/// <summary>
///     Represents a table or a chunk of a table.
///     Or put in another way, a table with a subset of all the rows of a SQL query.
/// </summary>
public class TableChunk
{
    private readonly Dictionary<string, int> _columnIndex;

    public TableChunk(string[] columnNames, List<string?[]> rows)
    {
        _columnIndex = columnNames.Select((name, i) => (name, i)).ToDictionary(x => x.name, x => x.i);
        ColumnNames = columnNames;
        Rows = rows.AsReadOnly();
    }

    public string? this[Index rowIndex, string columnName] => Rows[rowIndex][_columnIndex[columnName]];

    public string[] ColumnNames { get; }

    private ReadOnlyCollection<string?[]> Rows { get; }
}
