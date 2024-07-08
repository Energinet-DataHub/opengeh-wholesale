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

from package.calculation.output.output_table_column_names import OutputTableColumnNames
from package.constants import WholesaleResultColumnNames

amounts_per_charge_schema = StructType(
    [
        StructField(OutputTableColumnNames.calculation_id, StringType(), False),
        StructField(OutputTableColumnNames.result_id, StringType(), False),
        StructField(OutputTableColumnNames.grid_area_code, StringType(), False),
        StructField(OutputTableColumnNames.energy_supplier_id, StringType(), False),
        StructField(OutputTableColumnNames.quantity, DecimalType(18, 3), True),
        StructField(OutputTableColumnNames.quantity_unit, StringType(), False),
        StructField(
            OutputTableColumnNames.quantity_qualities,
            ArrayType(StringType()),
            True,
        ),
        StructField(OutputTableColumnNames.time, TimestampType(), False),
        StructField(OutputTableColumnNames.resolution, StringType(), False),
        StructField(OutputTableColumnNames.metering_point_type, StringType(), True),
        StructField(OutputTableColumnNames.settlement_method, StringType(), True),
        StructField(OutputTableColumnNames.price, DecimalType(18, 6), True),
        StructField(OutputTableColumnNames.amount, DecimalType(18, 6), True),
        StructField(OutputTableColumnNames.is_tax, BooleanType(), True),
        StructField(OutputTableColumnNames.charge_code, StringType(), True),
        StructField(OutputTableColumnNames.charge_type, StringType(), True),
        StructField(OutputTableColumnNames.charge_owner_id, StringType(), True),
    ]
)
