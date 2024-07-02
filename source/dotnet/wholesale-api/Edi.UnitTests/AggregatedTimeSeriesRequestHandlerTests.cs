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
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Extensions;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using Resolution = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.Resolution;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests;

public class AggregatedTimeSeriesRequestHandlerTests
{
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");
    private static readonly ValidationError _noDataForRequestedGridArea = new("Forkert netområde / invalid grid area", "D46");
    private static readonly ValidationError _invalidEnergySupplierField = new("Feltet EnergySupplier skal være udfyldt med et valid GLN/EIC nummer når en elleverandør anmoder om data / EnergySupplier must be submitted with a valid GLN/EIC number when an energy supplier requests data", "E16");

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithTotalProductionPerGridAreaRequest_SendsAcceptedEdiMessage(
        [Frozen] Mock<IAggregatedTimeSeriesQueries> aggregatedTimeSeriesQueries,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger)
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

        var start = DateTimeOffset.Parse("2022-01-01T00:00Z").ToInstant();
        var end = DateTimeOffset.Parse("2022-01-01T00:15Z").ToInstant();
        var aggregatedTimeSeries = CreateAggregatedTimeSeries(start, end);
        aggregatedTimeSeriesQueries
            .Setup(parameters => parameters.GetAsync(It.IsAny<AggregatedTimeSeriesQueryParameters>()))
            .Returns(() => aggregatedTimeSeries.ToAsyncEnumerable());

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object);

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
    public async Task ProcessAsync_WithRequestIsLatestCorrection_SendsAcceptedEdiMessage(
        [Frozen] Mock<IAggregatedTimeSeriesQueries> aggregatedTimeSeriesQueries,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithBusinessReason("Correction")
            .Build();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        var start = DateTimeOffset.Parse("2022-01-01T00:00Z").ToInstant();
        var end = DateTimeOffset.Parse("2022-01-01T00:15Z").ToInstant();
        var aggregatedTimeSeries = CreateAggregatedTimeSeries(start, end);
        aggregatedTimeSeriesQueries
            .Setup(parameters => parameters.GetAsync(It.IsAny<AggregatedTimeSeriesQueryParameters>()))
            .Returns(() => aggregatedTimeSeries.ToAsyncEnumerable());

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object);

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
    public async Task ProcessAsync_WhenNoAggregatedTimeSeries_SendsRejectedEdiMessage(
        [Frozen] Mock<IAggregatedTimeSeriesQueries> aggregatedTimeSeriesQueries,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object);

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

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WhenNoAggregatedTimeSeriesForRequestedGridArea_SendsRejectedEdiMessage(
        [Frozen] Mock<IAggregatedTimeSeriesQueries> aggregatedTimeSeriesQueries,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .WithGridArea("303")
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        var start = DateTimeOffset.Parse("2022-01-01T00:00Z").ToInstant();
        var end = DateTimeOffset.Parse("2022-01-01T00:15Z").ToInstant();
        var aggregatedTimeSeries = CreateAggregatedTimeSeries(start, end);
        aggregatedTimeSeriesQueries
            .Setup(parameters =>
                parameters.GetAsync(
                    It.Is<AggregatedTimeSeriesQueryParameters>(x => x.GridAreaCodes.Count == 0)))
            .Returns(() => aggregatedTimeSeries.ToAsyncEnumerable());

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object);

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
                && message.WithErrorCode(_noDataForRequestedGridArea.ErrorCode)
                && message.ApplicationProperties.ContainsKey("ReferenceId")
                && message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WhenNoCalculations_SendsRejectedEdiMessage(
        [Frozen] Mock<IAggregatedTimeSeriesQueries> aggregatedTimeSeriesQueries,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .WithGridArea("303")
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object);

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

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WhenNoAggregatedTimeSeriesForAnyGridArea_SendsRejectedEdiMessage(
        [Frozen] Mock<IAggregatedTimeSeriesQueries> aggregatedTimeSeriesQueries,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .WithGridArea("303")
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object);

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

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WhenRequestHasValidationErrors_SendsRejectedEdiMessage(
        [Frozen] Mock<IAggregatedTimeSeriesQueries> aggregatedTimeSeriesQueries,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .WithGridArea("303")
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(vali => vali.ValidateAsync(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .ReturnsAsync(() => new List<ValidationError> { _invalidEnergySupplierField });

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object);

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
                && message.WithErrorCode(_invalidEnergySupplierField.ErrorCode)
                && message.ApplicationProperties.ContainsKey("ReferenceId")
                && message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private IReadOnlyCollection<AggregatedTimeSeries> CreateAggregatedTimeSeries(Instant start, Instant end)
    {
        var timeSeriesPoints = new List<EnergyTimeSeriesPoint>();

        for (var i = start; i < end; i += Duration.FromMinutes(15))
        {
            timeSeriesPoints.Add(new EnergyTimeSeriesPoint(i.ToDateTimeOffset(), 0, new List<QuantityQuality> { QuantityQuality.Measured }));
        }

        return new List<AggregatedTimeSeries>
        {
            new(
                gridArea: "543",
                timeSeriesPoints: timeSeriesPoints.ToArray(),
                timeSeriesType: TimeSeriesType.Production,
                calculationType: CalculationType.Aggregation,
                DateTimeOffset.Parse("2022-01-01T00:00Z").ToInstant(),
                DateTimeOffset.Parse("2022-01-01T00:15Z").ToInstant(),
                Resolution.Quarter,
                1),
        };
    }
}
