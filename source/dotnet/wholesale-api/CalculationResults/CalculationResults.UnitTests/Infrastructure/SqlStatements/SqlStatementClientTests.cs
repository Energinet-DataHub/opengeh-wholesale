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
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements;

public class SqlStatementClientTests
{
    private const string Running = @"
{
  ""statement_id"": ""01ed9db9-24c4-1cb6-a320-fb6ebbe7410d"",
  ""status"": {
    ""state"": ""RUNNING""
  }
}";

    private const string Closed = @"
{
    ""statement_id"": ""01ee34ba-ae2f-1ae5-92ba-ff597e3f57fa"",
    ""status"": {
        ""state"": ""CLOSED""
    }
}
";

    private const string Error = @"
{
    ""error_code"": ""NOT_FOUND"",
    ""message"": ""The statement 01ee338a-2b5c-1287-8f90-ed2274193b76 was not found."",
    ""details"": [
        {
            ""@type"": ""type.googleapis.com/google.rpc.ResourceInfo"",
            ""resource_type"": ""statement"",
            ""resource_name"": ""01ee338a-2b5c-1287-8f90-ed2274193b76"",
            ""owner"": """",
            ""description"": ""does not exist""
        }
    ]
}
";

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenStatementIsClosed_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<IDatabricksSqlResponseParser> parserMock,
        SqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(Running))
            .Returns(DatabricksSqlResponse.CreateAsRunning(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(Closed))
            .Returns(DatabricksSqlResponse.CreateAsClosed(statementId));
        var sut = builder
            .AddHttpClientResponse(Running)
            .AddHttpClientResponse(Closed)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }
}
