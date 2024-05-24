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

using System.Diagnostics.CodeAnalysis;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.AggregatedTimeSeriesRequest;

[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Async suffix is not needed for test methods")]
public class GridAreaValidatorTest
{
    private const string ValidGlnNumber = "qwertyuiopasd"; // Must be 13 characters to be a valid GLN
    private static readonly ValidationError _missingGridAreaCode = new("Netområde er obligatorisk for rollen MDR / Grid area is mandatory for the role MDR.", "D64");
    private static readonly ValidationError _invalidGridArea = new("Ugyldig netområde / Invalid gridarea", "E86");

    [Theory]
    [InlineAutoMoqData]
    public async Task Validate_WhenRequesterIsGridOwnerOfRequestedGridArea_ReturnsNoValidationErrors(
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
            .WithRequestedByActorRole(DataHubNames.ActorRole.MeteredDataResponsible)
            .WithGridArea(gridAreaCode)
            .Build();

        // Act
        var errors = await sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Validate_WhenRequesterIsNotGridOwnerOfRequestedGridArea_ReturnsExpectedValidationError(
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
            .WithRequestedByActorRole(DataHubNames.ActorRole.MeteredDataResponsible)
            .WithGridArea(gridAreaCode)
            .Build();

        // Act
        var errors = await sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        using var assertionScope = new AssertionScope();
        var error = errors.Single();
        error.Message.Should().Be(_invalidGridArea.Message);
        error.ErrorCode.Should().Be(_invalidGridArea.ErrorCode);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Validate_WhenGridAreaCodeIsEmpty_ReturnsExpectedValidationError(
        GridAreaValidationRule sut)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(DataHubNames.ActorRole.MeteredDataResponsible)
            .WithGridArea(null)
            .Build();

        // Act
        var errors = await sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        using var assertionScope = new AssertionScope();
        var error = errors.Single();
        error.Message.Should().Be(_missingGridAreaCode.Message);
        error.ErrorCode.Should().Be(_missingGridAreaCode.ErrorCode);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task Validate_WhenGridAreaCodeDoesNotExist_ReturnsExpectedValidationError(
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
            .WithRequestedByActorRole(DataHubNames.ActorRole.MeteredDataResponsible)
            .WithGridArea(notExistingGridAreaCode)
            .Build();

        // Act
        var errors = await sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        using var assertionScope = new AssertionScope();
        var error = errors.Single();
        error.Message.Should().Be(_invalidGridArea.Message);
        error.ErrorCode.Should().Be(_invalidGridArea.ErrorCode);
    }
}
