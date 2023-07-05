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
    private readonly string[] _columnNames = { "someColumn" };
    private readonly DatabricksSqlChunkResponse _chunkResponse = new(new Uri("https://foo.com"), "bar");

    [Theory]
    [AutoMoqData]
    public void CreateAsPending_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = DatabricksSqlResponse.CreateAsPending(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(DatabricksSqlResponseState.Pending);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsRunning_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = DatabricksSqlResponse.CreateAsRunning(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(DatabricksSqlResponseState.Running);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsSucceeded_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = DatabricksSqlResponse.CreateAsSucceeded(statementId, _columnNames, _chunkResponse);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeEquivalentTo(_columnNames);
        actual.Chunk.Should().BeEquivalentTo(_chunkResponse);
        actual.State.Should().Be(DatabricksSqlResponseState.Succeeded);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsFailed_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = DatabricksSqlResponse.CreateAsFailed(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(DatabricksSqlResponseState.Failed);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsCanceled_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = DatabricksSqlResponse.CreateAsCancelled(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(DatabricksSqlResponseState.Cancelled);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsClosed_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = DatabricksSqlResponse.CreateAsClosed(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(DatabricksSqlResponseState.Closed);
    }
}
