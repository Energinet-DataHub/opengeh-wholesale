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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient;

[UnitTest]
public class DatabricksSqlResponseParserTests
{
    private readonly string _sampleJson;

    public DatabricksSqlResponseParserTests()
    {
        var stream = EmbeddedResources.GetStream("Infrastructure.CalculationResultClient.CalculationResult.json");
        using var reader = new StreamReader(stream);
        _sampleJson = reader.ReadToEnd();
    }

    [Theory]
    [AutoMoqData]
    public void Parse_ReturnsResponseWithExpectedState(DatabricksSqlResponseParser sut)
    {
        // Arrange
        const string expectedState = "SUCCEEDED";

        // Act
        var actual = sut.Parse(_sampleJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_ReturnsResponseWithExpectedArrayLength(DatabricksSqlResponseParser sut)
    {
        // Arrange
        const int expectedLength = 96;

        // Act
        var actual = sut.Parse(_sampleJson);

        // Assert
        actual.Table.RowCount.Should().Be(expectedLength);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_ReturnsDataArrayWithExpectedContent(DatabricksSqlResponseParser sut)
    {
        // Arrange
        var expectedFirstArray = new[]
        {
            "543", null, null, "0.000", "missing", "2023-04-04T22:00:00.000Z", "total_ga", "net_exchange_per_ga", "0ff76fd9-7d07-48f0-9752-e94d38d93498", "BalanceFixing", "2023-04-05T08:47:41.000Z", null,
        };
        var expectedLastArray = new[]
        {
            "543", null, null, "1.235", "estimated", "2023-04-05T21:45:00.000Z", "total_ga", "net_exchange_per_ga", "0ff76fd9-7d07-48f0-9752-e94d38d93498", "BalanceFixing", "2023-04-05T08:47:41.000Z", null,
        };

        // Act
        var actual = sut.Parse(_sampleJson);

        // Assert
        actual.Table[0].Should().Equal(expectedFirstArray);
        actual.Table[^1].Should().Equal(expectedLastArray);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenValidJson_ReturnsResult(DatabricksSqlResponseParser sut)
    {
        // Arrange
        var status = new JProperty("status", new JObject(new JProperty("state", "PENDING")));
        var manifest = new JProperty("manifest", new JObject(new JProperty("schema", new JObject(new JProperty("columns", new JArray(new JObject(new JProperty("name", "grid_area"))))))));
        var result = new JProperty("result", new JObject(new JProperty("data_array", new List<string[]>())));
        var obj = new JObject(status, manifest, result);
        var jsonString = obj.ToString();

        // Act + Assert
        sut.Parse(jsonString).Should().NotBeNull();
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenInvalidJson_ThrowsException(DatabricksSqlResponseParser sut)
    {
        // Arrange
        var status = new JProperty("not_status", new JObject(new JProperty("state", "PENDING")));
        var manifest = new JProperty("manifest", new JObject(new JProperty("schema", new JObject(new JProperty("columns", new JArray(new JObject(new JProperty("name", "grid_area"))))))));
        var result = new JProperty("result", new JObject(new JProperty("data_array", new List<string[]>())));
        var obj = new JObject(status, manifest, result);
        var jsonString = obj.ToString();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => sut.Parse(jsonString));
    }
}
