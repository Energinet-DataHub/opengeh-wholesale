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
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class BalanceResponsibleValidatorTest
{
    private const string BalanceResponsibleRole = "DDK";
    private const string ValidGlnNumber = "qwertyuiopasd"; // Must be 13 characters to be a valid GLN
    private const string ValidEicNumber = "qwertyuiopasdfgh"; // Must be 16 characters to be a valid GLN

    private readonly BalanceResponsibleValidationRule _sut = new();

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndBalanceResponsibleFieldIsValidGlnNumber_ReturnsNoValidationErrors()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActor(ValidGlnNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .WithBalanceResponsibleId(ValidGlnNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndBalanceResponsibleFieldIsValidEicNumber_ReturnsNoValidationErrors()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActor(ValidEicNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .WithBalanceResponsibleId(ValidEicNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndMissingBalanceResponsibleField_ReturnsExceptedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActor(ValidGlnNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(ValidationError.InvalidBalanceResponsible.Message);
        error.ErrorCode.Should().Be(ValidationError.InvalidBalanceResponsible.ErrorCode);
    }

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndBalanceResponsibleFieldNotEqualRequestedById_ReturnsExceptedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActor(ValidGlnNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .WithBalanceResponsibleId(ValidEicNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(ValidationError.InvalidBalanceResponsible.Message);
        error.ErrorCode.Should().Be(ValidationError.InvalidBalanceResponsible.ErrorCode);
    }

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndInvalidBalanceResponsibleField_ReturnsExceptedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActor(ValidGlnNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .WithBalanceResponsibleId("invalid-format")
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(ValidationError.InvalidBalanceResponsible.Message);
        error.ErrorCode.Should().Be(ValidationError.InvalidBalanceResponsible.ErrorCode);
    }

    [Fact]
    public void Validate_WhenRequesterIsNotBalanceResponsibleAndMissingBalanceResponsibleField_NoValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActor(ValidGlnNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }
}
