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
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.DurableTask;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
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
    ///  - A report file is generated.
    /// </summary>
    [Fact]
    public async Task MockExternalDependencies_WhenCallingDurableFunctionEndPoint_OrchestrationCompletesWithReportPresent()
    {
        // Arrange
        var settlementReportRequest = new SettlementReportRequestDto(
            CalculationType.BalanceFixing,
            new SettlementReportRequestFilterDto(
                [new GridAreaCode("042")],
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null));

        // => Databricks Jobs API
        var jobId = Random.Shared.Next(1, 1000);
        var runId = Random.Shared.Next(1000, 2000);

        Fixture.MockServer
            .MockJobsList(jobId)
            .MockJobsGet(jobId)
            .MockJobsRunNow(runId)
            .MockJobsRunsGet(runId, "TERMINATED", "SUCCESS");

        // => Databricks SQL Statement API
        var chunkIndex = 0;
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        // This is the calculationId returned in the energyResult from the mocked databricks.
        // It should match the ID returned by the http client calling 'api/StartCalculation'
        // But we have to set up the mocked response before we reach this step, hence we have a mismatch.
        var calculationIdInMock = Guid.NewGuid();

        Fixture.MockServer
            .MockEnergySqlStatements(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockEnergySqlStatementsResultStream(path, calculationIdInMock);

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
            TimeSpan.FromMinutes(3));

        Assert.Equal(OrchestrationRuntimeStatus.Completed, completeOrchestrationStatus.RuntimeStatus);

        var isReportPresent = await IsReportPresentAsync(httpResponse.RequestId);
        Assert.True(isReportPresent);
    }

    private async Task<bool> IsReportPresentAsync(SettlementReportRequestId id)
    {
        // Read from Fixture, unzip and return contents
        var containerClient = Fixture.CreateBlobContainerClient();
        var reportBlobClient = containerClient.GetBlobClient($"settlement-reports/{id.Id}/Report.zip");

        var reportStream = await reportBlobClient.DownloadStreamingAsync();
        await using (reportStream.Value.Content)
        {
            using var archive = new ZipArchive(reportStream.Value.Content, ZipArchiveMode.Read);
            return archive.Entries.Count == 1;
        }
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
            [userClaim, actorClaim],
            validFrom,
            validTo,
            new SigningCredentials(testKey, SecurityAlgorithms.RsaSha256));

        var handler = new JwtSecurityTokenHandler();
        var writtenToken = handler.WriteToken(internalToken);
        return writtenToken;
    }
}
