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
from package.codelists import Colname
from pyspark.sql.types import DecimalType, StructType, StructField, StringType, TimestampType, IntegerType

time_series_points_schema = StructType([
    StructField(Colname.metering_point_id, StringType(), True),
    StructField(Colname.transaction_id, StringType(), True),
    StructField(Colname.quantity, DecimalType(18, 3), True),
    StructField(Colname.quality, IntegerType(), True),
    StructField(Colname.time, TimestampType(), True),
    StructField(Colname.resolution, IntegerType(), True),
    StructField(Colname.year, IntegerType(), True),
    StructField(Colname.month, IntegerType(), True),
    StructField(Colname.day, IntegerType(), True),
    StructField(Colname.registration_date_time, TimestampType(), True),
])
