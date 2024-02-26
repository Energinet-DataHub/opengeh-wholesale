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
from datetime import datetime

from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col, udf
from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    DecimalType,
    TimestampType,
    ArrayType,
)


def create_result_dataframe(*args) -> DataFrame:  # type: ignore
    spark: SparkSession = args[0]
    df: DataFrame = args[1]

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants import Colname
    from package.calculation.energy.energy_results import energy_results_schema

    parse_time_window_udf = udf(
        _parse_time_window,
        StructType(
            [
                StructField(Colname.start, TimestampType()),
                StructField(Colname.end, TimestampType()),
            ]
        ),
    )

    df = df.withColumn(
        Colname.time_window, parse_time_window_udf(df[Colname.time_window])
    )
    df = df.withColumn(
        Colname.sum_quantity, col(Colname.sum_quantity).cast(DecimalType(38, 6))
    )

    parse_qualities_string_udf = udf(_parse_qualities, ArrayType(StringType()))
    df = df.withColumn(
        Colname.quantity, parse_qualities_string_udf(df[Colname.quantity])
    )
    df = df.withColumnRenamed(Colname.quantity, Colname.qualities)

    return spark.createDataFrame(df.rdd, energy_results_schema)


def _parse_time_window(time_window_str: str) -> tuple[datetime, datetime]:
    time_window_str = time_window_str.replace("{", "").replace("}", "")
    start_str, end_str = time_window_str.split(",")
    start = datetime.strptime(start_str, "%Y-%m-%d %H:%M:%S")
    end = datetime.strptime(end_str, "%Y-%m-%d %H:%M:%S")
    return start, end


def _parse_qualities(qualities_str: str) -> list[str]:
    return literal_eval(qualities_str)
