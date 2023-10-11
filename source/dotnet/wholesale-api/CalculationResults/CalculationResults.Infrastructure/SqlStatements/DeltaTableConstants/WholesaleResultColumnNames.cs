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

public class WholesaleResultColumnNames
{
    public const string CalculationId = "calculation_id";
    public const string BatchExecutionTimeStart = "calculation_execution_time_start";
    public const string CalculationType = "calculation_type";
    public const string CalculationResultId = "calculation_result_id";
    public const string GridArea = "grid_area";
    public const string EnergySupplierId = "energy_supplier_id";
    public const string Time = "time";
    public const string Quantity = "quantity";

    public const string QuantityUnit = "quantity_unit";
    public const string QuantityQualities = "quantity_qualities";
    public const string ChargeResolution = "resolution";
    public const string MeteringPointType = "metering_point_type";
    public const string SettlementMethod = "settlement_method";
    public const string Price = "price";
    public const string Amount = "amount";
    public const string IsTax = "is_tax";
    public const string ChargeCode = "charge_id";
    public const string ChargeType = "charge_type";
    public const string ChargeOwnerId = "charge_owner_id";
}
