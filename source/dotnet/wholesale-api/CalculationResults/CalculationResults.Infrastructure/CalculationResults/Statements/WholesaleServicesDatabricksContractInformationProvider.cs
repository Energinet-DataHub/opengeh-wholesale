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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public class WholesaleServicesDatabricksContractInformationProvider
{
    private readonly Dictionary<AmountType, IWholesaleServicesDatabricksContract> _databricksContracts;

    public WholesaleServicesDatabricksContractInformationProvider(IEnumerable<IWholesaleServicesDatabricksContract> databricksContracts)
    {
        _databricksContracts = databricksContracts
            .DistinctBy(hc => hc.GetAmountType())
            .ToDictionary(h => h.GetAmountType());
    }

    /// <summary>
    /// Get (or more precisely, guess) the amount type from the row.
    /// The amount type is inferred based on the potential absence of columns.
    /// If the columns in the view changes, this method must be updated too.
    /// The rules are as follows:
    /// <list type="number">
    /// <item><description>
    /// If the column "Resolution" is present, the amount type is "AmountPerCharge"
    /// </description></item>
    /// <item><description>
    /// If the column "ChargeCode" is present, the amount type is "MonthlyAmountPerCharge"
    /// </description></item>
    /// <item><description>
    /// Otherwise, the amount type is "TotalMonthlyAmount"
    /// </description></item>
    /// </list>
    /// </summary>
    internal AmountType GetAmountTypeFromRow(DatabricksSqlRow databricksSqlRow)
    {
        return databricksSqlRow.HasColumn(AmountsPerChargeViewColumnNames.Resolution)
            ? AmountType.AmountPerCharge
            : databricksSqlRow.HasColumn(MonthlyAmountsPerChargeViewColumnNames.ChargeCode)
                ? AmountType.MonthlyAmountPerCharge
                : AmountType.TotalMonthlyAmount;
    }

    internal string GetCalculationIdColumnName(AmountType amountType)
    {
        return _databricksContracts[amountType].GetCalculationIdColumnName();
    }

    internal string[] GetColumnsToAggregateBy(AmountType amountType)
    {
        return _databricksContracts[amountType].GetColumnsToAggregateBy();
    }
}
