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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Factories;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Extensions;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Period = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.Period;
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
        [Frozen] Mock<IWholesaleServicesQueries> queries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(WholesaleServicesRequestAccepted);
        var expectedReferenceId = Guid.NewGuid().ToString();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(new WholesaleServicesRequestBuilder().Build().ToByteArray()));

        List<WholesaleServices> wholesaleServices =
        [
            CreateWholesaleServices(),
            CreateWholesaleServices(),
            CreateWholesaleServices(),
        ];
        var expectedSeriesCount = wholesaleServices.Count;

        queries
            .Setup(q => q.GetAsync(It.IsAny<WholesaleServicesQueryParameters>()))
            .Returns(wholesaleServices.ToAsyncEnumerable);

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            queries.Object,
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
                    message.Subject.Equals(expectedAcceptedSubject) &&
                    WholesaleServicesRequestAccepted.Parser.ParseFrom(message.Body).Series.Count == expectedSeriesCount &&
                    message.ApplicationProperties.ContainsKey("ReferenceId") &&
                    message.ApplicationProperties["ReferenceId"].Equals(expectedReferenceId)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify wholesale query was executed once for AmountPerCharge
        queries.Verify(
            q =>
                q.GetAsync(It.Is<WholesaleServicesQueryParameters>(queryParameters =>
                    queryParameters.AmountType == AmountType.AmountPerCharge)),
            Times.Once);

        // Verify that only the query above was executed
        queries.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithMonthlyResolution_QueriesBothTotalAndMonthlyCalculationResults(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleServicesQueries> queries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(WholesaleServicesRequestAccepted);
        var expectedReferenceId = Guid.NewGuid().ToString();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(new WholesaleServicesRequestBuilder()
                .WithResolution(DataHubNames.Resolution.Monthly)
                .Build()
                .ToByteArray()));

        List<WholesaleServices> wholesaleServices =
        [
            CreateWholesaleServices(),
            CreateWholesaleServices(),
            CreateWholesaleServices(),
        ];

        // Query is executed twice (for both MonthlyAmountPerCharge and TotalMonthlyAmount) which gives double results
        var expectedSeriesCount = wholesaleServices.Count * 2;

        queries
            .Setup(q => q.GetAsync(It.IsAny<WholesaleServicesQueryParameters>()))
            .Returns(wholesaleServices.ToAsyncEnumerable());

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            queries.Object,
            mapper.Object,
            logger.Object);

        // Act
        await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert

        // Verify accepted response message
        ediClient.Verify(
            client => client.SendAsync(
                It.Is<ServiceBusMessage>(message =>
                    message.Subject.Equals(expectedAcceptedSubject) &&
                    WholesaleServicesRequestAccepted.Parser.ParseFrom(message.Body).Series.Count == expectedSeriesCount),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify wholesale query was executed once for MonthlyAmountPerCharge
        queries.Verify(
            q =>
                q.GetAsync(It.Is<WholesaleServicesQueryParameters>(queryParameters =>
                    queryParameters.AmountType == AmountType.MonthlyAmountPerCharge)),
            Times.Once);

        // Verify wholesale query was executed once for TotalMonthlyAmount
        queries.Verify(
            q =>
                q.GetAsync(It.Is<WholesaleServicesQueryParameters>(queryParameters =>
                    queryParameters.AmountType == AmountType.TotalMonthlyAmount)),
            Times.Once);

        // Verify that only the two queries above was executed
        queries.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithMonthlyResolutionAndAChargeType_QueriesMonthlyCalculationResults(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleServicesQueries> queries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger)
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(WholesaleServicesRequestAccepted);
        var expectedReferenceId = Guid.NewGuid().ToString();

        var chargeTypeInRequest = new Energinet.DataHub.Edi.Requests.ChargeType() { ChargeCode = "ChargeCode", ChargeType_ = ChargeType.Tariff.ToString(), };
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(new WholesaleServicesRequestBuilder()
                .WithResolution(DataHubNames.Resolution.Monthly)
                .WithChargeTypes(chargeTypeInRequest)
                .Build()
                .ToByteArray()));

        List<WholesaleServices> wholesaleServices =
        [
            CreateWholesaleServices(),
            CreateWholesaleServices(),
            CreateWholesaleServices(),
        ];

        // Query is executed once for both MonthlyAmountPerCharge
        var expectedSeriesCount = wholesaleServices.Count;

        queries
            .Setup(q => q.GetAsync(It.IsAny<WholesaleServicesQueryParameters>()))
            .Returns(wholesaleServices.ToAsyncEnumerable());

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            queries.Object,
            mapper.Object,
            logger.Object);

        // Act
        await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert

        // Verify accepted response message
        ediClient.Verify(
            client => client.SendAsync(
                It.Is<ServiceBusMessage>(message =>
                    message.Subject.Equals(expectedAcceptedSubject) &&
                    WholesaleServicesRequestAccepted.Parser.ParseFrom(message.Body).Series.Count == expectedSeriesCount),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify wholesale query was executed once for MonthlyAmountPerCharge
        queries.Verify(
            q =>
                q.GetAsync(It.Is<WholesaleServicesQueryParameters>(queryParameters =>
                    queryParameters.AmountType == AmountType.MonthlyAmountPerCharge)),
            Times.Once);

        // Verify wholesale query was never executed for TotalMonthlyAmount
        queries.Verify(
            q =>
                q.GetAsync(It.Is<WholesaleServicesQueryParameters>(queryParameters =>
                    queryParameters.AmountType == AmountType.TotalMonthlyAmount)),
            Times.Never);

        // Verify that only the two queries above was executed
        queries.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithNoWholesaleResultData_SendsRejectedEdiMessage(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleServicesQueries> queries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger)
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
            queries.Object,
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
        [Frozen] Mock<IWholesaleServicesQueries> queries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger)
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
            queries.Object,
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
    public async Task ProcessAsync_WithNoDataInRequestedGridArea_SendsRejectedEdiMessage(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleServicesQueries> queries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger)
    {
        // Arrange
        const string expectedRejectedSubject = nameof(WholesaleServicesRequestRejected);
        const string expectedValidationErrorCode = "D46";
        var expectedReferenceId = Guid.NewGuid().ToString();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(
                new WholesaleServicesRequestBuilder()
                    .WithGridAreaCode("123")
                    .WithRequestedByActorRole(DataHubNames.ActorRole.SystemOperator)
                    .Build().ToByteArray()));

        queries
            .Setup(parameters =>
                parameters.AnyAsync(
                    It.Is<WholesaleServicesQueryParameters>(x => x.GridAreaCodes.Count == 0)))
            .Returns(() => Task.FromResult(true));

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            queries.Object,
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

    private WholesaleServices CreateWholesaleServices()
    {
        var timeSeriesPoints = new List<WholesaleTimeSeriesPoint>
        {
            new(
                new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                1,
                Array.Empty<QuantityQuality>(),
                2,
                3),
        };

        return new WholesaleServices(
            new Period(
                Instant.FromUtc(2024, 1, 1, 0, 0),
                Instant.FromUtc(2024, 1, 2, 0, 0)),
            "001",
            "002",
            "003",
            ChargeType.Tariff,
            "004",
            AmountType.AmountPerCharge,
            Resolution.Day,
            QuantityUnit.Kwh,
            MeteringPointType.Consumption,
            SettlementMethod.Flex,
            Currency.DKK,
            CalculationType.WholesaleFixing,
            timeSeriesPoints,
            1);
    }
}
