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
    MeteringPointType,
    SettlementMethod,
    TimeSeriesQuality,
)
from package.constants import Colname, ResultKeyName
from package.steps.aggregation.aggregation_result_formatter import (
    create_dataframe_from_aggregation_result_schema,
)
from pyspark.sql import DataFrame
from pyspark.sql.functions import (
    array_contains,
    col,
    collect_set,
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
    return create_dataframe_from_aggregation_result_schema(exchange)


# Function to aggregate net exchange per grid area (step 2)
def aggregate_net_exchange_per_ga(df: DataFrame) -> DataFrame:
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
            lit(MeteringPointType.exchange.value).alias(Colname.metering_point_type),
        )
    )
    return create_dataframe_from_aggregation_result_schema(resultDf)


def aggregate_non_profiled_consumption_ga_brp_es(
    enriched_time_series: DataFrame,
) -> DataFrame:
    return _aggregate_per_ga_and_brp_and_es(
        enriched_time_series,
        MeteringPointType.consumption,
        SettlementMethod.non_profiled,
    )


def aggregate_flex_consumption_ga_brp_es(enriched_time_series: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_brp_and_es(
        enriched_time_series,
        MeteringPointType.consumption,
        SettlementMethod.flex,
    )


def aggregate_production_ga_brp_es(enriched_time_series: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_brp_and_es(
        enriched_time_series, MeteringPointType.production, None
    )


# Function to aggregate sum per grid area and energy supplier (step 12, 13 and 14)
def _aggregate_per_ga_and_brp_and_es(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
    settlement_method: Union[SettlementMethod, None],
) -> DataFrame:
    """This function creates a intermediate result, which is subsequently used as input to achieve result for different process steps.

    The function is responsible for
    - Converting hour data to quarter data.
    - Sum quantities across metering points per grid area, energy supplier, and balance responsible.
    - Assign quality when performing sum.

    Each row in the output dataframe corresponds to a unique combination of: ga, brp, es, and quarter_time

    """

    result = df.filter(
        col(Colname.metering_point_type) == market_evaluation_point_type.value
    )
    if settlement_method is not None:
        result = result.filter(
            col(Colname.settlement_method) == settlement_method.value
        )

    sum_group_by = [
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
    ]
    result = _aggregate_sum_and_set_quality(result, "quarter_quantity", sum_group_by)

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
            lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
            lit(None if settlement_method is None else settlement_method.value).alias(
                Colname.settlement_method
            ),
            Colname.position,
        )
    )

    return create_dataframe_from_aggregation_result_schema(result)


def aggregate_production_ga_es(results: dict) -> DataFrame:
    return _aggregate_per_ga_and_es(
        results[ResultKeyName.production_with_system_correction_and_grid_loss],
        MeteringPointType.production,
    )


def aggregate_non_profiled_consumption_ga_es(
    non_profiled_consumption: DataFrame,
) -> DataFrame:
    return _aggregate_per_ga_and_es(
        non_profiled_consumption,
        MeteringPointType.consumption,
    )


def aggregate_flex_consumption_ga_es(results: dict) -> DataFrame:
    return _aggregate_per_ga_and_es(
        results[ResultKeyName.flex_consumption_with_grid_loss],
        MeteringPointType.consumption,
    )


# Function to aggregate sum per grid area and energy supplier (step 12, 13 and 14)
def _aggregate_per_ga_and_es(
    df: DataFrame, market_evaluation_point_type: MeteringPointType
) -> DataFrame:
    group_by = [Colname.grid_area, Colname.energy_supplier_id, Colname.time_window]
    result = _aggregate_sum_and_set_quality(df, Colname.sum_quantity, group_by)

    result = result.select(
        Colname.grid_area,
        Colname.energy_supplier_id,
        Colname.time_window,
        Colname.quality,
        Colname.sum_quantity,
        lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
    )
    return create_dataframe_from_aggregation_result_schema(result)


def aggregate_production_ga_brp(results: dict) -> DataFrame:
    return _aggregate_per_ga_and_brp(
        results[ResultKeyName.production_with_system_correction_and_grid_loss],
        MeteringPointType.production,
    )


def aggregate_non_profiled_consumption_ga_brp(result: DataFrame) -> DataFrame:
    return _aggregate_per_ga_and_brp(
        result,
        MeteringPointType.consumption,
    )


def aggregate_flex_consumption_ga_brp(results: dict) -> DataFrame:
    return _aggregate_per_ga_and_brp(
        results[ResultKeyName.flex_consumption_with_grid_loss],
        MeteringPointType.consumption,
    )


# Function to aggregate sum per grid area and balance responsible party (step 15, 16 and 17)
def _aggregate_per_ga_and_brp(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
) -> DataFrame:
    group_by = [Colname.grid_area, Colname.balance_responsible_id, Colname.time_window]
    result = _aggregate_sum_and_set_quality(df, Colname.sum_quantity, group_by)

    result = result.select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.time_window,
        Colname.quality,
        Colname.sum_quantity,
        lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
    )
    return create_dataframe_from_aggregation_result_schema(result)


def aggregate_production_ga(production: DataFrame) -> DataFrame:
    return _aggregate_per_ga(
        production,
        MeteringPointType.production,
    )


def aggregate_non_profiled_consumption_ga(consumption: DataFrame) -> DataFrame:
    return _aggregate_per_ga(
        consumption,
        MeteringPointType.consumption,
    )


def aggregate_flex_consumption_ga(results: dict) -> DataFrame:
    return _aggregate_per_ga(
        results[ResultKeyName.flex_consumption_with_grid_loss],
        MeteringPointType.consumption,
    )


# Function to aggregate sum per grid area (step 18, 19 and 20)
def _aggregate_per_ga(
    df: DataFrame,
    market_evaluation_point_type: MeteringPointType,
) -> DataFrame:
    group_by = [Colname.grid_area, Colname.time_window]
    result = _aggregate_sum_and_set_quality(df, Colname.sum_quantity, group_by)

    result = result.withColumnRenamed(
        f"sum({Colname.sum_quantity})", Colname.sum_quantity
    ).select(
        Colname.grid_area,
        Colname.time_window,
        Colname.quality,
        Colname.sum_quantity,
        lit(market_evaluation_point_type.value).alias(Colname.metering_point_type),
    )

    return create_dataframe_from_aggregation_result_schema(result)


def _aggregate_sum_and_set_quality(
    result: DataFrame, quantity_col_name: str, group_by: list[str]
) -> DataFrame:
    result = result.na.fill(value=0, subset=[quantity_col_name])
    result = (
        result.groupBy(group_by).agg(
            sum(quantity_col_name).alias(Colname.sum_quantity),
            collect_set("Quality"),
        )
        # TODO: What about calculated (A06)?
        .withColumn(
            "Quality",
            when(
                array_contains(
                    col("collect_set(Quality)"), lit(TimeSeriesQuality.missing.value)
                )
                | array_contains(
                    col("collect_set(Quality)"), lit(TimeSeriesQuality.incomplete.value)
                ),
                lit(TimeSeriesQuality.incomplete.value),
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

    return result
