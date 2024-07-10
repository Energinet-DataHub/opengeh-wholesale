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

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

public class TotalMonthlyAmountsViewSchemaDefinition
{
    /// <summary>
    /// The schema definition of the table expressed as (Column name, Data type, Is nullable).
    /// </summary>
    public static Dictionary<string, (string DataType, bool IsNullable)> SchemaDefinition => new()
    {
        { TotalMonthlyAmountsViewColumnNames.CalculationId, ("string", false) },
        { TotalMonthlyAmountsViewColumnNames.CalculationType, ("string", false) },
        { TotalMonthlyAmountsViewColumnNames.CalculationVersion, ("string", false) },
        { TotalMonthlyAmountsViewColumnNames.CalculationResultId, ("string", false) },
        { TotalMonthlyAmountsViewColumnNames.GridAreaCode, ("string", false) },
        { TotalMonthlyAmountsViewColumnNames.EnergySupplierId, ("string", false) },
        { TotalMonthlyAmountsViewColumnNames.ChargeOwnerId, ("string", true) },
        { TotalMonthlyAmountsViewColumnNames.Currency, ("string", false) },
        { TotalMonthlyAmountsViewColumnNames.Time, ("timestamp", false) },
        { TotalMonthlyAmountsViewColumnNames.Amount, ("decimal(18,6)", false) },
    };
}
