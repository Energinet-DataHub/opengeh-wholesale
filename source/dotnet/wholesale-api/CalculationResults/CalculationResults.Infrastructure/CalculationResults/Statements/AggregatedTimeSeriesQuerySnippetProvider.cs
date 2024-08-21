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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public sealed class AggregatedTimeSeriesQuerySnippetProvider(
    AggregatedTimeSeriesQueryParameters queryParameters,
    IAggregatedTimeSeriesDatabricksContract databricksContract)
{
    private readonly AggregatedTimeSeriesQueryParameters _queryParameters = queryParameters;

    internal IAggregatedTimeSeriesDatabricksContract DatabricksContract { get; } = databricksContract;

    internal string GetProjection(string prefix)
    {
        return string.Join(", ", DatabricksContract.GetColumnsToProject().Select(ctp => $"`{prefix}`.`{ctp}`"));
    }

    internal string GetOrdering(string prefix)
    {
        return $"""
                {string.Join(", ", DatabricksContract.GetColumnsToAggregateBy().Select(ctab => $"{prefix}.{ctab}"))}, {prefix}.{DatabricksContract.GetTimeColumnName()}
                """;
    }

    internal string GetSelection(string table, TimeSeriesType? timeSeriesType)
    {
        if (timeSeriesType is not null)
        {
            return $"""
                    ({GetTimeSeriesTypeSelection(
                        _queryParameters,
                        timeSeriesType.Value,
                        table)})
                    """;
        }

        return $"""
                ({string.Join(
                    " OR ",
                    _queryParameters.TimeSeriesTypes
                        .Select(tst => GetTimeSeriesTypeSelection(
                            _queryParameters,
                            tst,
                            table))
                        .Select(s => $"({s})"))})
                """;
    }

    internal string GetLatestOrFixedCalculationTypeSelection(
        string prefix,
        IReadOnlyCollection<CalculationTypeForGridArea> calculationTypeForGridAreas)
    {
        if (_queryParameters.CalculationType is not null)
        {
            return $"""
                    {prefix}.{DatabricksContract.GetCalculationTypeColumnName()} = '{CalculationTypeMapper.ToDeltaTableValue(_queryParameters.CalculationType.Value)}'
                    """;
        }

        if (calculationTypeForGridAreas.Count <= 0)
        {
            return """
                   FALSE
                   """;
        }

        var calculationTypePerGridAreaConstraints = calculationTypeForGridAreas
            .Select(ctpga => $"""
                              ({prefix}.{DatabricksContract.GetGridAreaCodeColumnName()} = '{ctpga.GridArea}' AND {prefix}.{DatabricksContract.GetCalculationTypeColumnName()} = '{ctpga.CalculationType}')
                              """);

        return $"({string.Join(" OR ", calculationTypePerGridAreaConstraints)})";
    }

    private string GetTimeSeriesTypeSelection(
        AggregatedTimeSeriesQueryParameters parameters,
        TimeSeriesType timeSeriesType,
        string table)
    {
        var meteringPointType = MeteringPointTypeMapper.ToDeltaTableValue(
            MeteringPointTypeMapper.FromTimeSeriesTypeDeltaTableValue(
                TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)));
        var settlementMethod = SettlementMethodMapper.ToDeltaTableValue(
            SettlementMethodMapper.FromTimeSeriesTypeDeltaTableValue(
                TimeSeriesTypeMapper.ToDeltaTableValue(timeSeriesType)));

        var sqlConstraint =
            $"""
             {table}.{DatabricksContract.GetMeteringPointTypeColumnName()} = '{meteringPointType}'
             {(settlementMethod is not null ? $"AND {table}.{DatabricksContract.GetSettlementMethodColumnName()} = '{settlementMethod}'" : $"AND {table}.{DatabricksContract.GetSettlementMethodColumnName()} is null")} 
             AND ({table}.{DatabricksContract.GetTimeColumnName()} >= '{parameters.Period.Start}'
                  AND {table}.{DatabricksContract.GetTimeColumnName()} < '{parameters.Period.End}')
             """;

        if (parameters.GridAreaCodes.Count > 0)
        {
            sqlConstraint +=
                $" AND {table}.{DatabricksContract.GetGridAreaCodeColumnName()} IN ({string.Join(",", parameters.GridAreaCodes.Select(gridAreaCode => $"'{gridAreaCode}'"))})";
        }

        if (parameters.EnergySupplierId is not null)
        {
            sqlConstraint +=
                $" AND {table}.{DatabricksContract.GetEnergySupplierIdColumnName()} = '{parameters.EnergySupplierId}'";
        }

        if (parameters.BalanceResponsibleId is not null)
        {
            sqlConstraint +=
                $" AND {table}.{DatabricksContract.GetBalanceResponsiblePartyIdColumnName()} = '{parameters.BalanceResponsibleId}'";
        }

        return sqlConstraint;
    }
}
