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
import pyspark.sql.functions as f
from . import transformations as t
from package.constants import Colname
from package.calculation.energy.energy_results import EnergyResults

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
    return _calculate_grid_loss_or_residual_ga(
        net_exchange_per_ga,
        non_profiled_consumption,
        flex_consumption,
        production,
    )


def _calculate_grid_loss_or_residual_ga(
    agg_net_exchange: EnergyResults,
    agg_non_profiled_consumption: EnergyResults,
    agg_flex_consumption: EnergyResults,
    agg_production: EnergyResults,
) -> EnergyResults:
    agg_non_profiled_consumption_result = t.aggregate_sum_quantity_and_qualities(
        agg_non_profiled_consumption.df,
        [Colname.grid_area, Colname.time_window],
    ).withColumnRenamed(Colname.sum_quantity, hourly_result)

    agg_flex_consumption_result = t.aggregate_sum_quantity_and_qualities(
        agg_flex_consumption.df,
        [Colname.grid_area, Colname.time_window],
    ).withColumnRenamed(Colname.sum_quantity, flex_result)

    agg_production_result = t.aggregate_sum_quantity_and_qualities(
        agg_production.df,
        [Colname.grid_area, Colname.time_window],
    ).withColumnRenamed(Colname.sum_quantity, prod_result)

    result = (
        agg_net_exchange.df.withColumnRenamed(Colname.sum_quantity, net_exchange_result)
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

    return EnergyResults(result, ignore_nullability=True)


def calculate_negative_grid_loss(grid_loss: EnergyResults) -> EnergyResults:
    result = grid_loss.df.select(
        Colname.grid_area,
        Colname.time_window,
        f.when(f.col(Colname.sum_quantity) < 0, -f.col(Colname.sum_quantity))
        .otherwise(0)
        .alias(Colname.sum_quantity),
        f.lit(MeteringPointType.PRODUCTION.value).alias(Colname.metering_point_type),
        Colname.qualities,
    )

    return EnergyResults(result, ignore_nullability=True)


def calculate_positive_grid_loss(grid_loss: EnergyResults) -> EnergyResults:
    result = grid_loss.df.select(
        Colname.grid_area,
        Colname.time_window,
        f.when(f.col(Colname.sum_quantity) > 0, f.col(Colname.sum_quantity))
        .otherwise(0)
        .alias(Colname.sum_quantity),
        f.lit(MeteringPointType.CONSUMPTION.value).alias(Colname.metering_point_type),
        Colname.qualities,
    )
    return EnergyResults(result, ignore_nullability=True)


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

    return EnergyResults(result, ignore_nullability=True)
