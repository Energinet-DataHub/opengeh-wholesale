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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.EDI.Client;
using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Extensions;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

public class AggregatedTimeSeriesRequestHandlerTests
{
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithTotalProductionPerGridAreaRequest_SendsAcceptedEdiMessage(
        [Frozen] Mock<IRequestCalculationResultQueries> requestCalculationResultQueriesMock,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<AggregatedTimeSeriesRequestFactory> aggregatedTimeSeriesRequestMessageParseMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> loggerMock)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .Build();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        var calculationResult = CreateEnergyResult();
        requestCalculationResultQueriesMock.Setup(calculationResultQueries =>
                calculationResultQueries.GetAsync(It.IsAny<EnergyResultQuery>()))
            .ReturnsAsync(() => calculationResult);

        validator.Setup(vali => vali.Validate(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .Returns(() => new List<ValidationError>());

        var sut = new AggregatedTimeSeriesRequestHandler(
            requestCalculationResultQueriesMock.Object,
            senderMock.Object,
            aggregatedTimeSeriesRequestMessageParseMock.Object,
            validator.Object,
            loggerMock.Object);

        // Act
        await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert
        senderMock.Verify(
            bus => bus.SendAsync(
            It.Is<ServiceBusMessage>(message =>
                message.Subject.Equals(expectedAcceptedSubject)
                && message.ApplicationProperties.ContainsKey("ReferenceId")
                && message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithTotalProductionPerGridAreaRequest_SendsRejectedEdiMessage(
        [Frozen] Mock<IRequestCalculationResultQueries> requestCalculationResultQueriesMock,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<AggregatedTimeSeriesRequestFactory> aggregatedTimeSeriesRequestMessageParseMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> loggerMock)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(vali => vali.Validate(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .Returns(() => new List<ValidationError>());

        var sut = new AggregatedTimeSeriesRequestHandler(
            requestCalculationResultQueriesMock.Object,
            senderMock.Object,
            aggregatedTimeSeriesRequestMessageParseMock.Object,
            validator.Object,
            loggerMock.Object);

        // Act
        await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert
        senderMock.Verify(
            bus => bus.SendAsync(
            It.Is<ServiceBusMessage>(message =>
                message.Subject.Equals(expectedRejectedSubject)
                && message.ApplicationProperties.ContainsKey("ReferenceId")
                && message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WhenNoCalculationResult_SendsRejectedEdiMessage(
        [Frozen] Mock<IRequestCalculationResultQueries> requestCalculationResultQueriesMock,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<AggregatedTimeSeriesRequestFactory> aggregatedTimeSeriesRequestMessageParseMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> loggerMock)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(vali => vali.Validate(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .Returns(() => new List<ValidationError>());

        requestCalculationResultQueriesMock.Setup(vali => vali.GetAsync(
                It.IsAny<EnergyResultQuery>()))
            .ReturnsAsync(() => null);

        var sut = new AggregatedTimeSeriesRequestHandler(
            requestCalculationResultQueriesMock.Object,
            senderMock.Object,
            aggregatedTimeSeriesRequestMessageParseMock.Object,
            validator.Object,
            loggerMock.Object);

        // Act
        await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert
        senderMock.Verify(
            bus => bus.SendAsync(
            It.Is<ServiceBusMessage>(message =>
                message.Subject.Equals(expectedRejectedSubject)
                && message.WithErrorCode(_noDataAvailable.ErrorCode)
                && message.ApplicationProperties.ContainsKey("ReferenceId")
                && message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private EnergyResult CreateEnergyResult()
    {
        return new EnergyResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "543",
            TimeSeriesType.Production,
            "1223456",
            "123456",
            timeSeriesPoints: new EnergyTimeSeriesPoint[] { new(DateTime.Now, 0, QuantityQuality.Measured, new List<QuantityQuality>()) },
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 12, 31, 23, 0),
            Instant.FromUtc(2023, 1, 31, 23, 0),
            null);
    }
}
