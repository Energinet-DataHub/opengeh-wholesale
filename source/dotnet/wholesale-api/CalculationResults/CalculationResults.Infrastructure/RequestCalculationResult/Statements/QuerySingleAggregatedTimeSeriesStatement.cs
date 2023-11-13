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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.RequestCalculationResult.Statements;

public class QuerySingleAggregatedTimeSeriesStatement : DatabricksStatement
{
    private readonly AggregatedTimeSeriesQueryParameters _parameters;
    private readonly DeltaTableOptions _deltaTableOptions;

    public QuerySingleAggregatedTimeSeriesStatement(AggregatedTimeSeriesQueryParameters parameters, DeltaTableOptions deltaTableOptions)
    {
        _parameters = parameters;
        _deltaTableOptions = deltaTableOptions;
    }

    protected override string GetSqlStatement()
    {
        return $@"
            SELECT 1
            FROM {_deltaTableOptions.SCHEMA_NAME}.{_deltaTableOptions.ENERGY_RESULTS_TABLE_NAME} t1
            WHERE {CreateSqlQueryFilters(_parameters)}";
    }

    private string CreateSqlQueryFilters(AggregatedTimeSeriesQueryParameters parameters)
    {
        var whereClausesSql = $@"t1.{EnergyResultColumnNames.GridArea} IN ({parameters.GridArea})
            AND t1.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(parameters.TimeSeriesType)}')
            AND t1.{EnergyResultColumnNames.Time} >= '{parameters.StartOfPeriod.ToString()}'
            AND t1.{EnergyResultColumnNames.Time} < '{parameters.EndOfPeriod.ToString()}'
            AND t1.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(parameters.TimeSeriesType, parameters.EnergySupplierId, parameters.BalanceResponsibleId)}'
            ";
        if (parameters.ProcessType != null)
        {
            whereClausesSql +=
                $@"AND t1.{EnergyResultColumnNames.BatchProcessType} = '{ProcessTypeMapper.ToDeltaTableValue((ProcessType)parameters.ProcessType)}'";
        }

        if (parameters.EnergySupplierId != null)
        {
            whereClausesSql += $@"AND t1.{EnergyResultColumnNames.EnergySupplierId} = '{parameters.EnergySupplierId}'";
        }

        if (parameters.BalanceResponsibleId != null)
        {
            whereClausesSql += $@"AND t1.{EnergyResultColumnNames.BalanceResponsibleId} = '{parameters.BalanceResponsibleId}'";
        }

        return whereClausesSql;
    }
}
