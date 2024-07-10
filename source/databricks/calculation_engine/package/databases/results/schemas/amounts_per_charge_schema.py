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

import pyspark.sql.types as t

from package.databases.output_table_column_names import OutputTableColumnNames

amounts_per_charge_schema = t.StructType(
    [
        t.StructField(OutputTableColumnNames.calculation_id, t.StringType(), False),
        t.StructField(OutputTableColumnNames.result_id, t.StringType(), False),
        t.StructField(OutputTableColumnNames.grid_area_code, t.StringType(), False),
        t.StructField(OutputTableColumnNames.energy_supplier_id, t.StringType(), False),
        t.StructField(OutputTableColumnNames.quantity, t.DecimalType(18, 3), False),
        t.StructField(OutputTableColumnNames.quantity_unit, t.StringType(), False),
        t.StructField(
            OutputTableColumnNames.quantity_qualities,
            t.ArrayType(t.StringType()),
            True,
        ),
        t.StructField(OutputTableColumnNames.time, t.TimestampType(), False),
        t.StructField(OutputTableColumnNames.resolution, t.StringType(), False),
        t.StructField(
            OutputTableColumnNames.metering_point_type, t.StringType(), False
        ),
        t.StructField(OutputTableColumnNames.settlement_method, t.StringType(), True),
        t.StructField(OutputTableColumnNames.price, t.DecimalType(18, 6), True),
        t.StructField(OutputTableColumnNames.amount, t.DecimalType(18, 6), True),
        t.StructField(OutputTableColumnNames.is_tax, t.BooleanType(), False),
        t.StructField(OutputTableColumnNames.charge_code, t.StringType(), False),
        t.StructField(OutputTableColumnNames.charge_type, t.StringType(), False),
        t.StructField(OutputTableColumnNames.charge_owner_id, t.StringType(), False),
    ]
)
