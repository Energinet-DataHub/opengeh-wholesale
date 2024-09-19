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
using System.Net.Http.Json;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Application.AuditLog;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.AuditLog;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json.Linq;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V3;

public class CalculationControllerTests : WebApiTestBase
{
    public CalculationControllerTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    public override async Task InitializeAsync()
    {
        await using (var dbContext = Fixture.DatabaseManager.CreateDbContext())
        {
            await dbContext.Outbox.ExecuteDeleteAsync();
        }

        await base.InitializeAsync();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_ReturnsHttpStatusCodeOkAtExpectedUrl(
        Mock<ICalculationsClient> mock,
        CalculationDto calculationDto)
    {
        // Arrange
        mock.Setup(service => service.GetAsync(calculationDto.CalculationId))
            .ReturnsAsync(calculationDto);
        Factory.CalculationsClientMock = mock;

        // Act
        var response = await Client.GetAsync($"/v3/calculations/{calculationDto.CalculationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HTTP_GET_V3_SearchReturnsHttpStatusCodeOkAtExpectedUrl()
    {
        // Arrange + Act
        var response = await Client.GetAsync("/v3/calculations", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_AddsCorrectAuditLogMessageToOutbox(
        Mock<ICalculationsClient> mock,
        CalculationDto calculationDto)
    {
        // Arrange
        // => Make the calculations client used inside the API return the expected calculation in the test,
        // instead of throwing an exception because the calculation does not exist.
        mock.Setup(service => service.GetAsync(calculationDto.CalculationId))
            .ReturnsAsync(calculationDto);
        Factory.CalculationsClientMock = mock;

        // Act
        var response = await Client.GetAsync($"/v3/calculations/{calculationDto.CalculationId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var dbContext = Fixture.DatabaseManager.CreateDbContext();
        var actualOutboxMessage = await dbContext.Outbox.SingleOrDefaultAsync();

        actualOutboxMessage.Should().NotBeNull();

        using (new AssertionScope())
        {
            actualOutboxMessage!.Type.Should().Be(AuditLogOutboxMessageV1.OutboxMessageType);
            actualOutboxMessage.Payload.Should().NotBeNullOrWhiteSpace();
        }

        using var assertionScope = new AssertionScope();
        var jsonSerializer = new JsonSerializer();
        var actualOutboxPayload = jsonSerializer.Deserialize<AuditLogOutboxMessageV1Payload>(actualOutboxMessage.Payload);
        actualOutboxPayload.Activity.Should().Be(AuditLogActivity.GetCalculation.Identifier);
        actualOutboxPayload.Payload.Should().Be(calculationDto.CalculationId.ToString());
        actualOutboxPayload.Origin.Should().Be($"{Client.BaseAddress}v3/calculations/{calculationDto.CalculationId}");
        actualOutboxPayload.SystemId.Should().Be(Guid.Parse("467ab87d-9494-4add-bf01-703540067b9e")); // Wholesale system id
        actualOutboxPayload.AffectedEntityType.Should().Be(AuditLogEntityType.Calculation.Identifier);
        actualOutboxPayload.AffectedEntityKey.Should().Be(calculationDto.CalculationId.ToString());
    }

    [Fact]
    public async Task SearchAsync_AddsCorrectAuditLogMessageToOutbox()
    {
        // Arrange
        var calculationSearchParameters = new CalculationSearchParameters(
            gridAreaCodes: null,
            executionState: CalculationState.Completed.ToString(),
            minExecutionTime: new DateTimeOffset(2024, 09, 18, 13, 37, 0, TimeSpan.Zero)
                .ToString("O"),
            maxExecutionTime: null,
            periodStart: null,
            periodEnd: null);

        var queryString = QueryString.Create(new List<KeyValuePair<string, string?>>
        {
            new(nameof(calculationSearchParameters.executionState), calculationSearchParameters.executionState),
            new(nameof(calculationSearchParameters.minExecutionTime), calculationSearchParameters.minExecutionTime),
        });

        // Act
        var searchRequestUrl = $"v3/calculations{queryString}";
        var searchRequest = new HttpRequestMessage(HttpMethod.Get, searchRequestUrl);
        var response = await Client.SendAsync(searchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var dbContext = Fixture.DatabaseManager.CreateDbContext();
        var actualOutboxMessage = await dbContext.Outbox.SingleOrDefaultAsync();

        actualOutboxMessage.Should().NotBeNull();

        using (new AssertionScope())
        {
            actualOutboxMessage!.Type.Should().Be(AuditLogOutboxMessageV1.OutboxMessageType);
            actualOutboxMessage.Payload.Should().NotBeNullOrWhiteSpace();
        }

        using var assertionScope = new AssertionScope();
        var jsonSerializer = new JsonSerializer();
        var actualOutboxPayload = jsonSerializer.Deserialize<AuditLogOutboxMessageV1Payload>(actualOutboxMessage.Payload);
        actualOutboxPayload.Activity.Should().Be(AuditLogActivity.SearchCalculation.Identifier);
        actualOutboxPayload.Payload.Should().NotBeNullOrWhiteSpace();
        actualOutboxPayload.Origin.Should().Be($"{Client.BaseAddress}{searchRequestUrl}");
        actualOutboxPayload.SystemId.Should().Be(Guid.Parse("467ab87d-9494-4add-bf01-703540067b9e")); // Wholesale system id
        actualOutboxPayload.AffectedEntityType.Should().Be(AuditLogEntityType.Calculation.Identifier);
        actualOutboxPayload.AffectedEntityKey.Should().BeNull();

        var actualCalculationSearchPayload = jsonSerializer.Deserialize<CalculationSearchParameters>(actualOutboxPayload.Payload);
        actualCalculationSearchPayload.Should().BeEquivalentTo(calculationSearchParameters);
    }

    private record CalculationSearchParameters(
#pragma warning disable SA1300
        string[]? gridAreaCodes,
        string? executionState,
        string? minExecutionTime,
        string? maxExecutionTime,
        string? periodStart,
        string? periodEnd);
#pragma warning restore SA1300
}
