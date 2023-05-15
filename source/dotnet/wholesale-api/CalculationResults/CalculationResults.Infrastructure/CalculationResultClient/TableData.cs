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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public class TableData
{
    private readonly List<string[]> _data;
    private readonly Dictionary<string, int> _columnIndex;

    public TableData(IEnumerable<string> columnNames, IEnumerable<string[]> data)
    {
        _columnIndex = columnNames.Select((name, i) => (name, i)).ToDictionary(x => x.name, x => x.i);
        _data = data.ToList();
    }

    public string this[string columnName, int rowIndex] =>
        _data[rowIndex][_columnIndex[columnName]] ??
        throw new InvalidOperationException();
}
