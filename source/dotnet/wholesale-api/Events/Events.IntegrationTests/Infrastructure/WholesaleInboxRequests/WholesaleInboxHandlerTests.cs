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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Infrastructure.WholesaleInboxRequests;

public class WholesaleInboxHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task ReceiveWholesaleInboxMessage_WhenHasSubjectAndReferenceId_ReceivesMessage(
        Mock<IWholesaleInboxRequestHandler> handlerMock,
        ILogger<WholesaleInboxHandler> logger)
    {
        // Arrange
        var messageHasBeenReceivedEvent = new AutoResetEvent(false);
        var expectedReferenceId = "valid-reference-id";
        handlerMock
            .Setup(handler => handler.ProcessAsync(It.IsAny<ServiceBusReceivedMessage>(), expectedReferenceId, It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                messageHasBeenReceivedEvent.Set();
            });

        handlerMock
            .Setup(handler => handler.CanHandle(It.IsAny<string>()))
            .Returns(true);

        var sut = new WholesaleInboxHandler(
            logger,
            [handlerMock.Object]);

        var serviceBusReceivedMessage = CreateServiceBusReceivedMessage(
            subject: "ValidSubject",
            referenceId: expectedReferenceId);

        // Act
        var receiveWholesaleInboxMessage = () => sut.ProcessAsync(serviceBusReceivedMessage, CancellationToken.None);

        // Assert
        await receiveWholesaleInboxMessage.Should().NotThrowAsync();
        var messageHasBeenReceived = messageHasBeenReceivedEvent.WaitOne(timeout: TimeSpan.FromSeconds(5));
        messageHasBeenReceived.Should().BeTrue();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ReceiveWholesaleInboxMessage_WhenMissingReferenceId_ThrowsInvalidOperationException(
        IWholesaleInboxRequestHandler handler,
        ILogger<WholesaleInboxHandler> logger)
    {
        // Arrange
        var sut = new WholesaleInboxHandler(
            logger,
            [handler]);

        var serviceBusReceivedMessage = CreateServiceBusReceivedMessage(
            subject: "ValidSubject",
            referenceId: null);

        // Act
        var receiveRequestWithoutReferenceId = () => sut.ProcessAsync(serviceBusReceivedMessage, CancellationToken.None);

        // Assert
        await receiveRequestWithoutReferenceId.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Missing reference id for received Wholesale inbox service bus message");
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ReceiveWholesaleInboxMessage_WhenMissingSubject_ThrowsInvalidOperationException(
        IWholesaleInboxRequestHandler handler,
        ILogger<WholesaleInboxHandler> logger)
    {
        // Arrange
        var sut = new WholesaleInboxHandler(
            logger,
            [handler]);

        var serviceBusReceivedMessage = CreateServiceBusReceivedMessage(
            subject: null,
            referenceId: "valid-reference-id");

        // Act
        var receiveRequestWithoutReferenceId = () => sut.ProcessAsync(serviceBusReceivedMessage, CancellationToken.None);

        // Assert
        await receiveRequestWithoutReferenceId.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Missing subject for received Wholesale inbox service bus message");
    }

    private static ServiceBusReceivedMessage CreateServiceBusReceivedMessage(string? subject, string? referenceId)
    {
        var properties = new Dictionary<string, object>();

        if (referenceId is not null)
            properties.Add("ReferenceId", referenceId);

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            subject: subject,
            properties: properties);

        return serviceBusReceivedMessage;
    }
}
