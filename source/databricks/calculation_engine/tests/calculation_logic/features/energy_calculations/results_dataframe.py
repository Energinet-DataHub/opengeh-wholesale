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
from pyspark.sql.functions import col, udf, lit
from pyspark.sql.types import (
    StringType,
    DecimalType,
    TimestampType,
    ArrayType,
)

from package.calculation_output.schemas import energy_results_schema
from package.codelists import TimeSeriesType, AggregationLevel, CalculationType
from package.constants import EnergyResultColumnNames

CSV_DATE_FORMAT = "%Y-%m-%d %H:%M:%S"


def create_result_dataframe(*args) -> DataFrame:  # type: ignore
    spark: SparkSession = args[0]
    df: DataFrame = args[1]
    calculation_args = args[2]

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.

    df = df.withColumn(
        EnergyResultColumnNames.time,
        col(EnergyResultColumnNames.time).cast(TimestampType()),
    )
    df = df.withColumn(
        EnergyResultColumnNames.quantity,
        col(EnergyResultColumnNames.quantity).cast(DecimalType(18, 3)),
    )

    parse_qualities_string_udf = udf(_parse_qualities, ArrayType(StringType()))
    df = df.withColumn(
        EnergyResultColumnNames.quantity_qualities,
        parse_qualities_string_udf(df[EnergyResultColumnNames.quantity_qualities]),
    )

    df = df.withColumn(
        EnergyResultColumnNames.aggregation_level,
        lit(str(AggregationLevel.ES_PER_GA.value)),
    )

    df = df.withColumn(
        EnergyResultColumnNames.calculation_id, lit(calculation_args.calculation_id)
    )

    df = df.withColumn(
        EnergyResultColumnNames.calculation_type,
        lit(CalculationType(calculation_args.calculation_type).value),
    )

    df = df.withColumn(
        EnergyResultColumnNames.calculation_execution_time_start, lit(datetime.now())
    )

    df = df.withColumn(
        EnergyResultColumnNames.time_series_type,
        lit(TimeSeriesType.FLEX_CONSUMPTION.value),
    )

    df = df.withColumn(
        EnergyResultColumnNames.calculation_result_id,
        lit(calculation_args.calculation_id),
    )

    return spark.createDataFrame(df.rdd, energy_results_schema)


def _parse_qualities(qualities_str: str) -> list[str]:
    return literal_eval(qualities_str)
