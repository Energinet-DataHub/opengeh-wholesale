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
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col
from pyspark.sql.types import (
    TimestampType,
    DecimalType,
)


def create_metering_point_period(df: DataFrame, spark: SparkSession) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants import MeteringPointPeriodColname
    from package.calculation.basis_data.schemas import metering_point_period_schema

    df = df.withColumn(
        MeteringPointPeriodColname.from_date,
        col(MeteringPointPeriodColname.from_date).cast(TimestampType()),
    )

    df = df.withColumn(
        MeteringPointPeriodColname.to_date,
        col(MeteringPointPeriodColname.to_date).cast(TimestampType()),
    )

    return spark.createDataFrame(df.rdd, metering_point_period_schema)


def create_time_series_point(df: DataFrame, spark: SparkSession) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants import TimeSeriesColname
    from package.calculation.basis_data.schemas import time_series_point_schema

    df = df.withColumn(
        TimeSeriesColname.quantity,
        col(TimeSeriesColname.quantity).cast(DecimalType(18, 3)),
    )
    df = df.withColumn(
        TimeSeriesColname.observation_time,
        col(TimeSeriesColname.observation_time).cast(TimestampType()),
    )

    return spark.createDataFrame(df.rdd, time_series_point_schema)
