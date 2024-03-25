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

using Energinet.DataHub.Wholesale.Edi.Contracts;
using Energinet.DataHub.Wholesale.Edi.UnitTests.Builders;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Energinet.DataHub.Wholesale.Edi.Validation.AggregatedTimeSeriesRequest.Rules;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Validators.AggregatedTimeSeriesRequest;

public class TimeSeriesTypeValidatorTests
{
    private static readonly ValidationError _invalidTimeSeriesTypeForActor = new("Den forespurgte tidsserie type kan ikke forespørges som en {PropertyName} / The requested time series type can not be requested as a {PropertyName}", "D11");

    private readonly TimeSeriesTypeValidationRule _sut = new();

    [Theory]
    [InlineData(DataHubNames.MeteringPointType.Production, null)]
    [InlineData(DataHubNames.MeteringPointType.Exchange, null)]
    [InlineData(DataHubNames.MeteringPointType.Consumption, null)]
    [InlineData(DataHubNames.MeteringPointType.Consumption, DataHubNames.SettlementMethod.NonProfiled)]
    [InlineData(DataHubNames.MeteringPointType.Consumption, DataHubNames.SettlementMethod.Flex)]
    public async Task Validate_AsMeteredDataResponsible_ReturnsNoValidationErrors(string meteringPointType, string? settlementMethod)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .WithSettlementMethod(settlementMethod)
            .WithRequestedByActorId("1234567890123")
            .WithRequestedByActorRole(DataHubNames.ActorRole.MeteredDataResponsible)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(DataHubNames.MeteringPointType.Production, null)]
    [InlineData(DataHubNames.MeteringPointType.Consumption, DataHubNames.SettlementMethod.NonProfiled)]
    [InlineData(DataHubNames.MeteringPointType.Consumption, DataHubNames.SettlementMethod.Flex)]
    public async Task Validate_AsEnergySupplier_ReturnsNoValidationErrors(string meteringPointType, string? settlementMethod)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .WithSettlementMethod(settlementMethod)
            .WithRequestedByActorId("1234567890123")
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(DataHubNames.MeteringPointType.Production, null)]
    [InlineData(DataHubNames.MeteringPointType.Consumption, DataHubNames.SettlementMethod.NonProfiled)]
    [InlineData(DataHubNames.MeteringPointType.Consumption, DataHubNames.SettlementMethod.Flex)]
    public async Task Validate_AsBalanceResponsible_ReturnsNoValidationErrors(string meteringPointType, string? settlementMethod)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .WithSettlementMethod(settlementMethod)
            .WithRequestedByActorId("1234567890123")
            .WithRequestedByActorRole(DataHubNames.ActorRole.BalanceResponsibleParty)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(DataHubNames.MeteringPointType.Exchange)]
    [InlineData(DataHubNames.MeteringPointType.Consumption)]
    public async Task Validate_AsEnergySupplierAndNoSettlementMethod_ReturnsExceptedValidationErrors(string meteringPointType)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .WithSettlementMethod(null)
            .WithRequestedByActorId("1234567890123")
            .WithRequestedByActorRole(DataHubNames.ActorRole.EnergySupplier)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidTimeSeriesTypeForActor.WithPropertyName(DataHubNames.ActorRole.EnergySupplier).Message);
        error.ErrorCode.Should().Be(_invalidTimeSeriesTypeForActor.ErrorCode);
    }

    [Theory]
    [InlineData(DataHubNames.MeteringPointType.Exchange)]
    [InlineData(DataHubNames.MeteringPointType.Consumption)]
    public async Task Validate_AsBalanceResponsibleAndNoSettlementMethod_ValidationErrors(string meteringPointType)
    {
        // Arrange
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .WithSettlementMethod(null)
            .WithRequestedByActorId("1234567890123")
            .WithRequestedByActorRole(DataHubNames.ActorRole.BalanceResponsibleParty)
            .Build();

        // Act
        var errors = await _sut.ValidateAsync(message);

        // Assert
        errors.Should().ContainSingle();

        var error = errors.First();
        error.Message.Should().Be(_invalidTimeSeriesTypeForActor.WithPropertyName(DataHubNames.ActorRole.BalanceResponsibleParty).Message);
        error.ErrorCode.Should().Be(_invalidTimeSeriesTypeForActor.ErrorCode);
    }
}
