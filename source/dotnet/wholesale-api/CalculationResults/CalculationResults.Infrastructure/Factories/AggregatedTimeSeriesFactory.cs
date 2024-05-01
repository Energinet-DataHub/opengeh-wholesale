﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public class AggregatedTimeSeriesFactory
{
    public static AggregatedTimeSeries Create(
        DatabricksSqlRow databricksSqlRow,
        Instant periodStart,
        Instant periodEnd,
        IReadOnlyCollection<EnergyTimeSeriesPoint> timeSeriesPoints,
        long version)
    {
        var gridArea = databricksSqlRow[EnergyResultColumnNames.GridArea];
        var timeSeriesType = databricksSqlRow[EnergyResultColumnNames.TimeSeriesType];
        var calculationType = databricksSqlRow[EnergyResultColumnNames.CalculationType];
        var resolution = databricksSqlRow[EnergyResultColumnNames.Resolution];

        return new AggregatedTimeSeries(
            gridArea: gridArea!,
            timeSeriesPoints: timeSeriesPoints.ToArray()!,
            timeSeriesType: SqlResultValueConverters.ToTimeSeriesType(timeSeriesType!),
            calculationType: CalculationTypeMapper.FromDeltaTableValue(calculationType!),
            periodStart: periodStart,
            periodEnd: periodEnd,
            ResolutionMapper.FromDeltaTableValue(resolution!),
            version: version);
    }
}
