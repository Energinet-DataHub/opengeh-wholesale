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

using Energinet.DataHub.Wholesale.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.IntegrationTests.TestCommon.WebApi;

/// <summary>
/// When we execute the tests on build agents we use the builded output (assemblies).
/// To avoid an 'System.IO.DirectoryNotFoundException' exception from WebApplicationFactory
/// during creation, we must set the path to the 'content root' using an environment variable
/// named 'ASPNETCORE_TEST_CONTENTROOT_ENERGINET_DATAHUB_WHOLESALE_WEBAPI'.
/// </summary>
public class WebApiFactory : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        builder.ConfigureServices(services =>
        {
            // This can be used for changing registrations in the container (e.g. for mocks).
            DisableAuthentication(services);
        });
    }

    private static void DisableAuthentication(IServiceCollection services)
    {
        services.Configure<AuthenticationOptions>(o => o.RequireAuthenticatedSignIn = false);
    }
}
