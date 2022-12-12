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
from pyspark.sql.types import StructType, StructField, StringType, TimestampType, IntegerType

metering_point_schema = StructType([
      StructField(Colname.metering_point_id, StringType(), False),
      StructField(Colname.metering_point_type, StringType(), False),
      StructField(Colname.settlement_method, StringType()),
      StructField(Colname.grid_area, StringType(), False),
      StructField(Colname.connection_state, StringType(), False),
      StructField(Colname.resolution, StringType(), False),
      StructField(Colname.in_grid_area, StringType()),
      StructField(Colname.out_grid_area, StringType()),
      StructField(Colname.metering_method, StringType(), False),
      StructField(Colname.parent_metering_point_id, StringType()),
      StructField(Colname.unit, StringType(), False),
      StructField(Colname.product, StringType()),
      StructField(Colname.from_date, TimestampType(), False),
      StructField(Colname.to_date, TimestampType(), False)
])
