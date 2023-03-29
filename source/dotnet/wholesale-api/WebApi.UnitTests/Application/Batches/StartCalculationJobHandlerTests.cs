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
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Application.Batches;

[UnitTest]
public class StartCalculationJobHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task Handle_ActivatesDomainService(
        [Frozen] Mock<ICalculationDomainService> calculationDomainServiceMock,
        Guid batchId,
        StartCalculationJobHandler sut)
    {
        // Arrange
        var batchCreatedDomainEvent = new BatchCreatedEvent(batchId);

        // Act
        await sut.Handle(batchCreatedDomainEvent, default);

        // Assert
        calculationDomainServiceMock.Verify(x => x.StartAsync(batchId));
    }
}
