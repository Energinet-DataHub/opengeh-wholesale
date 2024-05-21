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

from pyspark.sql.types import (
    StructField,
    StringType,
    TimestampType,
    StructType,
    BooleanType,
    ArrayType,
    DecimalType,
)

from features.utils.dataframes.settlement_report.settlement_report_view_column_names import (
    ChargePricesV1ColumnNames,
)

charge_time_and_price = StructType(
    [
        StructField(ChargePricesV1ColumnNames.charge_time, TimestampType(), False),
        StructField(ChargePricesV1ColumnNames.charge_price, DecimalType(18, 6), False),
    ]
)

charge_prices_v1_view_schema = StructType(
    [
        StructField(ChargePricesV1ColumnNames.calculation_id, StringType(), False),
        StructField(ChargePricesV1ColumnNames.calculation_type, StringType(), False),
        StructField(ChargePricesV1ColumnNames.charge_type, StringType(), False),
        StructField(ChargePricesV1ColumnNames.charge_code, StringType(), False),
        StructField(ChargePricesV1ColumnNames.charge_owner_id, StringType(), False),
        StructField(ChargePricesV1ColumnNames.resolution, StringType(), False),
        StructField(ChargePricesV1ColumnNames.is_tax, BooleanType(), False),
        StructField(ChargePricesV1ColumnNames.start_date_time, TimestampType(), False),
        StructField(
            ChargePricesV1ColumnNames.prices,
            ArrayType(charge_time_and_price, False),
            False,
        ),
        StructField(ChargePricesV1ColumnNames.grid_area, StringType(), False),
        StructField(ChargePricesV1ColumnNames.energy_supplier_id, StringType(), False),
    ]
)
