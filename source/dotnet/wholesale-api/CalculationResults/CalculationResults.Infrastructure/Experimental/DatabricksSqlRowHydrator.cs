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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Experimental;

public sealed class DatabricksSqlRowHydrator
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, (PropertyInfo Property, TypeConverter Converter)>> _typeInfoCache = new();

    public async IAsyncEnumerable<TElement> HydrateAsync<TElement>(IAsyncEnumerable<dynamic> rows, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var propertyMap = _typeInfoCache.GetOrAdd(typeof(TElement), CreateTypeInfoCache);

        await foreach (ExpandoObject row in rows.ConfigureAwait(false).WithCancellation(cancellationToken))
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
                if (property.Value != null)
                {
                    var convertedValue = prop.Converter.ConvertFrom(null, CultureInfo.InvariantCulture, property.Value);
                    prop.Property.SetValue(instance, convertedValue);
                }
            }
        }

        return instance;
    }

    private static IReadOnlyDictionary<string, (PropertyInfo Property, TypeConverter Converter)> CreateTypeInfoCache(Type targetType)
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
}
