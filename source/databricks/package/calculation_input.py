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
    year,
)
from package.codelists import (
    MeteringPointType,
)

from package.db_logging import debug
from datetime import datetime


def get_metering_point_periods_df(
    metering_points_periods_df: DataFrame,
    grid_area_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:

    grid_area_df = grid_area_df.withColumnRenamed("GridAreaCode", "ga_GridAreaCode")
    metering_points_in_grid_area = metering_points_periods_df.join(
        grid_area_df,
        metering_points_periods_df["GridAreaCode"] == grid_area_df["ga_GridAreaCode"],
        "inner",
    )

    metering_point_periods_df = (
        metering_points_in_grid_area.where(col("FromDate") < period_end_datetime)
        .where(col("ToDate_Year") < year(period_end_datetime))
        .where(col("ToDate") > period_start_datetime)
        .where(col("Type") == MeteringPointType.production.value)
        .withColumnRenamed("FromDate", "EffectiveDate")
        .withColumnRenamed("ToDate", "toEffectiveDate")
    )

    master_basis_data_df = metering_point_periods_df.withColumn(
        "EffectiveDate",
        when(
            col("EffectiveDate") < period_start_datetime, period_start_datetime
        ).otherwise(col("EffectiveDate")),
    ).withColumn(
        "toEffectiveDate",
        when(
            col("toEffectiveDate") > period_end_datetime, period_end_datetime
        ).otherwise(col("toEffectiveDate")),
    )

    master_basis_data_df = master_basis_data_df.select(
        "MeteringPointId",
        "GridAreaCode",
        "EffectiveDate",
        "toEffectiveDate",
        "Type",
        "SettlementMethod",
        "ToGridAreaCode",
        "FromGridAreaCode",
        "Resolution",
        "EnergySupplierId",
    )
    debug(
        "Metering point events before join with grid areas",
        metering_point_periods_df.orderBy(col("EffectiveDate").desc()),
    )

    debug(
        "Metering point periods",
        metering_point_periods_df.orderBy(
            col("GridAreaCode"), col("MeteringPointId"), col("EffectiveDate")
        ),
    )
    return master_basis_data_df
