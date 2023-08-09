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

using System.Net;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements;

public sealed class SqlStatementClientBuilder
{
    private readonly List<HttpResponseMessage> _responseMessages = new();
    private IDatabricksSqlResponseParser? _parser;

    public SqlStatementClientBuilder AddHttpClientResponse(string content)
    {
        _responseMessages.Add(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content), });
        return this;
    }

    public SqlStatementClientBuilder UseParser(IDatabricksSqlResponseParser parser)
    {
        _parser = parser;
        return this;
    }

    public SqlStatementClient Build()
    {
        var handlerMock = new HttpMessageHandlerMock(_responseMessages);
        var client = new HttpClient(handlerMock);
        var options = new Mock<IOptions<DatabricksOptions>>();
        options.Setup(o => o.Value).Returns(new DatabricksOptions
        {
            DATABRICKS_WORKSPACE_URL = "https://foo.com",
        });
        var parser = _parser ?? new Mock<IDatabricksSqlResponseParser>().Object;
        var logger = new Mock<ILogger<SqlStatementClient>>();
        return new SqlStatementClient(client, options.Object, parser, logger.Object);
    }

    private class HttpMessageHandlerMock : HttpMessageHandler
    {
        private readonly List<HttpResponseMessage> _messages;
        private int _index;

        public HttpMessageHandlerMock(List<HttpResponseMessage> messages)
        {
            _index = 0;
            _messages = messages;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_messages[_index++]);
        }
    }
}
