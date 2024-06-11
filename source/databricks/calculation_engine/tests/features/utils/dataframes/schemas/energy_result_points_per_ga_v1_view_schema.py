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

import pyspark.sql.types as t

energy_result_points_per_ga_v1_view_schema = t.StructType(
    [
        t.StructField("calculation_id", t.StringType(), False),
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_period_start", t.TimestampType(), False),
        t.StructField("calculation_period_end", t.TimestampType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("result_id", t.StringType(), False),
        t.StructField("grid_area_code", t.StringType(), False),
        t.StructField("metering_point_type", t.StringType(), False),
        t.StructField("settlement_method", t.StringType(), True),
        t.StructField(
            "resolution", t.StringType(), True
        ),  # TODO BJM: Should this be False?
        t.StructField("time", t.TimestampType(), False),
        t.StructField("quantity", t.DecimalType(18, 3), False),
        t.StructField("unit", t.StringType(), False),
        t.StructField(
            "quantity_qualities", t.ArrayType(t.StringType(), True), False
        ),  # TODO BJM: Should this be False?
    ]
)
