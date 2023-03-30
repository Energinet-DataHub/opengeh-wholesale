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
from pyspark.sql.functions import (
    col,
    lit,
    window,
)

in_sum = "in_sum"
out_sum = "out_sum"
exchange_in_in_grid_area = "ExIn_InMeteringGridArea_Domain_mRID"
exchange_in_out_grid_area = "ExIn_OutMeteringGridArea_Domain_mRID"
exchange_out_in_grid_area = "ExOut_InMeteringGridArea_Domain_mRID"
exchange_out_out_grid_area = "ExOut_OutMeteringGridArea_Domain_mRID"


# Function to aggregate net exchange per neighbouring grid areas (step 1)
def aggregate_net_exchange_per_neighbour_ga(
    enriched_time_series: DataFrame,
) -> DataFrame:
    df = enriched_time_series.where(
        col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )
    exchange_in = (
        df.groupBy(
            Colname.in_grid_area,
            Colname.out_grid_area,
            Colname.time_window,
            Colname.aggregated_quality,
        )
        .sum(Colname.quantity)
        .withColumnRenamed(f"sum({Colname.quantity})", in_sum)
        .withColumnRenamed(Colname.in_grid_area, exchange_in_in_grid_area)
        .withColumnRenamed(Colname.out_grid_area, exchange_in_out_grid_area)
    )
    exchange_out = (
        df.groupBy(
            Colname.in_grid_area,
            Colname.out_grid_area,
            Colname.time_window,
        )
        .sum(Colname.quantity)
        .withColumnRenamed(f"sum({Colname.quantity})", out_sum)
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
        .withColumn(Colname.sum_quantity, col(in_sum) - col(out_sum))
        .withColumnRenamed(exchange_in_in_grid_area, Colname.in_grid_area)
        .withColumnRenamed(exchange_in_out_grid_area, Colname.out_grid_area)
        .withColumnRenamed(Colname.aggregated_quality, Colname.quality)
        .select(
            Colname.in_grid_area,
            Colname.out_grid_area,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            col(Colname.in_grid_area).alias(Colname.grid_area),
            lit(MeteringPointType.exchange.value).alias(Colname.metering_point_type),
        )
    )
    return T.create_dataframe_from_aggregation_result_schema(exchange)


# Function to aggregate net exchange per grid area (step 2)
def aggregate_net_exchange_per_ga(df: DataFrame) -> DataFrame:
    exchange_in = df.filter(
        col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )
    exchange_in = (
        exchange_in.groupBy(
            Colname.in_grid_area,
            window(col(Colname.observation_time), "1 hour"),
            Colname.aggregated_quality,
        )
        .sum(Colname.quantity)
        .withColumnRenamed(f"sum({Colname.quantity})", in_sum)
        .withColumnRenamed("window", Colname.time_window)
        .withColumnRenamed(Colname.in_grid_area, Colname.grid_area)
    )
    exchange_out = df.filter(
        col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )
    exchange_out = (
        exchange_out.groupBy(
            Colname.out_grid_area, window(col(Colname.observation_time), "1 hour")
        )
        .sum(Colname.quantity)
        .withColumnRenamed(f"sum({Colname.quantity})", out_sum)
        .withColumnRenamed("window", Colname.time_window)
        .withColumnRenamed(Colname.out_grid_area, Colname.grid_area)
    )
    joined = exchange_in.join(
        exchange_out,
        (exchange_in[Colname.grid_area] == exchange_out[Colname.grid_area])
        & (exchange_in[Colname.time_window] == exchange_out[Colname.time_window]),
        how="outer",
    ).select(exchange_in["*"], exchange_out[out_sum])
    result_df = (
        joined.withColumn(Colname.sum_quantity, joined[in_sum] - joined[out_sum])
        .withColumnRenamed(Colname.aggregated_quality, Colname.quality)
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.quality,
            lit(MeteringPointType.exchange.value).alias(Colname.metering_point_type),
        )
    )
    return T.create_dataframe_from_aggregation_result_schema(result_df)
