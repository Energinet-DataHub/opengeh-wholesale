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
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.Persistence;

[UnitTest]
public class UnitOfWorkPipelineBehaviorTests
{
    private readonly Guid _guid = Guid.NewGuid();

    [Theory]
    [AutoMoqData]
    public async Task Handle_CallsCommit(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        CreateBatchCommand createBatchCommand,
        UnitOfWorkPipelineBehavior<CreateBatchCommand, Guid> sut)
    {
        // Arrange & Act
        await sut.Handle(createBatchCommand, RequestHandlerDelegate, default);

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }

    [Theory]
    [AutoMoqData]
    public async Task Handle_ReturnsGuid(
        CreateBatchCommand createBatchCommand,
        UnitOfWorkPipelineBehavior<CreateBatchCommand, Guid> sut)
    {
        // Arrange & Act
        var actual = await sut.Handle(createBatchCommand, RequestHandlerDelegate, default);

        // Assert
        actual.GetType().Should().Be(typeof(Guid));
        actual.Should().Be(_guid);
    }

    private async Task<Guid> RequestHandlerDelegate()
    {
        return await Task.Run(() => _guid);
    }
}
