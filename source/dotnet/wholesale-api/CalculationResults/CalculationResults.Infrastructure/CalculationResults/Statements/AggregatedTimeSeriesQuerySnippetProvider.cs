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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class AggregatedTimeSeriesQuerySnippetProvider(
    AggregatedTimeSeriesQueryParameters queryParameters,
    IAggregatedTimeSeriesDatabricksContract databricksContract)
{
    private readonly AggregatedTimeSeriesQueryParameters _queryParameters = queryParameters;

    internal IAggregatedTimeSeriesDatabricksContract DatabricksContract { get; } = databricksContract;

    internal string GetWhereClauseSqlExpression(string table, TimeSeriesType? timeSeriesType)
    {
        if (timeSeriesType is not null)
        {
            return $"""
                    WHERE ({TimeSeriesTypeWhereClauseSqlExpression(
                    _queryParameters,
                    timeSeriesType.Value,
                    table)})
                    """;
        }

        return $"""
                WHERE ({string.Join(
                    " OR ",
                    _queryParameters.TimeSeriesTypes
                        .Select(tst => TimeSeriesTypeWhereClauseSqlExpression(
                            _queryParameters,
                            tst,
                            table))
                        .Select(s => $"({s})"))})
                """;
    }

    internal string GenerateLatestOrFixedCalculationTypeWhereClause(IReadOnlyCollection<CalculationTypeForGridArea> calculationTypeForGridAreas)
    {
        if (_queryParameters.CalculationType is not null)
        {
            return $"""
                    er.{EnergyPerEsBrpGaViewColumnNames.CalculationType} = '{CalculationTypeMapper.ToDeltaTableValue(_queryParameters.CalculationType.Value)}'
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
                              (er.{EnergyPerEsBrpGaViewColumnNames.GridAreaCode} = '{ctpga.GridArea}' AND er.{EnergyPerEsBrpGaViewColumnNames.CalculationType} = '{ctpga.CalculationType}')
                              """);

        return $"({string.Join(" OR ", calculationTypePerGridAreaConstraints)})";
    }

    private static string TimeSeriesTypeWhereClauseSqlExpression(
        AggregatedTimeSeriesQueryParameters parameters,
        TimeSeriesType timeSeriesType,
        string table)
    {
        // var whereClausesSql = $@"
        //         {table}.{EnergyResultColumnNames.TimeSeriesType} IN ('{TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)}')
        //     AND {table}.{EnergyResultColumnNames.AggregationLevel} = '{AggregationLevelMapper.ToDeltaTableValue(timeSeriesType, parameters.EnergySupplierId, parameters.BalanceResponsibleId)}'";
        var meteringPointType = MeteringPointTypeMapper.ToDeltaTableValue(
            MeteringPointTypeMapper.FromTimeSeriesTypeDeltaTableValue(
                TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)));
        var settlementMethod = SettlementMethodMapper.ToDeltaTableValue(
            SettlementMethodMapper.FromTimeSeriesTypeDeltaTableValue(
                TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)));

        var whereClausesSql =
            $"""
             {table}.{EnergyPerEsBrpGaViewColumnNames.MeteringPointType} = '{meteringPointType}'
             {(settlementMethod is not null ? $"AND {table}.{EnergyPerEsBrpGaViewColumnNames.SettlementMethod} = '{settlementMethod}'" : $"AND {table}.{EnergyPerEsBrpGaViewColumnNames.SettlementMethod} is null")} 
             AND ({table}.{EnergyPerEsBrpGaViewColumnNames.Time} >= '{parameters.Period.Start}'
                  AND {table}.{EnergyPerEsBrpGaViewColumnNames.Time} < '{parameters.Period.End}')
             """;

        if (parameters.GridAreaCodes.Count > 0)
        {
            whereClausesSql +=
                $" AND {table}.{EnergyPerEsBrpGaViewColumnNames.GridAreaCode} IN ({string.Join(",", parameters.GridAreaCodes.Select(gridAreaCode => $"'{gridAreaCode}'"))})";
        }

        if (parameters.EnergySupplierId is not null)
        {
            whereClausesSql +=
                $" AND {table}.{EnergyPerEsBrpGaViewColumnNames.EnergySupplierId} = '{parameters.EnergySupplierId}'";
        }

        if (parameters.BalanceResponsibleId is not null)
        {
            whereClausesSql +=
                $" AND {table}.{EnergyPerEsBrpGaViewColumnNames.BalanceResponsiblePartyId} = '{parameters.BalanceResponsibleId}'";
        }

        return whereClausesSql;
    }
}
