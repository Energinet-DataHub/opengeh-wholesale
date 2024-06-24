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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

public class EnergyResultsTableSchemaDefinition
{
    /// <summary>
    /// The schema definition of the table expressed as (Column name, Data type, Is nullable).
    /// </summary>
    public static Dictionary<string, (string DataType, bool IsNullable)> SchemaDefinition => new()
    {
        { EnergyResultColumnNames.GridArea, ("string", false) },
        { EnergyResultColumnNames.EnergySupplierId, ("string", true) },
        { EnergyResultColumnNames.BalanceResponsibleId, ("string", true) },
        { EnergyResultColumnNames.Quantity, ("decimal(18,3)", false) },
        { EnergyResultColumnNames.QuantityQualities, ("array<string>", false) },
        { EnergyResultColumnNames.Time, ("timestamp", false) },
        { EnergyResultColumnNames.AggregationLevel, ("string", false) },
        { EnergyResultColumnNames.TimeSeriesType, ("string", false) },
        { EnergyResultColumnNames.CalculationId, ("string", false) },
        { EnergyResultColumnNames.CalculationType, ("string", false) },
        { EnergyResultColumnNames.CalculationExecutionTimeStart, ("timestamp", false) },
        { EnergyResultColumnNames.NeighborGridArea, ("string", true) },
        { EnergyResultColumnNames.CalculationResultId, ("string", false) },
        { EnergyResultColumnNames.MeteringPointId, ("string", true) },
        { EnergyResultColumnNames.Resolution, ("string", false) },
    };
}
