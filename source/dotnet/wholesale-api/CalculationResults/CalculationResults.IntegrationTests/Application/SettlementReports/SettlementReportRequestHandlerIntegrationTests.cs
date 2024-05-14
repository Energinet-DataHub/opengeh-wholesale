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

using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Wholesale.CalculationResults.Application.SettlementReports_v2;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Application.SettlementReports;

public sealed class SettlementReportRequestHandlerIntegrationTests : TestBase<SettlementReportRequestHandler>
{
    [Fact]
    public async Task RequestReportAsync_ForBalanceFixing_ReturnsExpectedFiles()
    {
        // Arrange
        var filter = new SettlementReportRequestFilterDto(
            [new GridAreaCode("805")],
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(CalculationType.BalanceFixing, false, filter);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var energyResult = actual.Single();
        Assert.Equal(requestId, energyResult.RequestId);
        Assert.Equal(filter, energyResult.RequestFilter);
        Assert.Equal("Result Energy", energyResult.SuggestedName);
        Assert.Equal(SettlementReportFileContent.BalanceFixingResult, energyResult.FileContent);
    }

    [Fact]
    public async Task RequestReportAsync_SplitResult_ReturnsSplitFiles()
    {
        // Arrange
        var gridAreaCodeA = new GridAreaCode("805");
        var gridAreaCodeB = new GridAreaCode("806");

        var filter = new SettlementReportRequestFilterDto(
            [gridAreaCodeA, gridAreaCodeB],
            DateTimeOffset.UtcNow.Date,
            DateTimeOffset.UtcNow.Date.AddDays(2),
            null);

        var requestId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var reportRequest = new SettlementReportRequestDto(CalculationType.BalanceFixing, true, filter);

        // Act
        var actual = (await Sut.RequestReportAsync(requestId, reportRequest)).ToList();

        // Assert
        var energyResultA = actual[0];
        Assert.Equal(requestId, energyResultA.RequestId);
        Assert.Equal(gridAreaCodeA, energyResultA.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (805)", energyResultA.SuggestedName);
        Assert.Equal(SettlementReportFileContent.BalanceFixingResult, energyResultA.FileContent);

        var energyResultB = actual[1];
        Assert.Equal(requestId, energyResultB.RequestId);
        Assert.Equal(gridAreaCodeB, energyResultB.RequestFilter.GridAreas.Single());
        Assert.Equal("Result Energy (806)", energyResultB.SuggestedName);
        Assert.Equal(SettlementReportFileContent.BalanceFixingResult, energyResultB.FileContent);
    }
}
