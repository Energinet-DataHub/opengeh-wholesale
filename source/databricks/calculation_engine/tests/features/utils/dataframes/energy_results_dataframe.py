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
    StringType,
    DecimalType,
    TimestampType,
    ArrayType,
)


def create_energy_result_dataframe(*args) -> DataFrame:
    spark: SparkSession = args[0]
    df: DataFrame = args[1]

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.calculation.output.schemas import energy_results_schema
    from package.constants import EnergyResultColumnNames

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
        EnergyResultColumnNames.calculation_execution_time_start,
        col(EnergyResultColumnNames.calculation_execution_time_start).cast(
            TimestampType()
        ),
    )

    return spark.createDataFrame(df.rdd, energy_results_schema)


def _parse_qualities(qualities_str: str) -> list[str]:
    return literal_eval(qualities_str)
