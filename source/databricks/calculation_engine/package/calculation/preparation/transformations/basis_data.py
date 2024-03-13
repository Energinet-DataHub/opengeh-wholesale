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

import pyspark.sql.functions as F
from pyspark.sql import DataFrame
from pyspark.sql.window import Window

from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.codelists import (
    MeteringPointResolution,
)
from package.constants import Colname, BasisDataColname
from package.infrastructure import logging_configuration


@logging_configuration.use_span("get_master_basis_data")
def get_master_basis_data_df(
    metering_point_df: DataFrame,
) -> DataFrame:
    return metering_point_df.select(
        F.col(Colname.metering_point_id).alias(BasisDataColname.metering_point_id),
        F.col(Colname.from_date).alias(BasisDataColname.valid_from),
        F.col(Colname.to_date).alias(BasisDataColname.valid_to),
        F.col(Colname.grid_area).alias(BasisDataColname.grid_area),
        F.col(Colname.to_grid_area).alias(BasisDataColname.to_grid_area),
        F.col(Colname.from_grid_area).alias(BasisDataColname.from_grid_area),
        F.col(Colname.metering_point_type).alias(BasisDataColname.metering_point_type),
        F.col(Colname.settlement_method).alias(BasisDataColname.settlement_method),
        F.col(Colname.energy_supplier_id).alias(BasisDataColname.energy_supplier_id),
    )


@logging_configuration.use_span("get_metering_point_time_series_basis_data")
def get_metering_point_time_series_basis_data_dfs(
    metering_point_time_series: PreparedMeteringPointTimeSeries, time_zone: str
) -> tuple[DataFrame, DataFrame]:
    """Returns tuple (time_series_quarter_basis_data, time_series_hour_basis_data)"""

    time_series_quarter_basis_data_df = (
        _get_metering_point_time_series_basis_data_by_resolution(
            metering_point_time_series,
            MeteringPointResolution.QUARTER.value,
            time_zone,
        )
    )

    time_series_hour_basis_data_df = (
        _get_metering_point_time_series_basis_data_by_resolution(
            metering_point_time_series,
            MeteringPointResolution.HOUR.value,
            time_zone,
        )
    )

    return time_series_quarter_basis_data_df, time_series_hour_basis_data_df


def _get_metering_point_time_series_basis_data_by_resolution(
    metering_point_time_series: PreparedMeteringPointTimeSeries,
    resolution: str,
    time_zone: str,
) -> DataFrame:
    w = Window.partitionBy(Colname.metering_point_id, Colname.local_date).orderBy(
        Colname.observation_time
    )

    metering_point_time_series_basis_data_df = (
        metering_point_time_series.df.where(F.col(Colname.resolution) == resolution)
        .withColumn(
            Colname.local_date,
            F.to_date(F.from_utc_timestamp(F.col(Colname.observation_time), time_zone)),
        )
        .withColumn(
            "position",
            F.concat(F.lit(BasisDataColname.quantity_prefix), F.row_number().over(w)),
        )
        .withColumn(Colname.start_datetime, F.first(Colname.observation_time).over(w))
        .groupBy(
            Colname.metering_point_id,
            Colname.local_date,
            Colname.start_datetime,
            Colname.grid_area,
            Colname.metering_point_type,
            Colname.resolution,
            Colname.energy_supplier_id,
        )
        .pivot("position")
        .agg(F.first(Colname.quantity))
    )

    quantity_columns = _get_sorted_quantity_columns(
        metering_point_time_series_basis_data_df
    )
    metering_point_time_series_basis_data_df = (
        metering_point_time_series_basis_data_df.select(
            F.col(Colname.grid_area).alias(BasisDataColname.grid_area),
            F.col(Colname.metering_point_id).alias(BasisDataColname.metering_point_id),
            F.col(Colname.metering_point_type).alias(
                BasisDataColname.metering_point_type
            ),
            F.col(Colname.start_datetime).alias(BasisDataColname.start_datetime),
            F.col(Colname.energy_supplier_id).alias(
                BasisDataColname.energy_supplier_id
            ),
            *quantity_columns,
        )
    )
    return metering_point_time_series_basis_data_df


def _get_sorted_quantity_columns(
    metering_point_time_series_basis_data: DataFrame,
) -> list[str]:
    def num_sort(col_name: str) -> int:
        """Extracts the nuber in the string"""
        import re

        return list(map(int, re.findall(r"\d+", col_name)))[0]

    quantity_columns = [
        c
        for c in metering_point_time_series_basis_data.columns
        if c.startswith(BasisDataColname.quantity_prefix)
    ]
    quantity_columns.sort(key=num_sort)
    return quantity_columns
