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

to_sum = "to_sum"
from_sum = "from_sum"
exchange_in_to_grid_area = "ExIn_ToGridArea"
exchange_in_from_grid_area = "ExIn_FromGridArea"
exchange_out_to_grid_area = "ExOut_ToGridArea"
exchange_out_from_grid_area = "ExOut_FromGridArea"


# Function to aggregate net exchange per neighbouring grid areas
def aggregate_net_exchange_per_neighbour_ga(
    enriched_time_series: DataFrame,
) -> DataFrame:
    df = enriched_time_series.where(
        F.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )

    group_by = [
        Colname.to_grid_area,
        Colname.from_grid_area,
        Colname.time_window,
    ]

    exchange_to = (
        T.aggregate_sum_and_set_quality(df, Colname.quantity, group_by)
        .withColumnRenamed(Colname.sum_quantity, to_sum)
        .withColumnRenamed(Colname.to_grid_area, exchange_in_to_grid_area)
        .withColumnRenamed(Colname.from_grid_area, exchange_in_from_grid_area)
    )
    exchange_from = (
        T.aggregate_sum_and_set_quality(df, Colname.quantity, group_by)
        .withColumnRenamed(Colname.sum_quantity, from_sum)
        .withColumnRenamed(Colname.to_grid_area, exchange_out_to_grid_area)
        .withColumnRenamed(Colname.from_grid_area, exchange_out_from_grid_area)
    )

    exchange = (
        exchange_to.join(exchange_from, [Colname.time_window], "inner")
        .filter(
            exchange_to[exchange_in_to_grid_area]
            == exchange_from[exchange_out_from_grid_area]
        )
        .filter(
            exchange_to[exchange_in_from_grid_area]
            == exchange_from[exchange_out_to_grid_area]
        )
        .select(exchange_to["*"], exchange_from[from_sum])
        .withColumn(Colname.sum_quantity, F.col(to_sum) - F.col(from_sum))
        .withColumnRenamed(exchange_in_to_grid_area, Colname.to_grid_area)
        .withColumnRenamed(exchange_in_from_grid_area, Colname.from_grid_area)
        .select(
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            F.col(Colname.to_grid_area).alias(Colname.grid_area),
            F.lit(MeteringPointType.EXCHANGE.value).alias(Colname.metering_point_type),
        )
    )
    return T.create_dataframe_from_aggregation_result_schema(exchange)


# Function to aggregate net exchange per grid area
def aggregate_net_exchange_per_ga(df: DataFrame) -> DataFrame:
    exchange_to = df.filter(
        F.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )
    exchange_to_group_by = [
        Colname.to_grid_area,
        Colname.time_window,
    ]
    exchange_to = (
        T.aggregate_sum_and_set_quality(
            exchange_to, Colname.quantity, exchange_to_group_by
        )
        .withColumnRenamed(Colname.sum_quantity, to_sum)
        .withColumnRenamed(Colname.to_grid_area, Colname.grid_area)
    )

    exchange_from = df.filter(
        F.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )
    exchange_from_group_by = [
        Colname.from_grid_area,
        Colname.time_window,
    ]
    exchange_from = (
        T.aggregate_sum_and_set_quality(
            exchange_from, Colname.quantity, exchange_from_group_by
        )
        .withColumnRenamed(Colname.sum_quantity, from_sum)
        .withColumnRenamed(Colname.from_grid_area, Colname.grid_area)
        .withColumnRenamed(Colname.time_window, "from_time_window")
    )
    joined = exchange_to.join(
        exchange_from,
        (exchange_to[Colname.grid_area] == exchange_from[Colname.grid_area])
        & (exchange_to[Colname.time_window] == exchange_from["from_time_window"]),
        how="outer",
    ).select(
        exchange_to["*"],
        exchange_from[from_sum],
        exchange_from[Colname.grid_area].alias("from_grid_area"),
        exchange_from["from_time_window"],
    )
    result_df = (
        joined
        # Set null sums to 0 to avoid null values in the sum column
        .withColumn(
            to_sum,
            F.when(joined[to_sum].isNotNull(), joined[to_sum]).otherwise(F.lit(0)),
        )
        .withColumn(
            from_sum,
            F.when(joined[from_sum].isNotNull(), joined[from_sum]).otherwise(F.lit(0)),
        )
        .withColumn(Colname.sum_quantity, F.col(to_sum) - F.col(from_sum))
        # when().otherwise() cases to handle the case where a metering point exists with an from-grid-area, which never occurs as an to-grid-area
        .withColumn(
            Colname.grid_area,
            F.when(
                F.col(Colname.grid_area).isNotNull(), F.col(Colname.grid_area)
            ).otherwise(F.col("from_grid_area")),
        )
        .withColumn(
            Colname.time_window,
            F.when(
                F.col(Colname.time_window).isNotNull(), F.col(Colname.time_window)
            ).otherwise(F.col("from_time_window")),
        )
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.quality,
            F.lit(MeteringPointType.EXCHANGE.value).alias(Colname.metering_point_type),
        )
    )
    return T.create_dataframe_from_aggregation_result_schema(result_df)
