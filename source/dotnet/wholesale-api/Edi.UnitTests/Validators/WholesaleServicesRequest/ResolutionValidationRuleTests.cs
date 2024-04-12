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
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.WholesaleServicesRequest;

public class ResolutionValidationRuleTests
{
    private const string PropertyName = "aggregationSeries_Period.resolution";
    private static readonly ValidationError _notMonthlyResolution =
        new(
            $"{PropertyName} skal være 'P1M'/{PropertyName} must be 'P1M'",
            "D23");

    private readonly ResolutionValidationRule _sut;

    public ResolutionValidationRuleTests()
    {
        _sut = new ResolutionValidationRule();
    }

    [Fact]
    public async Task Validate_WhenResolutionIsMonthly_ReturnsNoErrors()
    {
        // Arrange
        var request = new WholesaleServicesRequestBuilder()
            .WithResolution("Monthly")
            .Build();

        // Act
        var actual = await _sut.ValidateAsync(request);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenResolutionIsMissing_ReturnsNoErrors()
    {
        // Arrange
        var request = new WholesaleServicesRequestBuilder()
            .WithResolution(null)
            .Build();

        // Act
        var actual = await _sut.ValidateAsync(request);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenResolutionIsNotMonthly_ReturnsError()
    {
        // Arrange
        var request = new WholesaleServicesRequestBuilder()
            .WithResolution("NotMonthly")
            .Build();

        // Act
        var actual = await _sut.ValidateAsync(request);

        // Assert
        actual.Should().HaveCount(1);
        actual.First().Message.Should().BeSameAs(_notMonthlyResolution.Message);
        actual.First().ErrorCode.Should().BeSameAs(_notMonthlyResolution.ErrorCode);
    }

    [Fact]
    public async Task Validate_WhenResolutionIsNotHourly_ReturnsErrors()
    {
        // Arrange
        var request = new WholesaleServicesRequestBuilder()
            .WithResolution("Hourly")
            .Build();

        // Act
        var actual = await _sut.ValidateAsync(request);

        // Assert
        actual.Should().HaveCount(1);
        actual.First().Message.Should().BeSameAs(_notMonthlyResolution.Message);
        actual.First().ErrorCode.Should().BeSameAs(_notMonthlyResolution.ErrorCode);
    }

    [Fact]
    public async Task Validate_WhenResolutionIsDaily_ReturnsErrors()
    {
        // Arrange
        var request = new WholesaleServicesRequestBuilder()
            .WithResolution("Daily")
            .Build();

        // Act
        var actual = await _sut.ValidateAsync(request);

        // Assert
        actual.Should().HaveCount(1);
        actual.First().Message.Should().BeSameAs(_notMonthlyResolution.Message);
        actual.First().ErrorCode.Should().BeSameAs(_notMonthlyResolution.ErrorCode);
    }
}
