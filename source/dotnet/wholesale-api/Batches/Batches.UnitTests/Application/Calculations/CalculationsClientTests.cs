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
using Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.CalculationAggregate;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Application.Calculations;

public class CalculationsClientTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_NoMatchingBatches_ReturnsZeroBatches(
       [Frozen] Mock<IBatchRepository> batchRepositoryMock,
       CalculationsClient sut)
    {
        // Arrange
        var noBatches = new List<Calculation>();
        batchRepositoryMock
            .Setup(x => x.SearchAsync(
                Array.Empty<GridAreaCode>(),
                Array.Empty<CalculationExecutionState>(),
                null,
                null,
                null,
                null))
            .ReturnsAsync(noBatches);

        // Act
        var searchResult = await sut.SearchAsync(
            Array.Empty<string>(),
            null,
            null,
            null,
            null,
            null);

        // Assert
        searchResult.Count().Should().Be(0);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task SearchAsync_ReturnsCorrectNumberOfBatches(
        [Frozen] Mock<IBatchRepository> batchRepositoryMock,
        CalculationsClient sut)
    {
        // Arrange
        const int numberOfBatches = 3;
        var batches = new List<Calculation>()
        {
            new CalculationBuilder().Build(),
            new CalculationBuilder().Build(),
            new CalculationBuilder().Build(),
        };

        batchRepositoryMock
            .Setup(x => x.SearchAsync(
                It.IsAny<List<GridAreaCode>>(),
                Array.Empty<CalculationExecutionState>(),
                null,
                null,
                null,
                null))
            .ReturnsAsync(batches);

        // Act
        var searchResult = await sut.SearchAsync(
            Array.Empty<string>(),
            null,
            null,
            null,
            null,
            null);

        // Assert
        searchResult.Count().Should().Be(numberOfBatches);
    }
}
