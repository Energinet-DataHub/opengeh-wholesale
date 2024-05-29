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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Edi.Requests;
using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Google.Protobuf;
using Xunit.Abstractions;
using Period = Energinet.DataHub.Edi.Requests.Period;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Functions;

[Collection(nameof(OrchestrationsAppCollectionFixture))]
public class WholesaleInboxTriggerTests : IAsyncLifetime
{
    public WholesaleInboxTriggerTests(
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
        Fixture.ServiceBusListenerMock.ResetMessageHandlersAndReceivedMessages();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Fixture.SetTestOutputHelper(null!);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task GivenAggregatedTimeSeriesRequest_WhenEventIsHandled_FunctionCompletesWithEdiInboxServiceBusResponse()
    {
        // Arrange
        var aggregatedTimeSeriesRequest = new AggregatedTimeSeriesRequest
        {
            Period = new Period
            {
                Start = "2023-01-01T00:00:00Z",
                End = "2023-01-02T00:00:00Z",
            },
            BusinessReason = DataHubNames.BusinessReason.BalanceFixing,
            SettlementMethod = DataHubNames.SettlementMethod.NonProfiled,
            GridAreaCodes = { "804" },
            MeteringPointType = DataHubNames.MeteringPointType.Consumption,
            RequestedForActorNumber = "1234567890123",
            RequestedForActorRole = DataHubNames.ActorRole.MeteredDataResponsible,
        };

        var referenceId = "valid-reference-id";
        await SendMessageToWholesaleInbox(
            subject: AggregatedTimeSeriesRequest.Descriptor.Name,
            body: aggregatedTimeSeriesRequest.ToByteArray(),
            referenceId: referenceId);

        // Act
        // => WholesaleInboxTrigger is running in the fixture and triggered by the given Wholesale inbox message

        // Assert
        // Handling a AggregatedTimeSeriesRequest should send a message to the EDI inbox
        await WaitForMessageSentToEdiInbox(referenceId);

        var sentEdiInboxMessages = Fixture.ServiceBusListenerMock.ReceivedMessages;
        sentEdiInboxMessages
            .Should()
            .ContainSingle(m =>
                m.Subject == AggregatedTimeSeriesRequestAccepted.Descriptor.Name ||
                m.Subject == AggregatedTimeSeriesRequestRejected.Descriptor.Name)
            .Which
            .ApplicationProperties.Should().ContainKey("ReferenceId")
            .WhoseValue.Should().Be(referenceId);
    }

    [Fact]
    public async Task GivenWholesaleServicesRequest_WhenEventIsHandled_FunctionCompletesWithEdiInboxServiceBusResponse()
    {
        // Arrange
        var wholesaleServicesRequest = new WholesaleServicesRequest
        {
            PeriodStart = "2023-01-01T00:00:00Z",
            PeriodEnd = "2023-01-02T00:00:00Z",
            BusinessReason = DataHubNames.BusinessReason.WholesaleFixing,
            GridAreaCodes = { "804" },
            RequestedForActorNumber = "1234567890123",
            RequestedForActorRole = DataHubNames.ActorRole.GridOperator,
            Resolution = DataHubNames.Resolution.Hourly,
        };

        var referenceId = "valid-reference-id";
        await SendMessageToWholesaleInbox(
            subject: WholesaleServicesRequest.Descriptor.Name,
            body: wholesaleServicesRequest.ToByteArray(),
            referenceId: referenceId);

        // Act
        // => WholesaleInboxTrigger is running in the fixture and triggered by the given Wholesale inbox message

        // Assert
        // Handling a WholesaleServicesRequest should send a message to the EDI inbox
        await WaitForMessageSentToEdiInbox(referenceId);

        var messagesSentToEdiInbox = Fixture.ServiceBusListenerMock.ReceivedMessages;
        messagesSentToEdiInbox
            .Should()
            .ContainSingle(m =>
                m.Subject == WholesaleServicesRequestAccepted.Descriptor.Name ||
                m.Subject == WholesaleServicesRequestRejected.Descriptor.Name)
            .Which
            .ApplicationProperties.Should().ContainKey("ReferenceId")
            .WhoseValue.Should().Be(referenceId);
    }

    [Fact]
    public async Task GivenUnhandledSubject_WhenEventIsHandled_FunctionCompletesWithNoHandlerFoundException()
    {
        // Arrange
        await SendMessageToWholesaleInbox(
            subject: "UnhandledSubject",
            body: null);

        // Act
        // => WholesaleInboxTrigger is running in the fixture and triggered by the given Wholesale inbox message

        // Assert
        await AssertWholesaleInboxTriggerIsCompleted();

        using var scope = new AssertionScope();
        var didThrowException = Fixture.AppHostManager.CheckIfFunctionThrewException();
        didThrowException.Should().BeTrue("function should throw exception because no handler should be found for the given subject");

        var functionHostLogs = Fixture.AppHostManager.GetHostLogSnapshot();
        functionHostLogs.Should()
            .ContainMatch(
                "*No request handler found for Wholesale inbox message with subject \"UnhandledSubject\"*");
    }

    private async Task SendMessageToWholesaleInbox(string subject, byte[]? body, string? referenceId = "required-reference-id")
    {
        var serviceBusMessage = new ServiceBusMessage
        {
            Subject = subject,
        };

        if (referenceId is not null)
            serviceBusMessage.ApplicationProperties.Add("ReferenceId", referenceId);

        if (body is not null)
            serviceBusMessage.Body = new BinaryData(body);

        await Fixture.WholesaleInboxQueue.SenderClient.SendMessageAsync(serviceBusMessage);
    }

    private async Task WaitForMessageSentToEdiInbox(string referenceId)
    {
        // Wait for a service bus response with the given reference id
        var waitForServiceBusMessage = await Fixture.ServiceBusListenerMock
            .When(m =>
                m.ApplicationProperties.TryGetValue("ReferenceId", out var receivedReferenceId) &&
                receivedReferenceId.Equals(referenceId))
            .VerifyOnceAsync();

        waitForServiceBusMessage.Wait(TimeSpan.FromSeconds(30));
    }

    private Task AssertWholesaleInboxTriggerIsCompleted()
    {
        return Fixture.AppHostManager.AssertFunctionWasExecutedAsync("WholesaleInboxTrigger", waitTimeSpan: TimeSpan.FromSeconds(30));
    }
}
