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

using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Calculations.Application.AuditLog;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Orchestrations.Functions.Calculation.Model;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions.Calculation;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class CalculationTriggerTests : IAsyncLifetime
{
    public CalculationTriggerTests(
        OrchestrationsAppFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);
    }

    private OrchestrationsAppFixture Fixture { get; }

    public Task InitializeAsync()
    {
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

    [Fact]
    public async Task WhenStartCalculationIsCalled_CorrectAuditLogIsAddedToOutbox()
    {
        var startCalculationRequest = new StartCalculationRequestDto(
            CalculationType.BalanceFixing,
            ["123"],
            new DateTimeOffset(2024, 09, 17, 22, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2024, 09, 18, 22, 00, 00, TimeSpan.Zero),
            ScheduledAt: DateTimeOffset.UtcNow,
            false);

        var outboxCreatedAfter = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromSeconds(1));

        // Act
        await Fixture.StartCalculationAsync(startCalculationRequest);

        // Assert
        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
        var outboxMessages = await dbContext.Outbox
            .Where(o => o.CreatedAt > outboxCreatedAfter)
            .ToListAsync();

        var outboxMessage = outboxMessages.Should().ContainSingle()
            .Subject;

        var jsonSerializer = new JsonSerializer();
        var payload = jsonSerializer.Deserialize<AuditLogOutboxMessageV1Payload>(outboxMessage.Payload);

        payload.Activity.Should().Be(AuditLogActivity.StartNewCalculation.Identifier);

        var serializedRequest = jsonSerializer.Serialize(startCalculationRequest);
        payload.Payload.Should().Be(serializedRequest);
    }
}
