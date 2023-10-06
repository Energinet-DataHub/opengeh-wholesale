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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public static class WholesaleTimeSeriesPointFactory
{
    public static WholesaleTimeSeriesPoint Create(SqlResultRow row)
    {
        var time = SqlResultValueConverters.ToDateTimeOffset(row[WholesaleResultColumnNames.Time])!.Value;
        var quantity = SqlResultValueConverters.ToDecimal(row[WholesaleResultColumnNames.Quantity]);
        var qualities = QuantityQualitiesMapper.FromDeltaTableValue(row[WholesaleResultColumnNames.QuantityQualities]);
        var price = SqlResultValueConverters.ToDecimal(row[WholesaleResultColumnNames.Price]);
        var amount = SqlResultValueConverters.ToDecimal(row[WholesaleResultColumnNames.Amount]);
        return new WholesaleTimeSeriesPoint(time, quantity, qualities, price, amount);
    }
}
