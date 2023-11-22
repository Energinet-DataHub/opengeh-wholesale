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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests;

[Collection(nameof(CompositionRootTests))]
public class CompositionRootTests
{
    /// <summary>
    /// The test is composed from the ideas and examples at
    /// https://stackoverflow.com/questions/49149065/how-do-i-validate-the-di-container-in-asp-net-core
    ///
    /// However it is not guranteed to find all issues, as can be learned by reading this article:
    /// https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/#detecting-unregistered-dependencies-on-startup
    ///
    /// To summarize it won't find:
    ///  - [FromServices] injected dependencies
    ///  - Service sourced directly from IServiceProvider
    ///  - Services registered using factory functions
    ///  - Open generic types
    /// </summary>
    [Fact]
    public void AllServicesConstructSuccessfully()
    {
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseDefaultServiceProvider((_, options) =>
                    {
                        // See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-7.0#scope-validation
                        options.ValidateScopes = true;
                        // Validate the service provider during build
                        options.ValidateOnBuild = true;
                    })
                    // Add controllers as services to enable validation of controller dependencies
                    // See https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/#1-controller-constructor-dependencies-aren-t-checked
                    .ConfigureServices(collection => collection.AddControllers().AddControllersAsServices())
                    .UseStartup<Startup>();
            }).Build();
    }
}
