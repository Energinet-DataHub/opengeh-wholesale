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
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.EDI.Client;
using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Extensions;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

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
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);

        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .Build();

        var periodStart = InstantPattern.General.Parse(request.Period.Start).Value;
        var periodEnd = InstantPattern.General.Parse(request.Period.End).Value;

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(vali => vali.ValidateAsync(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>());

        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();
        calculationsClient
            .Setup(client => client.SearchAsync(
                request.HasGridAreaCode
                    ? new[] { request.GridAreaCode }
                    : new string[] { },
                CalculationState.Completed,
                periodStart,
                periodEnd))
            .ReturnsAsync(() => new List<CalculationDto>() { calculation });

        aggregatedTimeSeriesQueries
            .Setup(parameters => parameters.GetAsync(It.IsAny<AggregatedTimeSeriesQueryParameters>()))
            .Returns(() => new List<AggregatedTimeSeries>()
            {
                CalculationResultBuilder
                    .AggregatedTimeSeries(calculation)
                    .Build(),
            }.ToAsyncEnumerable());

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object,
            calculationsClient.Object,
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);

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
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestAccepted);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithBusinessReason("D32")
            .Build();

        var periodStart = InstantPattern.General.Parse(request.Period.Start).Value;
        var periodEnd = InstantPattern.General.Parse(request.Period.End).Value;

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(vali => vali.ValidateAsync(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>());

        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();
        calculationsClient
            .Setup(client => client.SearchAsync(
                request.HasGridAreaCode
                    ? new[] { request.GridAreaCode }
                    : new string[] { },
                CalculationState.Completed,
                periodStart,
                periodEnd))
            .ReturnsAsync(() => new List<CalculationDto>() { calculation });

        aggregatedTimeSeriesQueries
            .Setup(parameters => parameters.GetLatestCorrectionForGridAreaAsync(It.IsAny<AggregatedTimeSeriesQueryParameters>()))
            .Returns(() => new List<AggregatedTimeSeries>()
            {
                CalculationResultBuilder
                .AggregatedTimeSeries(calculation)
                .Build(),
            }.ToAsyncEnumerable());

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object,
            calculationsClient.Object,
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);

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
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(vali => vali.ValidateAsync(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>());

        var periodStart = InstantPattern.General.Parse(request.Period.Start).Value;
        var periodEnd = InstantPattern.General.Parse(request.Period.End).Value;
        calculationsClient
            .Setup(client => client.SearchAsync(
                request.HasGridAreaCode
                    ? new[] { request.GridAreaCode }
                    : new string[] { },
                CalculationState.Completed,
                periodStart,
                periodEnd))
            .ReturnsAsync(() => new List<CalculationDto>());

        aggregatedTimeSeriesQueries
            .Setup(parameters => parameters.GetAsync(It.IsAny<AggregatedTimeSeriesQueryParameters>()))
            .Returns(() => new List<AggregatedTimeSeries>().ToAsyncEnumerable());

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object,
            calculationsClient.Object,
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);

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
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithGridArea("303")
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(vali => vali.ValidateAsync(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>());

        var periodStart = InstantPattern.General.Parse(request.Period.Start).Value;
        var periodEnd = InstantPattern.General.Parse(request.Period.End).Value;
        var calculation = CalculationDtoBuilder.CalculationDto()
            .WithPeriodStart(periodStart)
            .WithPeriodEnd(periodEnd)
            .Build();
        calculationsClient
            .Setup(client => client.SearchAsync(
                request.HasGridAreaCode
                    ? new[] { request.GridAreaCode }
                    : new string[] { },
                CalculationState.Completed,
                periodStart,
                periodEnd))
            .ReturnsAsync(() => new List<CalculationDto>() { calculation });

        aggregatedTimeSeriesQueries
            .Setup(parameters =>
                parameters.GetAsync(
                    It.IsAny<AggregatedTimeSeriesQueryParameters>()))
            .Returns(() => new List<AggregatedTimeSeries>().ToAsyncEnumerable());

        aggregatedTimeSeriesQueries
            .Setup(parameters =>
                parameters.GetAsync(
                    It.Is<AggregatedTimeSeriesQueryParameters>(x => x.GridArea == null)))
            .Returns(() => new List<AggregatedTimeSeries>()
            {
                CalculationResultBuilder
                    .AggregatedTimeSeries(calculation)
                    .Build(),
            }.ToAsyncEnumerable());

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object,
            calculationsClient.Object,
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);

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
    public async Task ProcessAsync_WhenNoAggregatedTimeSeriesForAnyGridArea_SendsRejectedEdiMessage(
        [Frozen] Mock<IAggregatedTimeSeriesQueries> aggregatedTimeSeriesQueries,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<IValidator<AggregatedTimeSeriesRequest>> validator,
        [Frozen] Mock<ILogger<AggregatedTimeSeriesRequestHandler>> logger,
        [Frozen] Mock<ICalculationsClient> calculationsClient)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithGridArea("303")
            .Build();
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(vali => vali.ValidateAsync(
                It.IsAny<AggregatedTimeSeriesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>());

        var periodStart = InstantPattern.General.Parse(request.Period.Start).Value;
        var periodEnd = InstantPattern.General.Parse(request.Period.End).Value;
        calculationsClient
            .Setup(client => client.SearchAsync(
                request.HasGridAreaCode
                    ? new[] { request.GridAreaCode }
                    : new string[] { },
                CalculationState.Completed,
                periodStart,
                periodEnd))
            .ReturnsAsync(() => new List<CalculationDto>());

        aggregatedTimeSeriesQueries
            .Setup(parameters =>
                parameters.GetAsync(
                    It.IsAny<AggregatedTimeSeriesQueryParameters>()))
            .Returns(() => new List<AggregatedTimeSeries>().ToAsyncEnumerable());

        aggregatedTimeSeriesQueries
            .Setup(parameters =>
                parameters.GetAsync(
                    It.Is<AggregatedTimeSeriesQueryParameters>(x => x.GridArea == null)))
            .Returns(() => new List<AggregatedTimeSeries>().ToAsyncEnumerable());

        var sut = new AggregatedTimeSeriesRequestHandler(
            senderMock.Object,
            validator.Object,
            aggregatedTimeSeriesQueries.Object,
            logger.Object,
            calculationsClient.Object,
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);

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
}
