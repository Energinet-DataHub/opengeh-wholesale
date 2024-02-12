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
import pyspark.sql.functions as f
from pyspark.sql import DataFrame

import package.calculation.energy.aggregators.transformations as t
from package.calculation.energy.energy_results import EnergyResults
from package.calculation.preparation.grid_loss_responsible import GridLossResponsible
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.constants import Colname

production_sum_quantity = "production_sum_quantity"
exchange_sum_quantity = "exchange_sum_quantity"
aggregated_production_qualities = "aggregated_production_quality"
aggregated_net_exchange_qualities = "aggregated_net_exchange_quality"
hourly_result = "hourly_result"
flex_result = "flex_result"
prod_result = "prod_result"
net_exchange_result = "net_exchange_result"


def calculate_grid_loss(
    net_exchange_per_ga: EnergyResults,
    non_profiled_consumption: EnergyResults,
    flex_consumption: EnergyResults,
    production: EnergyResults,
) -> EnergyResults:
    agg_non_profiled_consumption_result = t.aggregate_sum_quantity_and_qualities(
        non_profiled_consumption.df,
        [Colname.grid_area, Colname.time_window],
    ).withColumnRenamed(Colname.sum_quantity, hourly_result)

    agg_flex_consumption_result = t.aggregate_sum_quantity_and_qualities(
        flex_consumption.df,
        [Colname.grid_area, Colname.time_window],
    ).withColumnRenamed(Colname.sum_quantity, flex_result)

    agg_production_result = t.aggregate_sum_quantity_and_qualities(
        production.df,
        [Colname.grid_area, Colname.time_window],
    ).withColumnRenamed(Colname.sum_quantity, prod_result)

    result = (
        net_exchange_per_ga.df.withColumnRenamed(
            Colname.sum_quantity, net_exchange_result
        )
        .join(agg_production_result, [Colname.grid_area, Colname.time_window], "left")
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
        result[net_exchange_result]
        + result[prod_result]
        - (result[hourly_result] + result[flex_result]),
    )

    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        Colname.sum_quantity,  # grid loss
        f.lit(MeteringPointType.CONSUMPTION.value).alias(Colname.metering_point_type),
        # Quality of positive and negative grid loss must always be "calculated" as they become time series
        # that'll be sent to the metering points
        f.array(f.lit(QuantityQuality.CALCULATED.value)).alias(Colname.qualities),
    )

    return EnergyResults(result)


def _get_grid_loss_metering_point_ids_for_grid_areas_with_specific_metering_point_type(
    grid_loss_responsible: GridLossResponsible, metering_point_type: MeteringPointType
) -> DataFrame:
    return (
        grid_loss_responsible.df.select(Colname.grid_area, Colname.metering_point_id)
        .distinct()
        .where(
            grid_loss_responsible.df[Colname.metering_point_type]
            == metering_point_type.value
        )
    )


def calculate_negative_grid_loss(
    grid_loss: EnergyResults, grid_loss_responsible: GridLossResponsible
) -> EnergyResults:
    only_grid_area_and_metering_point_id = _get_grid_loss_metering_point_ids_for_grid_areas_with_specific_metering_point_type(
        grid_loss_responsible, MeteringPointType.PRODUCTION
    )

    only_grid_area_and_metering_point_id = only_grid_area_and_metering_point_id.withColumnRenamed(Colname.metering_point_id, "grid_loss_metering_point_id")

    result = grid_loss.df.join(
        only_grid_area_and_metering_point_id, Colname.grid_area, "left"
    ).select(
        Colname.grid_area,
        Colname.time_window,
        f.when(f.col(Colname.sum_quantity) < 0, -f.col(Colname.sum_quantity))
        .otherwise(0)
        .alias(Colname.sum_quantity),
        f.lit(MeteringPointType.PRODUCTION.value).alias(Colname.metering_point_type),
        Colname.qualities,
        only_grid_area_and_metering_point_id["grid_loss_metering_point_id"],
    )

    result = result.withColumnRenamed("grid_loss_metering_point_id", Colname.metering_point_id)

    return EnergyResults(result)


def calculate_positive_grid_loss(
    grid_loss: EnergyResults, grid_loss_responsible: GridLossResponsible
) -> EnergyResults:
    only_grid_area_and_metering_point_id = _get_grid_loss_metering_point_ids_for_grid_areas_with_specific_metering_point_type(
        grid_loss_responsible, MeteringPointType.CONSUMPTION
    )

    only_grid_area_and_metering_point_id = only_grid_area_and_metering_point_id.withColumnRenamed(Colname.metering_point_id, "grid_loss_metering_point_id")

    result = grid_loss.df.join(
        only_grid_area_and_metering_point_id, Colname.grid_area, "left"
    ).select(
        Colname.grid_area,
        Colname.time_window,
        f.when(f.col(Colname.sum_quantity) > 0, f.col(Colname.sum_quantity))
        .otherwise(0)
        .alias(Colname.sum_quantity),
        f.lit(MeteringPointType.CONSUMPTION.value).alias(Colname.metering_point_type),
        Colname.qualities,
        only_grid_area_and_metering_point_id["grid_loss_metering_point_id"],
    )

    result = result.withColumnRenamed("grid_loss_metering_point_id", Colname.metering_point_id)

    return EnergyResults(result)


