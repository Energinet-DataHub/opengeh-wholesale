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
    LongType,
)


def create_calculations_dataframe(spark: SparkSession, df: DataFrame) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants.basis_data_colname import CalculationsColumnName
    from package.calculation.basis_data.schemas import calculations_schema

    df = df.withColumn(
        CalculationsColumnName.period_start,
        col(CalculationsColumnName.period_start).cast(TimestampType()),
    )

    df = df.withColumn(
        CalculationsColumnName.period_end,
        col(CalculationsColumnName.period_end).cast(TimestampType()),
    )

    df = df.withColumn(
        CalculationsColumnName.execution_time_start,
        col(CalculationsColumnName.execution_time_start).cast(TimestampType()),
    )

    df = df.withColumn(
        CalculationsColumnName.version,
        col(CalculationsColumnName.version).cast(LongType()),
    )

    return spark.createDataFrame(df.rdd, calculations_schema)
