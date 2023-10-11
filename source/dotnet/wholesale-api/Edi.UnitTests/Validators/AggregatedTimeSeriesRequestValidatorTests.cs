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
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class AggregatedTimeSeriesRequestValidatorTests
{
    private static readonly PeriodValidationRule _periodValidator = new(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);
    private static readonly EnergySupplierFieldValidationRule _energySupplierFieldValidationRule = new();
    private readonly IValidator<AggregatedTimeSeriesRequest> _sut = new AggregatedTimeSeriesRequestValidator(new IValidationRule<AggregatedTimeSeriesRequest>[] { _periodValidator, _energySupplierFieldValidationRule });

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_SuccessValidation()
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest()
        {
            Period = new Edi.Requests.Period()
            {
                Start = Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString(),
                End = Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString(),
            },
            RequestedByActorRole = EnergySupplierValidatorTest.EnergySupplierActorRole,
            RequestedByActorId = EnergySupplierValidatorTest.ValidGlnNumber,
            EnergySupplierId = EnergySupplierValidatorTest.ValidGlnNumber,
        };

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        Assert.False(validationErrors.Any());
    }
}
