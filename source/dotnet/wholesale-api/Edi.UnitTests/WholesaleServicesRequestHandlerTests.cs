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
using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Calculations;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Factories;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Extensions;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using QuantityUnit = Energinet.DataHub.Wholesale.Common.Interfaces.Models.QuantityUnit;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults.Resolution;
using WholesaleServicesRequest = Energinet.DataHub.Edi.Requests.WholesaleServicesRequest;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests;

public class WholesaleServicesRequestHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithNoValidationErrors_SendsAcceptedEdiMessage(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger,
        [Frozen] Mock<CompletedCalculationRetriever> completedCalculationRetriever)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(WholesaleServicesRequestAccepted);
        var expectedReferenceId = Guid.NewGuid().ToString();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(new WholesaleServicesRequestBuilder().Build().ToByteArray()));

        var wholesaleResult = CreateWholesaleResult();
        wholesaleResultQueries.Setup(q => q.GetAsync(It.IsAny<WholesaleResultQueryParameters>()))
            .Returns(new List<WholesaleResult>
            {
                wholesaleResult,
            }.ToAsyncEnumerable());

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            completedCalculationRetriever.Object,
            wholesaleResultQueries.Object,
            mapper.Object,
            logger.Object);

        // Act
        await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert
        ediClient.Verify(
            client => client.SendAsync(
                It.Is<ServiceBusMessage>(message =>
                    message.Subject.Equals(expectedAcceptedSubject)
                    && message.ApplicationProperties.ContainsKey("ReferenceId")
                    && message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithNoWholesaleResultData_SendsRejectedEdiMessage(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger,
        [Frozen] Mock<CompletedCalculationRetriever> completedCalculationRetriever)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(WholesaleServicesRequestRejected);
        const string expectedValidationErrorCode = "E0H";
        var expectedReferenceId = Guid.NewGuid().ToString();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(new WholesaleServicesRequestBuilder().Build().ToByteArray()));

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            completedCalculationRetriever.Object,
            wholesaleResultQueries.Object,
            mapper.Object,
            logger.Object);

        // Act
        await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert
        ediClient.Verify(
            client => client.SendAsync(
                It.Is<ServiceBusMessage>(message =>
                    message.Subject.Equals(expectedRejectedSubject)
                    && message.WithErrorCode(expectedValidationErrorCode)
                    && message.ApplicationProperties.ContainsKey("ReferenceId")
                    && message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithValidationErrors_SendsRejectedEdiMessage(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger,
        [Frozen] Mock<CompletedCalculationRetriever> completedCalculationRetriever)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(WholesaleServicesRequestRejected);
        const string expectedValidationErrorCode = "001";
        var expectedReferenceId = Guid.NewGuid().ToString();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(new WholesaleServicesRequestBuilder().Build().ToByteArray()));

        validator.Setup(v => v.ValidateAsync(
                It.IsAny<WholesaleServicesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>
            {
                new("A validation error", expectedValidationErrorCode),
            });

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            completedCalculationRetriever.Object,
            wholesaleResultQueries.Object,
            mapper.Object,
            logger.Object);

        // Act
        await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert
        ediClient.Verify(
            client => client.SendAsync(
                It.Is<ServiceBusMessage>(message =>
                    message.Subject.Equals(expectedRejectedSubject)
                    && message.WithErrorCode(expectedValidationErrorCode)
                    && message.ApplicationProperties.ContainsKey("ReferenceId")
                    && message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private WholesaleResult CreateWholesaleResult()
    {
        return new WholesaleResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CalculationType.WholesaleFixing,
            Instant.FromUtc(2024, 1, 1, 0, 0),
            Instant.FromUtc(2024, 1, 2, 0, 0),
            "1",
            "1",
            AmountType.AmountPerCharge,
            "1",
            ChargeType.Tariff,
            "1",
            false,
            QuantityUnit.Kwh,
            Resolution.Day,
            null,
            null,
            new List<WholesaleTimeSeriesPoint>
            {
                new(
                    new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    1,
                    Array.Empty<QuantityQuality>(),
                    1,
                    1),
            },
            1);
    }
}
