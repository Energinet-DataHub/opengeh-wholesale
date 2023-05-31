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

using Energinet.DataHub.Wholesale.CalculationResults.Application;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

public class Table
{
    private readonly List<string[]> _rows;
    private readonly Dictionary<string, int> _columnIndex;

    public Table(IEnumerable<string> columnNames, IEnumerable<string[]> rows)
    {
        _columnIndex = columnNames.Select((name, i) => (name, i)).ToDictionary(x => x.name, x => x.i);
        _rows = rows.ToList();
    }

    public string this[Index rowIndex, string columnName] => _rows[rowIndex][_columnIndex[columnName]];

    public IEnumerable<string> this[Index rowIndex] => _rows[rowIndex];

    public int RowCount => _rows.Count;
}
