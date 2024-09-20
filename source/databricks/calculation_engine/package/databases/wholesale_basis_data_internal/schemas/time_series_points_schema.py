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
    StructField,
    StringType,
    TimestampType,
    StructType,
)

from package.databases.table_column_names import TableColumnNames

time_series_points_schema = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.metering_point_id, StringType(), False),
        StructField(TableColumnNames.quantity, DecimalType(18, 3), False),
        StructField(TableColumnNames.quality, StringType(), False),
        StructField(TableColumnNames.observation_time, TimestampType(), False),
        StructField(TableColumnNames.metering_point_type, StringType(), False),
        StructField(TableColumnNames.resolution, StringType(), False),
        StructField(TableColumnNames.grid_area_code, StringType(), False),
        StructField(TableColumnNames.energy_supplier_id, StringType(), True),
    ]
)
