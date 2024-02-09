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

from datetime import datetime

from pyspark.sql import DataFrame
import pyspark.sql.functions as f


def clamp_period(
    df: DataFrame,
    clamp_start_datetime: datetime,
    clamp_end_datetime: datetime,
    period_start_column_name: str,
    period_end_column_name: str,
) -> DataFrame:
    """
    Clamps the period of a dataframe to the given start and end datetimes.

    If the start date is earlier than ´clamp_start_datetime´: set start date equal to ´clamp_start_datetime´.

    If the end date is null or if it is later than ´clamp_end_datetime´: set end date equal to ´clamp_end_datetime´.
    """
    df = df.withColumn(
        period_start_column_name,
        f.when(
            f.col(period_start_column_name) < clamp_start_datetime, clamp_start_datetime
        ).otherwise(f.col(period_start_column_name)),
    ).withColumn(
        period_end_column_name,
        f.when(
            f.col(period_end_column_name).isNull()
            | (f.col(period_end_column_name) > clamp_end_datetime),
            clamp_end_datetime,
        ).otherwise(f.col(period_end_column_name)),
    )

    return df
