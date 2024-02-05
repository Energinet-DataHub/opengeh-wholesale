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
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Energinet.DataHub.Wholesale.Common.Interfaces.Models;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class AggregatedTimeSeriesQueryStatement : DatabricksStatement
{
    private readonly AggregatedTimeSeriesQueryParameters _parameters;
    private readonly DeltaTableOptions _deltaTableOptions;

    public AggregatedTimeSeriesQueryStatement(AggregatedTimeSeriesQueryParameters parameters, DeltaTableOptions deltaTableOptions)
    {
        _parameters = parameters;
        _deltaTableOptions = deltaTableOptions;
    }

    protected override string GetSqlStatement()
    {
        var sql = $@"
            SELECT {string.Join(", ", SqlColumnNames.Select(columnName => $"t1.{columnName}"))}
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t1
            WHERE {CreateSqlQueryFilters(_parameters)}";

        sql += $@"ORDER BY t1.{EnergyResultColumnNames.GridArea}, t1.{EnergyResultColumnNames.BatchId}, t1.{EnergyResultColumnNames.Time}";
        return sql;
    }

    private static string CreateSqlQueryFilters(AggregatedTimeSeriesQueryParameters parameters)
    {
        var whereClausesSql = $@"
                t1.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(parameters.TimeSeriesType)}')
            AND t1.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(parameters.TimeSeriesType, parameters.EnergySupplierId, parameters.BalanceResponsibleId)}'    
        ";

        for (int i = 0; i < parameters.LatestCalculationForPeriod.Count; i++)
        {
            if (i == 0)
                whereClausesSql += " AND (";

            var calculationForPeriod = parameters.LatestCalculationForPeriod.ToList()[i];
            whereClausesSql += $@"
                (t1.{EnergyResultColumnNames.BatchId} == '{calculationForPeriod.BatchId}'  
                AND t1.{EnergyResultColumnNames.Time} >= '{calculationForPeriod.Period.Start.ToString()}'
                AND t1.{EnergyResultColumnNames.Time} < '{calculationForPeriod.Period.End.ToString()}')
            ";

            if (i != parameters.LatestCalculationForPeriod.Count - 1)
                whereClausesSql += " OR";
            else
                whereClausesSql += ")";
        }

        if (!string.IsNullOrWhiteSpace(parameters.GridArea))
        {
            whereClausesSql += $"AND t1.{EnergyResultColumnNames.GridArea} IN ({parameters.GridArea})";
        }

        if (parameters.ProcessType != null)
        {
            whereClausesSql +=
                $"AND t1.{EnergyResultColumnNames.BatchProcessType} = '{ProcessTypeMapper.ToDeltaTableValue((ProcessType)parameters.ProcessType)}'";
        }

        if (!string.IsNullOrWhiteSpace(parameters.EnergySupplierId))
        {
            whereClausesSql += $"AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{parameters.EnergySupplierId}'";
        }

        if (!string.IsNullOrWhiteSpace(parameters.BalanceResponsibleId))
        {
            whereClausesSql += $"AND t1.{EnergyResultColumnNames.BalanceResponsibleId} = '{parameters.BalanceResponsibleId}'";
        }

        return whereClausesSql;
    }

    private static string[] SqlColumnNames { get; } =
    {
        EnergyResultColumnNames.BatchId,
        EnergyResultColumnNames.GridArea,
        EnergyResultColumnNames.FromGridArea,
        EnergyResultColumnNames.TimeSeriesType,
        EnergyResultColumnNames.EnergySupplierId,
        EnergyResultColumnNames.BalanceResponsibleId,
        EnergyResultColumnNames.Time,
        EnergyResultColumnNames.Quantity,
        EnergyResultColumnNames.QuantityQualities,
        EnergyResultColumnNames.CalculationResultId,
        EnergyResultColumnNames.BatchProcessType,
    };
}
