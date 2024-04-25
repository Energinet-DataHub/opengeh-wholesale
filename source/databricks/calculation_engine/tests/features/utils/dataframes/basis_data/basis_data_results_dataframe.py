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

BASIS_DATA_METERING_POINT_PERIODS_CSV = "metering_point_periods"
BASIS_DATA_TIME_SERIES_POINTS_CSV = "time_series_points"


def create_basis_data_result_dataframe(
    spark: SparkSession, df: DataFrame, filename: str
) -> DataFrame:

    if filename == BASIS_DATA_TIME_SERIES_POINTS_CSV:
        return create_time_series_points(df, spark)
    if filename == BASIS_DATA_METERING_POINT_PERIODS_CSV:
        return create_metering_point_periods(df, spark)

    raise Exception(f"Unknown expected basis data file {filename}.")


def create_time_series_points(df: DataFrame, spark: SparkSession) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants import TimeSeriesColname
    from package.calculation.basis_data.schemas import time_series_point_schema

    df = df.withColumn(
        TimeSeriesColname.quantity,
        col(TimeSeriesColname.quantity).cast(DecimalType(18, 6)),
    )
    df = df.withColumn(
        TimeSeriesColname.observation_time,
        col(TimeSeriesColname.observation_time).cast(TimestampType()),
    )

    return spark.createDataFrame(df.rdd, time_series_point_schema)


def create_metering_point_periods(df: DataFrame, spark: SparkSession) -> DataFrame:

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
