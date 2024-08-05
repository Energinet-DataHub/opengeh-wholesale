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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

/// <summary>
/// This class is used to wrap the result (a dynamic type) of a Databricks SQL query.
/// </summary>
public class DatabricksSqlRow
{
    private readonly IDictionary<string, object?> _dictionary;

    public DatabricksSqlRow(IDictionary<string, object?> dictionary)
    {
        _dictionary = dictionary;
    }

    public string? this[string key]
    {
        get
        {
            var value = _dictionary[key];
            return value == null ? null : Convert.ToString(value);
        }
    }

    public bool HasColumn(string columnName)
    {
        return _dictionary.ContainsKey(columnName);
    }

    public override string ToString()
    {
        return _dictionary.Aggregate(string.Empty, (current, kvp) => current + $"Key = {kvp.Key}, Value = {kvp.Value}");
    }
}
