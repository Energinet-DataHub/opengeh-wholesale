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

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Identity;

/// <summary>
/// Encapsulates REST call to augment an external token with permissions.
/// </summary>
public sealed class UserTokenPermissionAugmentation : IDisposable
{
    private readonly HttpClient _tokenHttpClient = new();

    public UserTokenPermissionAugmentation(B2CUserTokenConfiguration configuration)
    {
        _tokenHttpClient.BaseAddress = new Uri(configuration.TokenBaseAddress);
    }

    public void Dispose()
    {
        _tokenHttpClient.Dispose();
    }

    /// <summary>
    /// Augments the specified token with permissions.
    /// </summary>
    /// <returns>The augmented access token.</returns>
    public async Task<string> AugmentAccessTokenWithPermissionsAsync(string externalAccessToken)
    {
        var actorId = await GetActorAsync(externalAccessToken);
        return await AugmentTokenAsync(externalAccessToken, actorId);
    }

    private async Task<Guid> GetActorAsync(string externalToken)
    {
        using var response = await _tokenHttpClient.GetAsync($"user/actors?externalToken={externalToken}");
        response.EnsureSuccessStatusCode();

        var actors = await response.Content.ReadFromJsonAsync<GetAssociatedUserActorsResponseDto>();
        var chosenActor = actors?.ActorIds.FirstOrDefault();
        return chosenActor == null
            ? throw new InvalidOperationException("The user requested for the subsystem test does not have actors assigned.")
            : chosenActor.Value;
    }

    private async Task<string> AugmentTokenAsync(string externalToken, Guid actorId)
    {
        var request = new GetTokenRequestDto(actorId, externalToken);
        using var response = await _tokenHttpClient.PostAsJsonAsync("token", request);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<GetTokenResponseDto>();
        return token!.Token;
    }

    private sealed record GetAssociatedUserActorsResponseDto(IEnumerable<Guid> ActorIds);

    private sealed record GetTokenRequestDto(Guid ActorId, string ExternalToken);

    private sealed record GetTokenResponseDto(string Token);
}
