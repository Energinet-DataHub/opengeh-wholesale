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
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class AggregatedTimeSeriesRequestValidatorTests
{
    private readonly IValidator<AggregatedTimeSeriesRequest> _sut;

    public AggregatedTimeSeriesRequestValidatorTests()
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddTransient<DateTimeZone>(s => DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!);
        serviceCollection.AddTransient<IClock>(s => SystemClock.Instance);
        EdiRegistration.AddAggregatedTimeSeriesRequestValidation(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        _sut = serviceProvider.GetRequiredService<IValidator<AggregatedTimeSeriesRequest>>();
    }

    [Fact]
    public void Validate_WhenAggregatedTimeSeriesRequestIsValid_ReturnsSuccessValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString())
            .WithRequestedByActorId(EnergySupplierValidatorTest.ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(EnergySupplierValidatorTest.ValidGlnNumber)
            .Build();

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenPeriodSizeIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(Instant.FromUtc(2022, 3, 2, 23, 0, 0).ToString())
            .WithRequestedByActorId(EnergySupplierValidatorTest.ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(EnergySupplierValidatorTest.ValidGlnNumber)
            .Build();

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    [Fact]
    public void Validate_WhenMeteringPointTypeIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString())
            .WithMeteringPointType("invalid")
            .WithRequestedByActorId(EnergySupplierValidatorTest.ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(EnergySupplierValidatorTest.ValidGlnNumber)
            .Build();

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    [Fact]
    public void Validate_WhenEnergySupplierIdIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString())
            .WithRequestedByActorId(EnergySupplierValidatorTest.ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId("invalid-id")
            .Build();

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    [Fact]
    public void Validate_WhenSettlementMethodIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString())
            .WithSettlementMethod("invalid-settlement-method")
            .WithRequestedByActorId(EnergySupplierValidatorTest.ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(EnergySupplierValidatorTest.ValidGlnNumber)
            .Build();

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }

    [Fact]
    public void Validate_AsEnergySupplierTotalConsumption_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(Instant.FromUtc(2022, 1, 2, 23, 0, 0).ToString())
            .WithMeteringPointType(MeteringPointType.Consumption)
            .WithSettlementMethod(null)
            .WithRequestedByActorId(EnergySupplierValidatorTest.ValidGlnNumber)
            .WithRequestedByActorRole(ActorRoleCode.EnergySupplier)
            .WithEnergySupplierId(EnergySupplierValidatorTest.ValidGlnNumber)
            .Build();

        // Act
        var validationErrors = _sut.Validate(request);

        // Assert
        validationErrors.Should().ContainSingle();
    }
}
