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
using Energinet.DataHub.Core.TestCommon.FluentAssertionsExtensions;
using Energinet.DataHub.Wholesale.Application.BatchActor;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchActor;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using FluentAssertions;
using Moq;
using Test.Core;
using Xunit;
using Xunit.Categories;
using MarketRoleType = Energinet.DataHub.Wholesale.Domain.BatchActor.MarketRoleType;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Tests.Application.BatchActor;

[UnitTest]
public class BatchActorApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnBatchActorWithGln(
        BatchActorRequestDto request,
        Wholesale.Domain.BatchActor.BatchActor actor,
        [Frozen] Mock<IBatchActorRepository> repositoryMock,
        BatchActorApplicationService sut)
    {
        // Arrange
        request.SetPrivateProperty(dto => dto.GridAreaCode, "123");
        request.SetPrivateProperty(dto => dto.Type, Contracts.TimeSeriesType.Production);
        request.SetPrivateProperty(dto => dto.MarketRoleType, Contracts.MarketRoleType.EnergySupplier);

        repositoryMock
            .Setup(repository => repository.GetAsync(request.BatchId, new GridAreaCode(request.GridAreaCode), TimeSeriesType.Production, MarketRoleType.EnergySupplier))
            .ReturnsAsync(() => new[] { actor });

        // Act
        var actual = await sut.GetAsync(request);

        actual.First().Gln.Should().Be(actor.Gln);
    }
}
