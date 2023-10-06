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
    QuantityQuality,
)
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when, lit
from . import transformations as T
from package.constants import Colname

production_sum_quantity = "production_sum_quantity"
exchange_sum_quantity = "exchange_sum_quantity"
aggregated_production_quality = "aggregated_production_quality"
aggregated_net_exchange_quality = "aggregated_net_exchange_quality"
hourly_result = "hourly_result"
flex_result = "flex_result"
prod_result = "prod_result"


# Function used to calculate grid loss (step 6)
def calculate_grid_loss(
    net_exchange_per_ga: DataFrame,
    non_profiled_consumption: DataFrame,
    flex_consumption: DataFrame,
    production: DataFrame,
) -> DataFrame:
    return __calculate_grid_loss_or_residual_ga(
        net_exchange_per_ga,
        non_profiled_consumption,
        flex_consumption,
        production,
    )


def __calculate_grid_loss_or_residual_ga(
    agg_net_exchange: DataFrame,
    agg_non_profiled_consumption: DataFrame,
    agg_flex_consumption: DataFrame,
    agg_production: DataFrame,
) -> DataFrame:
    agg_net_exchange_result = agg_net_exchange.selectExpr(
        Colname.grid_area,
        f"{Colname.sum_quantity} as net_exchange_result",
        Colname.time_window,
    )
    agg_non_profiled_consumption_result = (
        agg_non_profiled_consumption.selectExpr(
            Colname.grid_area,
            f"{Colname.sum_quantity} as {hourly_result}",
            Colname.time_window,
        )
        .groupBy(Colname.grid_area, Colname.time_window)
        .sum(hourly_result)
        .withColumnRenamed(f"sum({hourly_result})", hourly_result)
    )
    agg_flex_consumption_result = (
        agg_flex_consumption.selectExpr(
            Colname.grid_area,
            f"{Colname.sum_quantity} as {flex_result}",
            Colname.time_window,
        )
        .groupBy(Colname.grid_area, Colname.time_window)
        .sum(flex_result)
        .withColumnRenamed(f"sum({flex_result})", flex_result)
    )
    agg_production_result = (
        agg_production.selectExpr(
            Colname.grid_area,
            f"{Colname.sum_quantity} as {prod_result}",
            Colname.time_window,
        )
        .groupBy(Colname.grid_area, Colname.time_window)
        .sum(prod_result)
        .withColumnRenamed(f"sum({prod_result})", prod_result)
    )

    result = (
        agg_net_exchange_result.join(
            agg_production_result, [Colname.grid_area, Colname.time_window], "left"
        )
        .join(
            agg_flex_consumption_result.join(
                agg_non_profiled_consumption_result,
                [Colname.grid_area, Colname.time_window],
                "left",
            ),
            [Colname.grid_area, Colname.time_window],
            "left",
        )
        .orderBy(Colname.grid_area, Colname.time_window)
    )

    result = result.withColumn(
        Colname.sum_quantity,
        result.net_exchange_result
        + result.prod_result
        - (result.hourly_result + result.flex_result),
    )
    # Quality is always calculated for grid loss entries
    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        Colname.sum_quantity,  # grid loss
        lit(MeteringPointType.CONSUMPTION.value).alias(Colname.metering_point_type),
        lit(QuantityQuality.CALCULATED.value).alias(Colname.quality),
    )
    return T.create_dataframe_from_aggregation_result_schema(result)


# Function to calculate negative grid loss to be added (step 8)
def calculate_negative_grid_loss(grid_loss: DataFrame) -> DataFrame:
    result = grid_loss.withColumn(
        Colname.negative_grid_loss,
        when(
            col(Colname.sum_quantity) < 0, (col(Colname.sum_quantity)) * (-1)
        ).otherwise(0),
    )
    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        col(Colname.negative_grid_loss).alias(Colname.sum_quantity),
        lit(MeteringPointType.PRODUCTION.value).alias(Colname.metering_point_type),
        Colname.quality,
    )

    return T.create_dataframe_from_aggregation_result_schema(result)


# Function to calculate positive grid loss to be added (step 9)
def calculate_positive_grid_loss(grid_loss: DataFrame) -> DataFrame:
    result = grid_loss.withColumn(
        Colname.positive_grid_loss,
        when(col(Colname.sum_quantity) > 0, col(Colname.sum_quantity)).otherwise(0),
    )
    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        col(Colname.positive_grid_loss).alias(Colname.sum_quantity),
        lit(MeteringPointType.CONSUMPTION.value).alias(Colname.metering_point_type),
        Colname.quality,
    )
    return T.create_dataframe_from_aggregation_result_schema(result)


# Function to calculate total consumption (step 21)
def calculate_total_consumption(
    agg_net_exchange: DataFrame, agg_production: DataFrame
) -> DataFrame:
    result_production = (
        agg_production.selectExpr(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.quality,
        )
        .groupBy(Colname.grid_area, Colname.time_window, Colname.quality)
        .sum(Colname.sum_quantity)
        .withColumnRenamed(f"sum({Colname.sum_quantity})", production_sum_quantity)
        .withColumnRenamed(Colname.quality, aggregated_production_quality)
    )

    result_net_exchange = (
        agg_net_exchange.selectExpr(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.quality,
        )
        .groupBy(Colname.grid_area, Colname.time_window, Colname.quality)
        .sum(Colname.sum_quantity)
        .withColumnRenamed(f"sum({Colname.sum_quantity})", exchange_sum_quantity)
        .withColumnRenamed(Colname.quality, aggregated_net_exchange_quality)
    )

    result = result_production.join(
        result_net_exchange, [Colname.grid_area, Colname.time_window], "inner"
    ).withColumn(
        Colname.sum_quantity, col(production_sum_quantity) + col(exchange_sum_quantity)
    )

    result = (
        T.aggregate_total_consumption_quality(result)
        .orderBy(Colname.grid_area, Colname.time_window)
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(MeteringPointType.CONSUMPTION.value).alias(Colname.metering_point_type),
        )
    )

    return T.create_dataframe_from_aggregation_result_schema(result)
