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

using Energinet.DataHub.Wholesale.Application.Infrastructure;
using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Application;

[UnitTest]
public class ProcessResultApplicationServiceTests
{
    [Fact]
    public async Task Do_the_testAsync()
    {
        // Arrange
        var mock = new Mock<IBatchFileManager>();
        var stream =
            new FileStream(
                "C:\\git\\opengeh-wholesale\\source\\dotnet\\Services\\Tests\\Application\\JsonNewLineStuff.json", FileMode.Open);

        mock.Setup(x => x.GetResultFileStreamAsync(It.IsAny<Guid>(), It.IsAny<GridAreaCode>()))
            .ReturnsAsync(stream);

        var sut = new ProcessResultApplicationService(mock.Object);

        // Act
        var actual = await sut.GetResultAsync(Guid.NewGuid(), "805", ProcessStepType.AggregateProductionPerGridArea);

        Assert.NotNull(actual);
    }
}
