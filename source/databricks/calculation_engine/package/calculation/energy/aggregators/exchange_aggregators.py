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

import pyspark.sql.functions as F

from package.codelists import MeteringPointType
from package.constants import Colname
import package.calculation.energy.aggregators.transformations as T
from package.calculation.energy.energy_results import EnergyResults
from package.calculation.preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
)

to_sum = "to_sum"
from_sum = "from_sum"
exchange_in_to_grid_area = "ExIn_ToGridArea"
exchange_in_from_grid_area = "ExIn_FromGridArea"
exchange_out_to_grid_area = "ExOut_ToGridArea"
exchange_out_from_grid_area = "ExOut_FromGridArea"


def aggregate_net_exchange_per_neighbour_ga(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
    selected_grid_areas: list[str],
) -> EnergyResults:
    """
    Function to aggregate net exchange per neighbouring grid areas.

    The result will only include exchange to/from grid areas specified in `selected_grid_areas`
    If no grid areas are specified (empty list) all grid areas are included.
    """

    df = quarterly_metering_point_time_series.df.where(
        F.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )

    group_by = [
        Colname.to_grid_area,
        Colname.from_grid_area,
        Colname.time_window,
    ]

    exchange_to = (
        T.aggregate_quantity_and_quality(df, group_by)
        .withColumnRenamed(Colname.sum_quantity, to_sum)
        .withColumnRenamed(Colname.to_grid_area, exchange_in_to_grid_area)
        .withColumnRenamed(Colname.from_grid_area, exchange_in_from_grid_area)
    )
    exchange_from = (
        T.aggregate_quantity_and_quality(df, group_by)
        .withColumnRenamed(Colname.sum_quantity, from_sum)
        .withColumnRenamed(Colname.to_grid_area, exchange_out_to_grid_area)
        .withColumnRenamed(Colname.from_grid_area, exchange_out_from_grid_area)
    )

    from_qualities = "from_qualities"
    to_time_window = "to_time_window"
    from_time_window = "from_time_window"

    # Workaround for ambiguous "time_window" column in select after join
    exchange_to = exchange_to.withColumnRenamed(Colname.time_window, to_time_window)
    exchange_from = exchange_from.withColumnRenamed(
        Colname.time_window, from_time_window
    )

    exchange = (
        # Outer self join: Outer is required as we must support cases where only metering point(s)
        # in one direction between two neighboring grid areas exist
        exchange_to.join(
            exchange_from,
            (exchange_to[to_time_window] == exchange_from[from_time_window])
            & (
                exchange_to[exchange_in_to_grid_area]
                == exchange_from[exchange_out_from_grid_area]
            )
            & (
                exchange_to[exchange_in_from_grid_area]
                == exchange_from[exchange_out_to_grid_area]
            ),
            "outer",
        )
        # Since both exchange_from or exchange_to can be missing we need to coalesce all columns
        .select(
            F.coalesce(
                exchange_to[to_time_window], exchange_from[from_time_window]
            ).alias(Colname.time_window),
            F.coalesce(
                exchange_to[exchange_in_to_grid_area],
                exchange_from[exchange_out_from_grid_area],
            ).alias(Colname.to_grid_area),
            F.coalesce(
                exchange_to[exchange_in_from_grid_area],
                exchange_from[exchange_out_to_grid_area],
            ).alias(Colname.from_grid_area),
            F.coalesce(exchange_to[to_sum], F.lit(0)).alias(to_sum),
            F.coalesce(exchange_from[from_sum], F.lit(0)).alias(from_sum),
            F.coalesce(exchange_from[Colname.qualities], F.array()).alias(
                from_qualities
            ),
            F.coalesce(exchange_to[Colname.qualities], F.array()).alias(
                Colname.qualities
            ),
        )
        # Calculate netto sum between neighboring grid areas
        .withColumn(Colname.sum_quantity, F.col(to_sum) - F.col(from_sum))
        # Finally select the result columns
        .select(
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.time_window,
            # Include qualities from all to- and from- metering point time series
            F.array_union(Colname.qualities, from_qualities).alias(Colname.qualities),
            Colname.sum_quantity,
            F.col(Colname.to_grid_area).alias(Colname.grid_area),
        )
    )

    if selected_grid_areas:
        exchange = exchange.filter(F.col(Colname.grid_area).isin(selected_grid_areas))

    return EnergyResults(exchange)


def aggregate_net_exchange_per_ga(
    data: QuarterlyMeteringPointTimeSeries, selected_grid_areas: list[str]
) -> EnergyResults:
    """
    Function to aggregate net exchange per grid area.

    The result will only include exchange to/from grid areas specified in `selected_grid_areas`
    If no grid areas are specified (empty list) all grid areas are included.
    """

    exchange_to = data.df.where(
        F.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )
    exchange_to_group_by = [
        Colname.to_grid_area,
        Colname.time_window,
    ]
    exchange_to = (
        T.aggregate_quantity_and_quality(exchange_to, exchange_to_group_by)
        .withColumnRenamed(Colname.sum_quantity, to_sum)
        .withColumnRenamed(Colname.to_grid_area, Colname.grid_area)
    )

    exchange_from = data.df.where(
        F.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )
    exchange_from_group_by = [
        Colname.from_grid_area,
        Colname.time_window,
    ]

    from_time_window = "from_time_window"

    exchange_from = (
        T.aggregate_quantity_and_quality(exchange_from, exchange_from_group_by)
        .withColumnRenamed(Colname.sum_quantity, from_sum)
        .withColumnRenamed(Colname.from_grid_area, Colname.grid_area)
        .withColumnRenamed(Colname.time_window, from_time_window)
    )

    from_grid_area = "from_grid_area"
    to_qualities = "to_qualities"
    from_qualities = "from_qualities"

    joined = exchange_to.join(
        exchange_from,
        (exchange_to[Colname.grid_area] == exchange_from[Colname.grid_area])
        & (exchange_to[Colname.time_window] == exchange_from[from_time_window]),
        how="outer",
    ).select(
        exchange_to[Colname.grid_area],
        exchange_to[Colname.time_window],
        exchange_to[to_sum],
        F.coalesce(exchange_to[Colname.qualities], F.array()).alias(to_qualities),
        F.coalesce(exchange_from[Colname.qualities], F.array()).alias(from_qualities),
        exchange_from[from_sum],
        exchange_from[Colname.grid_area].alias(from_grid_area),
        exchange_from[from_time_window],
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
            ).otherwise(F.col(from_grid_area)),
        )
        .withColumn(
            Colname.time_window,
            F.when(
                F.col(Colname.time_window).isNotNull(), F.col(Colname.time_window)
            ).otherwise(F.col(from_time_window)),
        )
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            # Include qualities from all to- and from- metering point time series
            F.array_union(to_qualities, from_qualities).alias(Colname.qualities),
        )
    )

    if selected_grid_areas:
        result_df = result_df.filter(F.col(Colname.grid_area).isin(selected_grid_areas))

    return EnergyResults(result_df)
