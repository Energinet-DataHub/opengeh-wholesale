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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class AggregatedTimeSeriesQuerySnippetProvider
{
    internal string GetWhereClauseSqlExpression(AggregatedTimeSeriesQueryParameters parameters, string table)
    {
        return $"""
                WHERE ({string.Join(
                    " OR ",
                    parameters.TimeSeriesTypes
                        .Select(timeSeriesType => TimeSeriesTypeWhereClauseSqlExpression(
                            parameters,
                            timeSeriesType,
                            table))
                        .Select(s => $"({s})"))})
                """;
    }

    internal string GenerateLatestOrFixedCalculationTypeWhereClause(AggregatedTimeSeriesQueryParameters queryParameters, IReadOnlyCollection<CalculationTypeForGridArea> calculationTypeForGridAreas)
    {
        if (queryParameters.CalculationType is not null)
        {
            return $"""
                    er.{WholesaleResultColumnNames.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(queryParameters.CalculationType.Value)}'
                    """;
        }

        if (calculationTypeForGridAreas.IsNullOrEmpty())
        {
            return """
                   FALSE
                   """;
        }

        var calculationTypePerGridAreaConstraints = calculationTypeForGridAreas
            .Select(ctpga => $"""
                              (er.{WholesaleResultColumnNames.GridArea} = '{ctpga.GridArea}' AND er.{WholesaleResultColumnNames.CalculationType} = '{ctpga.CalculationType}')
                              """);

        return $"({string.Join(" OR ", calculationTypePerGridAreaConstraints)})";
    }

    private static string TimeSeriesTypeWhereClauseSqlExpression(
        AggregatedTimeSeriesQueryParameters parameters,
        TimeSeriesType timeSeriesType,
        string table)
    {
        var whereClausesSql = $@"
                {table}.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)}')
            AND {table}.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(timeSeriesType, parameters.EnergySupplierId, parameters.BalanceResponsibleId)}'";

        whereClausesSql +=
            $"""
             AND ({table}.{EnergyResultColumnNames.Time} >= '{parameters.Period.Start}'
                  AND {table}.{EnergyResultColumnNames.Time} < '{parameters.Period.End}')
             """;

        if (parameters.GridAreaCodes.Count > 0)
        {
            whereClausesSql +=
                $" AND {table}.{EnergyResultColumnNames.GridArea} IN ({string.Join(",", parameters.GridAreaCodes.Select(gridAreaCode => $"'{gridAreaCode}'"))})";
        }

        if (parameters.EnergySupplierId is not null)
        {
            whereClausesSql +=
                $" AND {table}.{EnergyResultColumnNames.EnergySupplierId} = '{parameters.EnergySupplierId}'";
        }

        if (parameters.BalanceResponsibleId is not null)
        {
            whereClausesSql +=
                $" AND {table}.{EnergyResultColumnNames.BalanceResponsibleId} = '{parameters.BalanceResponsibleId}'";
        }

        return whereClausesSql;
    }
}
