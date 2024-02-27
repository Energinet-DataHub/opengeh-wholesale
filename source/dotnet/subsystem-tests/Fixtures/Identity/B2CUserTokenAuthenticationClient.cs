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

using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Identity;

/// <summary>
/// Encapsulates REST call to ROPC flow for retrieving a user access token.
///
/// See also "Test the ROPC flow": https://learn.microsoft.com/en-us/azure/active-directory-b2c/add-ropc-policy?#test-the-ropc-flow
/// </summary>
public sealed class B2CUserTokenAuthenticationClient : IDisposable
{
    /// <summary>
    /// Class to easily parse access token from JSON.
    /// </summary>
    private class AccessTokenResult
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
            = string.Empty;
    }

    public B2CUserTokenAuthenticationClient(B2CUserTokenConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        UserTokenPermissionAugmentation = new UserTokenPermissionAugmentation(configuration);
        AuthenticationHttpClient = new HttpClient();
    }

    private UserTokenPermissionAugmentation UserTokenPermissionAugmentation { get; }

    private B2CUserTokenConfiguration Configuration { get; }

    private HttpClient AuthenticationHttpClient { get; }

    public void Dispose()
    {
        UserTokenPermissionAugmentation.Dispose();
        AuthenticationHttpClient.Dispose();
    }

    /// <summary>
    /// Aquire an access token for the configured user.
    /// </summary>
    /// <returns>Access token.</returns>
    public async Task<string> AcquireAccessTokenAsync()
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(Configuration.Username), "username" },
            { new StringContent(Configuration.Password), "password" },
            { new StringContent("password"), "grant_type" },
            { new StringContent($"openid {Configuration.BackendBffScope} offline_access"), "scope" },
            { new StringContent(Configuration.FrontendAppId), "client_id" },
            { new StringContent("token id_token"), "response_type" },
        };

        var httpResponse = await AuthenticationHttpClient.PostAsync(Configuration.RopcUrl, form);
        httpResponse.EnsureSuccessStatusCode();

        var tokenResult = await httpResponse.Content.ReadFromJsonAsync<AccessTokenResult>();
        var externalAccessToken = tokenResult!.AccessToken;

        return await UserTokenPermissionAugmentation.AugmentAccessTokenWithPermissionsAsync(externalAccessToken);
    }
}
