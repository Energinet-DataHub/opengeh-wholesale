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
from pyspark.sql.functions import (
    col,
    when,
)


def clamp_period(
    df,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    from_date_column_name: str,
    to_date_column_name: str,
) -> DataFrame:
    df = df.withColumn(
        from_date_column_name,
        when(
            col(from_date_column_name) < period_start_datetime, period_start_datetime
        ).otherwise(col(from_date_column_name)),
    ).withColumn(
        to_date_column_name,
        when(
            col(to_date_column_name).isNull()
            | (col(to_date_column_name) > period_end_datetime),
            period_end_datetime,
        ).otherwise(col(period_end_datetime)),
    )

    return df
