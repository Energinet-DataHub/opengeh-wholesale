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
from typing import Union, Any

from pyspark.sql.types import (
    StringType,
    LongType,
    TimestampType,
    DecimalType,
    BooleanType,
    ArrayType,
    StructType,
    StructField,
    IntegerType,
)

from tests.features.utils.dataframes.columns.column import Column


class ViewColumns:
    """
    This class contains all the columns for views in the data products.
    If the column is deprecated, postfix the respective column with "_deprecated".
    """

    columns: Any = {}

    @staticmethod
    def _init() -> None:
        if not ViewColumns.columns:
            ViewColumns.columns = {
                attr.name: attr
                for attr in ViewColumns.__dict__.values()
                if isinstance(attr, Column)
            }

    @staticmethod
    def get(column_name: str) -> Union[Column, None]:
        """
        Get the column object for the given column name.
        """
        ViewColumns._init()

        return ViewColumns.columns.get(column_name)

    # Column names and types in alphabetical order
    aggregation_level = Column("aggregation_level", StringType())
    amount = Column("amount", DecimalType(18, 6))
    balance_responsible_party_id = Column("balance_responsible_party_id", StringType())
    calculation_id = Column("calculation_id", StringType())
    calculation_type = Column("calculation_type", StringType())
    calculation_period_start = Column("calculation_period_start", TimestampType())
    calculation_period_end = Column("calculation_period_end", TimestampType())
    calculation_succeeded_time = Column("calculation_succeeded_time", TimestampType())
    calculation_version = Column("calculation_version", LongType())
    charge_code = Column("charge_code", StringType())
    charge_key = Column("charge_key", StringType())
    charge_time = Column("charge_time", TimestampType())
    charge_type = Column("charge_type", StringType())
    charge_link_quantity = Column("charge_link_quantity", IntegerType())
    charge_owner_id = Column("charge_owner_id", StringType())
    charge_price = Column("charge_price", DecimalType(18, 6))
    currency = Column("currency", StringType())
    energy_supplier_id = Column("energy_supplier_id", StringType())
    from_date = Column("from_date", TimestampType())
    from_grid_area_code = Column("from_grid_area_code", StringType())
    grid_area_code = Column("grid_area_code", StringType())
    is_internal_calculation = Column("is_internal_calculation", BooleanType())
    is_tax = Column("is_tax", BooleanType())
    latest_from_time = Column("latest_from_time", TimestampType())
    latest_to_time = Column("latest_to_time", TimestampType())
    metering_point_id = Column("metering_point_id", StringType())
    metering_point_type = Column("metering_point_type", StringType())
    neighbor_grid_area_code = Column("neighbor_grid_area_code", StringType())
    observation_time = Column("observation_time", TimestampType())
    parent_metering_point_id = Column("parent_metering_point_id", StringType())
    price = Column("price", DecimalType(18, 6))
    price_points = Column(
        "price_points",
        ArrayType(
            StructType(
                [
                    StructField("time", TimestampType(), False),
                    StructField("price", DecimalType(18, 6), False),
                ],
            ),
            False,
        ),
    )
    quality = Column("quality", StringType())
    quantities = Column(
        "quantities",
        ArrayType(
            StructType(
                [
                    StructField("observation_time", TimestampType(), False),
                    StructField("quantity", DecimalType(18, 3), False),
                ],
            ),
            False,
        ),
    )

    # Quantity can be an integer or decimal depending on the context.
    quantity = Column("quantity", DecimalType(18, 3))
    quantity_qualities = Column("quantity_qualities", ArrayType(StringType(), True))
    quantity_unit = Column("quantity_unit", StringType())
    resolution = Column("resolution", StringType())
    result = Column("result_id", StringType())
    settlement_method = Column("settlement_method", StringType())
    start_date_time = Column("start_date_time", TimestampType())
    start_of_day = Column("start_of_day", TimestampType())
    time = Column("time", TimestampType())
    time_series_type = Column("time_series_type", StringType())
    to_date = Column("to_date", TimestampType())
    to_grid_area_code = Column("to_grid_area_code", StringType())
    unit = Column("unit", StringType())
