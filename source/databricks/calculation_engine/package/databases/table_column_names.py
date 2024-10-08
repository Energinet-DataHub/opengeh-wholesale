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


class TableColumnNames:
    """
    Class containing all the names of the columns in the Delta tables that are owned my the Wholesale subsystem.
    Different tables should use the same names to ensure consistency across the subsystem.
    """

    aggregation_level = (
        "aggregation_level"  # TODO JVM: Remove when only using Unity Catalog
    )
    amount_type = "amount_type"  # TODO JVM: Remove when only using Unity Catalog
    amount = "amount"
    balance_responsible_id = (
        "balance_responsible_id"  # TODO JVM: Remove when only using Unity Catalog
    )
    balance_responsible_party_id = "balance_responsible_party_id"
    charge_code = "charge_code"
    charge_key = "charge_key"
    charge_owner_id = "charge_owner_id"
    charge_price = "charge_price"
    charge_time = "charge_time"
    charge_type = "charge_type"
    calculation_execution_time_start = "calculation_execution_time_start"
    calculation_succeeded_time = "calculation_succeeded_time"
    calculation_id = "calculation_id"
    calculation_result_id = (
        "calculation_result_id"  # TODO JVM: Remove when only using Unity Catalog
    )
    calculation_period_start = "calculation_period_start"
    calculation_period_end = "calculation_period_end"
    calculation_type = "calculation_type"
    calculation_version = "calculation_version"
    is_internal_calculation = "is_internal_calculation"
    """True if the calculation is an internal calculation, False otherwise."""
    created_by_user_id = "created_by_user_id"
    energy_supplier_id = "energy_supplier_id"
    from_date = "from_date"
    from_grid_area_code = "from_grid_area_code"
    grid_area_code = "grid_area_code"
    is_tax = "is_tax"
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    neighbor_grid_area_code = "neighbor_grid_area_code"
    observation_time = "observation_time"
    price = "price"
    parent_metering_point_id = "parent_metering_point_id"
    quantity = "quantity"
    quantity_unit = "quantity_unit"
    quality = "quality"
    quantity_qualities = "quantity_qualities"
    resolution = "resolution"
    result_id = "result_id"
    settlement_method = "settlement_method"
    time = "time"
    time_series_type = "time_series_type"
    to_date = "to_date"
    to_grid_area_code = "to_grid_area_code"
