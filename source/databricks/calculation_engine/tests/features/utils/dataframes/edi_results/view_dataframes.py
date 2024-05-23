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
from pyspark.sql.functions import col, udf
from pyspark.sql.types import (
    TimestampType,
    ArrayType,
    StringType,
    LongType,
    DecimalType,
)

from features.utils.dataframes.edi_results.energy_result_points_per_ga_v1_view_schema import (
    energy_result_points_per_ga_v1_view_schema,
)


def create_energy_result_points_per_ga_v1_view(
    spark: SparkSession, df: DataFrame
) -> DataFrame:
    df = df.withColumn(
        "calculation_version",
        col("calculation_version").cast(LongType()),
    )

    df = df.withColumn(
        "calculation_period_start",
        col("calculation_period_start").cast(TimestampType()),
    )

    df = df.withColumn(
        "calculation_period_end",
        col("calculation_period_end").cast(TimestampType()),
    )

    df = df.withColumn(
        "quantity",
        col("quantity").cast(DecimalType(18, 3)),
    )

    df = df.withColumn(
        "time",
        col("time").cast(TimestampType()),
    )

    parse_qualities_string_udf = udf(_parse_qualities, ArrayType(StringType()))
    df = df.withColumn(
        "quantity_qualities",
        parse_qualities_string_udf(df["quantity_qualities"]),
    )

    return spark.createDataFrame(df.rdd, energy_result_points_per_ga_v1_view_schema)


def _parse_qualities(qualities_str: str) -> list[dict]:
    # Parse the string into a list of dictionaries
    return literal_eval(qualities_str)
