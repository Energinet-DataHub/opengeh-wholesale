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
using Energinet.DataHub.Edi.Requests;
using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Energinet.DataHub.Wholesale.Edi.Calculations;
using Energinet.DataHub.Wholesale.Edi.Client;
using Energinet.DataHub.Wholesale.Edi.Models;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Extensions;
using Energinet.DataHub.Wholesale.Edi.Validation;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime.Extensions;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests;

public class WholesaleServicesRequestHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithNoValidationErrors_RunsSuccessfully(
        [Frozen] Mock<IEdiClient> ediClient,
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger)
    {
        // Arrange
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = new WholesaleServicesRequestBuilder()
            .Build();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(v => v.ValidateAsync(
                It.IsAny<WholesaleServicesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>());

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
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
        [Frozen] Mock<IValidator<WholesaleServicesRequest>> validator,
        [Frozen] Mock<ILogger<WholesaleServicesRequestHandler>> logger)
    {
        // Arrange
        var expectedReferenceId = Guid.NewGuid().ToString();
        var request = new WholesaleServicesRequestBuilder()
            .Build();

        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            properties: new Dictionary<string, object> { { "ReferenceId", expectedReferenceId } },
            body: new BinaryData(request.ToByteArray()));

        validator.Setup(v => v.ValidateAsync(
                It.IsAny<WholesaleServicesRequest>()))
            .ReturnsAsync(() => new List<ValidationError>
            {
                new("A validation error", "001"),
            });

        var sut = new WholesaleServicesRequestHandler(
            ediClient.Object,
            validator.Object,
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
