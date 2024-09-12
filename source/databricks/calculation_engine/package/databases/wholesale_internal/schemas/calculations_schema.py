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
    LongType,
    BooleanType,
)

from package.databases.table_column_names import TableColumnNames

calculations_schema = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.calculation_type, StringType(), False),
        StructField(TableColumnNames.calculation_period_start, TimestampType(), False),
        StructField(TableColumnNames.calculation_period_end, TimestampType(), False),
        StructField(TableColumnNames.calculation_version, LongType(), False),
        StructField(
            TableColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        StructField(
            TableColumnNames.calculation_succeeded_time,
            TimestampType(),
            True,
        ),
        StructField(TableColumnNames.is_internal_calculation, BooleanType(), True),
    ]
)
