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
using Energinet.DataHub.Wholesale.Calculations.Application;
using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Calculations.Application.UseCases;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Test.Core;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Application.Calculations;

[UnitTest]
public class CreateCalculationHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task Handle_AddsBatchToRepository(
        [Frozen] Mock<ICalculationFactory> batchFactoryMock,
        [Frozen] Mock<ICalculationRepository> batchRepositoryMock,
        CreateCalculationHandler sut)
    {
        // Arrange
        var batchCommand = CreateBatchCommand();
        var batch = CreateBatchFromCommand(batchCommand);
        batchFactoryMock.Setup(x => x.Create(batch.ProcessType, batchCommand.GridAreaCodes, batchCommand.StartDate, batchCommand.EndDate, batchCommand.CreatedByUserId))
            .Returns(batch);

        // Act
        var actual = await sut.HandleAsync(batchCommand);

        // Assert
        batch.Id.Should().Be(actual);
        batchRepositoryMock.Verify(x => x.AddAsync(batch));
    }

    private static CreateBatchCommand CreateBatchCommand()
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new CreateBatchCommand(
            ProcessType.BalanceFixing,
            new List<string> { "805" },
            period.PeriodStart.ToDateTimeOffset(),
            period.PeriodEnd.ToDateTimeOffset(),
            Guid.NewGuid());
    }

    private static Calculation CreateBatchFromCommand(CreateBatchCommand command)
    {
        var period = Periods.January_EuropeCopenhagen_Instant;
        return new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            ProcessType.BalanceFixing,
            command.GridAreaCodes.Select(x => new GridAreaCode(x)).ToList(),
            command.StartDate.ToInstant(),
            command.EndDate.ToInstant(),
            SystemClock.Instance.GetCurrentInstant(),
            period.DateTimeZone,
            command.CreatedByUserId);
    }
}
