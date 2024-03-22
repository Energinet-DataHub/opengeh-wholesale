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
from pyspark.sql import functions as f, DataFrame, SparkSession
from pyspark.sql.functions import lit, col
from pyspark.sql.types import (
    StringType,
    TimestampType,
    BooleanType,
    DecimalType,
    ArrayType,
)


def create_wholesale_result_dataframe(*args) -> DataFrame:
    spark: SparkSession = args[0]
    df: DataFrame = args[1]
    calculator_args = args[2]  # type: ignore

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.calculation.output.schemas import wholesale_results_schema
    from package.constants import WholesaleResultColumnNames

    df = df.withColumn(
        WholesaleResultColumnNames.calculation_execution_time_start,
        lit(calculator_args.calculation_execution_time_start).cast(TimestampType()),
    )

    df = df.withColumn(WholesaleResultColumnNames.calculation_result_id, lit(""))

    df = df.withColumn(
        WholesaleResultColumnNames.quantity,
        col(WholesaleResultColumnNames.quantity).cast(DecimalType(28, 3)),
    )

    df = df.withColumn(
        WholesaleResultColumnNames.price,
        col(WholesaleResultColumnNames.price).cast(DecimalType(18, 6)),
    )

    df = df.withColumn(
        WholesaleResultColumnNames.amount,
        col(WholesaleResultColumnNames.amount).cast(DecimalType(18, 6)),
    )
    df = df.withColumn(
        WholesaleResultColumnNames.time,
        col(WholesaleResultColumnNames.time).cast(TimestampType()),
    )

    df = df.withColumn(
        WholesaleResultColumnNames.is_tax,
        col(WholesaleResultColumnNames.is_tax).cast(BooleanType()),
    )

    df = df.withColumn(
        WholesaleResultColumnNames.quantity_qualities,
        f.split(
            f.regexp_replace(
                f.regexp_replace(
                    f.col(WholesaleResultColumnNames.quantity_qualities), r"[\[\]']", ""
                ),
                " ",
                "",
            ),
            ",",
        ).cast(ArrayType(StringType())),
    )

    return spark.createDataFrame(df.rdd, wholesale_results_schema)
