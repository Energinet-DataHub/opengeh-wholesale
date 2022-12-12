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

from geh_stream.codelists import Colname, ResultKeyName, ResolutionDuration, MarketEvaluationPointType, Quality
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when, lit
from .aggregate_quality import aggregate_total_consumption_quality
from geh_stream.shared.data_classes import Metadata
from geh_stream.aggregation_utils.aggregation_result_formatter import create_dataframe_from_aggregation_result_schema


production_sum_quantity = "production_sum_quantity"
exchange_sum_quantity = "exchange_sum_quantity"
aggregated_production_quality = "aggregated_production_quality"
aggregated_net_exchange_quality = "aggregated_net_exchange_quality"
hourly_result = "hourly_result"
flex_result = "flex_result"
prod_result = "prod_result"


# Function used to calculate grid loss (step 6)
def calculate_grid_loss(results: dict, metadata: Metadata) -> DataFrame:
    agg_net_exchange = results[ResultKeyName.net_exchange_per_ga]
    agg_hourly_consumption = results[ResultKeyName.hourly_consumption]
    agg_flex_consumption = results[ResultKeyName.flex_consumption]
    agg_production = results[ResultKeyName.hourly_production]
    return __calculate_grid_loss_or_residual_ga(agg_net_exchange, agg_hourly_consumption, agg_flex_consumption, agg_production, metadata)


def calculate_residual_ga(results: dict, metadata: Metadata) -> DataFrame:
    agg_net_exchange = results[ResultKeyName.net_exchange_per_ga]
    agg_hourly_consumption = results[ResultKeyName.hourly_settled_consumption_ga]
    agg_flex_consumption = results[ResultKeyName.flex_settled_consumption_ga]
    agg_production = results[ResultKeyName.hourly_production_ga]
    return __calculate_grid_loss_or_residual_ga(agg_net_exchange, agg_hourly_consumption, agg_flex_consumption, agg_production, metadata)


def __calculate_grid_loss_or_residual_ga(agg_net_exchange: DataFrame, agg_hourly_consumption: DataFrame, agg_flex_consumption: DataFrame, agg_production: DataFrame, metadata: Metadata) -> DataFrame:
    agg_net_exchange_result = agg_net_exchange.selectExpr(Colname.grid_area, f"{Colname.sum_quantity} as net_exchange_result", Colname.time_window)
    agg_hourly_consumption_result = agg_hourly_consumption \
        .selectExpr(Colname.grid_area, f"{Colname.sum_quantity} as {hourly_result}", Colname.time_window) \
        .groupBy(Colname.grid_area, Colname.time_window) \
        .sum(hourly_result) \
        .withColumnRenamed(f"sum({hourly_result})", hourly_result)
    agg_flex_consumption_result = agg_flex_consumption \
        .selectExpr(Colname.grid_area, f"{Colname.sum_quantity} as {flex_result}", Colname.time_window) \
        .groupBy(Colname.grid_area, Colname.time_window) \
        .sum(flex_result) \
        .withColumnRenamed(f"sum({flex_result})", flex_result)
    agg_production_result = agg_production \
        .selectExpr(Colname.grid_area, f"{Colname.sum_quantity} as {prod_result}", Colname.time_window) \
        .groupBy(Colname.grid_area, Colname.time_window) \
        .sum(prod_result) \
        .withColumnRenamed(f"sum({prod_result})", prod_result)

    result = agg_net_exchange_result \
        .join(agg_production_result, [Colname.grid_area, Colname.time_window], "left") \
        .join(agg_flex_consumption_result.join(agg_hourly_consumption_result, [Colname.grid_area, Colname.time_window], "left"), [Colname.grid_area, Colname.time_window], "left") \
        .orderBy(Colname.grid_area, Colname.time_window)

    result = result\
        .withColumn(Colname.sum_quantity, result.net_exchange_result + result.prod_result - (result.hourly_result + result.flex_result))
    # Quality is always calculated for grid loss entries
    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        Colname.sum_quantity,  # grid loss
        lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
        lit(MarketEvaluationPointType.consumption.value).alias(Colname.metering_point_type),
        lit(Quality.calculated.value).alias(Colname.quality))
    return create_dataframe_from_aggregation_result_schema(metadata, result)


# Function to calculate system correction to be added (step 8)
def calculate_added_system_correction(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.grid_loss]
    result = df.withColumn(Colname.added_system_correction, when(col(Colname.sum_quantity) < 0, (col(Colname.sum_quantity)) * (-1)).otherwise(0))
    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        Colname.added_system_correction,
        Colname.sum_quantity,
        lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
        lit(MarketEvaluationPointType.production.value).alias(Colname.metering_point_type),
        Colname.quality)
    return create_dataframe_from_aggregation_result_schema(metadata, result)


# Function to calculate grid loss to be added (step 9)
def calculate_added_grid_loss(results: dict, metadata: Metadata):
    df = results[ResultKeyName.grid_loss]
    result = df.withColumn(Colname.added_grid_loss, when(col(Colname.sum_quantity) > 0, col(Colname.sum_quantity)).otherwise(0))
    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        Colname.added_grid_loss,
        Colname.sum_quantity,
        lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
        lit(MarketEvaluationPointType.consumption.value).alias(Colname.metering_point_type),
        Colname.quality)
    return create_dataframe_from_aggregation_result_schema(metadata, result)


# Function to calculate total consumption (step 21)
def calculate_total_consumption(results: dict, metadata: Metadata) -> DataFrame:
    agg_net_exchange = results[ResultKeyName.net_exchange_per_ga]
    agg_production = results[ResultKeyName.hourly_production_ga]
    result_production = agg_production.selectExpr(Colname.grid_area, Colname.time_window, Colname.sum_quantity, Colname.quality) \
        .groupBy(Colname.grid_area, Colname.time_window, Colname.quality).sum(Colname.sum_quantity) \
        .withColumnRenamed(f"sum({Colname.sum_quantity})", production_sum_quantity) \
        .withColumnRenamed(Colname.quality, aggregated_production_quality)

    result_net_exchange = agg_net_exchange.selectExpr(Colname.grid_area, Colname.time_window, Colname.sum_quantity, Colname.quality) \
        .groupBy(Colname.grid_area, Colname.time_window, Colname.quality).sum(Colname.sum_quantity) \
        .withColumnRenamed(f"sum({Colname.sum_quantity})", exchange_sum_quantity) \
        .withColumnRenamed(Colname.quality, aggregated_net_exchange_quality)

    result = result_production.join(result_net_exchange, [Colname.grid_area, Colname.time_window], "inner") \
        .withColumn(Colname.sum_quantity, col(production_sum_quantity) + col(exchange_sum_quantity))

    result = aggregate_total_consumption_quality(result).orderBy(Colname.grid_area, Colname.time_window).select(
            Colname.grid_area,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
            lit(MarketEvaluationPointType.consumption.value).alias(Colname.metering_point_type))

    return create_dataframe_from_aggregation_result_schema(metadata, result)
