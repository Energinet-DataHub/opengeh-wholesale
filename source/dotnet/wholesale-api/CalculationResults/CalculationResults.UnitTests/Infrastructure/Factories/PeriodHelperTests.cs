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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.Factories;

public class PeriodHelperTests
{
    [Theory]

    // From summer time to winter time
    [InlineData("2021-10-30T22:00:00Z", Resolution.Day, "2021-10-31T23:00:00Z")]
    [InlineData("2021-09-30T22:00:00Z", Resolution.Month, "2021-10-31T23:00:00Z")]

    // From winter time to summer time
    [InlineData("2024-03-30T23:00:00Z", Resolution.Day, "2024-03-31T22:00:00Z")]
    [InlineData("2024-02-29T23:00:00Z", Resolution.Month, "2024-03-31T22:00:00Z")]
    public void Given_SummerWinterTimeChangeDate_When_Mapping_Then_ReturnsExpectedDateWithSummerWinterTimeCorrection(string date, Resolution resolution, string expected)
    {
        // Arrange
        var dateAsDateTimeOffset = DateTimeOffset.Parse(date);
        var expectedDate = DateTimeOffset.Parse(expected);

        // Act
        var actual = PeriodHelper.GetDateTimeWithResolutionOffset(resolution, dateAsDateTimeOffset);

        // Assert
        actual.Should().Be(expectedDate);
    }

    [Theory]
    [InlineData("2021-10-26T22:00:00Z", Resolution.Hour, "2021-10-27T22:00:00Z")]
    [InlineData("2021-07-31T22:00:00Z", Resolution.Month, "2021-08-31T22:00:00Z")]

    // From summer time to winter time
    [InlineData("2021-10-31T02:00:00Z", Resolution.Hour, "2021-10-31T03:00:00Z")]

    // From winter time to summer time
    [InlineData("2024-03-31T03:00:00Z", Resolution.Hour, "2024-03-31T04:00:00Z")]
    public void Given_DatesWithoutSummerWinterTimeChange_When_Mapping_Then_ReturnsExpectedDateWithNoCorrection(string date, Resolution resolution, string expected)
    {
        // Arrange
        var dateAsDateTimeOffset = DateTimeOffset.Parse(date);
        var expectedDate = DateTimeOffset.Parse(expected);

        // Act
        var actual = PeriodHelper.GetDateTimeWithResolutionOffset(resolution, dateAsDateTimeOffset);

        // Assert
        actual.Should().Be(expectedDate);
    }
}
