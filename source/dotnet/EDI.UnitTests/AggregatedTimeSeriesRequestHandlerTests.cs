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
using EDI;
using EDI.InboxEvents;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Models;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;
using AggregationPerGridArea = Energinet.DataHub.Edi.Requests.AggregationPerGridArea;
using CalculationTimeSeriesPoint = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesPoint;
using Period = Energinet.DataHub.Edi.Requests.Period;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

public class AggregatedTimeSeriesRequestHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithTotalProductionPerGridAreaRequest_SendsAcceptedEdiMessage(
        [Frozen] Mock<IRequestCalculationResultQueries> requestCalculationResultQueriesMock,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<AggregatedTimeSeriesRequestMessageParser> aggregatedTimeSeriesRequestMessageParseMock,
        [Frozen] Mock<AggregatedTimeSeriesMessageFactory> aggregatedTimeSeriesMessageFactoryMock,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> loggerMock)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = new AggregatedTimeSeriesRequest
        {
            AggregationPerGridarea = new AggregationPerGridArea(),
            TimeSeriesType = Energinet.DataHub.Edi.Requests.TimeSeriesType.Production,
            Period = new Period()
            {
                StartOfPeriod = new Timestamp(),
                EndOfPeriod = new Timestamp(),
            },
        };
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        var calculationResult = CreateEnergyResult();
        requestCalculationResultQueriesMock.Setup(calculationResultQueries =>
                calculationResultQueries.GetAsync(It.IsAny<CalculationResultQuery>()))
            .ReturnsAsync(() => calculationResult);

        var sut = new AggregatedTimeSeriesRequestHandler(
            requestCalculationResultQueriesMock.Object,
            senderMock.Object,
            aggregatedTimeSeriesRequestMessageParseMock.Object,
            aggregatedTimeSeriesMessageFactoryMock.Object,
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

    private EnergyResult CreateEnergyResult()
    {
        return new EnergyResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "543",
            Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType.Production,
            "1223456",
            "123456",
            timeSeriesPoints: new CalculationTimeSeriesPoint[] { new(DateTime.Now, 0, QuantityQuality.Measured) },
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 12, 31, 23, 0),
            Instant.FromUtc(2023, 1, 31, 23, 0),
            null);
    }
}
