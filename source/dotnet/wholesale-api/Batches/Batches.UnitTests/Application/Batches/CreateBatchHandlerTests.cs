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
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Test.Core;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Application.Batches;

[UnitTest]
public class CreateBatchHandlerTests
{
    [Theory]
    [InlineAutoMoqData(ProcessType.BalanceFixing)]
    [InlineAutoMoqData(ProcessType.Aggregation)]
    [InlineAutoMoqData(ProcessType.WholesaleFixing)]
    [InlineAutoMoqData(ProcessType.FirstCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.SecondCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.ThirdCorrectionSettlement)]
    public async Task Handle_AddsBatchToRepository(
        ProcessType processType,
        [Frozen] Mock<IBatchFactory> batchFactoryMock,
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        CreateBatchHandler sut)
    {
        // Arrange
        var period = Periods.January_EuropeCopenhagen_Instant;
        var periodStart = period.PeriodStart.ToDateTimeOffset();
        var periodEnd = period.PeriodEnd.ToDateTimeOffset();
        var gridAreaCodes = new List<string> { "805" };
        var batchCommand = CreateBatchCommand(processType, periodStart, periodEnd, gridAreaCodes);
        var batch = CreateBatchFromCommand(batchCommand);
        batchFactoryMock.Setup(x => x.Create(batch.ProcessType, batchCommand.GridAreaCodes, batchCommand.StartDate, batchCommand.EndDate, batchCommand.CreatedByUserId))
            .Returns(batch);

        // Act
        var actual = await sut.HandleAsync(batchCommand);

        // Assert
        batch.Id.Should().Be(actual);
        batchRepositoryMock.Verify(x => x.AddAsync(batch));
    }

    [Theory]
    [InlineAutoMoqData(ProcessType.WholesaleFixing)]
    [InlineAutoMoqData(ProcessType.FirstCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.SecondCorrectionSettlement)]
    [InlineAutoMoqData(ProcessType.ThirdCorrectionSettlement)]
    public void Handle_BadBatch_ErrorThrownWhenPeriodNotOneMonthForCertainProcessTypes(ProcessType processType)
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse("2021-12-31T23:00Z");
        var periodEnd = DateTimeOffset.Parse("2022-01-30T23:00Z");
        var gridAreaCodes = new List<string> { "805" };
        var processTypeName = Enum.GetName(typeof(ProcessType), processType)!;
        var expectedMessage =
            $"The period (start: 2021-12-31T23:00:00Z end: 2022-01-30T23:00:00Z) has to be an entire month when using process type {processTypeName}.";
        var batchCommand = CreateBatchCommand(processType, periodStart, periodEnd, gridAreaCodes);

        // Act
        Action act = () => CreateBatchFromCommand(batchCommand);

        // Assert
        act.Should().Throw<BusinessValidationException>().WithMessage(expectedMessage);
    }

    [Theory]
    [InlineAutoMoqData("2022-12-31T23:00Z", "2022-01-31T23:00Z", "periodStart is greater or equal to periodEnd")]
    [InlineAutoMoqData("2021-12-31T23:00Z", "2022-01-31T22:00Z", "The period end '2022-01-31T22:00:00Z' must be midnight.")]
    [InlineAutoMoqData("2021-12-31T22:00Z", "2022-01-31T23:00Z", "The period start '2021-12-31T22:00:00Z' must be midnight.")]
    public void Handle_BadBatch_ErrorThrownWhenPeriodIsWrong(
        string periodStartString,
        string periodEndString,
        string expectedMessage)
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse(periodStartString);
        var periodEnd = DateTimeOffset.Parse(periodEndString);
        var gridAreaCodes = new List<string> { "805" };
        var processType = ProcessType.BalanceFixing;
        var batchCommand = CreateBatchCommand(processType, periodStart, periodEnd, gridAreaCodes);

        // Act
        Action act = () => CreateBatchFromCommand(batchCommand);

        // Assert
        act.Should().Throw<BusinessValidationException>().WithMessage(expectedMessage);
    }

    private static CreateBatchCommand CreateBatchCommand(ProcessType processType, DateTimeOffset periodStart, DateTimeOffset periodEnd, IEnumerable<string> gridAreaCodes)
    {
        return new CreateBatchCommand(
            processType,
            gridAreaCodes,
            periodStart,
            periodEnd,
            Guid.NewGuid());
    }

    private static Batch CreateBatchFromCommand(CreateBatchCommand command)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Batch(
            SystemClock.Instance.GetCurrentInstant(),
            command.ProcessType,
            command.GridAreaCodes.Select(x => new GridAreaCode(x)).ToList(),
            command.StartDate.ToInstant(),
            command.EndDate.ToInstant(),
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone,
            command.CreatedByUserId);
    }
}
