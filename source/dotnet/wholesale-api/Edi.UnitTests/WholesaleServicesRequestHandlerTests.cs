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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.Edi.Calculations;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Factories;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Period = Energinet.DataHub.Wholesale.Edi.Models.Period;
using WholesaleServicesRequest = Energinet.DataHub.Edi.Requests.WholesaleServicesRequest;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests;

public class WholesaleServicesRequestHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithNoValidationErrors_RunsSuccessfully(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger,
        [Frozen] Mock<CompletedCalculationRetriever> completedCalculationRetriever)
    {
        // Arrange
        var expectedReferenceId = Guid.NewGuid().ToString();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(new WholesaleServicesRequestBuilder().Build().ToByteArray()));

        validator.Setup(v => v.ValidateAsync(
                It.IsAny<WholesaleServicesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>());

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            completedCalculationRetriever.Object,
            wholesaleResultQueries.Object,
            mapper.Object,
            logger.Object);

        // Act
        var act = async () => await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert
        // TODO: Update to "sends accepted message"
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithValidationErrors_ThrowsNotImplementedException(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IWholesaleResultQueries> wholesaleResultQueries,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<WholesaleServicesRequestMapper> mapper,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger,
        [Frozen] Mock<CompletedCalculationRetriever> completedCalculationRetriever)
    {
        // Arrange
        var expectedReferenceId = Guid.NewGuid().ToString();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(new WholesaleServicesRequestBuilder().Build().ToByteArray()));

        validator.Setup(v => v.ValidateAsync(
                It.IsAny<WholesaleServicesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>
            {
                new("A validation error", "001"),
            });

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
            completedCalculationRetriever.Object,
            wholesaleResultQueries.Object,
            mapper.Object,
            logger.Object);

        // Act
        var act = async () => await sut.ProcessAsync(
            serviceBusReceivedMessage,
            expectedReferenceId,
            CancellationToken.None);

        // Assert
        // TODO: Update to "sends rejected message"
        await act.Should().ThrowExactlyAsync<NotImplementedException>();
    }
}
