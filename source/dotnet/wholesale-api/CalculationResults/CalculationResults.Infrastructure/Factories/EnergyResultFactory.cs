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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public class EnergyResultFactory
{
    public static EnergyResult CreateEnergyResult(
        DatabricksSqlRow databricksSqlRow,
        List<EnergyTimeSeriesPoint> timeSeriesPoints,
        long version)
    {
        var id = databricksSqlRow[EnergyResultColumnNames.CalculationResultId];
        var calculationId = databricksSqlRow[EnergyResultColumnNames.CalculationId];
        var gridArea = databricksSqlRow[EnergyResultColumnNames.GridArea];
        var timeSeriesType = databricksSqlRow[EnergyResultColumnNames.TimeSeriesType];
        var energySupplierId = databricksSqlRow[EnergyResultColumnNames.EnergySupplierId];
        var balanceResponsibleId = databricksSqlRow[EnergyResultColumnNames.BalanceResponsibleId];
        var calculationType = databricksSqlRow[EnergyResultColumnNames.CalculationType];
        var fromGridArea = databricksSqlRow[EnergyResultColumnNames.NeighborGridArea];
        var meteringPointId = databricksSqlRow[EnergyResultColumnNames.MeteringPointId];
        var resolution = ResolutionMapper.FromDeltaTableValue(databricksSqlRow[EnergyResultColumnNames.Resolution]!);
        var period = PeriodHelper.GetPeriod(timeSeriesPoints, resolution);
        return new EnergyResult(
            SqlResultValueConverters.ToGuid(id!),
            Guid.Parse(calculationId!),
            gridArea!,
            SqlResultValueConverters.ToTimeSeriesType(timeSeriesType!),
            energySupplierId,
            balanceResponsibleId,
            timeSeriesPoints.ToArray(),
            CalculationTypeMapper.FromDeltaTableValue(calculationType!),
            period.Start,
            period.End,
            fromGridArea,
            meteringPointId,
            resolution,
            version);
    }
}