def calculate_total_consumption(
    net_exchange_per_ga: EnergyResults, production_per_ga: EnergyResults
) -> EnergyResults:
    result_production = (
        t.aggregate_sum_quantity_and_qualities(
            production_per_ga.df,
            [Colname.grid_area, Colname.time_window],
        )
        .withColumnRenamed(Colname.sum_quantity, production_sum_quantity)
        .withColumnRenamed(Colname.qualities, aggregated_production_qualities)
    )

    result_net_exchange = (
        t.aggregate_sum_quantity_and_qualities(
            net_exchange_per_ga.df,
            [Colname.grid_area, Colname.time_window],
        )
        .withColumnRenamed(Colname.sum_quantity, exchange_sum_quantity)
        .withColumnRenamed(Colname.qualities, aggregated_net_exchange_qualities)
    )

    result = (
        result_production.join(
            result_net_exchange, [Colname.grid_area, Colname.time_window], "inner"
        )
        .withColumn(
            Colname.sum_quantity,
            f.col(production_sum_quantity) + f.col(exchange_sum_quantity),
        )
        .withColumn(
            Colname.qualities,
            f.array_union(
                aggregated_production_qualities, aggregated_net_exchange_qualities
            ),
        )
    )

    result = result.select(
        Colname.grid_area,
        Colname.time_window,
        Colname.qualities,
        Colname.sum_quantity,
        f.lit(MeteringPointType.CONSUMPTION.value).alias(Colname.metering_point_type),
    )

    return EnergyResults(result)


def apply_grid_loss_adjustment(
    results: EnergyResults,
    grid_loss_result: EnergyResults,
    grid_loss_responsible: GridLossResponsible,
    metering_point_type: MeteringPointType,
) -> EnergyResults:
    grid_loss_responsible_grid_area = "GridLossResponsible_GridArea"
    adjusted_sum_quantity = "adjusted_sum_quantity"

    result_df = results.df
    grid_loss_result_df = grid_loss_result.df
    # grid_loss_result_df's energy supplier is always null
    grid_loss_result_df = grid_loss_result_df.drop(Colname.energy_supplier_id)

    grid_loss_responsible_df = grid_loss_responsible.df.where(
        f.col(Colname.metering_point_type) == metering_point_type.value
    ).select(
        Colname.from_date,
        Colname.to_date,
        Colname.energy_supplier_id,
        f.col(Colname.grid_area).alias(grid_loss_responsible_grid_area),
        Colname.metering_point_type,
    )

    joined_grid_loss_result_and_responsible = grid_loss_result_df.join(
        grid_loss_responsible_df,
        f.when(
            f.col(Colname.to_date).isNotNull(),
            f.col(Colname.time_window_start) <= f.col(Colname.to_date),
        ).otherwise(True)
        & (f.col(Colname.time_window_start) >= f.col(Colname.from_date))
        & (
            f.col(Colname.to_date).isNull()
            | (f.col(Colname.time_window_end) <= f.col(Colname.to_date))
        )
        & (f.col(Colname.grid_area) == f.col(grid_loss_responsible_grid_area)),
        "left",
    ).select(
        Colname.grid_area,
        Colname.energy_supplier_id,
        Colname.time_window,
        Colname.sum_quantity,
        Colname.qualities,
    )

    df = result_df.join(
        joined_grid_loss_result_and_responsible,
        [Colname.time_window, Colname.grid_area, Colname.energy_supplier_id],
        "outer",
    ).select(
        Colname.grid_area,
        result_df[Colname.balance_responsible_id],
        Colname.energy_supplier_id,
        Colname.time_window,
        result_df[Colname.sum_quantity],
        f.when(
            result_df[Colname.qualities].isNull(),
            joined_grid_loss_result_and_responsible[Colname.qualities],
        )
        .when(
            joined_grid_loss_result_and_responsible[Colname.qualities].isNull(),
            result_df[Colname.qualities],
        )
        .otherwise(
            f.array_union(
                result_df[Colname.qualities], grid_loss_result_df[Colname.qualities]
            )
        )
        .alias(Colname.qualities),
        joined_grid_loss_result_and_responsible[Colname.sum_quantity].alias(
            "grid_loss_sum_quantity"
        ),
    )
    df = df.na.fill(0, subset=["grid_loss_sum_quantity", Colname.sum_quantity])

    result_df = df.withColumn(
        adjusted_sum_quantity,
        f.col(Colname.sum_quantity) + f.col("grid_loss_sum_quantity"),
    )

    result = result_df.select(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
        f.col(adjusted_sum_quantity).alias(Colname.sum_quantity),
        Colname.qualities,
    ).orderBy(
        Colname.grid_area,
        Colname.balance_responsible_id,
        Colname.energy_supplier_id,
        Colname.time_window,
    )

    return EnergyResults(result)
