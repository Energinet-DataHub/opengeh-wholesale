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
using System.Text;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Newtonsoft.Json;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;

public static class AppHostManagerExtensions
{
    public static async Task<Guid> StartCalculationAsync(this FunctionAppHostManager appHostManager, string authenticationHeaderValue)
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

        request.Headers.Add("Authorization", $"{authenticationHeaderValue}");

        using var startCalculationResponse = await appHostManager.HttpClient.SendAsync(request);
        startCalculationResponse.EnsureSuccessStatusCode();

        return await startCalculationResponse.Content.ReadFromJsonAsync<Guid>();
    }
}
