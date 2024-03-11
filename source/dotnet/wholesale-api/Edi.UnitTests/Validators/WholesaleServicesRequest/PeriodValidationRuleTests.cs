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

using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.WholesaleServicesRequest;

public class PeriodValidationRuleTests
{
    private static readonly ValidationError _startDateMustBeLessThanOrEqualTo3YearsAnd2Months =
        new(
            "Der kan ikke anmodes om data for mere end 3 år og 2 måneder tilbage i tid/It is not possible to request data longer than 3 years and 2 months back in time",
            "E17");

    private readonly PeriodValidationRule _sut = new();

    [Fact]
    public async Task Validate_WhenPeriodIsOlderThenAllowed_ReturnsExpectedValidationError()
    {
        // Arrange
        var message = new WholesaleServicesRequestBuilder()
            .WithPeriodStart(Instant.FromUtc(2018, 1, 1, 23, 0, 0).ToString())
            .WithPeriodEnd(Instant.FromUtc(2018, 1, 2, 23, 0, 0).ToString())
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();
        var error = errors.First();
        error.ErrorCode.Should().Be(_startDateMustBeLessThanOrEqualTo3YearsAnd2Months.ErrorCode);
        error.Message.Should().Be(_startDateMustBeLessThanOrEqualTo3YearsAnd2Months.Message);
    }
}
