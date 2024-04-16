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

using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Wholesale.Edi.Client;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests;

public class EdiClientTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task SendAsync_WhenCalled_ServiceBusSenderIsCalled(
        [Frozen] Mock<IOptions<EdiInboxQueueOptions>> inboxQueueOptions,
        [Frozen] Mock<ServiceBusClient> serviceBusClient,
        [Frozen] Mock<ServiceBusSender> serviceBusSender,
        [Frozen] Mock<ServiceBusMessage> messageToSend)
    {
        // Arrange
        var expectedQueueName = "expected-edi-queue-name";
        inboxQueueOptions
            .Setup(o => o.Value)
            .Returns(new EdiInboxQueueOptions { QueueName = expectedQueueName });

        serviceBusSender
            .Setup(s => s.SendMessageAsync(messageToSend.Object, CancellationToken.None))
            .Returns(Task.CompletedTask);

        serviceBusClient
            .Setup(c => c.CreateSender(expectedQueueName))
            .Returns(serviceBusSender.Object);

        var responseToMessage = ServiceBusModelFactory.ServiceBusReceivedMessage();

        // Act
        var sut = new EdiClient(inboxQueueOptions.Object, serviceBusClient.Object);
        await sut.SendAsync(messageToSend.Object, responseToMessage, CancellationToken.None);

        // Assert
        serviceBusSender
            .Verify(x => x.SendMessageAsync(messageToSend.Object, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task SendAsync_WhenCalled_ServiceBusMessageCorrelationIdIsReceivedMessageId(
        [Frozen] Mock<IOptions<EdiInboxQueueOptions>> inboxQueueOptions,
        [Frozen] Mock<ServiceBusClient> serviceBusClient,
        [Frozen] Mock<ServiceBusSender> serviceBusSender,
        [Frozen] Mock<ServiceBusMessage> messageToSend)
    {
        // Arrange
        var responseToMessageId = "expected-response-to-message-id";

        serviceBusSender
            .Setup(s => s.SendMessageAsync(messageToSend.Object, CancellationToken.None))
            .Returns(Task.CompletedTask);

        serviceBusClient
            .Setup(c => c.CreateSender(It.IsAny<string>()))
            .Returns(serviceBusSender.Object);

        var responseToMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(messageId: responseToMessageId);

        // Act
        var sut = new EdiClient(inboxQueueOptions.Object, serviceBusClient.Object);
        await sut.SendAsync(messageToSend.Object, responseToMessage, CancellationToken.None);

        // Assert
        serviceBusSender
            .Verify(
                x => x.SendMessageAsync(
                    It.Is<ServiceBusMessage>(m =>
                        m == messageToSend.Object &&
                        m.CorrelationId == responseToMessageId),
                    CancellationToken.None),
                Times.Once);
    }
}
