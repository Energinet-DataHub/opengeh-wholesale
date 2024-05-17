# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from package.constants import (
    EnergyResultColumnNames,
    Colname,
    WholesaleResultColumnNames,
)


class MeteringPointTimeSeriesV1ColumnNames:
    start_date_time = "start_date_time"
    quantities = "quantities"


class EnergyResultsV1ColumnNames:
    calculation_id = EnergyResultColumnNames.calculation_id
    calculation_type = EnergyResultColumnNames.calculation_type
    energy_supplier_id = EnergyResultColumnNames.energy_supplier_id
    grid_area = EnergyResultColumnNames.grid_area
    time = EnergyResultColumnNames.time
    metering_point_type = "metering_point_type"
    quantity = EnergyResultColumnNames.quantity
    resolution = Colname.resolution
    settlement_method = Colname.settlement_method
    aggregation_level = EnergyResultColumnNames.aggregation_level


class WholesaleResultsV1ColumnNames:
    calculation_id = WholesaleResultColumnNames.calculation_id
    calculation_type = WholesaleResultColumnNames.calculation_type
    grid_area_code = WholesaleResultColumnNames.grid_area
    energy_supplier_id = WholesaleResultColumnNames.energy_supplier_id
    time = WholesaleResultColumnNames.time
    resolution = Colname.resolution
    quantity_unit = WholesaleResultColumnNames.quantity_unit
    currency = "currency"
    amount = WholesaleResultColumnNames.amount
    charge_type = WholesaleResultColumnNames.charge_type
    charge_code = WholesaleResultColumnNames.charge_code
    charge_owner_id = WholesaleResultColumnNames.charge_owner_id
