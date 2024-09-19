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

using System.Net;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.RevisionLog.Integration;
using Energinet.DataHub.Wholesale.Calculations.Application.AuditLog;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Outbox;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class CalculationTriggerTests : IAsyncLifetime
{
    public static readonly Guid _wholesaleSystemId = Guid.Parse("467ab87d-9494-4add-bf01-703540067b9e");

    public CalculationTriggerTests(
        OrchestrationsAppFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);
    }

    private OrchestrationsAppFixture Fixture { get; }

    public async Task InitializeAsync()
    {
        Fixture.AppHostManager.ClearHostLog();

        // Clear mappings etc. before each test
        Fixture.MockServer.Reset();
        Fixture.MockServer.MockAuditLogEndpoint();
        Fixture.MockServer.ResetLogEntries();

        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
        await dbContext.Outbox.ExecuteDeleteAsync();
    }

    public Task DisposeAsync()
    {
        Fixture.SetTestOutputHelper(null!);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task WhenStartCalculationIsCalled_CorrectAuditLogIsSentUsingOutbox()
    {
        // Arrange
        var startCalculationRequest = new StartCalculationRequestDto(
            CalculationType.BalanceFixing,
            ["123"],
            new DateTimeOffset(2024, 09, 17, 22, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2024, 09, 18, 22, 00, 00, TimeSpan.Zero),
            ScheduledAt: DateTimeOffset.UtcNow,
            false);

        var jsonSerializer = new JsonSerializer();
        var expectedAuditLogPayload = jsonSerializer.Serialize(startCalculationRequest);
        var outboxCreatedAfter = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromSeconds(1));

        // Act
        await Fixture.StartCalculationAsync(startCalculationRequest);
        await Fixture.AppHostManager.TriggerFunctionAsync(OutboxPublisher.FunctionName);

        // Assert
        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();

        // => Wait for the outbox message to be published
        var (success, outboxMessage) = await dbContext.WaitForPublishedOutboxMessageAsync(
            outboxCreatedAfter,
            Fixture.TestLogger,
            TimeSpan.FromSeconds(30));

        // => Asserts the outbox message was published successfully
        using (new AssertionScope())
        {
            success.Should().BeTrue();
            outboxMessage.Should().NotBeNull();

            var actualOutboxMessageState = new
            {
                outboxMessage?.PublishedAt,
                outboxMessage?.ProcessingAt,
                outboxMessage?.FailedAt,
                outboxMessage?.ErrorMessage,
            };
            outboxMessage?.PublishedAt.Should().NotBeNull("because the outbox message should be published, " +
                                                          $"but actual state was: {actualOutboxMessageState}");
        }

        // => Assert the audit log outbox message content was as expected
        var actualOutboxMessage = jsonSerializer.Deserialize<AuditLogOutboxMessageV1Payload>(outboxMessage!.Payload);

        actualOutboxMessage.Activity.Should().Be(AuditLogActivity.StartNewCalculation.Identifier);
        actualOutboxMessage.Payload.Should().Be(expectedAuditLogPayload);

        // => Assert that the request to the audit log mock server was as expected
        RevisionLogEntry? deserializedBody;
        using (new AssertionScope())
        {
            var actualAuditLogRequests = Fixture.MockServer.GetAuditLogCalls();
            var actualAuditLogRequest = actualAuditLogRequests.Should().ContainSingle()
                .Subject;

            actualAuditLogRequest.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            actualAuditLogRequest.Request.Body.Should().NotBeNull();

            // => Ensure that the audit log request body can be deserialized to an instance of RevisionLogEntry
            var deserializeBody = () =>
                jsonSerializer.Deserialize<RevisionLogEntry>(actualAuditLogRequest.Request.Body ?? string.Empty);

            deserializedBody = deserializeBody.Should().NotThrow().Subject;
            deserializedBody.Should().NotBeNull();
        }

        using var assertionScope = new AssertionScope();
        deserializedBody!.LogId.Should().Be(actualOutboxMessage.LogId);
        deserializedBody.Activity.Should().Be(AuditLogActivity.StartNewCalculation.Identifier);
        deserializedBody.Payload.Should().Be(expectedAuditLogPayload);
        deserializedBody.SystemId.Should().Be(_wholesaleSystemId); // Wholesale system id
        deserializedBody.UserId.Should().Be(Fixture.UserInformation.UserId);
        deserializedBody.ActorId.Should().Be(Fixture.UserInformation.ActorId);
        deserializedBody.ActorNumber.Should().Be(Fixture.UserInformation.ActorNumber);
        deserializedBody.MarketRoles.Should().Be(Fixture.UserInformation.ActorRole);
        deserializedBody.Permissions.Should().Be(string.Join(", ", Fixture.UserInformation.Permissions));
        deserializedBody.Origin.Should().Be($"{Fixture.AppHostManager.HttpClient.BaseAddress}api/StartCalculation");
        deserializedBody.AffectedEntityType.Should().Be(AuditLogEntityType.Calculation.Identifier);
        deserializedBody.AffectedEntityKey.Should().BeNull();
    }
}
