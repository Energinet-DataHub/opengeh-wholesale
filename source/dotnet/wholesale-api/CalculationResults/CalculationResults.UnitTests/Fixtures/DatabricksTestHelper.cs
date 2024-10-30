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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Fixtures;

public static class DatabricksTestHelper
{
    public static async IAsyncEnumerable<IDictionary<string, object?>> GetRowsAsync(TableChunk tableChunk, int rowCount)
    {
        await Task.Delay(0);
        for (var i = 0; i < rowCount; i++)
        {
            yield return GetRow(tableChunk, i);
        }
    }

    private static IDictionary<string, object?> GetRow(TableChunk tableChunk, int i = 0)
    {
        var dic = new Dictionary<string, object?>();
        foreach (var columnName in tableChunk.ColumnNames)
        {
            dic.Add(columnName, tableChunk[i, columnName]);
        }

        return dic;
    }
}
