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
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements;

[UnitTest]
public class DatabricksSqlResponseTests
{
    private readonly TableChunk _someTableChunk = new(new List<string> { "someColumn" }, new List<string[]> { new[] { "{someValue}" } });

    [Fact]
    public void CreateAsPending_ReturnsResponseWithExpectedProperties()
    {
        // Arrange
        var statementId = Guid.NewGuid();
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Pending;

        // Act
        var actual = DatabricksSqlResponse.CreateAsPending(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.HasMoreRows.Should().BeFalse();
        actual.NextChunkInternalLink.Should().BeNull();
        actual.State.Should().Be(expectedState);
        actual.Table.Should().BeNull();
    }

    [Theory]
    [InlineAutoMoqData(false, null!)]
    [InlineAutoMoqData(true, "some-link")]
    public void CreateAsSucceeded_ReturnsResponseWithExpectedProperties(bool hasMoreRows, string? nextChunkInternalLink, Guid statementId)
    {
        // Arrange
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Succeeded;

        // Act
        var actual = DatabricksSqlResponse.CreateAsSucceeded(statementId, _someTableChunk, nextChunkInternalLink);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.HasMoreRows.Should().Be(hasMoreRows);
        actual.NextChunkInternalLink.Should().Be(nextChunkInternalLink);
        actual.State.Should().Be(expectedState);
        actual.Table.Should().Be(_someTableChunk);
    }

    [Fact]
    public void CreateAsFailed_ReturnsResponseWithExpectedProperties()
    {
        // Arrange
        var statementId = Guid.NewGuid();
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Failed;

        // Act
        var actual = DatabricksSqlResponse.CreateAsFailed(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.HasMoreRows.Should().BeFalse();
        actual.NextChunkInternalLink.Should().BeNull();
        actual.State.Should().Be(expectedState);
        actual.Table.Should().BeNull();
    }

    [Fact]
    public void CreateAsCancelled_ReturnsResponseWithExpectedProperties()
    {
        // Arrange
        var statementId = Guid.NewGuid();
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Cancelled;

        // Act
        var actual = DatabricksSqlResponse.CreateAsCancelled(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.HasMoreRows.Should().BeFalse();
        actual.NextChunkInternalLink.Should().BeNull();
        actual.State.Should().Be(expectedState);
        actual.Table.Should().BeNull();
    }
}
