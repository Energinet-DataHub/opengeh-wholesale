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

public static class EnergyResultDeltaTableHelper
{
    public static IReadOnlyCollection<string> CreateRowValues(
        string calculationId = "ed39dbc5-bdc5-41b9-922a-08d3b12d4538",
        string calculationExecutionTimeStart = "2022-03-11T03:00:00.000Z",
        string calculationType = DeltaTableCalculationType.BalanceFixing,
        string calculationResultId = "aaaaaaaa-1111-1111-1c1c-08d3b12d4511",
        string timeSeriesType = DeltaTableTimeSeriesType.Production,
        string gridArea = "805",
        string? fromGridArea = null,
        string balanceResponsibleId = "1236552000028",
        string energySupplierId = "2236552000028",
        string time = "2022-05-16T03:00:00.000Z",
        string quantity = "1.123",
        string quantityQuality = "missing",
        string aggregationLevel = "total_ga",
        string? meteringPointId = null,
        string resolution = "PT15M")
    {
        return EnergyResultColumnNames.GetAllNames().Select(columnName => columnName switch
        {
            EnergyResultColumnNames.CalculationId => $@"'{calculationId}'",
            EnergyResultColumnNames.CalculationExecutionTimeStart => $@"'{calculationExecutionTimeStart}'",
            EnergyResultColumnNames.CalculationType => $@"'{calculationType}'",
            EnergyResultColumnNames.CalculationResultId => $@"'{calculationResultId}'",
            EnergyResultColumnNames.TimeSeriesType => $@"'{timeSeriesType}'",
            EnergyResultColumnNames.GridArea => $@"'{gridArea}'",
            EnergyResultColumnNames.NeighborGridArea => fromGridArea == null ? "NULL" : $@"'{fromGridArea}'",
            EnergyResultColumnNames.BalanceResponsibleId => $@"'{balanceResponsibleId}'",
            EnergyResultColumnNames.EnergySupplierId => $@"'{energySupplierId}'",
            EnergyResultColumnNames.Time => $@"'{time}'",
            EnergyResultColumnNames.Quantity => $@"{quantity}",
            EnergyResultColumnNames.QuantityQualities => $@"ARRAY('{quantityQuality}')",
            EnergyResultColumnNames.AggregationLevel => $@"'{aggregationLevel}'",
            EnergyResultColumnNames.MeteringPointId => meteringPointId == null ? "NULL" : $@"'{meteringPointId}'",
            EnergyResultColumnNames.Resolution => $@"'{resolution}'",
            _ => throw new ArgumentOutOfRangeException($"Unexpected column name: {columnName}."),
        }).ToArray();
    }
}
