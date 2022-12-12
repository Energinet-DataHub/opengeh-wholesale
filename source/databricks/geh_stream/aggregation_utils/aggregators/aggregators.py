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
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col, window, lit
from geh_stream.codelists import MarketEvaluationPointType, SettlementMethod, ConnectionState, Colname, ResultKeyName, ResolutionDuration
from geh_stream.shared.data_classes import Metadata
from geh_stream.aggregation_utils.aggregation_result_formatter import create_dataframe_from_aggregation_result_schema


in_sum = "in_sum"
out_sum = "out_sum"
exchange_in_in_grid_area = "ExIn_InMeteringGridArea_Domain_mRID"
exchange_in_out_grid_area = "ExIn_OutMeteringGridArea_Domain_mRID"
exchange_out_in_grid_area = "ExOut_InMeteringGridArea_Domain_mRID"
exchange_out_out_grid_area = "ExOut_OutMeteringGridArea_Domain_mRID"


# Function to aggregate hourly net exchange per neighbouring grid areas (step 1)
def aggregate_net_exchange_per_neighbour_ga(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe].filter(col(Colname.metering_point_type) == MarketEvaluationPointType.exchange.value) \
        .filter((col(Colname.connection_state) == ConnectionState.connected.value) | (col(Colname.connection_state) == ConnectionState.disconnected.value)) \

    exchange_in = df \
        .groupBy(
            Colname.in_grid_area,
            Colname.out_grid_area,
            window(col(Colname.time), "1 hour"),
            Colname.aggregated_quality) \
        .sum(Colname.quantity) \
        .withColumnRenamed(f"sum({Colname.quantity})", in_sum) \
        .withColumnRenamed("window", Colname.time_window) \
        .withColumnRenamed(Colname.in_grid_area, exchange_in_in_grid_area) \
        .withColumnRenamed(Colname.out_grid_area, exchange_in_out_grid_area)
    exchange_out = df \
        .groupBy(
            Colname.in_grid_area,
            Colname.out_grid_area,
            window(col(Colname.time), "1 hour")) \
        .sum(Colname.quantity) \
        .withColumnRenamed(f"sum({Colname.quantity})", out_sum) \
        .withColumnRenamed("window", Colname.time_window) \
        .withColumnRenamed(Colname.in_grid_area, exchange_out_in_grid_area) \
        .withColumnRenamed(Colname.out_grid_area, exchange_out_out_grid_area)

    exchange = exchange_in.join(
        exchange_out, [Colname.time_window], "inner") \
        .filter(exchange_in.ExIn_InMeteringGridArea_Domain_mRID == exchange_out.ExOut_OutMeteringGridArea_Domain_mRID) \
        .filter(exchange_in.ExIn_OutMeteringGridArea_Domain_mRID == exchange_out.ExOut_InMeteringGridArea_Domain_mRID) \
        .select(exchange_in["*"], exchange_out[out_sum]) \
        .withColumn(
            Colname.sum_quantity,
            col(in_sum) - col(out_sum)) \
        .withColumnRenamed(exchange_in_in_grid_area, Colname.in_grid_area) \
        .withColumnRenamed(exchange_in_out_grid_area, Colname.out_grid_area) \
        .withColumnRenamed(Colname.aggregated_quality, Colname.quality) \
        .select(
            Colname.in_grid_area,
            Colname.out_grid_area,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            col(Colname.in_grid_area).alias(Colname.grid_area),
            lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
            lit(MarketEvaluationPointType.exchange.value).alias(Colname.metering_point_type))
    return create_dataframe_from_aggregation_result_schema(metadata, exchange)


# Function to aggregate hourly net exchange per grid area (step 2)
def aggregate_net_exchange_per_ga(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe]
    exchangeIn = df \
        .filter(col(Colname.metering_point_type) == MarketEvaluationPointType.exchange.value) \
        .filter((col(Colname.connection_state) == ConnectionState.connected.value) | (col(Colname.connection_state) == ConnectionState.disconnected.value))
    exchangeIn = exchangeIn \
        .groupBy(Colname.in_grid_area, window(col(Colname.time), "1 hour"), Colname.aggregated_quality) \
        .sum(Colname.quantity) \
        .withColumnRenamed(f"sum({Colname.quantity})", in_sum) \
        .withColumnRenamed("window", Colname.time_window) \
        .withColumnRenamed(Colname.in_grid_area, Colname.grid_area)
    exchangeOut = df \
        .filter(col(Colname.metering_point_type) == MarketEvaluationPointType.exchange.value) \
        .filter((col(Colname.connection_state) == ConnectionState.connected.value) | (col(Colname.connection_state) == ConnectionState.disconnected.value))
    exchangeOut = exchangeOut \
        .groupBy(Colname.out_grid_area, window(col(Colname.time), "1 hour")) \
        .sum(Colname.quantity) \
        .withColumnRenamed(f"sum({Colname.quantity})", out_sum) \
        .withColumnRenamed("window", Colname.time_window) \
        .withColumnRenamed(Colname.out_grid_area, Colname.grid_area)
    joined = exchangeIn \
        .join(exchangeOut,
              (exchangeIn[Colname.grid_area] == exchangeOut[Colname.grid_area]) & (exchangeIn[Colname.time_window] == exchangeOut[Colname.time_window]),
              how="outer") \
        .select(exchangeIn["*"], exchangeOut[out_sum])
    resultDf = joined.withColumn(
        Colname.sum_quantity, joined[in_sum] - joined[out_sum]) \
        .withColumnRenamed(Colname.aggregated_quality, Colname.quality) \
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.quality,
            lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
            lit(MarketEvaluationPointType.exchange.value).alias(Colname.metering_point_type))
    return create_dataframe_from_aggregation_result_schema(metadata, resultDf)


