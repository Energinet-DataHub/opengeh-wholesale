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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class WholesaleServicesQueries(
    DatabricksSqlWarehouseQueryExecutor databricksSqlWarehouseQueryExecutor,
    WholesaleServicesQueryStatementHelperFactory helperFactory,
    IOptions<DeltaTableOptions> deltaTableOptions,
    WholesaleServicesDatabricksContractInformationProvider databricksContractInformationProvider)
    : IWholesaleServicesQueries
{
    private readonly DatabricksSqlWarehouseQueryExecutor _databricksSqlWarehouseQueryExecutor = databricksSqlWarehouseQueryExecutor;
    private readonly IOptions<DeltaTableOptions> _deltaTableOptions = deltaTableOptions;
    private readonly WholesaleServicesDatabricksContractInformationProvider _databricksContractInformationProvider = databricksContractInformationProvider;
    private readonly WholesaleServicesQueryStatementHelperFactory _helperFactory = helperFactory;

    public async IAsyncEnumerable<WholesaleServices> GetAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var calculationTypePerGridAreas =
            await GetCalculationTypeForGridAreasAsync(queryParameters).ConfigureAwait(false);

        var helper = _helperFactory.Create(queryParameters);
        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Select,
            calculationTypePerGridAreas,
            helper,
            _deltaTableOptions.Value);

        var timeSeriesPoints = new List<WholesaleTimeSeriesPoint>();
        DatabricksSqlRow? previous = null;

        await foreach (var databricksCurrentRow in _databricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(sqlStatement, Format.JsonArray).ConfigureAwait(false))
        {
            var current = new DatabricksSqlRow(databricksCurrentRow);

            // Yield a package created from previous data, if the current row belongs to a new package
            var currentAmountType = _databricksContractInformationProvider.GetAmountTypeFromRow(current);
            var calculationIdColumn =
                _databricksContractInformationProvider.GetCalculationIdColumnName(currentAmountType);

            if (previous != null
                && (helper.GetColumnsToAggregateBy().Any(column => current[column] != previous[column])
                    || current[calculationIdColumn] != previous[calculationIdColumn]))
            {
                var previousAmountType = _databricksContractInformationProvider.GetAmountTypeFromRow(previous);
                yield return WholesaleServicesFactory.Create(previous, previousAmountType, timeSeriesPoints);
                timeSeriesPoints = [];
            }

            timeSeriesPoints.Add(WholesaleTimeSeriesPointFactory.Create(current));
            previous = current;
        }

        // Yield the last package
        if (previous != null)
        {
            var amountType = _databricksContractInformationProvider.GetAmountTypeFromRow(previous);
            yield return WholesaleServicesFactory.Create(previous, amountType, timeSeriesPoints);
        }
    }

    public async Task<bool> AnyAsync(WholesaleServicesQueryParameters queryParameters)
    {
        var calculationTypePerGridAreas =
            await GetCalculationTypeForGridAreasAsync(queryParameters).ConfigureAwait(false);

        var sqlStatement = new WholesaleServicesQueryStatement(
            WholesaleServicesQueryStatement.StatementType.Exists,
            calculationTypePerGridAreas,
            _helperFactory.Create(queryParameters),
            _deltaTableOptions.Value);

        return await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(sqlStatement)
            .AnyAsync()
            .ConfigureAwait(false);
    }

    private async Task<List<CalculationTypeForGridArea>> GetCalculationTypeForGridAreasAsync(
        WholesaleServicesQueryParameters queryParameters)
    {
        var helper = _helperFactory.Create(queryParameters);

        var calculationTypeForGridAreaStatement =
            new CalculationTypeForGridAreasStatement(_deltaTableOptions.Value, helper, queryParameters);

        var calculationTypeForGridAreas = await _databricksSqlWarehouseQueryExecutor
            .ExecuteStatementAsync(calculationTypeForGridAreaStatement, Format.JsonArray)
            .Select(d =>
            {
                var databricksSqlRow = new DatabricksSqlRow(d);
                return new CalculationTypeForGridArea(
                    databricksSqlRow[helper.GetGridAreaCodeColumnName()]!,
                    databricksSqlRow[helper.GetCalculationTypeColumnName()]!);
            })
            .ToListAsync()
            .ConfigureAwait(false);

        return calculationTypeForGridAreas
            .GroupBy(ctpga => ctpga.GridArea)
            .Select(g =>
            {
                if (queryParameters.CalculationType is not null)
                {
                    return new CalculationTypeForGridArea(g.Key, CalculationTypeMapper.ToDeltaTableValue(queryParameters.CalculationType.Value));
                }

                var calculationTypes = g.Select(ctpga => ctpga.CalculationType).ToList();

                if (calculationTypes.Contains(DeltaTableCalculationType.ThirdCorrectionSettlement))
                {
                    return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.ThirdCorrectionSettlement);
                }

                if (calculationTypes.Contains(DeltaTableCalculationType.SecondCorrectionSettlement))
                {
                    return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.SecondCorrectionSettlement);
                }

                if (calculationTypes.Contains(DeltaTableCalculationType.FirstCorrectionSettlement))
                {
                    return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.FirstCorrectionSettlement);
                }

                return new CalculationTypeForGridArea(g.Key, DeltaTableCalculationType.WholesaleFixing);
            })
            .Where(ctpga => queryParameters.CalculationType is not null || ctpga.CalculationType != DeltaTableCalculationType.WholesaleFixing)
            .Distinct()
            .ToList();
    }

    private class CalculationTypeForGridAreasStatement(
        DeltaTableOptions deltaTableOptions,
        WholesaleServicesQueryStatementHelper helper,
        WholesaleServicesQueryParameters queryParameters)
        : DatabricksStatement
    {
        private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
        private readonly WholesaleServicesQueryStatementHelper _helper = helper;
        private readonly WholesaleServicesQueryParameters _queryParameters = queryParameters;

        protected override string GetSqlStatement()
        {
            return $"""
                    SELECT {_helper.GetGridAreaCodeColumnName()}, {_helper.GetCalculationTypeColumnName()}
                    FROM {_helper.GetSource(_deltaTableOptions)} wrv
                    WHERE {_helper.GetSelection()}
                    GROUP BY {_helper.GetGridAreaCodeColumnName()}, {_helper.GetCalculationTypeColumnName()}
                    """;
        }
    }
}
