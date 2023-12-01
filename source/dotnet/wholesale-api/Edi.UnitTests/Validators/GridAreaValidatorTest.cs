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

using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Repositories;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSeries.Rules;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class GridAreaValidatorTest
{
    private const string MeteredDataResponsible = "MDR";
    private const string ValidGlnNumber = "qwertyuiopasd"; // Must be 13 characters to be a valid GLN
    private static readonly ValidationError _missingGridAreaCode = new("Netområde er obligatorisk for rollen MDR / Grid area is mandatory for the role MDR.", "D64");
    private static readonly ValidationError _invalidGridArea = new("ugyldig netområde / invalid gridarea", "E86");
    private readonly GridAreaOwnerRepositoryInMemory _gridAreaOwnerRepository = new();
    private readonly GridAreaValidationRule _sut;

    public GridAreaValidatorTest()
    {
        _sut = new GridAreaValidationRule(_gridAreaOwnerRepository);
    }

    [Fact]
    public async Task Validate_WhenRequesterIsGridOwnerOfRequestedGridArea_ReturnsNoValidationErrorsAsync()
    {
        // Arrange
        var gridAreaCode = "123";
        await _gridAreaOwnerRepository.AddAsync(gridAreaCode, ValidGlnNumber, Instant.MinValue, 0);
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(MeteredDataResponsible)
            .WithGridArea(gridAreaCode)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenRequesterIsNotGridOwnerOfRequestedGridArea_ReturnsExpectedValidationErrorAsync()
    {
        // Arrange
        var gridAreaCode = "123";
        var gridAreaOwner = "qwertyuiopash";
        await _gridAreaOwnerRepository.AddAsync(gridAreaCode, gridAreaOwner, Instant.MinValue, 0);
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(MeteredDataResponsible)
            .WithGridArea(gridAreaCode)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidGridArea.Message);
        error.ErrorCode.Should().Be(_invalidGridArea.ErrorCode);
    }

    [Fact]
    public async Task Validate_WhenGridAreaCodeIsEmpty_ReturnsExpectedValidationErrorAsync()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(MeteredDataResponsible)
            .WithGridArea(null)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_missingGridAreaCode.Message);
        error.ErrorCode.Should().Be(_missingGridAreaCode.ErrorCode);
    }

    // [Fact]
    // public async Task Validate_WhenGridAreaCodeDoesNotExist_ReturnsExpectedValidationErrorAsync()
    // {
    //     // Arrange
    //     var gridAreaCode = "123";
    //     var message = AggregatedTimeSeriesRequestBuilder
    //         .AggregatedTimeSeriesRequest()
    //         .WithRequestedByActorId(ValidGlnNumber)
    //         .WithRequestedByActorRole(MeteredDataResponsible)
    //         .WithGridArea(gridAreaCode)
    //         .Build();
    //
    //     // Act
    //     var errors = await _sut.ValidateAsync(message);
    //
    //     // Assert
    //     errors.Should().ContainSingle();
    //
    //     var error = errors.First();
    //     error.Message.Should().Be(_missingGridAreaCode.Message);
    //     error.ErrorCode.Should().Be(_missingGridAreaCode.ErrorCode);
    // }
}
