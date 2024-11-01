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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.Wholesale.Calculations.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.Wholesale.Calculations.Interfaces.GridArea;
using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.Extensions.DependencyInjection;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Edi.Validation.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.AggregatedTimeSeriesRequest;

[SuppressMessage(
    "StyleCop.CSharp.LayoutRules",
    "SA1512:Single-line comments should not be followed by blank line",
    Justification = "Tests")]
public class AggregatedTimeSeriesRequestValidatorTests
{
    private readonly IValidator<DataHub.Edi.Requests.AggregatedTimeSeriesRequest> _sut;
    private readonly Mock<IClock> _clockMock;
    private readonly DateTimeZone _timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;

    public AggregatedTimeSeriesRequestValidatorTests()
    {
        _clockMock = new Mock<IClock>();
        _clockMock.Setup(c => c.GetCurrentInstant()).Returns(Instant.FromUtc(2024, 11, 15, 16, 46, 43));

        IServiceCollection services = new ServiceCollection();

        services.AddTransient<DateTimeZone>(s => _timeZone);
        services.AddTransient<IClock>(s => _clockMock.Object);
        services.AddTransient<PeriodValidationHelper>();
        services.AddScoped<IGridAreaOwnerRepository, GridAreaOwnerRepository>();
        services.AddScoped<IDatabaseContext, DatabaseContext>();

        services.AddAggregatedTimeSeriesRequestValidation();

        var serviceProvider = services.BuildServiceProvider();

        _sut = serviceProvider.GetRequiredService<IValidator<DataHub.Edi.Requests.AggregatedTimeSeriesRequest>>();
    }

