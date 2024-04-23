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


class MeteringPointTimeSeries(DataFrameWrapper):
    """
    Time series points of metering points with resolution hourly or quarterly.

    The points are enriched with metering point data required by calculations.

    When points are missing the time series are padded with
    points where quantity=0 and quality=missing.

    Can be either all hourly or all quarterly.
    """

    def __init__(self, df: DataFrame):
        super().__init__(
            df,
            metering_point_time_series_schema,
            # Setting these too False would cause errors, and there is no nice and easy fix for this.
            # Should they eventually be set to False?
            ignore_nullability=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
        )


metering_point_time_series_schema = t.StructType(
    [
        t.StructField(Colname.grid_area, t.StringType(), False),
        t.StructField(Colname.to_grid_area, t.StringType(), True),
        t.StructField(Colname.from_grid_area, t.StringType(), True),
        t.StructField(Colname.metering_point_id, t.StringType(), False),
        t.StructField(Colname.metering_point_type, t.StringType(), False),
        t.StructField(Colname.quantity, t.DecimalType(18, 6), False),
        t.StructField(Colname.quality, t.StringType(), False),
        t.StructField(Colname.energy_supplier_id, t.StringType(), True),
        t.StructField(Colname.balance_responsible_id, t.StringType(), True),
        t.StructField(Colname.settlement_method, t.StringType(), True),
        t.StructField(Colname.observation_time, t.TimestampType(), False),
    ]
)
