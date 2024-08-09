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

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions;

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods

/// <summary>
/// A mocked token function used to test authentication and authorization extensions and middleware.
///
/// This function is called when we from tests:
///  * Retrieve an "internal token".
///  * Validates the "internal token".
///
/// </summary>
[AllowAnonymous]
public class MockedTokenFunction
{
    private const string Kid = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F";
    private const string Issuer = "https://test.datahub.dk";

    private const string TokenClaim = "token";
    private const string RoleClaim = "role";

    private static readonly RsaSecurityKey _testKey = new(RSA.Create()) { KeyId = Kid };

    [Function(nameof(GetConfiguration))]
    public IActionResult GetConfiguration(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "v2.0/.well-known/openid-configuration")]
        HttpRequest httpRequest)
    {
        var configuration = new
        {
            issuer = Issuer,
            jwks_uri = $"http://{httpRequest.Host}/api/discovery/v2.0/keys",
        };

        return new OkObjectResult(configuration);
    }

    [Function(nameof(GetPublicKeys))]
    public IActionResult GetPublicKeys(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "discovery/v2.0/keys")]
        HttpRequest httpRequest)
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_testKey);

        var keys = new
        {
            keys = new[]
            {
                new
                {
                    kid = jwk.Kid,
                    kty = jwk.Kty,
                    n = jwk.N,
                    e = jwk.E,
                },
            },
        };

        return new OkObjectResult(keys);
    }

    [Function(nameof(GetToken))]
    public async Task<IActionResult> GetToken(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "token")]
        HttpRequest httpRequest)
    {
        using var bodyStreamReader = new StreamReader(httpRequest.Body);
        var jsonString = await bodyStreamReader.ReadToEndAsync().ConfigureAwait(false);
        // Parsing to JSON DOM because we don't want to share a type between projects
        var bodyAsNode = JsonNode.Parse(jsonString)!;

        // Parse external token
        var externalTokenString = bodyAsNode["ExternalToken"]!.ToString();
        var externalJwt = new JwtSecurityToken(externalTokenString);

        var claims = new List<Claim>
        {
            new(TokenClaim, externalTokenString),
            new(JwtRegisteredClaimNames.Sub, "A1AAB954-136A-444A-94BD-E4B615CA4A78"),
            new(JwtRegisteredClaimNames.Azp, "A1DEA55A-3507-4777-8CF3-F425A6EC2094"),

            new Claim("actornumber", "0000000000000"), // TODO: We need to set these so "FrontendUserProvider" can create a "FrontendUser", but is that OK?
            new Claim("marketroles", "EnergySupplier"), // TODO: We need to set these so "FrontendUserProvider" can create a "FrontendUser", but is that OK?
        };

        // Parse roles and add as claims
        var roles = bodyAsNode["Roles"]!.ToString();
        if (!string.IsNullOrWhiteSpace(roles))
        {
            foreach (var role in roles.Split(','))
            {
                claims.Add(new Claim(RoleClaim, role));
            }
        }

        var internalToken = new JwtSecurityToken(
            issuer: Issuer,
            audience: externalJwt.Audiences.Single(),
            claims: claims,
            notBefore: externalJwt.ValidFrom,
            expires: externalJwt.ValidTo,
            signingCredentials: new SigningCredentials(_testKey, SecurityAlgorithms.RsaSha256));

        var writtenToken = new JwtSecurityTokenHandler().WriteToken(internalToken);

        return new OkObjectResult(writtenToken);
    }
}
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
