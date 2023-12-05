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
using Energinet.DataHub.Wholesale.Batches.Application.GridArea;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSeries.Rules;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class GridAreaValidatorTest
{
    private const string MeteredDataResponsible = "MDR";
    private const string ValidGlnNumber = "qwertyuiopasd"; // Must be 13 characters to be a valid GLN
    private static readonly ValidationError _missingGridAreaCode = new("Netområde er obligatorisk for rollen MDR / Grid area is mandatory for the role MDR.", "D64");
    private static readonly ValidationError _invalidGridArea = new("ugyldig netområde / invalid gridarea", "E86");

    [Theory]
    [InlineAutoMoqData]
    public async Task Validate_WhenRequesterIsGridOwnerOfRequestedGridArea_ReturnsNoValidationErrorsAsync(
        [Frozen] Mock<IGridAreaOwnerRepository> gridAreaOwnerRepository,
        GridAreaValidationRule sut)
    {
        // Arrange
        var gridAreaCode = "123";

        gridAreaOwnerRepository.Setup(repo => repo.GetCurrentOwnerAsync(gridAreaCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridAreaOwner(
                Guid.NewGuid(),
                gridAreaCode,
                ValidGlnNumber,
                Instant.MinValue,
                0));

        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(MeteredDataResponsible)
            .WithGridArea(gridAreaCode)
            .Build();

        // Act
        var errors = await sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Validate_WhenRequesterIsNotGridOwnerOfRequestedGridArea_ReturnsExpectedValidationErrorAsync(
        [Frozen] Mock<IGridAreaOwnerRepository> gridAreaOwnerRepository,
        GridAreaValidationRule sut)
    {
        // Arrange
        var gridAreaCode = "123";
        var gridAreaOwner = "qwertyuiopash";

        gridAreaOwnerRepository.Setup(repo => repo.GetCurrentOwnerAsync(gridAreaCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridAreaOwner(
                Guid.NewGuid(),
                gridAreaCode,
                gridAreaOwner,
                Instant.MinValue,
                0));

        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(MeteredDataResponsible)
            .WithGridArea(gridAreaCode)
            .Build();

        // Act
        var errors = await sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidGridArea.Message);
        error.ErrorCode.Should().Be(_invalidGridArea.ErrorCode);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Validate_WhenGridAreaCodeIsEmpty_ReturnsExpectedValidationErrorAsync(
        GridAreaValidationRule sut)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(MeteredDataResponsible)
            .WithGridArea(null)
            .Build();

        // Act
        var errors = await sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_missingGridAreaCode.Message);
        error.ErrorCode.Should().Be(_missingGridAreaCode.ErrorCode);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Validate_WhenGridAreaCodeDoesNotExist_ReturnsExpectedValidationErrorAsync(
        [Frozen] Mock<IGridAreaOwnerRepository> gridAreaOwnerRepository,
        GridAreaValidationRule sut)
    {
        // Arrange
        var notExistingGridAreaCode = "404";

        gridAreaOwnerRepository.Setup(repo => repo.GetCurrentOwnerAsync(notExistingGridAreaCode, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<GridAreaOwner?>(null));

        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(MeteredDataResponsible)
            .WithGridArea(notExistingGridAreaCode)
            .Build();

        // Act
        var errors = await sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidGridArea.Message);
        error.ErrorCode.Should().Be(_invalidGridArea.ErrorCode);
    }
}
