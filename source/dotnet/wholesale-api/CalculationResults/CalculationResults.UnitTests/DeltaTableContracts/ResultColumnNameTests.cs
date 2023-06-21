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

using System.Reflection;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.DeltaTableContracts;

public class ResultColumnNameTests
{
    [Fact]
    public async Task ResultColumnNames_MatchesContract()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.result-table-column-names.json");
        var contractDescription = await ContractComplianceTestHelper.GetJsonObjectAsync(stream);
        var expectedColumnNames = new List<string>();
        var expectedColumnTypes = new List<string>();
        foreach (var expectedField in contractDescription.fields)
        {
            expectedColumnNames.Add((string)expectedField.name);
            expectedColumnTypes.Add((string)expectedField.type);
        }

        var fieldInfos = typeof(ResultColumnNames).GetFields(BindingFlags.Public | BindingFlags.Static);
        var actualColumnNames = fieldInfos.Select(x => x.GetValue(null)).Cast<string>().ToList();
        var actualColumnTypes = actualColumnNames.Select(ResultColumnNames.GetType).ToList();

        // Assert
        actualColumnNames.Should().BeEquivalentTo(expectedColumnNames);
        actualColumnTypes.Should().BeEquivalentTo(expectedColumnTypes);
    }
}
