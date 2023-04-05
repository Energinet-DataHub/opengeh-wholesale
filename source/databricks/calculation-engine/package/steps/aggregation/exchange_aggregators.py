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

from package.codelists import (
    MeteringPointType,
)
from package.constants import Colname
from . import transformations as T
from pyspark.sql import DataFrame
import pyspark.sql.functions as F

in_sum = "in_sum"
out_sum = "out_sum"
exchange_in_in_grid_area = "ExIn_InMeteringGridArea_Domain_mRID"
exchange_in_out_grid_area = "ExIn_OutMeteringGridArea_Domain_mRID"
exchange_out_in_grid_area = "ExOut_InMeteringGridArea_Domain_mRID"
exchange_out_out_grid_area = "ExOut_OutMeteringGridArea_Domain_mRID"


# Function to aggregate net exchange per neighbouring grid areas
def aggregate_net_exchange_per_neighbour_ga(
    enriched_time_series: DataFrame,
) -> DataFrame:
    df = enriched_time_series.where(
        F.col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )

    group_by = [
        Colname.in_grid_area,
        Colname.out_grid_area,
        Colname.time_window,
    ]

    exchange_in = (
        T.aggregate_sum_and_set_quality(df, Colname.quantity, group_by)
        .withColumnRenamed(Colname.sum_quantity, in_sum)
        .withColumnRenamed(Colname.in_grid_area, exchange_in_in_grid_area)
        .withColumnRenamed(Colname.out_grid_area, exchange_in_out_grid_area)
    )
    exchange_out = (
        T.aggregate_sum_and_set_quality(df, Colname.quantity, group_by)
        .withColumnRenamed(Colname.sum_quantity, out_sum)
        .withColumnRenamed(Colname.in_grid_area, exchange_out_in_grid_area)
        .withColumnRenamed(Colname.out_grid_area, exchange_out_out_grid_area)
    )

    exchange = (
        exchange_in.join(exchange_out, [Colname.time_window], "inner")
        .filter(
            exchange_in.ExIn_InMeteringGridArea_Domain_mRID
            == exchange_out.ExOut_OutMeteringGridArea_Domain_mRID
        )
        .filter(
            exchange_in.ExIn_OutMeteringGridArea_Domain_mRID
            == exchange_out.ExOut_InMeteringGridArea_Domain_mRID
        )
        .select(exchange_in["*"], exchange_out[out_sum])
        .withColumn(Colname.sum_quantity, F.col(in_sum) - F.col(out_sum))
        .withColumnRenamed(exchange_in_in_grid_area, Colname.in_grid_area)
        .withColumnRenamed(exchange_in_out_grid_area, Colname.out_grid_area)
        .select(
            Colname.in_grid_area,
            Colname.out_grid_area,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            F.col(Colname.in_grid_area).alias(Colname.grid_area),
            F.lit(MeteringPointType.exchange.value).alias(Colname.metering_point_type),
        )
    )
    return T.create_dataframe_from_aggregation_result_schema(exchange)


# Function to aggregate net exchange per grid area
def aggregate_net_exchange_per_ga(df: DataFrame) -> DataFrame:
    exchange_in = df.filter(
        F.col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )
    exchange_in_group_by = [
        Colname.in_grid_area,
        Colname.time_window,
    ]
    exchange_in = (
        T.aggregate_sum_and_set_quality(
            exchange_in, Colname.quantity, exchange_in_group_by
        )
        .withColumnRenamed(Colname.sum_quantity, in_sum)
        .withColumnRenamed(Colname.in_grid_area, Colname.grid_area)
    )

    exchange_out = df.filter(
        F.col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )
    exchange_out_group_by = [
        Colname.out_grid_area,
        Colname.time_window,
    ]
    exchange_out = (
        T.aggregate_sum_and_set_quality(
            exchange_out, Colname.quantity, exchange_out_group_by
        )
        .withColumnRenamed(Colname.sum_quantity, out_sum)
        .withColumnRenamed(Colname.out_grid_area, Colname.grid_area)
        .withColumnRenamed(Colname.time_window, "out_time_window")
    )
    joined = exchange_in.join(
        exchange_out,
        (exchange_in[Colname.grid_area] == exchange_out[Colname.grid_area])
        & (exchange_in[Colname.time_window] == exchange_out["out_time_window"]),
        how="outer",
    ).select(
        exchange_in["*"],
        exchange_out[out_sum],
        exchange_out[Colname.grid_area].alias("out_grid_area"),
        exchange_out["out_time_window"],
    )
    result_df = (
        joined.withColumn(Colname.sum_quantity, joined[in_sum] - joined[out_sum])
        # when().otherwise() cases to handle the case where a metering point exists with an out-grid-area, which never occurs as an in-grid-area
        .withColumn(
            Colname.grid_area,
            F.when(
                F.col(Colname.grid_area).isNotNull(), F.col(Colname.grid_area)
            ).otherwise(F.col("out_grid_area")),
        )
        .withColumn(
            Colname.time_window,
            F.when(
                F.col(Colname.time_window).isNotNull(), F.col(Colname.time_window)
            ).otherwise(F.col("out_time_window")),
        )
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.quality,
            F.lit(MeteringPointType.exchange.value).alias(Colname.metering_point_type),
        )
    )
    return T.create_dataframe_from_aggregation_result_schema(result_df)
