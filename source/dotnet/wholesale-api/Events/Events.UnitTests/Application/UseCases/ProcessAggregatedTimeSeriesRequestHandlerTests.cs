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
using Energinet.DataHub.Wholesale.Events.Application.InboxEvents;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using Energinet.DataHub.Wholesale.Events.Infrastructure.InboxEvents;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.UseCases;

public class ProcessAggregatedTimeSeriesRequestHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task ProcessAsync_WithEmptyRequest_SendsRejectedEdiMessage(
        [Frozen] Mock<ICalculationResultQueries> calculationResultQueriesMock,
        [Frozen] Mock<IEdiClient> senderMock,
        [Frozen] Mock<AggregatedTimeSeriesMessageFactory> aggregatedTimeSeriesMessageFactoryMock)
    {
        // Arrange
        var expectedRejectedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var request = new object();
        var sut = new AggregatedTimeSeriesRequestHandler(
            calculationResultQueriesMock.Object,
            senderMock.Object,
            aggregatedTimeSeriesMessageFactoryMock.Object);

        // Act
        await sut.ProcessAsync(request, CancellationToken.None);

        // Assert
        senderMock.Verify(
            bus => bus.SendAsync(
            It.Is<ServiceBusMessage>(message => message.Subject.Equals(expectedRejectedSubject)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
