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
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType

from package.constants import Colname
from package.codelists import MeteringPointResolution
from package.infrastructure.db_logging import debug
from package.calculation_input.schemas import time_series_point_schema


def get_basis_data_time_series_points_df(
    raw_time_series_points_df: DataFrame,
    metering_point_periods_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    """Get enriched time-series points in quarterly resolution"""
    if raw_time_series_points_df.schema != time_series_point_schema:
        raise ValueError(
            f"Schema mismatch. Expected {time_series_point_schema}, got {raw_time_series_points_df.schema}"
        )

    raw_time_series_points_df = raw_time_series_points_df.where(
        F.col(Colname.observation_time) >= period_start_datetime
    ).where(F.col(Colname.observation_time) < period_end_datetime)

    quarterly_mp_df = metering_point_periods_df.where(
        F.col(Colname.resolution) == MeteringPointResolution.QUARTER.value
    ).withColumn(
        Colname.to_date, (F.col(Colname.to_date) - F.expr("INTERVAL 1 seconds"))
    )

    hourly_mp_df = metering_point_periods_df.where(
        F.col(Colname.resolution) == MeteringPointResolution.HOUR.value
    ).withColumn(
        Colname.to_date, (F.col(Colname.to_date) - F.expr("INTERVAL 1 seconds"))
    )

    quarterly_times_df = (
        quarterly_mp_df.select(
            Colname.metering_point_id, Colname.from_date, Colname.to_date
        )
        .distinct()
        .select(
            Colname.metering_point_id,
            F.expr(
                f"sequence(to_timestamp({Colname.from_date}), to_timestamp({Colname.to_date}), interval 15 minutes)"
            ).alias("quarter_times"),
        )
        .select(
            Colname.metering_point_id,
            F.explode("quarter_times").alias(Colname.observation_time),
        )
    )

    hourly_times_df = (
        hourly_mp_df.select(
            Colname.metering_point_id, Colname.from_date, Colname.to_date
        )
        .distinct()
        .select(
            Colname.metering_point_id,
            F.expr(
                f"sequence(to_timestamp({Colname.from_date}), to_timestamp({Colname.to_date}), interval 1 hour)"
            ).alias("times"),
        )
        .select(
            Colname.metering_point_id,
            F.explode("times").alias(Colname.observation_time),
        )
    )

    empty_points_for_each_metering_point_df = quarterly_times_df.union(hourly_times_df)

    debug(
        "Time series points where time is within period",
        raw_time_series_points_df.orderBy(
            Colname.metering_point_id, F.col(Colname.observation_time)
        ),
    )

    raw_time_series_points_df = raw_time_series_points_df.select(
        Colname.metering_point_id,
        Colname.observation_time,
        Colname.quantity,
        Colname.quality,
    )

    new_points_for_each_metering_point_df = (
        empty_points_for_each_metering_point_df.join(
            raw_time_series_points_df,
            [Colname.metering_point_id, Colname.observation_time],
            "left",
        )
    )

    # the master_basis_data_df is allready used once when creating the empty_points_for_each_metering_point_df
    # rejoining master_basis_data_df with empty_points_for_each_metering_point_df requires the GsrNumber and
    # Resolution column must be renamed for the select to be succesfull.

    new_points_for_each_metering_point_df = (
        new_points_for_each_metering_point_df.withColumnRenamed(
            Colname.metering_point_id, "pfemp_MeteringPointId"
        ).withColumnRenamed(Colname.resolution, "pfemp_Resolution")
    )

    master_basis_data_renamed_df = metering_point_periods_df.withColumnRenamed(
        Colname.metering_point_id, "master_MeteringPointId"
    ).withColumnRenamed(Colname.resolution, "master_Resolution")

    return (
        new_points_for_each_metering_point_df.withColumn(
            Colname.quantity, F.col(Colname.quantity).cast(DecimalType(18, 6))
        )
        .join(
            master_basis_data_renamed_df,
            (
                master_basis_data_renamed_df["master_MeteringPointId"]
                == new_points_for_each_metering_point_df["pfemp_MeteringPointId"]
            )
            & (
                new_points_for_each_metering_point_df[Colname.observation_time]
                >= F.col(Colname.from_date)
            )
            & (
                new_points_for_each_metering_point_df[Colname.observation_time]
                < F.col(Colname.to_date)
            ),
            "left",
        )
        .select(
            Colname.grid_area,
            Colname.to_grid_area,
            Colname.from_grid_area,
            master_basis_data_renamed_df["master_MeteringPointId"].alias(
                Colname.metering_point_id
            ),
            Colname.metering_point_type,
            master_basis_data_renamed_df["master_Resolution"].alias(Colname.resolution),
            Colname.observation_time,
            Colname.quantity,
            Colname.quality,
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
        )
    )
