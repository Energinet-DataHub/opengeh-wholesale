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

from features.utils.dataframes.settlement_report.settlement_report_view_column_names import (
    MeteringPointTimeSeriesV1ColumnNames,
)
from package.constants import TimeSeriesColname, MeteringPointPeriodColname

element = StructType(
    [
        StructField(TimeSeriesColname.observation_time, TimestampType(), False),
        StructField(TimeSeriesColname.quantity, DecimalType(18, 3), False),
    ]
)


metering_point_time_series_v1_view_schema = StructType(
    [
        StructField(MeteringPointPeriodColname.calculation_id, StringType(), False),
        StructField(MeteringPointPeriodColname.calculation_type, StringType(), False),
        StructField(MeteringPointPeriodColname.metering_point_id, StringType(), False),
        StructField(
            MeteringPointPeriodColname.metering_point_type, StringType(), False
        ),
        StructField(MeteringPointPeriodColname.resolution, StringType(), False),
        StructField(MeteringPointPeriodColname.grid_area, StringType(), False),
        StructField(MeteringPointPeriodColname.energy_supplier_id, StringType(), True),
        StructField(
            MeteringPointTimeSeriesV1ColumnNames.start_date_time,
            TimestampType(),
            False,
        ),
        StructField(
            MeteringPointTimeSeriesV1ColumnNames.quantities,
            ArrayType(element, False),
            False,
        ),
    ]
)
