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

using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Wholesale.Client;

public static class Registration
{
    /// <summary>
    /// Add dependency injection registrations required in order to resolve an <see cref="IWholesaleClient"/>.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="wholesaleBaseUri">The base uri of the wholesale domain backend API.</param>
    /// <param name="authorizationHeaderProvider">
    /// Provider that can provide the HTTP authorization header value
    /// to enable access to the wholesale domain backend services.
    /// </param>
    public static IServiceCollection AddWholesaleClient(
        this IServiceCollection serviceCollection,
        Uri wholesaleBaseUri,
        Func<IServiceProvider, string> authorizationHeaderProvider)
    {
        int i = 10;
        if (serviceCollection.All(x => x.ServiceType != typeof(IHttpClientFactory)))
            serviceCollection.AddHttpClient();

        serviceCollection.AddSingleton(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new AuthorizedHttpClientFactory(factory, () => authorizationHeaderProvider(provider));
        });

        serviceCollection.AddScoped<IWholesaleClient, WholesaleClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<AuthorizedHttpClientFactory>();
            return new WholesaleClient(httpClientFactory, wholesaleBaseUri);
        });

        return serviceCollection;
    }
}
