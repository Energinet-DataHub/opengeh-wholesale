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

using Energinet.DataHub.Wholesale.EDI.Models;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSeries.Rules;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class EnergySupplierValidatorTest
{
    public const string ValidGlnNumber = "qwertyuiopasd"; // Must be 13 characters to be a valid GLN
    private const string ValidEicNumber = "qwertyuiopasdfgh"; // Must be 16 characters to be a valid GLN

    private static readonly ValidationError _invalidEnergySupplier = new("Feltet EnergySupplier skal være udfyldt med et valid GLN/EIC nummer når en elleverandør anmoder om data / EnergySupplier must be submitted with a valid GLN/EIC number when an energy supplier requests data", "E16");
    private static readonly ValidationError _notEqualToRequestedBy = new("Elleverandør i besked stemmer ikke overenes med elleverandør i header / Energy supplier in message does not correspond with energy supplier in header", "E16");

    private readonly EnergySupplierValidationRule _sut = new();

    [Fact]
    public void Validate_WhenEnergySupplierAndEnergySupplierIsValidGlnNumber_ReturnsNoValidationErrors()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(ValidGlnNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenEnergySupplierAndEnergySupplierIsValidEicNumber_ReturnsNoValidationErrors()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidEicNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(ValidEicNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenEnergySupplierAndMissingEnergySupplier_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(null)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidEnergySupplier.Message);
        error.ErrorCode.Should().Be(_invalidEnergySupplier.ErrorCode);
    }

    [Fact]
    public void Validate_WhenEnergySupplierAndEnergySupplierNotEqualRequestedById_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(ValidEicNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_notEqualToRequestedBy.Message);
        error.ErrorCode.Should().Be(_notEqualToRequestedBy.ErrorCode);
    }

    [Fact]
    public void Validate_WhenEnergySupplierAndInvalidFormatEnergySupplier_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId("invalid-format")
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidEnergySupplier.Message);
        error.ErrorCode.Should().Be(_invalidEnergySupplier.ErrorCode);
    }

    [Fact]
    public void Validate_WhenNotEnergySupplierAndMissingEnergySupplier_ReturnsNoValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole("not-energy-supplier")
            .WithEnergySupplierId(null)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenNotEnergySupplierAndInvalidEnergySupplierFormat_ReturnsNoValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole("not-energy-supplier")
            .WithEnergySupplierId("invalid-format")
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_IsNotEnergySupplierAndEnergySupplierNotEqualRequestedById_ReturnsNoValidationError()
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorId(ValidGlnNumber)
            .WithRequestedByActorRole("not-energy-supplier")
            .WithEnergySupplierId(ValidEicNumber)
            .Build();

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }
}
