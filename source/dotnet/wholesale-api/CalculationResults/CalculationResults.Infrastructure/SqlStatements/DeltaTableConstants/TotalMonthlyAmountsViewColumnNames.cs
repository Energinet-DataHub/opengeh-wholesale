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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;

public class TotalMonthlyAmountsViewColumnNames
{
    public const string CalculationId = "calculation_id";
    public const string CalculationType = "calculation_type";
    public const string CalculationVersion = "calculation_version";
    public const string CalculationResultId = "result_id";
    public const string GridAreaCode = "grid_area_code";
    public const string EnergySupplierId = "energy_supplier_id";
    public const string ChargeOwnerId = "charge_owner_id";
    public const string Currency = "currency";
    public const string Time = "time";
    public const string Amount = "amount";
}
