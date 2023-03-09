﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Application.Processes;

public class ProcessApplicationServiceTest
{
    [Theory]
    [InlineAutoMoqData]
    public async Task PublishProcessCompletedEventsAsync_Publishes(
        BatchCompletedEventDto batchCompleted,
        [Frozen] Mock<IDomainEventPublisher> publisherMock,
        ProcessApplicationService sut)
    {
        await sut.PublishProcessCompletedEventsAsync(batchCompleted);
        publisherMock.Verify(publisher => publisher.PublishAsync(It.IsAny<List<ProcessCompletedEventDto>>()), Times.Once);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task PublishProcessCompletedIntegrationEventsAsync_Publishes(
        ProcessCompletedEventDto eventDto,
        [Frozen] Mock<IIntegrationEventPublisher> publisherMock,
        ProcessApplicationService sut)
    {
        await sut.PublishProcessCompletedIntegrationEventsAsync(eventDto);
        publisherMock.Verify(publisher => publisher.PublishAsync(eventDto), Times.Once);
    }
}
