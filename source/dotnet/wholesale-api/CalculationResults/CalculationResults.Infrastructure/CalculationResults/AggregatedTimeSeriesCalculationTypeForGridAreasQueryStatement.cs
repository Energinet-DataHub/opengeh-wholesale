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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

internal class AggregatedTimeSeriesCalculationTypeForGridAreasQueryStatement(
    DeltaTableOptions deltaTableOptions,
    AggregatedTimeSeriesQuerySnippetProvider querySnippetProvider)
    : CalculationTypeForGridAreasQueryStatementBase(
        EnergyResultColumnNames.GridArea,
        EnergyResultColumnNames.CalculationType)
{
    private readonly DeltaTableOptions _deltaTableOptions = deltaTableOptions;
    private readonly AggregatedTimeSeriesQuerySnippetProvider _querySnippetProvider = querySnippetProvider;

    protected override string GetSource() => _querySnippetProvider.DatabricksContract.GetSource(_deltaTableOptions);

    protected override string GetSelection(string table) => _querySnippetProvider.GetSelection(table, null);
}
