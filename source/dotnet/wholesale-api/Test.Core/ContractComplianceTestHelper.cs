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

using FluentAssertions;
using Newtonsoft.Json;

namespace Test.Core;

public static class ContractComplianceTestHelper
{
    public static async Task<dynamic> GetJsonObjectAsync(Stream contractStream)
    {
        using var streamReader = new StreamReader(contractStream);
        var contractJson = await streamReader.ReadToEndAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<dynamic>(contractJson)!;
    }

    public static async Task<string> GetRequiredMessageTypeAsync(Stream contractStream)
    {
        var contractDescription = await GetJsonObjectAsync(contractStream).ConfigureAwait(false);

        foreach (var fieldDescriptor in contractDescription.fields)
        {
            if (fieldDescriptor.name == "MessageType")
                return fieldDescriptor.value;
        }

        throw new InvalidOperationException("Could not find required MessageType in contract.");
    }

    public static async Task VerifyEnumCompliesWithContractAsync<T>(Stream contractStream)
        where T : struct, Enum
    {
        var contractDescription = await GetJsonObjectAsync(contractStream).ConfigureAwait(false);

        var expectedLiterals = contractDescription.literals;
        var actualNames = Enum.GetNames<T>();

        // Assert: Number of literals must match.
        actualNames.Length.Should().Be(expectedLiterals.Count);

        foreach (var expectedLiteral in expectedLiterals)
        {
            // Ignore underscores in names and values
            var expectedName = ((string)expectedLiteral.name).Replace("_", string.Empty);
            var expectedValue = Enum.Parse<T>(((string)expectedLiteral.value).Replace("_", string.Empty), true);

            // Assert: Lookup literal by name
            var actualLiteral = Enum.Parse<T>(expectedName, true);

            // Assert: Value of literal match
            actualLiteral.Should().Be(expectedValue);
        }
    }

    public static async Task VerifyTypeCompliesWithContractAsync<T>(Stream contractStream)
    {
        var contractDescription = await GetJsonObjectAsync(contractStream).ConfigureAwait(false);

        var expectedProps = contractDescription.fields;
        var actualProps = typeof(T)
            .GetProperties()
            .ToDictionary(info => info.Name);

        // Assert: Number of props match
        actualProps.Count.Should().Be(expectedProps.Count);

        foreach (var expectedProp in expectedProps)
        {
            string expectedPropName = expectedProp.name;
            string expectedPropType = expectedProp.type;

            // Assert: Lookup property by name
            var actualProp = actualProps[expectedPropName];

            // Assert: Property types match
            var actualPropertyType = MapToContractType(actualProp.PropertyType);
            actualPropertyType.Should().Contain(expectedPropType);
        }
    }

    public static async Task<List<string>> GetCodeListValuesAsync(Stream contractStream)
    {
        var contractDescription = await GetJsonObjectAsync(contractStream).ConfigureAwait(false);

        var values = new List<string>();
        foreach (var expectedLiteral in contractDescription.literals)
        {
            values.Add((string)expectedLiteral.value);
        }

        return values;
    }

    private static string[] MapToContractType(Type propertyType)
    {
        return propertyType.IsEnum
            ? MapToContractType(Enum.GetUnderlyingType(propertyType))
            : Nullable.GetUnderlyingType(propertyType) is { } underlyingType
            ? MapToContractType(underlyingType)
            : propertyType.Name switch
            {
                "Int32" => new[] { "integer", "long" },
                "String" => new[] { "string" },
                "Guid" => new[] { "string" },
                "Instant" => new[] { "timestamp" },
                _ => throw new NotImplementedException($"Property type '{propertyType.Name}' not implemented."),
            };
    }
}
