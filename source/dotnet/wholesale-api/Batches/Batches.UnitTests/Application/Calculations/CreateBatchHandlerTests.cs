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
using Energinet.DataHub.Wholesale.Batches.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Application.Calculations;

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
        [Frozen] Mock<ICalculationFactory> batchFactoryMock,
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
    [InlineData(ProcessType.WholesaleFixing)]
    [InlineData(ProcessType.FirstCorrectionSettlement)]
    [InlineData(ProcessType.SecondCorrectionSettlement)]
    [InlineData(ProcessType.ThirdCorrectionSettlement)]
    public void Handle_WhenPeriodNotOneMonthForCertainProcessTypes_ThrowBusinessValidationException(ProcessType processType)
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse("2021-12-31T23:00Z");
        var periodEnd = DateTimeOffset.Parse("2022-01-30T23:00Z");
        var gridAreaCodes = new List<string> { "805" };
        var batchCommand = CreateBatchCommand(processType, periodStart, periodEnd, gridAreaCodes);

        // Act
        var actual = () => CreateBatchFromCommand(batchCommand);

        // Assert
        actual.Should().Throw<BusinessValidationException>();
    }

    [Theory]
    [InlineData("2021-12-31T23:00Z", "2022-01-31T22:00Z")]
    [InlineData("2021-12-31T22:00Z", "2022-01-31T23:00Z")]
    public void Handle_WhenPeriodIsNotMidnight_ThrowBusinessValidationException(
        string periodStartString,
        string periodEndString)
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse(periodStartString);
        var periodEnd = DateTimeOffset.Parse(periodEndString);
        var gridAreaCodes = new List<string> { "805" };
        var processType = ProcessType.BalanceFixing;
        var batchCommand = CreateBatchCommand(processType, periodStart, periodEnd, gridAreaCodes);

        // Act
        var actual = () => CreateBatchFromCommand(batchCommand);

        // Assert
        actual.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void Handle_WhenPeriodStartIsLaterThePeriodEnd_ThrowBusinessValidationException()
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse("2022-12-31T23:00Z");
        var periodEnd = DateTimeOffset.Parse("2022-01-31T23:00Z");
        var gridAreaCodes = new List<string> { "805" };
        var processType = ProcessType.BalanceFixing;
        var batchCommand = CreateBatchCommand(processType, periodStart, periodEnd, gridAreaCodes);

        // Act
        var actual = () => CreateBatchFromCommand(batchCommand);

        // Assert
        actual.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void Handle_WhenNoGridAreaCodes_ThrowBusinessValidationException()
    {
        // Arrange
        var periodStart = DateTimeOffset.Parse("2021-12-31T23:00Z");
        var periodEnd = DateTimeOffset.Parse("2022-01-31T23:00Z");
        var gridAreaCodes = new List<string> { };
        var processType = ProcessType.BalanceFixing;
        var batchCommand = CreateBatchCommand(processType, periodStart, periodEnd, gridAreaCodes);

        // Act
        var actual = () => CreateBatchFromCommand(batchCommand);

        // Assert
        actual.Should().Throw<BusinessValidationException>();
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

    private static Calculation CreateBatchFromCommand(CreateBatchCommand command)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Calculation(
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
