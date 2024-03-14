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
using Energinet.DataHub.Wholesale.Edi.Factories.WholesaleServices;
using Energinet.DataHub.Wholesale.Edi.Validation;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Wholesale.Edi.UnitTests.Factories.WholesaleServices;

public class WholesaleServiceRequestRejectedMessageFactoryTests
{
    private static readonly ValidationError _noDataAvailable = new("Ingen data tilgængelig / No data available", "E0H");

    [Fact]
    public void Create_WithNoCalculationResult_SendsRejectMessage()
    {
        // Arrange
        const string expectedAcceptedSubject = nameof(WholesaleServicesRequestRejected);
        const string expectedReferenceId = "123456789";

        // Act
        var actual = WholesaleServicesRequestRejectedMessageFactory.Create(new[] { _noDataAvailable }, expectedReferenceId);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.ApplicationProperties.Should().ContainKey("ReferenceId");
        actual.ApplicationProperties["ReferenceId"].ToString().Should().Be(expectedReferenceId);
        actual.Subject.Should().Be(expectedAcceptedSubject);

        var responseBody = AggregatedTimeSeriesRequestRejected.Parser.ParseFrom(actual.Body);
        responseBody.RejectReasons.Should().ContainSingle();
        responseBody.RejectReasons[0].ErrorCode.Should().Be(_noDataAvailable.ErrorCode);
    }
}
