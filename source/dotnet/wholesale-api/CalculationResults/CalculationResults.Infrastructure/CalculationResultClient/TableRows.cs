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
// namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

using System.Collections;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public class TableRows
{
    private readonly List<string[]> _rowData;
    private readonly Dictionary<string, int> _columnIndex;

    public TableRows(IEnumerable<string> columnNames, IEnumerable<string[]> rowData)
    {
        _columnIndex = columnNames.Select((name, i) => (name, i)).ToDictionary(x => x.name, x => x.i);
        _rowData = rowData.ToList();
    }

    public string this[int rowIndex, string columnName] => _rowData[rowIndex][_columnIndex[columnName]];

    public IEnumerable<string> this[int rowIndex] => _rowData[rowIndex];

    public int Count => _rowData.Count;
}
