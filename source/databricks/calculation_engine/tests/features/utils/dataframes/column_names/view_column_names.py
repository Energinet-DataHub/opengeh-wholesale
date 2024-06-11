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
from typing import Union

from pyspark.sql.types import StringType, LongType, TimestampType

from features.utils.dataframes.column_names.column_name import ColumnName


class ViewColumnNames:

    def __init__(self) -> None:
        self.column_classes = {
            attr.name: attr
            for attr in ViewColumnNames.__dict__.values()
            if isinstance(attr, ColumnName)
        }

    def get(self, column_name: str) -> Union[ColumnName, None]:
        column = self.column_classes.get(column_name, None)
        return column.get() if column else None

    # Column Names in alphabetical order
    calculation_id = ColumnName("calculation_id", StringType())
    calculation_type = ColumnName("calculation_type", StringType())
    calculation_version = ColumnName("calculation_version", LongType())
    energy_supplier_id = ColumnName("energy_supplier_id", StringType())
    from_date = ColumnName("from_date", TimestampType())
    from_grid_area_code = ColumnName("from_grid_area_code", StringType())
    grid_area_code = ColumnName("grid_area_code", StringType())
    metering_point_id = ColumnName("metering_point_id", StringType())
    metering_point_type = ColumnName("metering_point_type", StringType())
    settlement_method = ColumnName("settlement_method", StringType())
    to_date = ColumnName("to_date", TimestampType())
    to_grid_area_code = ColumnName("to_grid_area_code", StringType())


# charge_count = "charge_count"
# charge_code = "charge_code"
# charge_key = "charge_key"
# charge_owner = "charge_owner_id"
# charge_price = "charge_price"
# charge_tax = "is_tax"
# charge_time = "charge_time"
# charge_type = "charge_type"
# created_by_user_id = "created_by_user_id"
# """The user id of the user who created/started the calculation."""
# currency = "currency"
# date = "date"
# energy_supplier_id = "energy_supplier_id"
# from_date = "from_date"
# from_grid_area_code = "from_grid_area_code"
# "The grid area sending current"
# grid_area_code = "grid_area_code"
# metering_point_id = "metering_point_id"
# metering_point_type = "type"
# observation_time = "observation_time"
# """When the production/consumption/exchange actually happened."""
# parent_metering_point_id = "parent_metering_point_id"
# price_per_day = "price_per_day"
# qualities = "qualities"
# """Aggregated qualities: An array of unique quality values aggregated from metering point time series."""
# quality = "quality"
# quantity = "quantity"
# resolution = "resolution"
# settlement_method = "settlement_method"
# start_date_time = "start_date_time"
# time_series_type = "time_series_type"
# to_date = "to_date"
# to_grid_area_code = "to_grid_area_code"
# "The grid area receiving current"
# total_daily_charge_price = "total_daily_charge_price"
# total_amount = "total_amount"
# total_quantity = "total_quantity"
# unit = "unit"
