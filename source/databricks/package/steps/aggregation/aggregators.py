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
from decimal import Decimal

from package.codelists import (
    MeteringPointResolution,
    MeteringPointType,
    SettlementMethod,
    TimeSeriesQuality,
)
from package.constants import Colname, ResultKeyName
from package.shared.data_classes import Metadata
from package.steps.aggregation.aggregation_result_formatter import (
    create_dataframe_from_aggregation_result_schema,
)
from pyspark.sql import DataFrame
from pyspark.sql.functions import (
    array,
    array_contains,
    col,
    collect_set,
    explode,
    expr,
    lit,
    row_number,
    sum,
    when,
    window,
)
from pyspark.sql.window import Window
from typing import Union

in_sum = "in_sum"
out_sum = "out_sum"
exchange_in_in_grid_area = "ExIn_InMeteringGridArea_Domain_mRID"
exchange_in_out_grid_area = "ExIn_OutMeteringGridArea_Domain_mRID"
exchange_out_in_grid_area = "ExOut_InMeteringGridArea_Domain_mRID"
exchange_out_out_grid_area = "ExOut_OutMeteringGridArea_Domain_mRID"


# Function to aggregate hourly net exchange per neighbouring grid areas (step 1)
def aggregate_net_exchange_per_neighbour_ga(
    results: dict, metadata: Metadata
) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe].filter(
        col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )
    exchange_in = (
        df.groupBy(
            Colname.in_grid_area,
            Colname.out_grid_area,
            window(col(Colname.observation_time), "1 hour"),
            Colname.aggregated_quality,
        )
        .sum(Colname.quantity)
        .withColumnRenamed(f"sum({Colname.quantity})", in_sum)
        .withColumnRenamed("window", Colname.time_window)
        .withColumnRenamed(Colname.in_grid_area, exchange_in_in_grid_area)
        .withColumnRenamed(Colname.out_grid_area, exchange_in_out_grid_area)
    )
    exchange_out = (
        df.groupBy(
            Colname.in_grid_area,
            Colname.out_grid_area,
            window(col(Colname.observation_time), "1 hour"),
        )
        .sum(Colname.quantity)
        .withColumnRenamed(f"sum({Colname.quantity})", out_sum)
        .withColumnRenamed("window", Colname.time_window)
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
            lit(MeteringPointResolution.hour.value).alias(
                Colname.resolution
            ),  # TODO take resolution from metadata
            lit(MeteringPointType.exchange.value).alias(Colname.metering_point_type),
        )
    )
    return create_dataframe_from_aggregation_result_schema(metadata, exchange)


# Function to aggregate hourly net exchange per grid area (step 2)
def aggregate_net_exchange_per_ga(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe]
    exchangeIn = df.filter(
        col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )
    exchangeIn = (
        exchangeIn.groupBy(
            Colname.in_grid_area,
            window(col(Colname.observation_time), "1 hour"),
            Colname.aggregated_quality,
        )
        .sum(Colname.quantity)
        .withColumnRenamed(f"sum({Colname.quantity})", in_sum)
        .withColumnRenamed("window", Colname.time_window)
        .withColumnRenamed(Colname.in_grid_area, Colname.grid_area)
    )
    exchangeOut = df.filter(
        col(Colname.metering_point_type) == MeteringPointType.exchange.value
    )
    exchangeOut = (
        exchangeOut.groupBy(
            Colname.out_grid_area, window(col(Colname.observation_time), "1 hour")
        )
        .sum(Colname.quantity)
        .withColumnRenamed(f"sum({Colname.quantity})", out_sum)
        .withColumnRenamed("window", Colname.time_window)
        .withColumnRenamed(Colname.out_grid_area, Colname.grid_area)
    )
    joined = exchangeIn.join(
        exchangeOut,
        (exchangeIn[Colname.grid_area] == exchangeOut[Colname.grid_area])
        & (exchangeIn[Colname.time_window] == exchangeOut[Colname.time_window]),
        how="outer",
    ).select(exchangeIn["*"], exchangeOut[out_sum])
    resultDf = (
        joined.withColumn(Colname.sum_quantity, joined[in_sum] - joined[out_sum])
        .withColumnRenamed(Colname.aggregated_quality, Colname.quality)
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            Colname.quality,
            lit(MeteringPointResolution.hour.value).alias(
                Colname.resolution
            ),  # TODO take resolution from metadata
            lit(MeteringPointType.exchange.value).alias(Colname.metering_point_type),
        )
    )
    return create_dataframe_from_aggregation_result_schema(metadata, resultDf)


