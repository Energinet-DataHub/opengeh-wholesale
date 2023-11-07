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
from pyspark.sql.types import DecimalType

from package.constants import Colname
from package.codelists import MeteringPointResolution, QuantityQuality
from package.infrastructure.db_logging import debug
from package.common import assert_schema
from package.calculation_input.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
)


# TODO BJM: Settle on name: Basis data ts? Enriched ts? Metering point ts? Other?
#           And update file names, suts, variables etc
def get_metering_point_time_series(
    raw_time_series_points_df: DataFrame,
    metering_point_periods_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    """
    Get enriched time-series points - both for metering points with hourly and quarterly resolution.
    """
    assert_schema(raw_time_series_points_df.schema, time_series_point_schema)
    assert_schema(metering_point_periods_df.schema, metering_point_period_schema)

    raw_time_series_points_df = raw_time_series_points_df.withColumn(
        Colname.quantity, f.col(Colname.quantity).cast(DecimalType(18, 6))
    )

    raw_time_series_points_df = raw_time_series_points_df.where(
        f.col(Colname.observation_time) >= period_start_datetime
    ).where(f.col(Colname.observation_time) < period_end_datetime)

    quarterly_mp_df = metering_point_periods_df.where(
        f.col(Colname.resolution) == MeteringPointResolution.QUARTER.value
    ).withColumn(
        Colname.to_date, (f.col(Colname.to_date) - f.expr("INTERVAL 1 seconds"))
    )

    hourly_mp_df = metering_point_periods_df.where(
        f.col(Colname.resolution) == MeteringPointResolution.HOUR.value
    ).withColumn(
        Colname.to_date, (f.col(Colname.to_date) - f.expr("INTERVAL 1 seconds"))
    )

    quarterly_times_df = (
        quarterly_mp_df.select(
            Colname.metering_point_id,
            Colname.metering_point_type,
            Colname.resolution,
            Colname.grid_area,
            Colname.from_date,
            Colname.to_date,
        )
        .distinct()
        .withColumn(
            "quarter_times",
            f.expr(
                f"sequence(to_timestamp({Colname.from_date}), to_timestamp({Colname.to_date}), interval 15 minutes)"
            ),
        )
        .select(
            Colname.metering_point_id,
            Colname.grid_area,
            Colname.metering_point_type,
            Colname.resolution,
            f.explode("quarter_times").alias(Colname.observation_time),
        )
    )

    hourly_times_df = (
        hourly_mp_df.select(
            Colname.metering_point_id,
            Colname.metering_point_type,
            Colname.resolution,
            Colname.grid_area,
            Colname.from_date,
            Colname.to_date,
        )
        .distinct()
        .withColumn(
            "times",
            f.expr(
                f"sequence(to_timestamp({Colname.from_date}), to_timestamp({Colname.to_date}), interval 1 hour)"
            ),
        )
        .select(
            Colname.metering_point_id,
            Colname.grid_area,
            Colname.metering_point_type,
            Colname.resolution,
            f.explode("times").alias(Colname.observation_time),
        )
    )

    empty_points_for_each_metering_point_df = quarterly_times_df.union(hourly_times_df)

    debug(
        "Time series points where time is within period",
        raw_time_series_points_df.orderBy(
            Colname.metering_point_id, f.col(Colname.observation_time)
        ),
    )

    # Quality of metering point time series are mandatory. This result has, however, been padded with
    # time series points that haven't been provided by the market actors. These added points must have
    # the quality "missing".
    # Quantity is set to 0 as well in case of missing values.
    new_points_for_each_metering_point_df = (
        empty_points_for_each_metering_point_df.join(
            raw_time_series_points_df,
            [Colname.metering_point_id, Colname.observation_time],
            "left",
        )
        .na.fill(value=QuantityQuality.MISSING.value, subset=[Colname.quality])
        .select(
            Colname.metering_point_id,
            Colname.observation_time,
            Colname.grid_area,
            Colname.metering_point_type,
            Colname.resolution,
            f.coalesce(Colname.quantity, f.lit(0)).alias(Colname.quantity),
            Colname.quality,
        )
    )

    # The master_basis_data_df is already used once when creating the empty_points_for_each_metering_point_df
    # rejoining master_basis_data_df with empty_points_for_each_metering_point_df requires the GSRN number and
    # Resolution column must be renamed for the select to be successful.

    result = (
        new_points_for_each_metering_point_df.withColumn(
            Colname.quantity, f.col(Colname.quantity).cast(DecimalType(18, 6))
        )
        .join(
            metering_point_periods_df,
            (
                metering_point_periods_df[Colname.metering_point_id]
                == new_points_for_each_metering_point_df[Colname.metering_point_id]
            )
            & (
                new_points_for_each_metering_point_df[Colname.observation_time]
                >= f.col(Colname.from_date)
            )
            & (
                new_points_for_each_metering_point_df[Colname.observation_time]
                < f.col(Colname.to_date)
            ),
            "left",
        )
        .select(
            new_points_for_each_metering_point_df[Colname.grid_area],
            Colname.to_grid_area,
            Colname.from_grid_area,
            new_points_for_each_metering_point_df[Colname.metering_point_id],
            new_points_for_each_metering_point_df[Colname.metering_point_type],
            new_points_for_each_metering_point_df[Colname.resolution],
            new_points_for_each_metering_point_df[Colname.observation_time],
            Colname.quantity,
            Colname.quality,
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
            Colname.settlement_method,
        )
    )

    return result
