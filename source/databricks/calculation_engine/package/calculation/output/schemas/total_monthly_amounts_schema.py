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
        StructField(WholesaleResultColumnNames.energy_supplier_id, StringType(), False),
        StructField(WholesaleResultColumnNames.time, TimestampType(), False),
        StructField(WholesaleResultColumnNames.amount, DecimalType(18, 6), True),
        StructField(WholesaleResultColumnNames.charge_owner_id, StringType(), True),
    ]
)
