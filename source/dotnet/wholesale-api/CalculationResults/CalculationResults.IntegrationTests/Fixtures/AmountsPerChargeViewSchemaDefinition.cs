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

public class AmountsPerChargeViewSchemaDefinition
{
    /// <summary>
    /// The schema definition of the table expressed as (Column name, Data type, Is nullable).
    /// </summary>
    public static Dictionary<string, (string DataType, bool IsNullable)> SchemaDefinition => new()
    {
        { AmountsPerChargeViewColumnNames.CalculationId, ("string", false) },
        { AmountsPerChargeViewColumnNames.CalculationType, ("string", false) },
        { AmountsPerChargeViewColumnNames.CalculationVersion, ("string", false) },
        { AmountsPerChargeViewColumnNames.CalculationResultId, ("string", false) },
        { AmountsPerChargeViewColumnNames.GridAreaCode, ("string", false) },
        { AmountsPerChargeViewColumnNames.EnergySupplierId, ("string", false) },
        { AmountsPerChargeViewColumnNames.ChargeCode, ("string", false) },
        { AmountsPerChargeViewColumnNames.ChargeType, ("string", false) },
        { AmountsPerChargeViewColumnNames.ChargeOwnerId, ("string", false) },
        { AmountsPerChargeViewColumnNames.Resolution, ("string", false) },
        { AmountsPerChargeViewColumnNames.QuantityUnit, ("string", false) },
        { AmountsPerChargeViewColumnNames.MeteringPointType, ("string", false) },
        { AmountsPerChargeViewColumnNames.SettlementMethod, ("string", true) },
        { AmountsPerChargeViewColumnNames.IsTax, ("boolean", false) },
        { AmountsPerChargeViewColumnNames.Currency, ("string", false) },
        { AmountsPerChargeViewColumnNames.Time, ("timestamp", false) },
        { AmountsPerChargeViewColumnNames.Quantity, ("decimal(18,3)", false) },
        { AmountsPerChargeViewColumnNames.QuantityQualities, ("array<string>", true) },
        { AmountsPerChargeViewColumnNames.Price, ("decimal(18,6)", false) },
        { AmountsPerChargeViewColumnNames.Amount, ("decimal(18,6)", false) },
    };
}
