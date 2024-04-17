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
using Microsoft.Net.Http.Headers;
using WireMock.Server;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;

public class WiremockFixture
{
    public WireMockServer Server { get; }

    public WiremockFixture(string[] urls)
    {
        Server = WireMockServer.Start(urls);

        Server.Given(WireMock.RequestBuilders.Request.Create().WithPath("/*").UsingAnyMethod())
            .AtPriority(1000)
            .RespondWith(
                WireMock.ResponseBuilders.Response.Create()
                    .WithStatusCode(HttpStatusCode.NotImplemented)
                    .WithHeader(HeaderNames.ContentType, "application/text")
                    .WithBody("Request not mapped!"));
    }
}
