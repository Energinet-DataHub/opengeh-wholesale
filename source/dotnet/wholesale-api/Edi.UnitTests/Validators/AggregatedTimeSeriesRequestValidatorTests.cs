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
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie;
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using FluentAssertions;
using NodaTime;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class AggregatedTimeSeriesRequestValidatorTests
{
    private static readonly PeriodValidationRule _periodValidator = new(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!, SystemClock.Instance);
    private static readonly MeteringPointTypeValidationRule _meteringPointTypeValidationRule = new();
    private static readonly EnergySupplierValidationRule _energySupplierValidationRule = new();
    private static readonly SettlementMethodValidationRule _settlementMethodValidationRule = new();
    private static readonly TimeSeriesTypeValidationRule _timeSeriesTypeValidationRule = new();
    private readonly IValidator<AggregatedTimeSeriesRequest> _sut = new AggregatedTimeSeriesRequestValidator(
        new IValidationRule<AggregatedTimeSeriesRequest>[]
        {
            _periodValidator,
            _energySupplierValidationRule,
            _meteringPointTypeValidationRule,
            _settlementMethodValidationRule,
            _timeSeriesTypeValidationRule,
        });

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_SuccessValidation()
    {
        // Arrange
        var request = CreateAggregatedTimeSeriesRequest();

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_WhenPeriodSizeIsInvalid_UnsuccessfulValidation()
    {
        // Arrange
        var request = CreateAggregatedTimeSeriesRequest(endDate: Instant.FromUtc(2022, 3, 2, 23, 0, 0).ToString());

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_WhenMeteringPointTypeIsInvalid_UnsuccessfulValidation()
    {
        // Arrange
        var request = CreateAggregatedTimeSeriesRequest(meteringPointType: "invalid");

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_WhenEnergySupplierIdIsInvalid_UnsuccessfulValidation()
    {
        // Arrange
        var request = CreateAggregatedTimeSeriesRequest(energySupplierId: "invadlid-id");

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_WhenSettlementMethodIsInvalid_UnsuccessfulValidation()
    {
        // Arrange
        var request = CreateAggregatedTimeSeriesRequest(settlementMethod: "invalid-settlement-method");

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    [Fact]
    public void Validate_AsEnergySupplierTotalConsumption_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request =
            CreateAggregatedTimeSeriesRequest(meteringPointType: MeteringPointType.Consumption, settlementMethod: null);

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    private AggregatedTimeSeriesRequest CreateAggregatedTimeSeriesRequest(
        string? startDate = null,
        string? endDate = null,
        string? meteringPointType = null,
        string? settlementMethod = null,
        string? requestedByActorRole = null,
        string? requestedByActorId = null,
        string? energySupplierId = null)
    {
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(startDate ?? Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(endDate ?? Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString())
            .WithMeteringPointType(meteringPointType ?? MeteringPointType.Production)
            .WithSettlementMethod(settlementMethod)
            .WithRequestedByActorId(requestedByActorId ?? EnergySupplierValidatorTest.ValidGlnNumber)
            .WithRequestedByActorRole(requestedByActorRole ?? ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(energySupplierId ?? EnergySupplierValidatorTest.ValidGlnNumber)
            .Build();

        return message;
    }
}
