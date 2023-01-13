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

from pyspark.sql import DataFrame

from pyspark.sql.functions import (
    col,
    when,
)
from package.codelists import (
    MeteringPointType,
)
from package.constants import Colname
from package.db_logging import debug
from datetime import datetime


def get_metering_point_periods_df(
    metering_points_periods_df: DataFrame,
    grid_area_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:

    grid_area_df = grid_area_df.withColumnRenamed(Colname.grid_area, "ga_GridAreaCode")
    metering_points_in_grid_area = metering_points_periods_df.join(
        grid_area_df,
        metering_points_periods_df[Colname.grid_area]
        == grid_area_df["ga_GridAreaCode"],
        "inner",
    )

    metering_point_periods_df = metering_points_in_grid_area.where(
        col(Colname.from_date) < period_end_datetime
    ).where(
        col(Colname.to_date).isNull() | (col(Colname.to_date) > period_start_datetime)
    )

    master_basis_data_df = metering_point_periods_df.withColumn(
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

    master_basis_data_df = master_basis_data_df.select(
        Colname.metering_point_id,
        Colname.grid_area,
        Colname.from_date,
        Colname.to_date,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.out_grid_area,
        Colname.in_grid_area,
        Colname.resolution,
        Colname.energy_supplier_id,
        Colname.balance_responsible_id,
    )
    debug(
        "Metering point events before join with grid areas",
        metering_point_periods_df.orderBy(col(Colname.from_date).desc()),
    )

    debug(
        "Metering point periods",
        metering_point_periods_df.orderBy(
            col(Colname.grid_area),
            col(Colname.metering_point_id),
            col(Colname.from_date),
        ),
    )
    return master_basis_data_df
