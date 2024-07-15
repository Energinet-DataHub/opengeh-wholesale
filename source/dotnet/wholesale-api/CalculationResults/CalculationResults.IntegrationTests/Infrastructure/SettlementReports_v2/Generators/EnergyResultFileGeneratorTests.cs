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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Moq;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports_v2.Generators;

public sealed class EnergyResultFileGeneratorTests
{
    [Theory]
    [InlineData(MarketRole.DataHubAdministrator, true)]
    [InlineData(MarketRole.GridAccessProvider, false)]
    public async Task Write_WhenMarketRoleIsDataHubAdministrator_IncludesEnergySupplierId(MarketRole marketRole, bool expectToFindEnergySupplier)
    {
        // arrange
        var dataSourceMock = new Mock<ISettlementReportEnergyResultRepository>();

        var energySupplierId = "2787902748213";

        dataSourceMock
            .Setup(x => x.GetAsync(It.IsAny<SettlementReportRequestFilterDto>(), It.IsAny<SettlementReportRequestedByActor>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(
                new List<SettlementReportEnergyResultRow>
                {
                    new(
                        DateTimeOffset.Now.ToInstant(),
                        1.0m,
                        "404",
                        Resolution.Hour,
                        CalculationType.WholesaleFixing,
                        null,
                        null,
                        energySupplierId),
                }.ToAsyncEnumerable());

        var sut = new EnergyResultFileGenerator(dataSourceMock.Object);

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
            new SettlementReportRequestedByActor(marketRole, null),
            new SettlementReportPartialFileInfo("test", false),
            1,
            wr);

        // assert
        var actualRows = Encoding.UTF8.GetString(ms.ToArray())
            .Split(["\r", "\n"], StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(expectToFindEnergySupplier, actualRows[0].Contains("ENERGYSUPPLIERID"));
        Assert.Equal(expectToFindEnergySupplier, actualRows[1].Contains(energySupplierId));
    }
}
