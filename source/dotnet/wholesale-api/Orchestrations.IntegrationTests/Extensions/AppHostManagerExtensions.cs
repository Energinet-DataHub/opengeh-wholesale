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
using System.Text;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

public static class AppHostManagerExtensions
{
    public static Task<HttpResponseMessage> StartCalculationAsync(this FunctionAppHostManager appHostManager)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/StartCalculation");

        var dateTimeZone = DateTimeZoneProviders.Tzdb["Europe/Copenhagen"];
        var dateAtMidnight = new LocalDate(2024, 5, 17)
            .AtMidnight()
            .InZoneStrictly(dateTimeZone)
            .ToDateTimeOffset();

        // Input parameters
        var requestDto = new StartCalculationRequestDto(
            CalculationType.Aggregation,
            GridAreaCodes: ["256", "512"],
            StartDate: dateAtMidnight,
            EndDate: dateAtMidnight.AddDays(2));

        request.Content = new StringContent(
            JsonConvert.SerializeObject(requestDto),
            Encoding.UTF8,
            "application/json");

        var token = CreateFakeInternalToken();
        request.Headers.Add("Authorization", $"Bearer {token}");

        return appHostManager.HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Create a fake token which is used by the 'UserMiddleware' to create
    /// the 'UserContext'.
    /// </summary>
    private static string CreateFakeInternalToken()
    {
        var kid = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F";
        RsaSecurityKey testKey = new(RSA.Create()) { KeyId = kid };

        var issuer = "https://test.datahub.dk";
        var audience = Guid.Empty.ToString();
        var validFrom = DateTime.UtcNow;
        var validTo = DateTime.UtcNow.AddMinutes(15);

        var userClaim = new Claim(JwtRegisteredClaimNames.Sub, "A1AAB954-136A-444A-94BD-E4B615CA4A78");
        var actorClaim = new Claim(JwtRegisteredClaimNames.Azp, "A1DEA55A-3507-4777-8CF3-F425A6EC2094");

        var internalToken = new JwtSecurityToken(
            issuer,
            audience,
            new[] { userClaim, actorClaim },
            validFrom,
            validTo,
            new SigningCredentials(testKey, SecurityAlgorithms.RsaSha256));

        var handler = new JwtSecurityTokenHandler();
        var writtenToken = handler.WriteToken(internalToken);
        return writtenToken;
    }
}