    [Fact]
    public async Task Validate_WhenAggregatedTimeSeriesRequestIsValid_ReturnsSuccessValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenPeriodSizeIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToString())
            .WithEndDate(Instant.FromUtc(2022, 3, 2, 23, 0, 0).ToString())
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be("E17");
    }

    [Fact]
    public async Task
        Validate_WhenPeriodIsMoreThan3AndAHalfYearBackInTimeButPartOfCutOffMonth_ReturnsSuccessfulValidation()
    {
        // Prerequisite: The current time is NOT the start of a month.
        _clockMock.Object.GetCurrentInstant().ToDateTimeUtc().Date.Should().NotHaveDay(1).And.NotHaveDay(2);

        // Arrange
        var periodStart = _clockMock.Object.GetCurrentInstant() // Assuming 2024-11-15 16:46:43 UTC
            .InZone(_timeZone) // 2024-11-15 17:46:43 CET
            .Date.PlusYears(-3) // 2021-11-15
            .PlusMonths(-6) // 2021-05-15
            .With(DateAdjusters.StartOfMonth) // 2021-05-01
            .AtMidnight() // 2021-05-01 00:00:00 UTC
            .InZoneStrictly(_timeZone) // 2021-05-01 00:00:00 CEST
            .ToInstant(); // 2021-04-30 22:00:00 UTC

        var periodEnd = _clockMock.Object.GetCurrentInstant() // Assuming 2024-11-15 16:46:43 UTC
            .InZone(_timeZone) // 2024-11-15 17:46:43 CET
            .Date.PlusYears(-3) // 2021-11-15
            .PlusMonths(-6) // 2021-05-15
            .With(DateAdjusters.StartOfMonth) // 2021-05-01
            .PlusDays(1) // 2021-05-02
            .AtMidnight() // 2021-05-02 00:00:00 UTC
            .InZoneStrictly(_timeZone) // 2021-05-02 00:00:00 CEST
            .ToInstant(); // 2021-05-01 22:00:00 UTC

        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(periodStart.ToString())
            .WithEndDate(periodEnd.ToString())
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task
        Validate_WhenPeriodOverlapsCutOffAt3AndAHalfYearBackInTime_ReturnsSuccessfulValidation()
    {
        // Prerequisite: The current time is NOT the start of a month.
        _clockMock.Object.GetCurrentInstant().ToDateTimeUtc().Date.Should().NotHaveDay(1).And.NotHaveDay(2);

        // Arrange
        var periodStart = _clockMock.Object.GetCurrentInstant() // Assuming 2024-11-15 16:46:43 UTC
            .InZone(_timeZone) // 2024-11-15 17:46:43 CET
            .Date.PlusYears(-3) // 2021-11-15
            .PlusMonths(-6) // 2021-05-15
            .PlusDays(-1) // 2021-05-14
            .AtMidnight() // 2021-05-14 00:00:00 UTC
            .InZoneStrictly(_timeZone) // 2021-05-14 00:00:00 CEST
            .ToInstant(); // 2021-05-13 22:00:00 UTC

        var periodEnd = _clockMock.Object.GetCurrentInstant() // Assuming 2024-11-15 16:46:43 UTC
            .InZone(_timeZone) // 2024-11-15 17:46:43 CET
            .Date.PlusYears(-3) // 2021-11-15
            .PlusMonths(-6) // 2021-05-15
            .PlusDays(1) // 2021-05-16
            .AtMidnight() // 2021-05-16 00:00:00 UTC
            .InZoneStrictly(_timeZone) // 2021-05-16 00:00:00 CEST
            .ToInstant(); // 2021-05-15 22:00:00 UTC

        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(periodStart.ToString())
            .WithEndDate(periodEnd.ToString())
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenPeriodIsMoreThan3AndAHalfYearBackInTimeAndNotPartOfCutOffMonth_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var periodStart = _clockMock.Object.GetCurrentInstant() // Assuming 2024-11-15 16:46:43 UTC
            .InZone(_timeZone) // 2024-11-15 17:46:43 CET
            .Date.PlusYears(-3) // 2021-11-15
            .PlusMonths(-7) // 2021-04-15
            .AtMidnight() // 2021-04-15 00:00:00 UTC
            .InZoneStrictly(_timeZone) // 2021-04-15 00:00:00 CEST
            .ToInstant(); // 2021-04-14 22:00:00 UTC

        var periodEnd = _clockMock.Object.GetCurrentInstant() // Assuming 2024-11-15 16:46:43 UTC
            .InZone(_timeZone) // 2024-11-15 17:46:43 CET
            .Date.PlusYears(-3) // 2021-11-15
            .PlusMonths(-7) // 2021-04-15
            .PlusDays(1) // 2021-04-16
            .AtMidnight() // 2021-04-16 00:00:00 UTC
            .InZoneStrictly(_timeZone) // 2021-04-16 00:00:00 CEST
            .ToInstant(); // 2021-04-15 22:00:00 UTC

        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithStartDate(periodStart.ToString())
            .WithEndDate(periodEnd.ToString())
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be("E17");
    }

    [Fact]
    public async Task Validate_WhenMeteringPointTypeIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType("invalid")
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be("D18");
    }

    [Fact]
    public async Task Validate_WhenEnergySupplierIdIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .WithRequestedByActorId(EnergySupplierValidatorTest.ValidGlnNumber)
            .WithEnergySupplierId("invalid-id")
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be("E16");
    }

    [Fact]
    public async Task Validate_WhenSettlementMethodIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithSettlementMethod("invalid-settlement-method")
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be("D15");
    }

    [Fact]
    public async Task Validate_WhenSettlementVersionIsInvalid_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithBusinessReason(DataHubNames.BusinessReason.Correction)
            .WithSettlementVersion("invalid-settlement-version")
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be("E86");
    }

    [Fact]
    public async Task Validate_WhenConsumptionAndNoSettlementMethod_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(DataHubNames.MeteringPointType.Consumption)
            .WithSettlementMethod(null)
            .WithRequestedByActorId(EnergySupplierValidatorTest.ValidGlnNumber)
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .WithEnergySupplierId(EnergySupplierValidatorTest.ValidGlnNumber)
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be("D11");
    }

    [Fact]
    public async Task Validate_WhenWholesaleFixingForBalanceResponsible_ReturnsUnsuccessfulValidation()
    {
        // Arrange
        var request = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithRequestedByActorRole(DataHubNames.ActorRole.BalanceResponsibleParty)
            .WithBusinessReason("D05")
            .WithBalanceResponsibleId(BalanceResponsibleValidatorTest.ValidGlnNumber)
            .Build();

        // Act
        var validationErrors = await _sut.ValidateAsync(request);

        // Assert
        validationErrors.Should()
            .ContainSingle()
            .Which.ErrorCode.Should()
            .Be("D11");
    }
}
