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

from package.codelists import MeteringPointType, QuantityQuality
from package.constants import Colname
from . import transformations as t
from package.calculation.energy.energy_results import EnergyResults
from ..preparation.quarterly_metering_point_time_series import (
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
) -> EnergyResults:
    """Function to aggregate net exchange per neighbouring grid areas."""

    df = quarterly_metering_point_time_series.df.where(
        f.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )

    group_by = [
        Colname.to_grid_area,
        Colname.from_grid_area,
        Colname.time_window,
    ]

    exchange_to = (
        t.aggregate_quantity_and_quality(df, group_by)
        .withColumnRenamed(Colname.sum_quantity, to_sum)
        .withColumnRenamed(Colname.to_grid_area, exchange_in_to_grid_area)
        .withColumnRenamed(Colname.from_grid_area, exchange_in_from_grid_area)
    )
    exchange_from = (
        t.aggregate_quantity_and_quality(df, group_by)
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
            f.coalesce(
                exchange_to[to_time_window], exchange_from[from_time_window]
            ).alias(Colname.time_window),
            f.coalesce(
                exchange_to[exchange_in_to_grid_area],
                exchange_from[exchange_out_from_grid_area],
            ).alias(Colname.to_grid_area),
            f.coalesce(
                exchange_to[exchange_in_from_grid_area],
                exchange_from[exchange_out_to_grid_area],
            ).alias(Colname.from_grid_area),
            f.coalesce(exchange_to[to_sum], f.lit(0)).alias(to_sum),
            f.coalesce(exchange_from[from_sum], f.lit(0)).alias(from_sum),
            f.coalesce(exchange_from[Colname.qualities], f.array()).alias(
                from_qualities
            ),
            f.coalesce(exchange_to[Colname.qualities], f.array()).alias(
                Colname.qualities
            ),
        )
        # Calculate netto sum between neighboring grid areas
        .withColumn(Colname.sum_quantity, f.col(to_sum) - f.col(from_sum))
        # Finally select the result columns
        .select(
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.time_window,
            # Include qualities from all to- and from- metering point time series
            f.array_union(Colname.qualities, from_qualities).alias(Colname.qualities),
            Colname.sum_quantity,
            f.col(Colname.to_grid_area).alias(Colname.grid_area),
            f.lit(MeteringPointType.EXCHANGE.value).alias(Colname.metering_point_type),
        )
    )
    return EnergyResults(exchange)


# Function to aggregate net exchange per grid area
def aggregate_net_exchange_per_ga(
    data: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    exchange_to = data.df.where(
        f.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )
    exchange_to_group_by = [
        Colname.to_grid_area,
        Colname.time_window,
    ]
    exchange_to = (
        t.aggregate_quantity_and_quality(exchange_to, exchange_to_group_by)
        .withColumnRenamed(Colname.sum_quantity, to_sum)
        .withColumnRenamed(Colname.to_grid_area, Colname.grid_area)
    )

    exchange_from = data.df.where(
        f.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value
    )
    exchange_from_group_by = [
        Colname.from_grid_area,
        Colname.time_window,
    ]

    from_time_window = "from_time_window"

    exchange_from = (
        t.aggregate_quantity_and_quality(exchange_from, exchange_from_group_by)
        .withColumnRenamed(Colname.sum_quantity, from_sum)
        .withColumnRenamed(Colname.from_grid_area, Colname.grid_area)
        .withColumnRenamed(Colname.time_window, from_time_window)
    )

    from_grid_area = "from_grid_area"
    joined = exchange_to.join(
        exchange_from,
        (exchange_to[Colname.grid_area] == exchange_from[Colname.grid_area])
        & (exchange_to[Colname.time_window] == exchange_from[from_time_window]),
        how="outer",
    ).select(
        exchange_to[Colname.grid_area],
        exchange_to[Colname.time_window],
        exchange_to[to_sum],
        f.coalesce(
            exchange_to[Colname.qualities],
            exchange_from[Colname.qualities],
            f.array(f.lit(QuantityQuality.MISSING.value)),
        ).alias(Colname.qualities),
        exchange_from[from_sum],
        exchange_from[Colname.grid_area].alias(from_grid_area),
        exchange_from[from_time_window],
    )

    result_df = (
        joined
        # Set null sums to 0 to avoid null values in the sum column
        .withColumn(
            to_sum,
            f.when(joined[to_sum].isNotNull(), joined[to_sum]).otherwise(f.lit(0)),
        )
        .withColumn(
            from_sum,
            f.when(joined[from_sum].isNotNull(), joined[from_sum]).otherwise(f.lit(0)),
        )
        .withColumn(Colname.sum_quantity, f.col(to_sum) - f.col(from_sum))
        # when().otherwise() cases to handle the case where a metering point exists with an from-grid-area, which never occurs as an to-grid-area
        .withColumn(
            Colname.grid_area,
            f.when(
                f.col(Colname.grid_area).isNotNull(), f.col(Colname.grid_area)
            ).otherwise(f.col(from_grid_area)),
        )
        .withColumn(
            Colname.time_window,
            f.when(
                f.col(Colname.time_window).isNotNull(), f.col(Colname.time_window)
            ).otherwise(f.col(from_time_window)),
        )
        .select(
            Colname.grid_area,
            Colname.time_window,
            Colname.sum_quantity,
            # TODO BJM: Missing the to-grid-area qualities?
            Colname.qualities,
            f.lit(MeteringPointType.EXCHANGE.value).alias(Colname.metering_point_type),
        )
    )

    return EnergyResults(result_df, ignore_nullability=True)
