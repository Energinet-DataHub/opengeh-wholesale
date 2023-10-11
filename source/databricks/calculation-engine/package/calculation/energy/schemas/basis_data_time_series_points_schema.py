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


basis_data_time_series_points_schema = StructType(
    [
        StructField(Colname.grid_area, StringType(), False),
        StructField(Colname.to_grid_area, StringType(), True),
        StructField(Colname.from_grid_area, StringType(), True),
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.metering_point_type, StringType(), False),
        StructField(Colname.resolution, StringType(), False),
        StructField(Colname.observation_time, TimestampType(), False),
        StructField(Colname.quantity, DecimalType(18, 3), True),
        StructField(Colname.quality, StringType(), False),
        StructField(Colname.energy_supplier_id, StringType(), True),
        StructField(Colname.balance_responsible_id, StringType(), True),
    ]
)