# Function to aggregate non-profiled consumption per grid area, balance responsible party and energy supplier (step 3)
def aggregate_non_profiled_consumption(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe]
    return aggregate_per_ga_and_brp_and_es(
        df,
        MeteringPointType.consumption,
        SettlementMethod.non_profiled,
        metadata,
    )


# Function to aggregate flex consumption per grid area, balance responsible party and energy supplier (step 4)
def aggregate_flex_consumption(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe]
    return aggregate_per_ga_and_brp_and_es(
        df,
        MeteringPointType.consumption,
        SettlementMethod.flex,
        metadata,
    )


# Function to aggregate hourly production per grid area, balance responsible party and energy supplier (step 5)
def aggregate_production(results: dict, metadata: Metadata) -> DataFrame:
    df = results[ResultKeyName.aggregation_base_dataframe]
    return aggregate_per_ga_and_brp_and_es(
        df, MeteringPointType.production, None, metadata
    )


# Function to aggregate sum per grid area, balance responsible party and energy supplier (step 3, 4 and 5)
def aggregate_per_ga_and_brp_and_es(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
    settlement_method: Union[SettlementMethod, None],
    metadata: Metadata,
) -> DataFrame:
    result = df.filter(
        col(Colname.metering_point_type) == market_evaluation_point_type.value
    )
    if settlement_method is not None:
        result = result.filter(
            col(Colname.settlement_method) == settlement_method.value
        )
    result = result.withColumn(
        "quarter_times",
        when(
            col(Colname.resolution) == MeteringPointResolution.hour.value,
            array(
                col(Colname.observation_time),
                col(Colname.observation_time) + expr("INTERVAL 15 minutes"),
                col(Colname.observation_time) + expr("INTERVAL 30 minutes"),
                col(Colname.observation_time) + expr("INTERVAL 45 minutes"),
            ),
        ).when(
            col(Colname.resolution) == MeteringPointResolution.quarter.value,
            array(col(Colname.observation_time)),
        ),
    ).select(
        result["*"],
        explode("quarter_times").alias("quarter_time"),
    )
    result = result.withColumn(
        Colname.time_window, window(col("quarter_time"), "15 minutes")
    )
    result = (
        result.withColumn(
            "quarter_quantity",
            when(
                col(Colname.resolution) == MeteringPointResolution.hour.value,
                col(Colname.quantity) / 4,
            ).when(
                col(Colname.resolution) == MeteringPointResolution.quarter.value,
                col(Colname.quantity),
            ),
        )
        .groupBy(
            Colname.grid_area,
            Colname.balance_responsible_id,
            Colname.energy_supplier_id,
            Colname.time_window,
        )
        .agg(
            # TODO: Doesn't this sum become null if just one quantity is already null? Should we replace null with 0 before this operation?
            sum("quarter_quantity").alias(Colname.sum_quantity),
            collect_set("Quality"),
        )
        # TODO: What about calculated (A06)?
        .withColumn(
            "Quality",
            when(
                array_contains(
                    col("collect_set(Quality)"), lit(TimeSeriesQuality.missing.value)
                ),
                lit(TimeSeriesQuality.missing.value),
            )
            .when(
                array_contains(
                    col("collect_set(Quality)"),
                    lit(TimeSeriesQuality.estimated.value),
                ),
                lit(TimeSeriesQuality.estimated.value),
            )
            .when(
                array_contains(
                    col("collect_set(Quality)"),
                    lit(TimeSeriesQuality.measured.value),
                ),
                lit(TimeSeriesQuality.measured.value),
            ),
        )
    )

    win = Window.partitionBy("GridAreaCode").orderBy(col(Colname.time_window))

    result = (
        result.withColumn("position", row_number().over(win))
        .withColumn(
            Colname.sum_quantity,
            when(col(Colname.sum_quantity).isNull(), Decimal("0.000")).otherwise(
                col(Colname.sum_quantity)
            ),
        )
        .withColumn(
            Colname.quality,
            when(
                col(Colname.quality).isNull(), TimeSeriesQuality.missing.value
            ).otherwise(col(Colname.quality)),
        )
        .select(
            Colname.grid_area,
            Colname.balance_responsible_id,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(MeteringPointResolution.quarter.value).alias(
                Colname.resolution
            ),  # TODO take resolution from metadata
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
            lit(None if settlement_method is None else settlement_method.value).alias(
                Colname.settlement_method
            ),
            Colname.position,
        )
    )

    return create_dataframe_from_aggregation_result_schema(metadata, result)


