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
    private static readonly ValidationError _invalidBalanceResponsible = new("Feltet BalanceResponsibleParty skal være udfyldt med et valid GLN/EIC når en balanceansvarlig anmoder om data / BalanceResponsibleParty must be submitted with a valid GLN/EIC when a balance responsible requests data", "E18");
    private static readonly ValidationError _mismatchedBalanceResponsibleInHeaderAndMessage = new("BalanceResponsibleParty i besked stemmer ikke overenes med balanceansvarlig anmoder i header / BalanceResponsibleParty in message does not correspond with balance responsible in header", "E18");

    private readonly BalanceResponsibleValidationRule _sut = new();

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndBalanceResponsibleFieldIsValidGlnNumber_ReturnsNoValidationErrors()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
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
            .WithRequestedByActorId(ValidEicNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .WithBalanceResponsibleId(ValidEicNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndMissingBalanceResponsibleField_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidBalanceResponsible.Message);
        error.ErrorCode.Should().Be(_invalidBalanceResponsible.ErrorCode);
    }

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndBalanceResponsibleFieldNotEqualRequestedById_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .WithBalanceResponsibleId(ValidEicNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_mismatchedBalanceResponsibleInHeaderAndMessage.Message);
        error.ErrorCode.Should().Be(_mismatchedBalanceResponsibleInHeaderAndMessage.ErrorCode);
    }

    [Fact]
    public void Validate_WhenRequesterIsBalanceResponsibleAndInvalidBalanceResponsibleField_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(BalanceResponsibleRole)
            .WithBalanceResponsibleId("invalid-format")
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidBalanceResponsible.Message);
        error.ErrorCode.Should().Be(_invalidBalanceResponsible.ErrorCode);
    }

    [Fact]
    public void Validate_WhenRequesterIsNotBalanceResponsibleAndMissingBalanceResponsibleField_ReturnsNoValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }
}
