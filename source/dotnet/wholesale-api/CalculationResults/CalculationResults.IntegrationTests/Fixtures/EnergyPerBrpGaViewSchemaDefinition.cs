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

public class EnergyPerBrpGaViewSchemaDefinition
{
    /// <summary>
    /// The schema definition of the table expressed as (Column name, Data type, Is nullable).
    /// See https://energinet.atlassian.net/wiki/spaces/D3/pages/1014202369/Wholesale+Results for more details.
    /// </summary>
    public static Dictionary<string, (string DataType, bool IsNullable)> SchemaDefinition => new()
    {
        { EnergyPerBrpGaViewColumnNames.CalculationId, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.CalculationType, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.CalculationPeriodStart, ("timestamp", false) },
        { EnergyPerBrpGaViewColumnNames.CalculationPeriodEnd, ("timestamp", false) },
        { EnergyPerBrpGaViewColumnNames.CalculationVersion, ("int", false) },
        { EnergyPerBrpGaViewColumnNames.ResultId, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.GridAreaCode, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.BalanceResponsiblePartyId, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.MeteringPointType, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.SettlementMethod, ("string", true) },
        { EnergyPerBrpGaViewColumnNames.Resolution, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.Time, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.Quantity, ("decimal(18,6)", false) },
        { EnergyPerBrpGaViewColumnNames.QuantityUnit, ("string", false) },
        { EnergyPerBrpGaViewColumnNames.QuantityQualities, ("array<string>", false) },
    };
}
