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
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Xunit.Abstractions;
using GridAreaCode = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models.GridAreaCode;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.SettlementReports;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class SettlementReportOrchestratorTests : IAsyncLifetime
{
    public SettlementReportOrchestratorTests(
        OrchestrationsAppFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);
    }

    private OrchestrationsAppFixture Fixture { get; }

    public Task InitializeAsync()
    {
        Fixture.EnsureAppHostUsesMockedDatabricksJobs();
        Fixture.AppHostManager.ClearHostLog();

        // Clear mappings etc. before each test
        Fixture.MockServer.Reset();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Fixture.SetTestOutputHelper(null!);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Verifies that:
    ///  - The orchestration can complete a full run.
    ///  - Every activity is executed once and in correct order.
    /// </summary>
    [Fact]
    public async Task MockExternalDependencies_WhenCallingDurableFunctionEndPoint_OrchestrationCompletesWithExpectedServiceBusMessage()
    {
        // Arrange
        var settlementReportRequest = new SettlementReportRequestDto(
            CalculationType.BalanceFixing,
            new SettlementReportRequestFilterDto(
                [new GridAreaCode("042")],
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null));

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/RequestSettlementReport");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(settlementReportRequest),
            Encoding.UTF8,
            "application/json");

        var token = CreateFakeInternalToken();
        request.Headers.Add("Authorization", $"Bearer {token}");

        var actualResponse = await Fixture.AppHostManager.HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actualResponse.StatusCode);

        var httpResponse = await actualResponse.Content.ReadFromJsonAsync<SettlementReportHttpResponse>();
        Assert.NotNull(httpResponse);

        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForInstanceCompletedAsync(
            httpResponse.RequestId.Id,
            TimeSpan.FromMinutes(10));

        Assert.Equal(OrchestrationRuntimeStatus.Completed, completeOrchestrationStatus.RuntimeStatus);
    }

    /// <summary>
    /// Create a fake token which is used by the 'UserMiddleware' to create
    /// the 'UserContext'.
    /// </summary>
    // TODO: Move this somewhere. Why does it work at all?
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
