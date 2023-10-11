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

using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Models;
using Energinet.DataHub.Wholesale.EDI.Factories;
using Energinet.DataHub.Wholesale.EDI.Validation;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;
using TimeSeriesType = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests;

public class AggregatedTimeSeriesRequestRejectedMessageFactoryTests
{
    [Fact]
    public void Create_WithNoCalculationResult_SendsRejectMessage()
    {
        // Arrange
        var expectedAcceptedSubject = nameof(AggregatedTimeSeriesRequestRejected);
        var expectedReferenceId = "123456789";

        // Act
        var response = AggregatedTimeSeriesRequestRejectedMessageFactory.Create(new[] { ValidationError.NoDataFound }, expectedReferenceId);

        // Assert
        response.Should().NotBeNull();
        response.ApplicationProperties.ContainsKey("ReferenceId").Should().BeTrue();
        response.ApplicationProperties["ReferenceId"].ToString().Should().Be(expectedReferenceId);
        response.Subject.Should().Be(expectedAcceptedSubject);

        var responseBody = AggregatedTimeSeriesRequestRejected.Parser.ParseFrom(response.Body);
        responseBody.RejectReasons.Should().ContainSingle();
        responseBody.RejectReasons[0].ErrorCode.Should().Be(ValidationError.NoDataFound.ErrorCode);
    }
}
