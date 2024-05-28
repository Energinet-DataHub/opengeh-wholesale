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
    IntegerType,
    BooleanType,
)

from features.utils.dataframes.basis_data.calculations_dataframe import (
    create_calculations,
)
from features.utils.dataframes.basis_data.grid_loss_metering_points import (
    create_grid_loss_metering_points,
)

BASIS_DATA_METERING_POINT_PERIODS_CSV = "metering_point_periods"
BASIS_DATA_TIME_SERIES_POINTS_CSV = "time_series_points"
BASIS_DATA_CHARGE_LINK_PERIODS_CSV = "charge_link_periods"
BASIS_DATA_EXECUTING_CALCULATION_CSV = "executing_calculation"
BASIS_DATA_CALCULATIONS_CSV = "calculations"
BASIS_GRID_LOSS_METERING_POINTS_CSV = "grid_loss_metering_points"
BASIS_DATA_CHARGE_PRICES_CSV = "charge_prices"
BASIS_DATA_CHARGE_PRICE_INFORMATION_PERIODS_CSV = "charge_price_information_periods"


def create_basis_data_result_dataframe(
    spark: SparkSession, df: DataFrame, filename: str
) -> DataFrame:

    if filename == BASIS_DATA_TIME_SERIES_POINTS_CSV:
        return create_time_series_points(spark, df)
    if filename == BASIS_DATA_METERING_POINT_PERIODS_CSV:
        return create_metering_point_periods(spark, df)
    if filename == BASIS_DATA_EXECUTING_CALCULATION_CSV:
        return create_executing_calculation(spark, df)
    if filename == BASIS_DATA_CALCULATIONS_CSV:
        return create_calculations(spark, df)
    if filename == BASIS_GRID_LOSS_METERING_POINTS_CSV:
        return create_grid_loss_metering_points(spark, df)
    if filename == BASIS_DATA_CHARGE_LINK_PERIODS_CSV:
        return create_charge_link_periods(spark, df)
    if filename == BASIS_DATA_CHARGE_PRICES_CSV:
        return create_charge_price_points(spark, df)
    if filename == BASIS_DATA_CHARGE_PRICE_INFORMATION_PERIODS_CSV:
        return create_charge_price_information_periods(spark, df)

    raise Exception(f"Unknown expected basis data file {filename}.")


def create_time_series_points(spark: SparkSession, df: DataFrame) -> DataFrame:

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


def create_metering_point_periods(spark: SparkSession, df: DataFrame) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants.basis_data_colname import MeteringPointPeriodColname
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


def create_charge_link_periods(spark: SparkSession, df: DataFrame) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.calculation.basis_data.schemas import charge_link_periods_schema
    from package.constants.basis_data_colname import ChargeLinkPeriodsColname

    df = df.withColumn(
        ChargeLinkPeriodsColname.quantity,
        col(ChargeLinkPeriodsColname.quantity).cast(IntegerType()),
    )
    df = df.withColumn(
        ChargeLinkPeriodsColname.from_date,
        col(ChargeLinkPeriodsColname.from_date).cast(TimestampType()),
    )
    df = df.withColumn(
        ChargeLinkPeriodsColname.to_date,
        col(ChargeLinkPeriodsColname.to_date).cast(TimestampType()),
    )

    return spark.createDataFrame(df.rdd, charge_link_periods_schema)


def create_charge_price_points(spark: SparkSession, df: DataFrame) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants import ChargePricePointsColname
    from package.calculation.basis_data.schemas import charge_price_points_schema

    df = df.withColumn(
        ChargePricePointsColname.charge_price,
        col(ChargePricePointsColname.charge_price).cast(DecimalType(18, 6)),
    )

    df = df.withColumn(
        ChargePricePointsColname.charge_time,
        col(ChargePricePointsColname.charge_time).cast(TimestampType()),
    )

    return spark.createDataFrame(df.rdd, charge_price_points_schema)


def create_charge_price_information_periods(
    spark: SparkSession, df: DataFrame
) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.calculation.basis_data.schemas import (
        charge_price_information_periods_schema,
    )
    from package.constants.basis_data_colname import ChargeMasterDataPeriodsColname

    df = df.withColumn(
        ChargeMasterDataPeriodsColname.from_date,
        col(ChargeMasterDataPeriodsColname.from_date).cast(TimestampType()),
    )
    df = df.withColumn(
        ChargeMasterDataPeriodsColname.to_date,
        col(ChargeMasterDataPeriodsColname.to_date).cast(TimestampType()),
    )
    df = df.withColumn(
        ChargeMasterDataPeriodsColname.is_tax,
        col(ChargeMasterDataPeriodsColname.is_tax).cast(BooleanType()),
    )

    return spark.createDataFrame(df.rdd, charge_price_information_periods_schema)
