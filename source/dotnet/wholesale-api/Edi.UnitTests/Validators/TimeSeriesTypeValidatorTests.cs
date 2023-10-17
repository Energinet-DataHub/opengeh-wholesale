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
using Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;
using FluentAssertions;
using Xunit;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class TimeSeriesTypeValidatorTests
{
    private const string ExpectedErrorMessage = "Den forespurgte tidsserie type kan ikke forespørges som en {PropertyName} / The requested times series type can not be requested as a {PropertyName}";
    private const string ExpectedErrorCode = "D11";

    private readonly TimeSeriesTypeValidationRule _sut = new();

    [Theory]
    [InlineData(MeteringPointType.Production, null)]
    [InlineData(MeteringPointType.Exchange, null)]
    [InlineData(MeteringPointType.Consumption, null)]
    [InlineData(MeteringPointType.Consumption, SettlementMethod.NonProfiled)]
    [InlineData(MeteringPointType.Consumption, SettlementMethod.Flex)]
    public void Validate_ValidMeteringPointAndSettlementCombinationForMeteredDataResponsible_NoValidationErrors(string meteringPointType, string? settlementMethod)
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(meteringPointType, settlementMethod, ActorRoleCode.MeteredDataResponsible);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(MeteringPointType.Production, null)]
    [InlineData(MeteringPointType.Consumption, SettlementMethod.NonProfiled)]
    [InlineData(MeteringPointType.Consumption, SettlementMethod.Flex)]
    public void Validate_ValidMeteringPointAndSettlementCombinationForEnergySupplier_NoValidationErrors(string meteringPointType, string? settlementMethod)
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(meteringPointType, settlementMethod, ActorRoleCode.EnergySupplier);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(MeteringPointType.Production, null)]
    [InlineData(MeteringPointType.Consumption, SettlementMethod.NonProfiled)]
    [InlineData(MeteringPointType.Consumption, SettlementMethod.Flex)]
    public void Validate_ValidMeteringPointAndSettlementCombinationForBalanceResponsible_NoValidationErrors(string meteringPointType, string? settlementMethod)
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(meteringPointType, settlementMethod, ActorRoleCode.BalanceResponsibleParty);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(MeteringPointType.Exchange)]
    [InlineData(MeteringPointType.Consumption)]
    public void Validate_InvalidMeteringPointAndSettlementCombinationForEnergySupplier_ValidationErrors(string meteringPointType)
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(meteringPointType, null, ActorRoleCode.EnergySupplier);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        AssertSingleErrorWithCorrectErrorCode(errors, ActorRoleCode.EnergySupplier);
    }

    [Theory]
    [InlineData(MeteringPointType.Exchange)]
    [InlineData(MeteringPointType.Consumption)]
    public void Validate_InvalidMeteringPointAndSettlementCombinationForBalanceResponsible_ValidationErrors(string meteringPointType)
    {
        // Arrange
        var message = CreateAggregatedTimeSeriesRequest(meteringPointType, null, ActorRoleCode.BalanceResponsibleParty);

        // Act
        var errors = _sut.Validate(message);

        // Assert
        AssertSingleErrorWithCorrectErrorCode(errors, ActorRoleCode.BalanceResponsibleParty);
    }

    private static AggregatedTimeSeriesRequest CreateAggregatedTimeSeriesRequest(string meteringPointType, string? settlementMethod, string actorRoleCode)
    {
        var message = AggregatedTimeSeriesRequestBuilder
            .AggregatedTimeSeriesRequest()
            .WithMeteringPointType(meteringPointType)
            .WithSettlementMethod(settlementMethod)
            .WithRequestedByActor(actorRoleCode, "1234567890123")
            .Build();

        return message;
    }

    private void AssertSingleErrorWithCorrectErrorCode(IList<ValidationError> errors, string actorRoleCode)
    {
        Assert.Single(errors);

        var error = errors.Single();
        Assert.Contains(ExpectedErrorMessage.Replace("{PropertyName}", actorRoleCode), error.Message);
        Assert.Contains(ExpectedErrorCode, error.ErrorCode);
    }
}
