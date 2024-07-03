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

using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;

public sealed class DatabricksSqlQueryExecutor
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, (PropertyInfo Property, TypeConverter Converter)>> _mapCache = new();

    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor;
    private readonly DatabricksSqlQueryCompiler _sqlQueryCompiler;

    public DatabricksSqlQueryExecutor(DbContext dbContext, DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor)
    {
        _sqlQueryCompiler = new DatabricksSqlQueryCompiler(dbContext);
        _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
    }

    public async Task<int> CountAsync(DatabricksSqlQueryable query, CancellationToken cancellationToken = default)
    {
        var databricksStatement = _sqlQueryCompiler.Compile(query);
        var countStatement = DatabricksStatement
            .FromRawSql($"SELECT COUNT(*) AS count FROM ({databricksStatement})")
            .Build();

        var rows = ExecuteStatementAsync<CountResult>(countStatement, cancellationToken).ConfigureAwait(false);

        await foreach (var row in rows)
            return row.Count;

        return 0;
    }

    public IAsyncEnumerable<TElement> ExecuteAsync<TElement>(DatabricksSqlQueryable query, CancellationToken cancellationToken = default)
    {
        var databricksStatement = _sqlQueryCompiler.Compile(query);
        return ExecuteStatementAsync<TElement>(databricksStatement, cancellationToken);
    }

    private async IAsyncEnumerable<TElement> ExecuteStatementAsync<TElement>(
        DatabricksStatement databricksStatement,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var rows = _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(databricksStatement, Format.JsonArray, cancellationToken)
            .ConfigureAwait(false);

        var propertyMap = _mapCache.GetOrAdd(typeof(TElement), CreatePropertyMap);

        await foreach (ExpandoObject row in rows)
        {
            yield return Hydrate<TElement>(row, propertyMap);
        }
    }

    private static TElement Hydrate<TElement>(ExpandoObject expandoObject, IReadOnlyDictionary<string, (PropertyInfo Property, TypeConverter Converter)> propertyMap)
    {
        var instance = Activator.CreateInstance<TElement>();

        foreach (var property in expandoObject)
        {
            if (propertyMap.TryGetValue(property.Key, out var prop))
            {
                var convertedValue = prop.Converter.ConvertFrom(null, CultureInfo.InvariantCulture, property.Value!);
                prop.Property.SetValue(instance, convertedValue);
            }
        }

        return instance;
    }

    private static IReadOnlyDictionary<string, (PropertyInfo Property, TypeConverter Converter)> CreatePropertyMap(Type targetType)
    {
        var propDict = new Dictionary<string, (PropertyInfo Property, TypeConverter Converter)>();

        foreach (var propertyInfo in targetType.GetProperties())
        {
            var typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);

            propDict.Add(propertyInfo.Name, (propertyInfo, typeConverter));

            var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute != null && !string.IsNullOrEmpty(columnAttribute.Name))
            {
                propDict.Add(columnAttribute.Name, (propertyInfo, typeConverter));
            }
        }

        return propDict;
    }

    private sealed class CountResult
    {
        [Column("count")]
        public int Count { get; set; }
    }
}