def aggregate_production_ga_es(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_es(
        results[ResultKeyName.production_with_system_correction_and_grid_loss],
        MeteringPointType.production,
        metadata,
    )


def aggregate_non_profiled_consumption_ga_es(
    results: dict, metadata: Metadata
) -> DataFrame:
    return __aggregate_per_ga_and_es(
        results[ResultKeyName.non_profiled_consumption],
        MeteringPointType.consumption,
        metadata,
    )


def aggregate_flex_consumption_ga_es(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_es(
        results[ResultKeyName.flex_consumption_with_grid_loss],
        MeteringPointType.consumption,
        metadata,
    )


# Function to aggregate sum per grid area and energy supplier (step 12, 13 and 14)
def __aggregate_per_ga_and_es(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
    metadata: Metadata,
) -> DataFrame:
    result = (
        df.groupBy(
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.quality,
        )
        .sum(Colname.sum_quantity)
        .withColumnRenamed(f"sum({Colname.sum_quantity})", Colname.sum_quantity)
        .select(
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(MeteringPointResolution.hour.value).alias(
                Colname.resolution
            ),  # TODO take resolution from metadata
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
        )
    )
    return create_dataframe_from_aggregation_result_schema(metadata, result)


def aggregate_production_ga_brp(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_brp(
        results[ResultKeyName.production_with_system_correction_and_grid_loss],
        MeteringPointType.production,
        metadata,
    )


def aggregate_non_profiled_consumption_ga_brp(
    results: dict, metadata: Metadata
) -> DataFrame:
    return __aggregate_per_ga_and_brp(
        results[ResultKeyName.non_profiled_consumption],
        MeteringPointType.consumption,
        metadata,
    )


def aggregate_flex_consumption_ga_brp(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga_and_brp(
        results[ResultKeyName.flex_consumption_with_grid_loss],
        MeteringPointType.consumption,
        metadata,
    )


# Function to aggregate sum per grid area and balance responsible party (step 15, 16 and 17)
def __aggregate_per_ga_and_brp(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
    metadata: Metadata,
) -> DataFrame:
    result = (
        df.groupBy(
            Colname.grid_area,
            Colname.balance_responsible_id,
            Colname.time_window,
            Colname.quality,
        )
        .sum(Colname.sum_quantity)
        .withColumnRenamed(f"sum({Colname.sum_quantity})", Colname.sum_quantity)
        .select(
            Colname.grid_area,
            Colname.balance_responsible_id,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(MeteringPointResolution.hour.value).alias(
                Colname.resolution
            ),  # TODO take resolution from metadata
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
        )
    )
    return create_dataframe_from_aggregation_result_schema(metadata, result)


def aggregate_production_ga(production: DataFrame, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga(
        production,
        MeteringPointType.production,
        metadata,
    )


def aggregate_non_profiled_consumption_ga(
    results: dict, metadata: Metadata
) -> DataFrame:
    return __aggregate_per_ga(
        results[ResultKeyName.non_profiled_consumption],
        MeteringPointType.consumption,
        metadata,
    )


def aggregate_flex_consumption_ga(results: dict, metadata: Metadata) -> DataFrame:
    return __aggregate_per_ga(
        results[ResultKeyName.flex_consumption_with_grid_loss],
        MeteringPointType.consumption,
        metadata,
    )


# Function to aggregate sum per grid area (step 18, 19 and 20)
def __aggregate_per_ga(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
    metadata: Metadata,
) -> DataFrame:
    result = (
        df.groupBy(Colname.grid_area, Colname.time_window, Colname.quality)
        .sum(Colname.sum_quantity)
        .withColumnRenamed(f"sum({Colname.sum_quantity})", Colname.sum_quantity)
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.quality,
            Colname.sum_quantity,
            lit(MeteringPointResolution.hour.value).alias(
                Colname.resolution
            ),  # TODO take resolution from metadata
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
        )
    )
    return create_dataframe_from_aggregation_result_schema(metadata, result)
