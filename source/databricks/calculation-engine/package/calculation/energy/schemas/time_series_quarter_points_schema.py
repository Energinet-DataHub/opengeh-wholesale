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

from package.constants import Colname
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
)


time_series_quarter_points_schema = StructType(
    [
        StructField(Colname.grid_area, StringType(), True),
        StructField(Colname.to_grid_area, StringType(), True),
        StructField(Colname.from_grid_area, StringType(), True),
        StructField(Colname.metering_point_id, StringType(), True),
        StructField(Colname.metering_point_type, StringType(), True),
        StructField(Colname.resolution, StringType(), True),
        StructField(Colname.observation_time, TimestampType(), True),
        StructField(Colname.quantity, DecimalType(18, 6), True),
        StructField(Colname.quality, StringType(), True),
        StructField(Colname.energy_supplier_id, StringType(), True),
        StructField(Colname.balance_responsible_id, StringType(), True),
        StructField("quarter_time", TimestampType(), True),
        StructField(
            Colname.time_window,
            StructType(
                [
                    StructField(Colname.start, TimestampType()),
                    StructField(Colname.end, TimestampType()),
                ]
            ),
            False,
        ),
        StructField("quarter_quantity", DecimalType(18, 6), True),
    ]
)
