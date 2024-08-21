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

public class EnergyPerEsBrpGaViewSchemaDefinition
{
    /// <summary>
    /// The schema definition of the table expressed as (Column name, Data type, Is nullable).
    /// See https://energinet.atlassian.net/wiki/spaces/D3/pages/1014202369/Wholesale+Results for more details.
    /// </summary>
    public static Dictionary<string, (string DataType, bool IsNullable)> SchemaDefinition => new()
    {
        { EnergyPerEsBrpGaViewColumnNames.CalculationId, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.CalculationType, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.CalculationPeriodStart, ("timestamp", false) },
        { EnergyPerEsBrpGaViewColumnNames.CalculationPeriodEnd, ("timestamp", false) },
        { EnergyPerEsBrpGaViewColumnNames.CalculationVersion, ("int", false) },
        { EnergyPerEsBrpGaViewColumnNames.ResultId, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.GridAreaCode, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.EnergySupplierId, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.BalanceResponsiblePartyId, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.MeteringPointType, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.SettlementMethod, ("string", true) },
        { EnergyPerEsBrpGaViewColumnNames.Resolution, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.Time, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.Quantity, ("decimal(18,6)", false) },
        { EnergyPerEsBrpGaViewColumnNames.QuantityUnit, ("string", false) },
        { EnergyPerEsBrpGaViewColumnNames.QuantityQualities, ("array<string>", false) },
    };
}
