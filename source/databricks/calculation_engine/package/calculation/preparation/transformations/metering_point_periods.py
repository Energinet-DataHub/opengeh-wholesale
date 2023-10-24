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

from pyspark.sql.functions import (
    col,
    when,
)
from package.constants import Colname
from datetime import datetime
from package.calculation_input import TableReader
from .batch_grid_areas import (
    get_batch_grid_areas_df,
    check_all_grid_areas_have_metering_points,
)


def get_metering_point_periods_df(
    calculation_input_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    batch_grid_areas: list[str],
) -> DataFrame:
    metering_point_periods_df = calculation_input_reader.read_metering_point_periods()

    metering_point_periods_df = _filter_by_grid_area(
        metering_point_periods_df, batch_grid_areas
    )

    metering_point_periods_df = _filter_by_period(
        metering_point_periods_df, period_start_datetime, period_end_datetime
    )

    metering_point_periods_df = metering_point_periods_df.select(
        Colname.metering_point_id,
        Colname.metering_point_type,
        Colname.calculation_type,
        Colname.settlement_method,
        Colname.grid_area,
        Colname.resolution,
        Colname.from_grid_area,
        Colname.to_grid_area,
        Colname.parent_metering_point_id,
        Colname.energy_supplier_id,
        Colname.balance_responsible_id,
        Colname.from_date,
        Colname.to_date,
    )

    return metering_point_periods_df


def _filter_by_grid_area(
    metering_points_periods_df: DataFrame, batch_grid_areas: list[str]
) -> DataFrame:
    spark = SparkSession.builder.getOrCreate()
    grid_area_df = get_batch_grid_areas_df(batch_grid_areas, spark)

    check_all_grid_areas_have_metering_points(grid_area_df, metering_points_periods_df)

    grid_area_df = grid_area_df.withColumnRenamed(Colname.grid_area, "ga_GridAreaCode")
    metering_points_periods_df = metering_points_periods_df.join(
        grid_area_df,
        metering_points_periods_df[Colname.grid_area]
        == grid_area_df["ga_GridAreaCode"],
        "inner",
    )

    return metering_points_periods_df


def _filter_by_period(
    metering_points_periods_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    metering_point_periods_df = metering_points_periods_df.where(
        col(Colname.from_date) < period_end_datetime
    ).where(
        col(Colname.to_date).isNull() | (col(Colname.to_date) > period_start_datetime)
    )

    metering_point_periods_df = metering_point_periods_df.withColumn(
        Colname.from_date,
        when(
            col(Colname.from_date) < period_start_datetime, period_start_datetime
        ).otherwise(col(Colname.from_date)),
    ).withColumn(
        Colname.to_date,
        when(
            col(Colname.to_date).isNull()
            | (col(Colname.to_date) > period_end_datetime),
            period_end_datetime,
        ).otherwise(col(Colname.to_date)),
    )

    return metering_point_periods_df
