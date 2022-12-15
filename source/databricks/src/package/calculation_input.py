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
    greatest,
    least,
)
from package.codelists import (
    ConnectionState,
    MeteringPointType,
)

from package.db_logging import debug
from datetime import datetime


def get_metering_point_periods_df(
    metering_points_periods_df: DataFrame,
    energy_supplier_periods_df: DataFrame,
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
        .where(col("ToDate") > period_start_datetime)
        .where(
            (col("ConnectionState") == ConnectionState.connected.value)
            | (col("ConnectionState") == ConnectionState.disconnected.value)
        )
        .where(col("MeteringPointType") == MeteringPointType.production.value)
    )

    energy_supplier_periods_df = energy_supplier_periods_df.where(
        col("FromDate") < period_end_datetime
    ).where(col("ToDate") > period_start_datetime)

    master_basis_data_df = (
        metering_point_periods_df.join(
            energy_supplier_periods_df,
            (
                metering_point_periods_df["MeteringPointId"]
                == energy_supplier_periods_df["MeteringPointId"]
            )
            & (
                energy_supplier_periods_df["FromDate"]
                < metering_point_periods_df["ToDate"]
            )
            & (
                metering_point_periods_df["FromDate"]
                < energy_supplier_periods_df["ToDate"]
            ),
            "left",
        )
        .withColumn(
            "EffectiveDate",
            greatest(
                metering_point_periods_df["FromDate"],
                energy_supplier_periods_df["FromDate"],
            ),
        )
        .withColumn(
            "toEffectiveDate",
            least(
                metering_point_periods_df["ToDate"],
                energy_supplier_periods_df["ToDate"],
            ),
        )
        .withColumn(
            "EffectiveDate",
            when(
                col("EffectiveDate") < period_start_datetime, period_start_datetime
            ).otherwise(col("EffectiveDate")),
        )
        .withColumn(
            "toEffectiveDate",
            when(
                col("toEffectiveDate") > period_end_datetime, period_end_datetime
            ).otherwise(col("toEffectiveDate")),
        )
    )

    master_basis_data_df = master_basis_data_df.select(
        metering_point_periods_df["MeteringPointId"],
        metering_point_periods_df["GridAreaCode"],
        "EffectiveDate",
        "toEffectiveDate",
        "MeteringPointType",
        "SettlementMethod",
        metering_point_periods_df["ToGridAreaCode"],
        metering_point_periods_df["FromGridAreaCode"],
        "Resolution",
        energy_supplier_periods_df["EnergySupplierId"],
    )
    debug(
        "Metering point events before join with grid areas",
        metering_point_periods_df.orderBy(col("FromDate").desc()),
    )

    debug(
        "Metering point periods",
        metering_point_periods_df.orderBy(
            col("GridAreaCode"), col("MeteringPointId"), col("FromDate")
        ),
    )
    return master_basis_data_df
