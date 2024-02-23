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
    ArrayType,
    BooleanType,
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from package.constants import WholesaleResultColumnNames

# Note: The order of the columns must match the order of the columns in the Delta table
wholesale_results_schema = StructType(
    [
        StructField(WholesaleResultColumnNames.calculation_id, StringType(), False),
        StructField(WholesaleResultColumnNames.calculation_type, StringType(), False),
        StructField(
            WholesaleResultColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        StructField(
            WholesaleResultColumnNames.calculation_result_id, StringType(), False
        ),
        StructField(WholesaleResultColumnNames.grid_area, StringType(), False),
        # Wholesale results are per energy supplier therefore energy_supplier_id cannot be null.
        StructField(WholesaleResultColumnNames.energy_supplier_id, StringType(), False),
        # quantity is never null. If there are no time series points for the observation, then quanity is set to zero
        StructField(WholesaleResultColumnNames.quantity, DecimalType(18, 3), False),
        StructField(WholesaleResultColumnNames.quantity_unit, StringType(), False),
        StructField(
            WholesaleResultColumnNames.quantity_qualities,
            ArrayType(StringType()),
            False,
        ),
        StructField(WholesaleResultColumnNames.time, TimestampType(), False),
        StructField(WholesaleResultColumnNames.resolution, StringType(), False),
        StructField(WholesaleResultColumnNames.metering_point_type, StringType(), True),
        StructField(WholesaleResultColumnNames.settlement_method, StringType(), True),
        StructField(WholesaleResultColumnNames.price, DecimalType(18, 6), True),
        StructField(WholesaleResultColumnNames.amount, DecimalType(18, 6), False),
        StructField(WholesaleResultColumnNames.is_tax, BooleanType(), True),
        StructField(WholesaleResultColumnNames.charge_code, StringType(), True),
        StructField(WholesaleResultColumnNames.charge_type, StringType(), True),
        StructField(WholesaleResultColumnNames.charge_owner_id, StringType(), True),
        StructField(WholesaleResultColumnNames.amount_type, StringType(), False),
    ]
)
