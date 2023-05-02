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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Processes;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Tests;

[UnitTest]
public class CalculationResultClientTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task DeserializeJson_ShouldReturnValidObject(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string energySupplierGln,
        string balanceResponsiblePartyGln,
        CalculationResultClient sut)
    {
        // Arrange
        int expectedRowCount = 96;
        var expectedFirstTimeSeriesPoint = new TimeSeriesPoint(
            DateTimeOffset.Parse("2023-04-04T22:00:00.000Z"),
            0.000m,
            QuantityQuality.Missing);
        var expectedLastTimeSeriesPoint = new TimeSeriesPoint(
            DateTimeOffset.Parse("2023-04-05T08:47:41.000Z"),
            1.235m,
            QuantityQuality.Estimated);

        var stream = EmbeddedResources.GetStream("Infrastructure.Processes.CalculationResultTest.json");
        using var reader = new StreamReader(stream);

        // Act
        var actual = await sut.GetAsync(batchId, gridAreaCode, timeSeriesType, energySupplierGln, balanceResponsiblePartyGln);

        // Assert
        actual.TimeSeriesType.Should().Be(timeSeriesType);
        actual.TimeSeriesPoints.Length.Should().Be(expectedRowCount);
        actual.TimeSeriesPoints.First().Should().Be(expectedFirstTimeSeriesPoint);
        actual.TimeSeriesPoints.Last().Should().Be(expectedLastTimeSeriesPoint);
    }
}
