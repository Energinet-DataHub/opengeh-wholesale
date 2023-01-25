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
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Application.ProcessResult.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using FluentAssertions;
using Moq;
using Test.Core;
using Xunit;
using Xunit.Categories;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Tests.Application.ProcessResult;

[UnitTest]
public class ProcessResultApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetResultAsync_ReturnsDto(
        ProcessStepResultRequestDto request,
        ProcessStepResult result,
        ProcessStepResultDto resultDto,
        [Frozen] Mock<IProcessStepResultRepository> repositoryMock,
        [Frozen] Mock<IProcessStepResultMapper> mapperMock,
        ProcessStepResultApplicationService sut)
    {
        // Arrange
        request.SetPrivateProperty(dto => dto.GridAreaCode, "123");
        repositoryMock
            .Setup(repository => repository.GetAsync(request.BatchId, new GridAreaCode(request.GridAreaCode), TimeSeriesType.Production, "grid_area"))
            .ReturnsAsync(() => result);
        mapperMock
            .Setup(mapper => mapper.MapToDto(result))
            .Returns(() => resultDto);

        // Act
        var actual = await sut.GetResultAsync(request);

        actual.Should().BeEquivalentTo(resultDto);
    }
}
