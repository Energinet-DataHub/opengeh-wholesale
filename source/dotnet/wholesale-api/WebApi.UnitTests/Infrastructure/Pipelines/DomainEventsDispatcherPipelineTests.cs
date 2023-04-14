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
using Energinet.DataHub.Wholesale.Infrastructure.EventDispatching.Domain;
using Energinet.DataHub.Wholesale.Infrastructure.Pipelines;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.Pipelines;

[UnitTest]
public class DomainEventsDispatcherPipelineTests
{
    private readonly Guid _guid = Guid.NewGuid();

    [Theory]
    [AutoMoqData]
    public async Task Handle_WhenCalled_CallsCommit(
        [Frozen] Mock<IDomainEventDispatcher> unitOfWorkMock,
        CreateBatchCommand createBatchCommand,
        DomainEventsDispatcherPipelineBehavior<CreateBatchCommand, Guid> sut)
    {
        // Arrange & Act
        var cancellationToken = new CancellationToken(false);
        await sut.Handle(createBatchCommand, RequestHandlerDelegate, cancellationToken);

        // Assert
        unitOfWorkMock.Verify(x => x.DispatchAsync(cancellationToken));
    }

    private async Task<Guid> RequestHandlerDelegate()
    {
        return await Task.Run(() => _guid);
    }
}
