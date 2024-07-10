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
)

from package.calculation.databases.output_table_column_names import (
    OutputTableColumnNames,
)

calculations_schema = StructType(
    [
        StructField(OutputTableColumnNames.calculation_id, StringType(), False),
        StructField(OutputTableColumnNames.calculation_type, StringType(), False),
        StructField(
            OutputTableColumnNames.calculation_period_start, TimestampType(), False
        ),
        StructField(
            OutputTableColumnNames.calculation_period_end, TimestampType(), False
        ),
        StructField(
            OutputTableColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        StructField(OutputTableColumnNames.created_by_user_id, StringType(), False),
        StructField(OutputTableColumnNames.calculation_version, LongType(), False),
    ]
)

hive_calculations_schema = StructType(
    [
        StructField("calculation_id", StringType(), False),
        StructField("calculation_type", StringType(), False),
        StructField("period_start", TimestampType(), False),
        StructField("period_end", TimestampType(), False),
        StructField("execution_time_start", TimestampType(), False),
        StructField("created_by_user_id", StringType(), False),
        StructField("version", LongType(), False),
    ]
)
