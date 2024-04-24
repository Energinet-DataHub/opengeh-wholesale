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


def create_total_monthly_amounts_dataframe(*args) -> DataFrame:
    spark: SparkSession = args[0]
    df: DataFrame = args[1]

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.calculation.output.schemas.total_monthly_amounts_schema import (
        total_monthly_amounts_schema,
    )
    from package.constants import TotalMonthlyAmountsColumnNames

    df = df.withColumn(
        TotalMonthlyAmountsColumnNames.amount,
        col(TotalMonthlyAmountsColumnNames.amount).cast(DecimalType(18, 6)),
    )
    df = df.withColumn(
        TotalMonthlyAmountsColumnNames.time,
        col(TotalMonthlyAmountsColumnNames.time).cast(TimestampType()),
    )

    return spark.createDataFrame(df.rdd, total_monthly_amounts_schema)
