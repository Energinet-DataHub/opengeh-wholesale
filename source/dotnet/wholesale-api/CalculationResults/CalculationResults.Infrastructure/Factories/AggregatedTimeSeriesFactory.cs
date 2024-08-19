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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public static class AggregatedTimeSeriesFactory
{
    public static AggregatedTimeSeries Create(
        IAggregatedTimeSeriesDatabricksContract databricksContract,
        TimeSeriesType timeSeriesType,
        DatabricksSqlRow databricksSqlRow,
        IReadOnlyCollection<EnergyTimeSeriesPoint> timeSeriesPoints)
    {
        var gridArea = databricksSqlRow[databricksContract.GetGridAreaCodeColumnName()];
        var calculationType = databricksSqlRow[databricksContract.GetCalculationTypeColumnName()];
        var resolution = ResolutionMapper.FromDeltaTableValue(databricksSqlRow[databricksContract.GetResolutionColumnName()]!);
        var (periodStart, periodEnd) = PeriodHelper.GetPeriod(timeSeriesPoints, resolution);

        return new AggregatedTimeSeries(
            gridArea: gridArea!,
            timeSeriesPoints: [.. timeSeriesPoints],
            timeSeriesType: timeSeriesType,
            calculationType: CalculationTypeMapper.FromDeltaTableValue(calculationType!),
            periodStart: periodStart,
            periodEnd: periodEnd,
            resolution,
            SqlResultValueConverters.ToInt(databricksSqlRow[databricksContract.GetCalculationVersionColumnName()]!)!.Value);
    }
}
