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
from package.constants import EnergyResultColumnNames, Colname, ChargeLinkPeriodsColname
from package.constants.basis_data_colname import (
    CalculationsColumnName,
    MeteringPointPeriodColname,
)


class ChargeLinkPeriodsV1ColumnNames:
    calculation_id = ChargeLinkPeriodsColname.calculation_id
    calculation_type = CalculationsColumnName.calculation_type
    metering_point_id = ChargeLinkPeriodsColname.metering_point_id
    metering_point_type = MeteringPointPeriodColname.metering_point_type
    charge_type = ChargeLinkPeriodsColname.charge_type
    charge_owner_id = ChargeLinkPeriodsColname.charge_owner_id
    charge_code = ChargeLinkPeriodsColname.charge_code
    quantity = ChargeLinkPeriodsColname.quantity
    from_date = ChargeLinkPeriodsColname.from_date
    to_date = ChargeLinkPeriodsColname.to_date
    grid_area = MeteringPointPeriodColname.grid_area
    energy_supplier_id = MeteringPointPeriodColname.energy_supplier_id


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
