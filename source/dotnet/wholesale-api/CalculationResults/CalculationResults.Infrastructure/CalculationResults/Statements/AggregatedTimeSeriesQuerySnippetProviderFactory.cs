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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.EnergyResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

[SuppressMessage(
    "StyleCop.CSharp.ReadabilityRules",
    "SA1118:Parameter should not span multiple lines",
    Justification = "It looks better this way")]
public sealed class AggregatedTimeSeriesQuerySnippetProviderFactory(
    IEnumerable<IAggregatedTimeSeriesDatabricksContract> databricksContracts)
{
    private readonly Dictionary<string, IAggregatedTimeSeriesDatabricksContract> _databricksContracts =
        databricksContracts
            .DistinctBy(dc => dc.GetAggregationLevel())
            .ToDictionary(dc => dc.GetAggregationLevel());

    public AggregatedTimeSeriesQuerySnippetProvider Create(
        AggregatedTimeSeriesQueryParameters parameters,
        TimeSeriesType timeSeriesType)
    {
        return new AggregatedTimeSeriesQuerySnippetProvider(
            parameters,
            _databricksContracts[AggregationLevelMapper.ToDeltaTableValue(
                timeSeriesType,
                parameters.EnergySupplierId,
                parameters.BalanceResponsibleId)]);
    }
}
