import pyspark.sql.functions as F

import geh_wholesale.calculation.energy.aggregators.transformations as T
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from geh_wholesale.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from geh_wholesale.calculation.preparation.transformations.rounding import (
    round_quantity,
)
from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname

to_sum = "to_sum"
from_sum = "from_sum"
exchange_in_to_grid_area = "ExIn_ToGridArea"
exchange_in_from_grid_area = "ExIn_FromGridArea"
exchange_out_to_grid_area = "ExOut_ToGridArea"
exchange_out_from_grid_area = "ExOut_FromGridArea"


def aggregate_exchange_per_neighbor(
    metering_point_time_series: MeteringPointTimeSeries,
    calculation_grid_areas: list[str],
) -> EnergyResults:
    """Aggregate net exchange per neighboring grid areas.

    The result will only include exchange to/from grid areas specified in `calculation_grid_areas`.
    """
    df = metering_point_time_series.df.where(F.col(Colname.metering_point_type) == MeteringPointType.EXCHANGE.value)

    group_by = [
        Colname.to_grid_area_code,
        Colname.from_grid_area_code,
        Colname.observation_time,
    ]

    exchange_to = (
        T.aggregate_quantity_and_quality(df, group_by)
        .withColumnRenamed(Colname.quantity, to_sum)
        .withColumnRenamed(Colname.to_grid_area_code, exchange_in_to_grid_area)
        .withColumnRenamed(Colname.from_grid_area_code, exchange_in_from_grid_area)
    )
    exchange_from = (
        T.aggregate_quantity_and_quality(df, group_by)
        .withColumnRenamed(Colname.quantity, from_sum)
        .withColumnRenamed(Colname.to_grid_area_code, exchange_out_to_grid_area)
        .withColumnRenamed(Colname.from_grid_area_code, exchange_out_from_grid_area)
    )

    from_qualities = "from_qualities"
    to_observation_time = "to_observation_time"
    from_observation_time = "from_observation_time"

    # Workaround for ambiguous "time_window" column in select after join
    exchange_to = exchange_to.withColumnRenamed(Colname.observation_time, to_observation_time)
    exchange_from = exchange_from.withColumnRenamed(Colname.observation_time, from_observation_time)

    exchange = (
        # Outer self join: Outer is required as we must support cases where only metering point(s)
        # in one direction between two neighboring grid areas exist
        exchange_to.join(
            exchange_from,
            (exchange_to[to_observation_time] == exchange_from[from_observation_time])
            & (exchange_to[exchange_in_to_grid_area] == exchange_from[exchange_out_from_grid_area])
            & (exchange_to[exchange_in_from_grid_area] == exchange_from[exchange_out_to_grid_area]),
            "outer",
        )
        # Since both exchange_from or exchange_to can be missing we need to coalesce all columns
        .select(
            F.coalesce(exchange_to[to_observation_time], exchange_from[from_observation_time]).alias(
                Colname.observation_time
            ),
            F.coalesce(
                exchange_to[exchange_in_to_grid_area],
                exchange_from[exchange_out_from_grid_area],
            ).alias(Colname.to_grid_area_code),
            F.coalesce(
                exchange_to[exchange_in_from_grid_area],
                exchange_from[exchange_out_to_grid_area],
            ).alias(Colname.from_grid_area_code),
            F.coalesce(exchange_to[to_sum], F.lit(0)).alias(to_sum),
            F.coalesce(exchange_from[from_sum], F.lit(0)).alias(from_sum),
            F.coalesce(exchange_from[Colname.qualities], F.array()).alias(from_qualities),
            F.coalesce(exchange_to[Colname.qualities], F.array()).alias(Colname.qualities),
        )
        # Calculate netto sum between neighboring grid areas
        .withColumn(Colname.quantity, F.col(to_sum) - F.col(from_sum))
        # Finally select the result columns
        .select(
            Colname.to_grid_area_code,
            Colname.from_grid_area_code,
            Colname.observation_time,
            # Include qualities from all to- and from- metering point time series
            F.array_union(Colname.qualities, from_qualities).alias(Colname.qualities),
            Colname.quantity,
            F.col(Colname.to_grid_area_code).alias(Colname.grid_area_code),
        )
    )

    exchange = exchange.filter(F.col(Colname.grid_area_code).isin(calculation_grid_areas))
    exchange = round_quantity(exchange)
    return EnergyResults(exchange)


def aggregate_exchange(
    exchange_per_neighbor: EnergyResults,
) -> EnergyResults:
    """Aggregate net exchange per grid area.

    The result will only include exchange to/from grid areas specified in `calculation_grid_areas`.
    """
    result_df = T.aggregate_sum_quantity_and_qualities(
        exchange_per_neighbor.df, [Colname.grid_area_code, Colname.observation_time]
    )

    return EnergyResults(result_df)
