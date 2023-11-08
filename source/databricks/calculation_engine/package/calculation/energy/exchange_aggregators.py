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
from pyspark.sql.types import StringType, StructType, StructField

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


# Function to aggregate net exchange per neighbouring grid areas
def aggregate_net_exchange_per_neighbour_ga(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    # meteringPoint_time_series with metering_point_type exchange will always have a from_grid_area and a to_grid_area
    # coalesce in exchange_to and exchange_from is used to align schema with reality,
    # to avoid making grid_area nullable in the schema
    # TODO: make sure that there is no chance that to and from grid area can be null when metering_point_type is exchange
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
        .withColumn(
            exchange_in_to_grid_area, f.coalesce(f.col(Colname.to_grid_area), f.lit(""))
        )
        .withColumn(
            exchange_in_from_grid_area,
            f.coalesce(f.col(Colname.from_grid_area), f.lit("")),
        )
    )
    # Set null sums to 0 to avoid null values in the sum column (only overflow can cause null values)
    exchange_to = exchange_to.withColumn(to_sum, f.coalesce(f.col(to_sum), f.lit(0)))

    exchange_from = (
        t.aggregate_quantity_and_quality(df, group_by)
        .withColumnRenamed(Colname.sum_quantity, from_sum)
        .withColumn(
            exchange_out_to_grid_area,
            f.coalesce(f.col(Colname.to_grid_area), f.lit("")),
        )
        .withColumn(
            exchange_out_from_grid_area,
            f.coalesce(f.col(Colname.from_grid_area), f.lit("")),
        )
    )
    # Set null sums to 0 to avoid null values in the sum column (only overflow can cause null values)
    exchange_from = exchange_from.withColumn(
        from_sum, f.coalesce(f.col(from_sum), f.lit(0))
    )

    from_qualities = "from_qualities"
    exchange = (
        exchange_to.join(exchange_from, [Colname.time_window], "inner")
        .where(
            exchange_to[exchange_in_to_grid_area]
            == exchange_from[exchange_out_from_grid_area]
        )
        .where(
            exchange_to[exchange_in_from_grid_area]
            == exchange_from[exchange_out_to_grid_area]
        )
        .select(
            exchange_to["*"],
            exchange_from[from_sum],
            exchange_from[Colname.qualities].alias(from_qualities),
        )
        .withColumn(
            Colname.sum_quantity, f.coalesce(f.col(to_sum) - f.col(from_sum), f.lit(0))
        )
        .select(
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.time_window,
            # Include qualities from all to- and from- metering point time series
            f.array_union(Colname.qualities, from_qualities).alias(Colname.qualities),
            Colname.sum_quantity,
            f.col(exchange_in_to_grid_area).alias(Colname.grid_area),
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
