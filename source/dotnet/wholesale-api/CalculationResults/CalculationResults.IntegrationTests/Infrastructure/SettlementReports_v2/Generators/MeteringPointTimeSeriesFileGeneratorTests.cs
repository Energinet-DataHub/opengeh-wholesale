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

using System.Text;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports_v2.Generators;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Moq;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2.Generators;

public sealed class MeteringPointTimeSeriesFileGeneratorTests
{
    [Theory]
    [InlineData(Resolution.Hour)]
    [InlineData(Resolution.Quarter)]
    public async Task Write_DataSourceRowFound_WritesExpectedCsv(Resolution resolution)
    {
        // arrange
        var numberOfQuantities = resolution == Resolution.Hour ? 24 : 96;

        var dataSourceMock = new Mock<ISettlementReportMeteringPointTimeSeriesResultRepository>();

        dataSourceMock
            .Setup(x => x.GetAsync(It.IsAny<SettlementReportRequestFilterDto>(), 1, resolution, It.IsAny<int>(), It.IsAny<int>()))
            .Returns(
                new List<SettlementReportMeteringPointTimeSeriesResultRow>
                {
                    new(
                        "1",
                        MeteringPointType.Consumption,
                        DateTimeOffset.Now.ToInstant(),
                        Enumerable.Range(0, numberOfQuantities).Select(x => new SettlementReportMeteringPointTimeSeriesResultQuantity(DateTimeOffset.Now.ToInstant(), 10.1m + x)).ToList()),
                }.ToAsyncEnumerable());

        var sut = new MeteringPointTimeSeriesFileGenerator(dataSourceMock.Object, resolution);

        // act
        await using var ms = new MemoryStream();
        await using var wr = new StreamWriter(ms);

        await sut.WriteAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "404", new CalculationId(Guid.NewGuid())
                    },
                },
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null),
            new SettlementReportPartialFileInfo("test", false),
            1,
            wr);

        // assert
        var actualFields = Encoding.UTF8.GetString(ms.ToArray())
            .Split(["\r", "\n"], StringSplitOptions.RemoveEmptyEntries)[1..].Single().Replace("\r", string.Empty).Replace("\n", string.Empty)
            .Split(';');

        // MeteringPointId, MeteringPointType, StartDate, (24/96)x Quantity, (1/4)x empty Quantity (for spring DST transition)
        Assert.Equal(3 + numberOfQuantities + (1 / (24 / (double)numberOfQuantities)), actualFields.Length);

        // asserts that the quantities are decimal numbers with 3 decimal places
        Assert.All(actualFields.Skip(3).Take(numberOfQuantities), x => Assert.True(decimal.TryParse(x, out _) && x.Split(",")[1].Length == 3));

        // asserts the empty quantites reserved for the extra hour during spring DST transition
        Assert.All(actualFields.Skip(3 + numberOfQuantities), x => Assert.True(x == string.Empty));
    }

    [Fact]
    public async Task Write_DataSourceRowFound_WritesExpectedQuotedColumnsCsv()
    {
        // arrange
        var numberOfQuantities = 24;

        var dataSourceMock = new Mock<ISettlementReportMeteringPointTimeSeriesResultRepository>();

        dataSourceMock
            .Setup(x => x.GetAsync(It.IsAny<SettlementReportRequestFilterDto>(), 1, Resolution.Hour, It.IsAny<int>(), It.IsAny<int>()))
            .Returns(
                new List<SettlementReportMeteringPointTimeSeriesResultRow>
                {
                    new(
                        "1",
                        MeteringPointType.Consumption,
                        DateTimeOffset.Now.ToInstant(),
                        Enumerable.Range(0, numberOfQuantities).Select(x => new SettlementReportMeteringPointTimeSeriesResultQuantity(DateTimeOffset.Now.ToInstant(), 10.1m + x)).ToList()),
                }.ToAsyncEnumerable());

        var sut = new MeteringPointTimeSeriesFileGenerator(dataSourceMock.Object, Resolution.Hour);

        // act
        await using var ms = new MemoryStream();
        await using var wr = new StreamWriter(ms);

        await sut.WriteAsync(
            new SettlementReportRequestFilterDto(
                new Dictionary<string, CalculationId?>
                {
                    {
                        "404", new CalculationId(Guid.NewGuid())
                    },
                },
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                CalculationType.WholesaleFixing,
                null,
                "da-DK"),
            new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, null),
            new SettlementReportPartialFileInfo("test", false),
            1,
            wr);

        // assert
        var actualRows = Encoding.UTF8.GetString(ms.ToArray())
            .Split(["\r", "\n"], StringSplitOptions.RemoveEmptyEntries);

        Assert.StartsWith("METERINGPOINTID;", actualRows[0]);
        Assert.StartsWith("\"1\";", actualRows[1]);
    }
}
