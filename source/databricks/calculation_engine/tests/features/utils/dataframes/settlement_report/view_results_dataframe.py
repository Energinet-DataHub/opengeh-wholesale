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
)

from features.public_data_model.given_basis_data_for_settlement_report.common.schemas.metering_point_time_series_schema import (
    element,
)


def create_metering_point_periods_view(df: DataFrame, spark: SparkSession) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from package.constants import MeteringPointPeriodColname
    from features.public_data_model.given_basis_data_for_settlement_report.common import (
        metering_point_period_schema,
    )

    df = df.withColumn(
        MeteringPointPeriodColname.from_date,
        col(MeteringPointPeriodColname.from_date).cast(TimestampType()),
    )
    df = df.withColumn(
        MeteringPointPeriodColname.to_date,
        col(MeteringPointPeriodColname.to_date).cast(TimestampType()),
    )

    return spark.createDataFrame(df.rdd, metering_point_period_schema)


def create_metering_point_time_series_view(
    df: DataFrame, spark: SparkSession
) -> DataFrame:

    # Don't remove. Believed needed because this function is an argument to the setup function
    # and therefore the following packages are not automatically included.
    from features.public_data_model.given_basis_data_for_settlement_report.common import (
        MeteringPointTimeSeriesColname,
    )
    from features.public_data_model.given_basis_data_for_settlement_report.common import (
        metering_point_time_series_schema,
    )

    df = df.withColumn(
        MeteringPointTimeSeriesColname.observation_day,
        col(MeteringPointTimeSeriesColname.observation_day).cast(TimestampType()),
    )

    df = df.withColumn(
        MeteringPointTimeSeriesColname.quantities,
        from_json(col(MeteringPointTimeSeriesColname.quantities), ArrayType(element)),
    )

    return spark.createDataFrame(df.rdd, metering_point_time_series_schema)


def _parse_qualities(qualities_str: str) -> list[dict]:
    # Parse the string into a list of dictionaries
    return literal_eval(qualities_str)
