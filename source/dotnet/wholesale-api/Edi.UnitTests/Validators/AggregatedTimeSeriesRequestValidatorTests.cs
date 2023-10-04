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
using Energinet.DataHub.Wholesale.EDI.Validators;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.EDI.UnitTests.Validators;

public class AggregatedTimeSeriesRequestValidatorTests
{
    private readonly AggregatedTimeSeriesRequestValidator _sut = new();

    [Fact]
    public void Validate_AggregatedTimeSeriesRequest_FailsOnPeriodCheck()
    {
        // Arrange
        var request = new AggregatedTimeSeriesRequest(
            new Models.Period(
            Instant.FromUtc(2022, 1, 1, 0, 0, 0),
            Instant.FromUtc(2022, 1, 1, 0, 0, 0)),
            TimeSeriesType.Production,
            new AggregationPerRoleAndGridArea("543"));

        // Act
        var validationStatus = _sut.Validate(request);

        // Assert
        Assert.False(validationStatus.IsValid);
        Assert.Equal("D66", validationStatus.Errors.First().ErrorCode);
    }
}
