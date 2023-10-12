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
using Energinet.DataHub.Wholesale.EDI.Validation;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using FluentAssertions;
using NodaTime;
using Xunit;
using Period = Energinet.DataHub.Edi.Requests.Period;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class AggregatedTimeSeriesRequestValidatorTests
{
    private const string ValidMeteringPointType = "E18";

    private static readonly PeriodValidationRule _periodValidator = new(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!, SystemClock.Instance);
    private static readonly MeteringPointTypeValidationRule _meteringPointTypeValidationRule = new();
    private static readonly EnergySupplierFieldValidationRule _energySupplierFieldValidationRule = new();
    private readonly IValidator<AggregatedTimeSeriesRequest> _sut = new AggregatedTimeSeriesRequestValidator(
        new IValidationRule<AggregatedTimeSeriesRequest>[]
        {
            _periodValidator,
            _energySupplierFieldValidationRule,
            _meteringPointTypeValidationRule,
        });

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_SuccessValidation()
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest()
        {
            Period = CreateValidPeriod(),
            RequestedByActorRole = EnergySupplierValidatorTest.EnergySupplierActorRole,
            RequestedByActorId = EnergySupplierValidatorTest.ValidGlnNumber,
            EnergySupplierId = EnergySupplierValidatorTest.ValidGlnNumber,
            MeteringPointType = ValidMeteringPointType,
        };

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_WhenPeriodSizeIsInvalid_UnsuccessfulValidation()
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest()
        {
            Period = new Period()
            {
                Start = Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString(),
                End = Instant.FromUtc(2022, 3, 2, 23, 0, 0).ToString(),
            },
            RequestedByActorRole = EnergySupplierValidatorTest.EnergySupplierActorRole,
            RequestedByActorId = EnergySupplierValidatorTest.ValidGlnNumber,
            EnergySupplierId = EnergySupplierValidatorTest.ValidGlnNumber,
            MeteringPointType = ValidMeteringPointType,
        };

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
        validationErrors.First().ErrorCode.Should().Be(ValidationError.PeriodIsGreaterThenAllowedPeriodSize.ErrorCode);
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_WhenMeteringPointTypeIsInvalid_UnsuccessfulValidation()
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest()
        {
            Period = CreateValidPeriod(),
            RequestedByActorRole = EnergySupplierValidatorTest.EnergySupplierActorRole,
            RequestedByActorId = EnergySupplierValidatorTest.ValidGlnNumber,
            EnergySupplierId = EnergySupplierValidatorTest.ValidGlnNumber,
            MeteringPointType = "Invalid",
        };

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
        validationErrors.First().ErrorCode.Should().Be(ValidationError.InvalidMeteringPointType.ErrorCode);
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_WhenEnergySupplierIdIsInvalid_UnsuccessfulValidation()
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest()
        {
            Period = CreateValidPeriod(),
            RequestedByActorRole = EnergySupplierValidatorTest.EnergySupplierActorRole,
            RequestedByActorId = EnergySupplierValidatorTest.ValidGlnNumber,
            EnergySupplierId = "invalid-id",
            MeteringPointType = ValidMeteringPointType,
        };

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();

        var validationError = validationErrors.First();
        validationError.Message.Should().Be(ValidationError.InvalidEnergySupplierField.Message);
        validationError.ErrorCode.Should().Be(ValidationError.InvalidEnergySupplierField.ErrorCode);
    }

    private Period CreateValidPeriod()
    {
        return new Period()
        {
            Start = Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString(),
            End = Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString(),
        };
    }
}
