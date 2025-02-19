﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;

/// <summary>
///     When we execute the tests on build agents we use the build output (assemblies).
///     To avoid an 'System.IO.DirectoryNotFoundException' exception from WebApplicationFactory
///     during creation, we must set the path to the 'content root' using an environment variable
///     named 'ASPNETCORE_TEST_CONTENTROOT_ENERGINET_DATAHUB_WHOLESALE_WEBAPI'.
/// </summary>
public class WebApiFactory : WebApplicationFactory<Program>
{
    private bool _authenticationEnabled;

    public Mock<ICalculationsClient>? CalculationsClientMock { get; set; }

    public Mock<IUserContext<FrontendUser>>? UserContextMock { get; }

    /// <summary>
    ///     Integration tests run without authentication, unless explicitly enabled.
    /// </summary>
    public void ReenableAuthentication()
    {
        _authenticationEnabled = true;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        // This can be used for changing registrations in the container (e.g. for mocks).
        builder.ConfigureServices(services =>
        {
            if (!_authenticationEnabled)
            {
                services.AddSingleton<IAuthorizationHandler>(new AllowAnonymous());
            }

            services.AddScoped(_ => CalculationsClientMock?.Object ?? new Mock<ICalculationsClient>().Object);

            var defaultUserContext = new Mock<IUserContext<FrontendUser>>();
            defaultUserContext.Setup(x => x.CurrentUser)
                .Returns(new FrontendUser(Guid.NewGuid(), false, new FrontendActor(Guid.NewGuid(), string.Empty, FrontendActorMarketRole.Other, [])));
            services.AddScoped(_ => UserContextMock?.Object ?? defaultUserContext.Object);
        });
    }

    private sealed class AllowAnonymous : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToList())
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
