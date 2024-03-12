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

using Energinet.DataHub.Wholesale.Edi.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.WholesaleServicesRequest;

public sealed class WholesaleServicesRequestValidatorTests
{
    private readonly IValidator<DataHub.Edi.Requests.WholesaleServicesRequest> _sut;

    public WholesaleServicesRequestValidatorTests()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddWholesaleServicesRequestValidation();

        var serviceProvider = services.BuildServiceProvider();

        _sut = serviceProvider.GetRequiredService<IValidator<DataHub.Edi.Requests.WholesaleServicesRequest>>();
    }

    [Fact]
    public async Task Validate_WhenWholesaleServicesRequestIsValid_ReturnsSuccessValidation()
    {
        // Arrange
        var request = new WholesaleServicesRequestBuilder()
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should().BeEmpty();
    }
}
