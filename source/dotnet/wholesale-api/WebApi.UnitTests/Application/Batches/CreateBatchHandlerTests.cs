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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using FluentAssertions;
using MediatR;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Application.Batches;

[UnitTest]
public class CreateBatchHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task Handle_PublishesBatchCreatedEvent(
        [Frozen] Mock<IMediator> mediatrMock,
        [Frozen] Mock<IBatchFactory> batchFactoryMock,
        CreateBatchHandler sut)
    {
        // Arrange
        var batchCommand = CreateBatchCommand();
        var batch = CreateBatchFromCommand(batchCommand);
        batchFactoryMock.Setup(x => x.Create(batch.ProcessType, batchCommand.GridAreaCodes, batchCommand.StartDate, batchCommand.EndDate))
            .Returns(batch);

        // Act
        await sut.Handle(batchCommand, default);

        // Assert
        mediatrMock.Verify(x => x.Publish(It.Is<BatchCreatedDomainEvent>(e => e.BatchId == batch.Id), default));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Handle_CallsAddAsync(
        [Frozen] Mock<IBatchFactory> batchFactoryMock,
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        CreateBatchHandler sut)
    {
        // Arrange
        var batchCommand = CreateBatchCommand();
        var batch = CreateBatchFromCommand(batchCommand);
        batchFactoryMock.Setup(x => x.Create(batch.ProcessType, batchCommand.GridAreaCodes, batchCommand.StartDate, batchCommand.EndDate))
            .Returns(batch);

        // Act
        var actual = await sut.Handle(batchCommand, default);

        // Assert
        batch.Id.Should().Be(actual);
        batchRepositoryMock.Verify(x => x.AddAsync(batch));
    }

    private static CreateBatchCommand CreateBatchCommand()
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new CreateBatchCommand(
            Contracts.ProcessType.BalanceFixing,
            new List<string> { "805" },
            period.PeriodStart.ToDateTimeOffset(),
            period.PeriodEnd.ToDateTimeOffset());
    }

    private static Batch CreateBatchFromCommand(CreateBatchCommand command)
    {
        return new BatchFactory(SystemClock.Instance, DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!)
            .Create(ProcessType.BalanceFixing, command.GridAreaCodes, command.StartDate, command.EndDate);
    }
}
