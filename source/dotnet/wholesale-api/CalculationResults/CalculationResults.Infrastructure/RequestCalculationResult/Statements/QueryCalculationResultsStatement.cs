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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult.Statements;

public class QueryCalculationResultsStatement2 : DatabricksStatement
{
    private readonly EnergyResultQuery _query;
    private readonly DeltaTableOptions _deltaTableOptions;

    public QueryCalculationResultsStatement2(DeltaTableOptions deltaTableOptions, EnergyResultQuery energyResultQuery)
    {
        _query = energyResultQuery;
        _deltaTableOptions = deltaTableOptions;
    }

    protected override string GetSqlStatement()
    {
        var sql = $@"
            SELECT {string.Join(", ", SqlColumnNames.Select(columenName => $"t1.{columenName}"))}
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t1
            LEFT JOIN {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t2
                ON t1.{EnergyResultColumnNames.Time} = t2.{EnergyResultColumnNames.Time}
                    AND t1.{EnergyResultColumnNames.BatchExecutionTimeStart} < t2.{EnergyResultColumnNames.BatchExecutionTimeStart}
                    AND t1.{EnergyResultColumnNames.GridArea} = t2.{EnergyResultColumnNames.GridArea}
                    AND COALESCE(t1.{EnergyResultColumnNames.FromGridArea}, 'N/A') = COALESCE(t2.{EnergyResultColumnNames.FromGridArea}, 'N/A')
                    AND t1.{EnergyResultColumnNames.TimeSeriesType} = t2.{EnergyResultColumnNames.TimeSeriesType}
                    AND t1.{EnergyResultColumnNames.BatchProcessType} = t2.{EnergyResultColumnNames.BatchProcessType}
                    AND t1.{EnergyResultColumnNames.AggregationLevel} = t2.{EnergyResultColumnNames.AggregationLevel}
            WHERE t2.time IS NULL
                AND t1.{EnergyResultColumnNames.GridArea} IN ({_query.GridArea})
                AND t1.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(_query.TimeSeriesType)}')
                AND t1.{EnergyResultColumnNames.Time} BETWEEN '{_query.StartOfPeriod.ToString()}' AND '{_query.EndOfPeriod.ToString()}'
                AND t1.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(_query.TimeSeriesType, _query.EnergySupplierId, _query.BalanceResponsibleId)}'
                AND t1.{EnergyResultColumnNames.BatchProcessType} = '{ProcessTypeMapper.ToDeltaTableValue(_query.ProcessType)}'
            ";

        if (_query.EnergySupplierId != null)
        {
            sql += $@"AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{_query.EnergySupplierId}'";
        }

        if (_query.BalanceResponsibleId != null)
        {
            sql += $@"AND t1.{EnergyResultColumnNames.BalanceResponsibleId} = '{_query.BalanceResponsibleId}'";
        }

        sql += $@"ORDER BY t1.time";
        return sql;
    }

    private static string[] SqlColumnNames { get; } =
    {
        EnergyResultColumnNames.BatchId, EnergyResultColumnNames.GridArea, EnergyResultColumnNames.FromGridArea,
        EnergyResultColumnNames.TimeSeriesType, EnergyResultColumnNames.EnergySupplierId,
        EnergyResultColumnNames.BalanceResponsibleId, EnergyResultColumnNames.Time,
        EnergyResultColumnNames.Quantity, EnergyResultColumnNames.QuantityQualities,
        EnergyResultColumnNames.CalculationResultId, EnergyResultColumnNames.BatchProcessType,
    };
}
