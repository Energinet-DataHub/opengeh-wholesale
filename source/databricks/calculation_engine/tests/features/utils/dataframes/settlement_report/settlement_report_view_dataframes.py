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
from ast import literal_eval

from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col, from_json
from pyspark.sql.types import (
    TimestampType,
    ArrayType,
    DecimalType,
    IntegerType,
)


def create_metering_point_periods_v1_view(
    spark: SparkSession, df: DataFrame
) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants import MeteringPointPeriodColname
    from features.utils.dataframes.settlement_report.metering_point_period_v1_view_schema import (
        metering_point_period_v1_view_schema,
    )

    df = df.withColumn(
        MeteringPointPeriodColname.from_date,
        col(MeteringPointPeriodColname.from_date).cast(TimestampType()),
    )
    df = df.withColumn(
        MeteringPointPeriodColname.to_date,
        col(MeteringPointPeriodColname.to_date).cast(TimestampType()),
    )

    return spark.createDataFrame(df.rdd, metering_point_period_v1_view_schema)


def create_metering_point_time_series_v1_view(
    spark: SparkSession,
    df: DataFrame,
) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from features.utils.dataframes.settlement_report.metering_point_time_series_v1_view_schema import (
        metering_point_time_series_v1_view_schema,
        element,
    )

    df = df.withColumn(
        "start_date_time",
        col("start_date_time").cast(TimestampType()),
    )

    df = df.withColumn(
        "quantities",
        from_json(col("quantities"), ArrayType(element)),
    )

    return spark.createDataFrame(df.rdd, metering_point_time_series_v1_view_schema)


def create_charge_link_periods_v1_view(spark: SparkSession, df: DataFrame) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from features.utils.dataframes.settlement_report.settlement_report_view_column_names import (
        ChargeLinkPeriodsV1ColumnNames,
    )
    from features.utils.dataframes.settlement_report.charge_link_periods_v1_view_schema import (
        charge_link_periods_v1_view_schema,
    )

    df = df.withColumn(
        ChargeLinkPeriodsV1ColumnNames.from_date,
        col(ChargeLinkPeriodsV1ColumnNames.from_date).cast(TimestampType()),
    )

    df = df.withColumn(
        ChargeLinkPeriodsV1ColumnNames.to_date,
        col(ChargeLinkPeriodsV1ColumnNames.to_date).cast(TimestampType()),
    )

    df = df.withColumn(
        ChargeLinkPeriodsV1ColumnNames.quantity,
        col(ChargeLinkPeriodsV1ColumnNames.quantity).cast(IntegerType()),
    )

    return spark.createDataFrame(df.rdd, charge_link_periods_v1_view_schema)


def create_energy_results_v1_view(spark: SparkSession, df: DataFrame) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from features.utils.dataframes.settlement_report.settlement_report_view_column_names import (
        EnergyResultsV1ColumnNames,
    )
    from features.utils.dataframes.settlement_report.energy_results_v1_view_schema import (
        energy_results_v1_view_schema,
    )

    df = df.withColumn(
        EnergyResultsV1ColumnNames.quantity,
        col(EnergyResultsV1ColumnNames.quantity).cast(DecimalType(18, 3)),
    )

    df = df.withColumn(
        EnergyResultsV1ColumnNames.time,
        col(
            EnergyResultsV1ColumnNames.time,
        ).cast(TimestampType()),
    )
    return spark.createDataFrame(df.rdd, energy_results_v1_view_schema)


def create_wholesale_results_v1_view(spark: SparkSession, df: DataFrame) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from features.utils.dataframes.settlement_report.wholesale_results_v1_view_schema import (
        wholesale_results_v1_view_schema,
    )
    from package.constants import WholesaleResultColumnNames

    df = df.withColumn(
        WholesaleResultColumnNames.time,
        col(
            WholesaleResultColumnNames.time,
        ).cast(TimestampType()),
    )

    df = df.withColumn(
        WholesaleResultColumnNames.quantity,
        col(WholesaleResultColumnNames.quantity).cast(DecimalType(18, 3)),
    )

    df = df.withColumn(
        WholesaleResultColumnNames.price,
        col(WholesaleResultColumnNames.price).cast(DecimalType(18, 3)),
    )

    df = df.withColumn(
        WholesaleResultColumnNames.amount,
        col(WholesaleResultColumnNames.amount).cast(DecimalType(18, 3)),
    )

    return spark.createDataFrame(df.rdd, wholesale_results_v1_view_schema)


def _parse_qualities(qualities_str: str) -> list[dict]:
    # Parse the string into a list of dictionaries
    return literal_eval(qualities_str)