# Function to aggregate hourly consumption per grid area, balance responsible party and energy supplier (step 3)
def aggregate_hourly_consumption(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe]
    return aggregate_per_ga_and_brp_and_es(df, MarketEvaluationPointType.consumption, SettlementMethod.non_profiled, metadata)


# Function to aggregate flex consumption per grid area, balance responsible party and energy supplier (step 4)
def aggregate_flex_consumption(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe]
    return aggregate_per_ga_and_brp_and_es(df, MarketEvaluationPointType.consumption, SettlementMethod.flex_settled, metadata)


# Function to aggregate hourly production per grid area, balance responsible party and energy supplier (step 5)
def aggregate_hourly_production(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe]
    return aggregate_per_ga_and_brp_and_es(df, MarketEvaluationPointType.production, None, metadata)


# Function to aggregate sum per grid area, balance responsible party and energy supplier (step 3, 4 and 5)
def aggregate_per_ga_and_brp_and_es(df: DataFrame, market_evaluation_point_type: MarketEvaluationPointType, settlement_method: SettlementMethod, metadata: Metadata):
    result = df.filter(col(Colname.metering_point_type) == market_evaluation_point_type.value)
    if settlement_method is not None:
        result = result.filter(col(Colname.settlement_method) == settlement_method.value)
    result = result.filter((col(Colname.connection_state) == ConnectionState.connected.value) | (col(Colname.connection_state) == ConnectionState.disconnected.value))
    result = result \
        .groupBy(Colname.grid_area, Colname.balance_responsible_id, Colname.energy_supplier_id, window(col(Colname.time), "1 hour"), Colname.aggregated_quality) \
        .sum(Colname.quantity) \
        .withColumnRenamed(f"sum({Colname.quantity})", Colname.sum_quantity) \
        .withColumnRenamed("window", Colname.time_window) \
        .withColumnRenamed(Colname.aggregated_quality, Colname.quality) \
        .select(
            Colname.grid_area,
            Colname.balance_responsible_id,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
            lit(None if settlement_method is None else settlement_method.value).alias(Colname.settlement_method))
    return create_dataframe_from_aggregation_result_schema(metadata, result)


def aggregate_hourly_production_ga_es(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_es(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss], MarketEvaluationPointType.production, metadata)


def aggregate_hourly_settled_consumption_ga_es(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_es(results[ResultKeyName.hourly_consumption], MarketEvaluationPointType.consumption, metadata)


def aggregate_flex_settled_consumption_ga_es(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_es(results[ResultKeyName.flex_consumption_with_grid_loss], MarketEvaluationPointType.consumption, metadata)


# Function to aggregate sum per grid area and energy supplier (step 12, 13 and 14)
def __aggregate_per_ga_and_es(df: DataFrame, market_evaluation_point_type: MarketEvaluationPointType, metadata: Metadata) -> DataFrame:
    result = df \
        .groupBy(Colname.grid_area, Colname.energy_supplier_id, Colname.time_window, Colname.quality) \
        .sum(Colname.sum_quantity) \
        .withColumnRenamed(f'sum({Colname.sum_quantity})', Colname.sum_quantity) \
        .select(
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type))
    return create_dataframe_from_aggregation_result_schema(metadata, result)


def aggregate_hourly_production_ga_brp(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_brp(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss], MarketEvaluationPointType.production, metadata)


def aggregate_hourly_settled_consumption_ga_brp(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_brp(results[ResultKeyName.hourly_consumption], MarketEvaluationPointType.consumption, metadata)


def aggregate_flex_settled_consumption_ga_brp(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_brp(results[ResultKeyName.flex_consumption_with_grid_loss], MarketEvaluationPointType.consumption, metadata)


# Function to aggregate sum per grid area and balance responsible party (step 15, 16 and 17)
def __aggregate_per_ga_and_brp(df: DataFrame, market_evaluation_point_type: MarketEvaluationPointType, metadata: Metadata) -> DataFrame:
    result = df \
        .groupBy(Colname.grid_area, Colname.balance_responsible_id, Colname.time_window, Colname.quality) \
        .sum(Colname.sum_quantity) \
        .withColumnRenamed(f'sum({Colname.sum_quantity})', Colname.sum_quantity) \
        .select(
            Colname.grid_area,
            Colname.balance_responsible_id,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type))
    return create_dataframe_from_aggregation_result_schema(metadata, result)


def aggregate_hourly_production_ga(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga(results[ResultKeyName.hourly_production_with_system_correction_and_grid_loss], MarketEvaluationPointType.production, metadata)


def aggregate_hourly_settled_consumption_ga(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga(results[ResultKeyName.hourly_consumption], MarketEvaluationPointType.consumption, metadata)


def aggregate_flex_settled_consumption_ga(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga(results[ResultKeyName.flex_consumption_with_grid_loss], MarketEvaluationPointType.consumption, metadata)


# Function to aggregate sum per grid area (step 18, 19 and 20)
def __aggregate_per_ga(df: DataFrame, market_evaluation_point_type: MarketEvaluationPointType, metadata: Metadata) -> DataFrame:
    result = df \
        .groupBy(Colname.grid_area, Colname.time_window, Colname.quality) \
        .sum(Colname.sum_quantity) \
        .withColumnRenamed(f'sum({Colname.sum_quantity})', Colname.sum_quantity) \
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(ResolutionDuration.hour).alias(Colname.resolution),  # TODO take resolution from metadata
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type))
    return create_dataframe_from_aggregation_result_schema(metadata, result)
