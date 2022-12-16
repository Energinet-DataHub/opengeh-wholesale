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
    concat,
    first,
    lit,
    col,
    to_date,
    from_utc_timestamp,
    row_number,
    when,
)
from pyspark.sql.window import Window
from package.codelists import (
    MeteringPointResolution,
)
from package.constants import Colname
from package.db_logging import debug
from datetime import datetime


def get_master_basis_data_df(
    metering_point_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    return (
        metering_point_df.withColumn(
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
        .select(
            col("GridAreaCode"),  # column is only used for partitioning
            col("MeteringPointId").alias("METERINGPOINTID"),
            col("EffectiveDate").alias("VALIDFROM"),
            col("toEffectiveDate").alias("VALIDTO"),
            col("GridAreaCode").alias("GRIDAREA"),
            col("ToGridAreaCode").alias("TOGRIDAREA"),
            col("FromGridAreaCode").alias("FROMGRIDAREA"),
            col("Type").alias("TYPEOFMP"),
            col("SettlementMethod").alias("SETTLEMENTMETHOD"),
            col("EnergySupplierId").alias(("ENERGYSUPPLIERID")),
        )
    )


def get_time_series_basis_data_dfs(
    enriched_time_series_point_df: DataFrame, time_zone: str
) -> tuple[DataFrame, DataFrame]:
    "Returns tuple (time_series_quarter_basis_data, time_series_hour_basis_data)"

    time_series_quarter_basis_data_df = _get_time_series_basis_data_by_resolution(
        enriched_time_series_point_df,
        MeteringPointResolution.quarter.value,
        time_zone,
    )

    time_series_hour_basis_data_df = _get_time_series_basis_data_by_resolution(
        enriched_time_series_point_df,
        MeteringPointResolution.hour.value,
        time_zone,
    )

    return (time_series_quarter_basis_data_df, time_series_hour_basis_data_df)


def _get_time_series_basis_data_by_resolution(
    enriched_time_series_point_df: DataFrame,
    resolution: str,
    time_zone: str,
) -> DataFrame:
    w = Window.partitionBy("MeteringPointId", "localDate").orderBy(
        Colname.observation_time
    )

    timeseries_basis_data_df = (
        enriched_time_series_point_df.where(col("Resolution") == resolution)
        .withColumn(
            "localDate",
            to_date(from_utc_timestamp(col(Colname.observation_time), time_zone)),
        )
        .withColumn("position", concat(lit("ENERGYQUANTITY"), row_number().over(w)))
        .withColumn("STARTDATETIME", first(Colname.observation_time).over(w))
        .groupBy(
            "MeteringPointId",
            "localDate",
            "STARTDATETIME",
            "GridAreaCode",
            "Type",
            "Resolution",
        )
        .pivot("position")
        .agg(first("Quantity"))
        .withColumnRenamed("MeteringPointId", "METERINGPOINTID")
        .withColumn("TYPEOFMP", col("Type"))
    )

    quantity_columns = _get_sorted_quantity_columns(timeseries_basis_data_df)
    timeseries_basis_data_df = timeseries_basis_data_df.select(
        "GridAreaCode",
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *quantity_columns,
    )
    return timeseries_basis_data_df


def _get_sorted_quantity_columns(timeseries_basis_data: DataFrame) -> list[str]:
    def num_sort(col_name: str) -> int:
        "Extracts the nuber in the string"
        import re

        return list(map(int, re.findall(r"\d+", col_name)))[0]

    quantity_columns = [
        c for c in timeseries_basis_data.columns if c.startswith("ENERGYQUANTITY")
    ]
    quantity_columns.sort(key=num_sort)
    return quantity_columns
