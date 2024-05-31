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

from package.constants import TotalMonthlyAmountsColumnNames

# Note: The order of the columns must match the order of the columns in the Delta table
total_monthly_amounts_schema = StructType(
    [
        StructField(TotalMonthlyAmountsColumnNames.calculation_id, StringType(), False),
        StructField(
            TotalMonthlyAmountsColumnNames.calculation_type, StringType(), False
        ),
        StructField(
            TotalMonthlyAmountsColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        StructField(
            TotalMonthlyAmountsColumnNames.calculation_result_id, StringType(), False
        ),
        StructField(TotalMonthlyAmountsColumnNames.grid_area_code, StringType(), False),
        StructField(
            TotalMonthlyAmountsColumnNames.energy_supplier_id, StringType(), True
        ),
        StructField(TotalMonthlyAmountsColumnNames.time, TimestampType(), False),
        StructField(TotalMonthlyAmountsColumnNames.amount, DecimalType(18, 6), True),
        StructField(TotalMonthlyAmountsColumnNames.charge_owner_id, StringType(), True),
    ]
)
