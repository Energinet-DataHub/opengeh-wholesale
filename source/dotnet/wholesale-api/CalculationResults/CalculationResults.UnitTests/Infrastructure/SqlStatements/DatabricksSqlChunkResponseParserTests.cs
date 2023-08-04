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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements;

public class DatabricksSqlChunkResponseParserTests
{
    [Theory]
    [AutoMoqData]
    public void Parse_WhenJsonResponseIsNull_ThrowsArgumentNullException(DatabricksSqlChunkResponseParser sut)
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => sut.Parse(null!));
    }

    [Theory]
    [InlineAutoMoqData]
    public void Parse_WhenValidJsonResponse_ReturnsResponseWithExpectedExternalLink(DatabricksSqlChunkResponseParser sut)
    {
        // Arrange
        var jsonResponse = @"{""external_links"":[{""external_link"":""https://example.com"",""next_chunk_internal_link"":""https://internal.example.com""}]}";
        var expectedExternalLink = new Uri("https://example.com");

        // Act
        var result = sut.Parse(jsonResponse);

        // Assert
        result.ExternalLink.Should().Be(expectedExternalLink);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Parse_WhenValidJsonResponse_ReturnsResponseWithExpectedNextChunkInternalLink(DatabricksSqlChunkResponseParser sut)
    {
        // Arrange
        var jsonResponse = @"{""external_links"":[{""external_link"":""https://example.com"",""next_chunk_internal_link"":""https://internal.example.com""}]}";
        var expectedNextChunkInternalLink = "https://internal.example.com";

        // Act
        var result = sut.Parse(jsonResponse);

        // Assert
        result.NextChunkInternalLink.Should().Be(expectedNextChunkInternalLink);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Parse_WhenInvalidJsonResponse_ThrowsException(DatabricksSqlChunkResponseParser sut)
    {
        // Arrange
        var jsonResponse = "invalid json";

        // Act & Assert
        sut.Invoking(s => s.Parse(jsonResponse)).Should().Throw<Exception>();
    }

    [Theory]
    [InlineAutoMoqData]
    public void Parse_WhenMissingExternalLink_ReturnsNull(DatabricksSqlChunkResponseParser sut)
    {
        // Arrange
        var jsonResponse = @"{""external_links"":[{}]}";

        // Act
        var actual = sut.Parse(jsonResponse);

        // Assert
        actual.ExternalLink.Should().BeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public void Parse_WhenMissingNextChunkInternalLink_ReturnsNullLink(DatabricksSqlChunkResponseParser sut)
    {
        // Arrange
        var jsonResponse = @"{""external_links"":[{""external_link"":""https://example.com""}]}";

        // Act
        var result = sut.Parse(jsonResponse);

        // Assert
        result.NextChunkInternalLink.Should().BeNull();
    }
}
