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
from geh_stream.codelists import Colname
from pyspark.sql.types import DecimalType, StructType, StructField, StringType, TimestampType, IntegerType

time_series_points_schema = StructType([
      StructField(Colname.metering_point_id, StringType(), False),
      StructField(Colname.quantity, DecimalType(18, 3), False),
      StructField(Colname.quality, StringType(), False),
      StructField(Colname.time, TimestampType(), False),
      StructField(Colname.year, IntegerType(), False),
      StructField(Colname.month, IntegerType(), False),
      StructField(Colname.day, IntegerType(), False),
      StructField(Colname.registration_date_time, TimestampType(), False),
])

time_series_points_schema.__doc__ = "This schema must conform to the schema used by the domain publishing the time series points."
