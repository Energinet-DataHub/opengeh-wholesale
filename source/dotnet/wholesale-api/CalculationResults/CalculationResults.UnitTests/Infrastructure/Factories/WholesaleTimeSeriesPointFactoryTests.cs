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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.Factories;

public class WholesaleTimeSeriesPointFactoryTests
{
    private const string DefaultTime = "2022-05-16T01:00:00.000Z";
    private const string DefaultQuantityQualities = "[\"measured\", \"missing\"]";

    [Fact]
    public void Create_WhenNullableFieldsNull_ReturnsNullForThoseFields()
    {
        // Arrange
        var row = CreateDefaultSqlResultRow(DefaultTime, null, null, null, null);

        // Act
        var actual = WholesaleTimeSeriesPointFactory.Create(row);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Quantity.Should().BeNull();
        actual.Price.Should().BeNull();
        actual.Amount.Should().BeNull();
        actual.Qualities.Should().BeNull();
    }

    private static DatabricksSqlRow CreateDefaultSqlResultRow(string time, string? quantity, string? quantityQualities, string? price, string? amount)
    {
        return new DatabricksSqlRow(new Dictionary<string, object?>
        {
            { WholesaleResultColumnNames.Time, time },
            { WholesaleResultColumnNames.Quantity, quantity! },
            { WholesaleResultColumnNames.QuantityQualities, quantityQualities },
            { WholesaleResultColumnNames.Price, price! },
            { WholesaleResultColumnNames.Amount, amount! },
        });
    }
}
