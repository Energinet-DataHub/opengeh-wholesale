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
    ArrayType,
    DecimalType,
)

from views.settlement_reports.factories.metering_point_time_series_colname import (
    MeteringPointTimeSeriesColname,
)

element = StructType(
    [
        StructField(
            MeteringPointTimeSeriesColname.observation_time, TimestampType(), False
        ),
        StructField(MeteringPointTimeSeriesColname.quantity, DecimalType(18, 3), False),
    ]
)

metering_point_time_series_schema = StructType(
    [
        StructField(MeteringPointTimeSeriesColname.calculation_id, StringType(), False),
        StructField(
            MeteringPointTimeSeriesColname.metering_point_id, StringType(), False
        ),
        StructField(
            MeteringPointTimeSeriesColname.metering_point_type, StringType(), False
        ),
        StructField(MeteringPointTimeSeriesColname.resolution, StringType(), True),
        StructField(MeteringPointTimeSeriesColname.grid_area, StringType(), True),
        StructField(
            MeteringPointTimeSeriesColname.energy_supplier_id, StringType(), True
        ),
        StructField(
            MeteringPointTimeSeriesColname.observation_day, TimestampType(), True
        ),
        StructField(
            MeteringPointTimeSeriesColname.quantities,
            ArrayType(element, False),
            False,
        ),
    ]
)
