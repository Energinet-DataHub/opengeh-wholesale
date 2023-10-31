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
from pyspark.sql import DataFrame

from package.common import DataFrameWrapper
from package.constants import Colname


class QuarterlyMeteringPointTimeSeries(DataFrameWrapper):
    """
    Time series points of metering points with resolution quarterly.

    The points are enriched with metering point data required by calculations.

    When points are missing the time series are padded with
    points where quantity=0 and quality=missing.

    Can be either hourly or quarterly.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            _time_series_quarter_points_schema,
            # TODO BJM: These should eventually all be set to False
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )


_time_series_quarter_points_schema = t.StructType(
    [
        t.StructField(Colname.grid_area, t.StringType(), False),
        t.StructField(Colname.to_grid_area, t.StringType(), True),
        t.StructField(Colname.from_grid_area, t.StringType(), True),
        t.StructField(Colname.metering_point_id, t.StringType(), False),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        # TODO BJM: Does it make sense to require a resolution col in a "quarterly type"
        t.StructField(Colname.resolution, t.StringType(), False),
        t.StructField(Colname.observation_time, t.TimestampType(), False),
        t.StructField(Colname.quantity, t.DecimalType(18, 6), False),
        t.StructField(Colname.quality, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), True),
        t.StructField(Colname.balance_responsible_id, t.StringType(), True),
        t.StructField(Colname.settlement_method, t.StringType(), True),
        # TODO BJM: Does it make sense to have both observation_time, time_window and resolution?
        t.StructField(
            Colname.time_window,
            t.StructType(
                [
                    t.StructField(Colname.start, t.TimestampType()),
                    t.StructField(Colname.end, t.TimestampType()),
                ]
            ),
            False,
        ),
    ]
)
