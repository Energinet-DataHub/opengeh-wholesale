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

using Energinet.DataHub.Wholesale.SubsystemTests.Clients.v3;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Identity;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;

public static class WholesaleClientFactory
{
    public static async Task<WholesaleClient_V3> CreateWebApiClientAsync(WholesaleSubsystemConfiguration configuration, bool useAuthentication)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = configuration.WebApiBaseAddress,
        };

        if (useAuthentication)
        {
            using var userAuthenticationClient = new B2CUserTokenAuthenticationClient(configuration.UserTokenConfiguration);
            var accessToken = await userAuthenticationClient.AcquireAccessTokenAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        }

        return new WholesaleClient_V3(
            configuration.WebApiBaseAddress.ToString(),
            httpClient);
    }

    public static async Task<HttpClient> CreateOrchestrationsApiClientAsync(WholesaleSubsystemConfiguration configuration, bool useAuthentication)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = configuration.OrchestrationsApiBaseAddress,
        };

        if (useAuthentication)
        {
            using var userAuthenticationClient = new B2CUserTokenAuthenticationClient(configuration.UserTokenConfiguration);
            var accessToken = await userAuthenticationClient.AcquireAccessTokenAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        }

        return httpClient;
    }
}
