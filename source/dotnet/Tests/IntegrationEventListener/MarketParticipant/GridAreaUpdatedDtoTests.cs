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

using Energinet.DataHub.Wholesale.IntegrationEventListener.MarketParticipant;
using Energinet.DataHub.Wholesale.Tests.TestHelpers;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener.MarketParticipant;

public class GridAreaUpdatedDtoTests
{
    [Fact]
    public async Task PropertyNamesAndTypesMatchContractWithCalculator()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("IntegrationEventListener.MarketParticipant.grid-area-updated.json");
        using var streamReader = new StreamReader(stream);
        var inputGridAreaUpdated = await streamReader.ReadToEndAsync();
        var gridAreaUpdatedDescription = JsonConvert.DeserializeObject<dynamic>(inputGridAreaUpdated)!;

        var actualProps = typeof(GridAreaUpdatedDto).GetProperties().ToDictionary(info => info.Name);
        var expectedProps = gridAreaUpdatedDescription.bodyFields;

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
            actualPropertyType.Should().Be(expectedPropType);
        }
    }

    private static string MapToContractType(Type propertyType) => propertyType.Name switch
    {
        "String" => "string",
        "Guid" => "guid",
        "Instant" => "timestamp",
        _ => throw new NotImplementedException($"Property type '{propertyType.Name}' not implemented."),
    };
}
