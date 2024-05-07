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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TotalMonthlyAmountResults;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Factories;

public class TotalMonthlyAmountResultFactory
{
    public static TotalMonthlyAmountResult CreateTotalMonthlyAmountResult(
        DatabricksSqlRow databricksSqlRow,
        Instant periodStart,
        Instant periodEnd,
        long version)
    {
        var id = databricksSqlRow[TotalMonthlyAmountsColumnNames.CalculationResultId];
        var calculationId = databricksSqlRow[TotalMonthlyAmountsColumnNames.CalculationId];
        var calculationType = databricksSqlRow[TotalMonthlyAmountsColumnNames.CalculationType];
        var gridArea = databricksSqlRow[TotalMonthlyAmountsColumnNames.GridArea];
        var energySupplierId = databricksSqlRow[TotalMonthlyAmountsColumnNames.EnergySupplierId];
        var chargeOwnerId = databricksSqlRow[TotalMonthlyAmountsColumnNames.ChargeOwnerId];
        var amount = databricksSqlRow[TotalMonthlyAmountsColumnNames.Amount];

        return new TotalMonthlyAmountResult(
            SqlResultValueConverters.ToGuid(id!),
            SqlResultValueConverters.ToGuid(calculationId!),
            CalculationTypeMapper.FromDeltaTableValue(calculationType!),
            periodStart,
            periodEnd,
            gridArea!,
            energySupplierId!,
            chargeOwnerId,
            SqlResultValueConverters.ToDecimal(amount)!.Value,
            version);
    }
}
