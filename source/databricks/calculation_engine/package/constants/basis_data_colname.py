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


class MeteringPointPeriodColname:
    calculation_id = "calculation_id"

    # Master data
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    grid_area = "grid_area_code"
    resolution = "resolution"
    settlement_method = "settlement_method"
    parent_metering_point_id = "parent_metering_point_id"
    energy_supplier_id = "energy_supplier_id"
    balance_responsible_id = "balance_responsible_id"

    # Exchange
    from_grid_area = "from_grid_area_code"
    to_grid_area = "to_grid_area_code"

    # Period
    from_date = "from_date"
    to_date = "to_date"


class TimeSeriesColname:
    calculation_id = "calculation_id"
    metering_point_id = "metering_point_id"
    observation_time = "observation_time"
    quantity = "quantity"
    quality = "quality"


class ChargeMasterDataPeriodsColname:
    calculation_id = "calculation_id"
    charge_key = "charge_key"
    charge_code = "charge_code"
    charge_type = "charge_type"
    charge_owner_id = "charge_owner_id"
    resolution = "resolution"
    is_tax = "is_tax"
    from_date = "from_date"
    to_date = "to_date"


class ChargePricePointsColname:
    calculation_id = "calculation_id"
    charge_key = "charge_key"
    charge_code = "charge_code"
    charge_type = "charge_type"
    charge_owner_id = "charge_owner_id"
    charge_price = "charge_price"
    charge_time = "charge_time"


class ChargeLinkPeriodsColname:
    calculation_id = "calculation_id"
    charge_key = "charge_key"
    charge_code = "charge_code"
    charge_type = "charge_type"
    charge_owner_id = "charge_owner_id"
    metering_point_id = "metering_point_id"
    quantity = "quantity"
    from_date = "from_date"
    to_date = "to_date"