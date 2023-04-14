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

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Factory to create an <see cref="T:System.Net.Http.HttpClient" />, which will re-apply the authorization header
    /// from the current HTTP context.
    /// </summary>
    public class AuthorizedHttpClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Func<string> _authorizationHeaderProvider;

        public AuthorizedHttpClientFactory(
            IHttpClientFactory httpClientFactory,
            Func<string> authorizationHeaderProvider)
        {
            _httpClientFactory = httpClientFactory;
            _authorizationHeaderProvider = authorizationHeaderProvider;
        }

        public HttpClient CreateClient(Uri baseUrl)
        {
            var client = _httpClientFactory.CreateClient();
            SetAuthorizationHeader(client);
            client.BaseAddress = baseUrl;
            return client;
        }

        private void SetAuthorizationHeader(HttpClient httpClient)
        {
            var str = _authorizationHeaderProvider();
            httpClient.DefaultRequestHeaders.Add("Authorization", str);
        }
    }
}
