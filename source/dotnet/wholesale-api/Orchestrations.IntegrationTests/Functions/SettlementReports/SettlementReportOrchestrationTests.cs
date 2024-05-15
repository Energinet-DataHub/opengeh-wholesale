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
public class SettlementReportOrchestrationTests : IAsyncLifetime
{
    public SettlementReportOrchestrationTests(
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
    public async Task MockExternalDependencies_WhenCallingDurableFunctionEndpoint_OrchestrationCompletesWithReportPresent()
    {
        // Arrange
        var settlementReportRequest = new SettlementReportRequestDto(
            CalculationType.BalanceFixing,
            false,
            new SettlementReportRequestFilterDto(
                [new GridAreaCode("042")],
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null));

        // => Databricks SQL Statement API
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        Fixture.MockServer
            .MockEnergySqlStatements(statementId, 0)
            .MockEnergySqlStatementsResultChunks(statementId, 0, path)
            .MockEnergySqlStatementsResultStream(path);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/RequestSettlementReport");
        request.Headers.Add("Authorization", $"Bearer {CreateFakeInternalToken()}");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(settlementReportRequest),
            Encoding.UTF8,
            "application/json");

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

    /// <summary>
    /// Verifies that:
    ///  - The orchestration can complete a full run.
    ///  - Every activity is executed once and in correct order.
    ///  - A report file is generated.
    ///  - The file can be downloaded from the download endpoint
    /// </summary>
    [Fact]
    public async Task MockExternalDependencies_WhenCallingDurableFunctionEndpoint_OrchestrationCompletesWithReportPresent_CanDownload()
    {
        // Arrange
        var settlementReportRequest = new SettlementReportRequestDto(
            CalculationType.BalanceFixing,
            false,
            new SettlementReportRequestFilterDto(
                [new GridAreaCode("042")],
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null));

        // => Databricks SQL Statement API
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        Fixture.MockServer
            .MockEnergySqlStatements(statementId, 0)
            .MockEnergySqlStatementsResultChunks(statementId, 0, path)
            .MockEnergySqlStatementsResultStream(path);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/RequestSettlementReport");
        request.Headers.Add("Authorization", $"Bearer {CreateFakeInternalToken()}");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(settlementReportRequest),
            Encoding.UTF8,
            "application/json");

        var actualResponse = await Fixture.AppHostManager.HttpClient.SendAsync(request);
        var httpResponse = await actualResponse.Content.ReadFromJsonAsync<SettlementReportHttpResponse>();
        var completeOrchestrationStatus = await Fixture.DurableClient.WaitForInstanceCompletedAsync(
            httpResponse!.RequestId.Id,
            TimeSpan.FromMinutes(3));

        using var downloadRequest = new HttpRequestMessage(HttpMethod.Post, "api/SettlementReportDownload");
        downloadRequest.Headers.Add("Authorization", $"Bearer {CreateFakeInternalToken()}");
        downloadRequest.Content = new StringContent(
            JsonConvert.SerializeObject(httpResponse!.RequestId),
            Encoding.UTF8,
            "application/json");

        var actualDownloadResponse = await Fixture.AppHostManager.HttpClient.SendAsync(downloadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actualDownloadResponse.StatusCode);
        Assert.NotNull(actualResponse.Content);

        using var archive = new ZipArchive(await actualResponse.Content.ReadAsStreamAsync(), ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }

    /// <summary>
    /// Verifies that listing the reports returns a correct status.
    /// </summary>
    [Fact]
    public async Task MockExternalDependencies_WhenCallingListReportsFunctionEndpoint_StatusIsValid()
    {
        // Arrange
        var settlementReportRequest = new SettlementReportRequestDto(
            CalculationType.BalanceFixing,
            false,
            new SettlementReportRequestFilterDto(
                [new GridAreaCode("042")],
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null));

        // => Databricks SQL Statement API
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        Fixture.MockServer
            .MockEnergySqlStatements(statementId, 0)
            .MockEnergySqlStatementsResultChunks(statementId, 0, path)
            .MockEnergySqlStatementsResultStream(path);

        // Act A: Start generating report.
        using var requestReport = new HttpRequestMessage(HttpMethod.Post, "api/RequestSettlementReport");
        requestReport.Headers.Add("Authorization", $"Bearer {CreateFakeInternalToken()}");
        requestReport.Content = new StringContent(
            JsonConvert.SerializeObject(settlementReportRequest),
            Encoding.UTF8,
            "application/json");

        var reportResponse = await Fixture.AppHostManager.HttpClient.SendAsync(requestReport);
        Assert.Equal(HttpStatusCode.OK, reportResponse.StatusCode);

        var reportHttpResponse = await reportResponse.Content.ReadFromJsonAsync<SettlementReportHttpResponse>();
        Assert.NotNull(reportHttpResponse);

        // Act B: Request report status.
        using var requestList = new HttpRequestMessage(HttpMethod.Get, "api/ListSettlementReports");
        requestList.Headers.Add("Authorization", $"Bearer {CreateFakeInternalToken()}");

        var listResponse = await Fixture.AppHostManager.HttpClient.SendAsync(requestList);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // Assert
        var listHttpResponse = await listResponse.Content.ReadFromJsonAsync<RequestedSettlementReportDto[]>();
        Assert.NotNull(listHttpResponse);
        Assert.Contains(
            listHttpResponse,
            generatingReport =>
                generatingReport.RequestId == reportHttpResponse.RequestId &&
                generatingReport.Status is SettlementReportStatus.InProgress or SettlementReportStatus.Completed);
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
