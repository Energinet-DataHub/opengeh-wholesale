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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Wholesale.WebApi.Extensions.Options;

namespace Energinet.DataHub.Wholesale.WebApi.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding authentication services to an ASP.NET Core app.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Register services necessary for enabling an ASP.NET Core app
    /// to use JWT bearer authentication and permission authorization.
    /// </summary>
    public static IServiceCollection AddTokenAuthenticationForWebApp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>().Bind(configuration);
        var options = configuration.Get<JwtOptions>()!;
        services.AddJwtBearerAuthentication(options.EXTERNAL_OPEN_ID_URL, options.INTERNAL_OPEN_ID_URL, options.BACKEND_BFF_APP_ID);

        return services;
    }

    /// <summary>
    /// Register services necessary for enabling an ASP.NET Core app
    /// user authentication.
    /// </summary>
    public static IServiceCollection AddUserAuthenticationForWebApp<TUser, TUserProvider>(this IServiceCollection services)
        where TUser : class
        where TUserProvider : class, IUserProvider<TUser>
    {
        services.AddUserAuthentication<TUser, TUserProvider>();

        return services;
    }
}
