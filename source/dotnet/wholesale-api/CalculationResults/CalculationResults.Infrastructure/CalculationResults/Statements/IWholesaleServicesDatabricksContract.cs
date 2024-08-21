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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.Wholesale.Common.Infrastructure.Options;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

// TODO (MWO): Extract common interface to share with energy maybe?
public interface IWholesaleServicesDatabricksContract
{
    AmountType GetAmountType();

    string GetSource(DeltaTableOptions tableOptions);

    string GetCalculationTypeColumnName();

    string GetGridAreaCodeColumnName();

    string GetTimeColumnName();

    string GetEnergySupplierIdColumnName();

    string GetChargeOwnerIdColumnName();

    string GetChargeCodeColumnName();

    string GetChargeTypeColumnName();

    string GetCalculationVersionColumnName();

    string GetCalculationIdColumnName();

    string GetResolutionColumnName();

    string GetIsTaxColumnName();

    string[] GetColumnsToProject();

    string[] GetColumnsToAggregateBy();
}
