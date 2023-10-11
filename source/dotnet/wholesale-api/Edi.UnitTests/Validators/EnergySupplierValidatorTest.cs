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

using Energinet.DataHub.Edi.Requests;
using Energinet.DataHub.Wholesale.EDI.UnitTests.Builders;
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class EnergySupplierValidatorTest
{
    public const string EnergySupplierActorRole = "DDQ";
    public const string ValidGlnNumber = "qwertyuiopasd"; // Must be 13 characters to be a valid GLN
    private const string ValidEicNumber = "qwertyuiopasdfgh"; // Must be 16 characters to be a valid GLN

    private const string ExpectedErrorMessage = "Feltet EnergySupplier skal være udfyldt med et valid GLN/EIC nummer når en elleverandør anmoder om data / EnergySupplier must be submitted with a valid GLN/EIC number when an energy supplier requests data";
    private const string ExpectedErrorCode = "E16";

    private readonly EnergySupplierFieldValidationRule _sut = new();

    [Fact]
    public void Validate_IsEnergySupplierAndEnergySupplierFieldIsValidGlnNumber_NoValidationErrors()
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(true, ValidGlnNumber, ValidGlnNumber);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_IsEnergySupplierAndEnergySupplierFieldIsValidEicNumber_NoValidationErrors()
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(true, ValidEicNumber, ValidEicNumber);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_IsEnergySupplierAndMissingEnergySupplierField_ValidationError()
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(true, ValidGlnNumber, null);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        AssertSingleAndCorrectError(errors);
    }

    [Fact]
    public void Validate_IsEnergySupplierAndEnergySupplierFieldNotEqualRequestedById_ValidationError()
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(true, ValidGlnNumber, ValidEicNumber);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        AssertSingleAndCorrectError(errors);
    }

    [Fact]
    public void Validate_IsEnergySupplierAndInvalidFormatEnergySupplierField_ValidationError()
    {
        // Arrange
        // Valid numbers are 13 or 16 characters
        var message = CreateAggregatedTimeSeriesRequest(true, ValidGlnNumber, "invalid-format");

        // Act
        var errors = _sut.Validate(message);

        // Assert
        AssertSingleAndCorrectError(errors);
    }

    [Fact]
    public void Validate_IsNotEnergySupplierAndMissingEnergySupplierField_NoValidationError()
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(false, ValidGlnNumber, null);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_IsNotEnergySupplierAndInvalidEnergySupplierFieldFormat_NoValidationError()
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(false, ValidGlnNumber, "invalid-format");

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_IsNotEnergySupplierAndEnergySupplierFieldNotEqualRequestedById_NoValidationError()
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(false, ValidGlnNumber, ValidEicNumber);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        Assert.Empty(errors);
    }

    private static AggregatedTimeSeriesRequest CreateAggregatedTimeSeriesRequest(bool isRequestedByEnergySupplier, string requestedById, string? energySupplierId)
    {
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActor(isRequestedByEnergySupplier ? EnergySupplierActorRole : "unknown-role-id", requestedById)
            .WithEnergySupplierId(energySupplierId)
            .Build();

        return message;
    }

    private void AssertSingleAndCorrectError(IList<ValidationError> errors)
    {
        Assert.Single(errors);

        var error = errors.Single();
        Assert.Contains(ExpectedErrorMessage, error.Message);
        Assert.Contains(ExpectedErrorCode, error.ErrorCode);
    }
}
