from typing import Any

import pyspark.sql.functions as f

import geh_wholesale.calculation.energy.aggregators.transformations as t
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_periods import (
    GridLossMeteringPointPeriods,
)
from geh_wholesale.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from geh_wholesale.constants import Colname

production_sum_quantity = "production_sum_quantity"
exchange_sum_quantity = "exchange_sum_quantity"
aggregated_production_qualities = "aggregated_production_quality"
aggregated_exchange_qualities = "aggregated_exchange_quality"
hourly_result = "hourly_result"
flex_result = "flex_result"
prod_result = "prod_result"
exchange_result = "exchange_result"


def calculate_grid_loss(
    exchange: EnergyResults,
    non_profiled_consumption: EnergyResults,
    flex_consumption: EnergyResults,
    production: EnergyResults,
) -> EnergyResults:
    agg_non_profiled_consumption_result = t.aggregate_sum_quantity_and_qualities(
        non_profiled_consumption.df,
        [Colname.grid_area_code, Colname.observation_time],
    ).withColumnRenamed(Colname.quantity, hourly_result)

    agg_flex_consumption_result = t.aggregate_sum_quantity_and_qualities(
        flex_consumption.df,
        [Colname.grid_area_code, Colname.observation_time],
    ).withColumnRenamed(Colname.quantity, flex_result)

    agg_production_result = t.aggregate_sum_quantity_and_qualities(
        production.df,
        [Colname.grid_area_code, Colname.observation_time],
    ).withColumnRenamed(Colname.quantity, prod_result)

    result = (
        exchange.df.withColumnRenamed(Colname.quantity, exchange_result)
        .join(
            agg_production_result,
            [Colname.grid_area_code, Colname.observation_time],
            "full",
        )
        .join(
            agg_flex_consumption_result.join(
                agg_non_profiled_consumption_result,
                [Colname.grid_area_code, Colname.observation_time],
                "full",
            ),
            [Colname.grid_area_code, Colname.observation_time],
            "full",
        )
        .orderBy(Colname.grid_area_code, Colname.observation_time)
    )

    # By having default values we ensure that the calculation below doesn't fail.
    # This can, however, hide errors that should have been handled earlier in the flow.
    result = (
        result.na.fill({exchange_result: 0})
        .na.fill({prod_result: 0})
        .na.fill({hourly_result: 0})
        .na.fill({flex_result: 0})
    )

    result = result.withColumn(
        Colname.quantity,
        result[exchange_result] + result[prod_result] - (result[hourly_result] + result[flex_result]),
    )

    result = result.select(
        Colname.grid_area_code,
        Colname.observation_time,
        Colname.quantity,  # grid loss
        # Quality of positive and negative grid loss must always be "calculated" as they become time series
        # that'll be sent to the metering points
        f.array(f.lit(QuantityQuality.CALCULATED.value)).alias(Colname.qualities),
    )

    return EnergyResults(result)


def calculate_negative_grid_loss(
    grid_loss: EnergyResults,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
) -> EnergyResults:
    return _calculate_negative_or_positive(
        grid_loss,
        grid_loss_metering_point_periods,
        MeteringPointType.PRODUCTION,
        f.when(f.col(Colname.quantity) < 0, -f.col(Colname.quantity)).otherwise(0),
    )


def calculate_positive_grid_loss(
    grid_loss: EnergyResults,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
) -> EnergyResults:
    return _calculate_negative_or_positive(
        grid_loss,
        grid_loss_metering_point_periods,
        MeteringPointType.CONSUMPTION,
        f.when(f.col(Colname.quantity) > 0, f.col(Colname.quantity)).otherwise(0),
    )


def _calculate_negative_or_positive(
    grid_loss: EnergyResults,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
    metering_point_type: MeteringPointType,
    value_expr: Any,
) -> EnergyResults:
    gl = grid_loss.df
    glr = grid_loss_metering_point_periods.df.where(
        f.col(Colname.metering_point_type) == metering_point_type.value
    ).alias("glr")

    result = (
        glr.join(
            gl,
            (gl[Colname.grid_area_code] == glr[Colname.grid_area_code])
            & (gl[Colname.observation_time] >= f.col(Colname.from_date))
            & (f.col(Colname.to_date).isNull() | (gl[Colname.observation_time] < f.col(Colname.to_date))),
            "inner",
        )
        .select(
            glr[Colname.grid_area_code],
            glr[Colname.energy_supplier_id],
            glr[Colname.balance_responsible_party_id],
            gl[Colname.observation_time],
            gl[Colname.quantity],
            gl[Colname.qualities],
            glr[Colname.metering_point_id],
        )
        .withColumn(Colname.quantity, value_expr)
    )

    return EnergyResults(result)


def calculate_total_consumption(production: EnergyResults, exchange: EnergyResults) -> EnergyResults:
    result_production = (
        t.aggregate_sum_quantity_and_qualities(
            production.df,
            [Colname.grid_area_code, Colname.observation_time],
        )
        .withColumnRenamed(Colname.quantity, production_sum_quantity)
        .withColumnRenamed(Colname.qualities, aggregated_production_qualities)
    )

    result_exchange = (
        t.aggregate_sum_quantity_and_qualities(
            exchange.df,
            [Colname.grid_area_code, Colname.observation_time],
        )
        .withColumnRenamed(Colname.quantity, exchange_sum_quantity)
        .withColumnRenamed(Colname.qualities, aggregated_exchange_qualities)
    )

    result = (
        result_production.join(
            result_exchange,
            [Colname.grid_area_code, Colname.observation_time],
            "inner",
        )
        .withColumn(
            Colname.quantity,
            f.col(production_sum_quantity) + f.col(exchange_sum_quantity),
        )
        .withColumn(
            Colname.qualities,
            f.array_union(aggregated_production_qualities, aggregated_exchange_qualities),
        )
    )

    result = result.select(
        Colname.grid_area_code,
        Colname.observation_time,
        Colname.qualities,
        Colname.quantity,
        f.lit(MeteringPointType.CONSUMPTION.value).alias(Colname.metering_point_type),
    )

    return EnergyResults(result)


def apply_grid_loss_adjustment(
    results: EnergyResults,
    grid_loss_result: EnergyResults,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
    metering_point_type: MeteringPointType,
) -> EnergyResults:
    grid_loss_responsible_grid_area = "GridLossResponsible_GridArea"
    adjusted_sum_quantity = "adjusted_sum_quantity"

    result_df = results.df
    grid_loss_result_df = grid_loss_result.df
    # grid_loss_result_df's energy supplier and balance responsible is always null
    grid_loss_result_df = grid_loss_result_df.drop(Colname.energy_supplier_id)
    grid_loss_result_df = grid_loss_result_df.drop(Colname.balance_responsible_party_id)

    grid_loss_metering_point_periods = grid_loss_metering_point_periods.df.where(
        f.col(Colname.metering_point_type) == metering_point_type.value
    ).select(
        Colname.from_date,
        Colname.to_date,
        Colname.energy_supplier_id,
        Colname.balance_responsible_party_id,
        f.col(Colname.grid_area_code).alias(grid_loss_responsible_grid_area),
        Colname.metering_point_type,
    )

    joined_grid_loss_result_and_responsible = grid_loss_result_df.join(
        grid_loss_metering_point_periods,
        f.when(
            f.col(Colname.to_date).isNotNull(),
            f.col(Colname.observation_time) <= f.col(Colname.to_date),
        ).otherwise(True)
        & (f.col(Colname.observation_time) >= f.col(Colname.from_date))
        & (f.col(Colname.to_date).isNull() | (f.col(Colname.observation_time) < f.col(Colname.to_date)))
        & (f.col(Colname.grid_area_code) == f.col(grid_loss_responsible_grid_area)),
        "left",
    ).select(
        Colname.grid_area_code,
        Colname.energy_supplier_id,
        Colname.balance_responsible_party_id,
        Colname.observation_time,
        Colname.quantity,
        Colname.qualities,
    )

    df = result_df.join(
        joined_grid_loss_result_and_responsible,
        [
            Colname.observation_time,
            Colname.grid_area_code,
            Colname.energy_supplier_id,
            Colname.balance_responsible_party_id,
        ],
        "outer",
    ).select(
        Colname.grid_area_code,
        Colname.balance_responsible_party_id,
        Colname.energy_supplier_id,
        Colname.observation_time,
        result_df[Colname.quantity],
        f.when(
            result_df[Colname.qualities].isNull(),
            joined_grid_loss_result_and_responsible[Colname.qualities],
        )
        .when(
            joined_grid_loss_result_and_responsible[Colname.qualities].isNull(),
            result_df[Colname.qualities],
        )
        .otherwise(f.array_union(result_df[Colname.qualities], grid_loss_result_df[Colname.qualities]))
        .alias(Colname.qualities),
        joined_grid_loss_result_and_responsible[Colname.quantity].alias("grid_loss_sum_quantity"),
    )
    df = df.na.fill(0, subset=["grid_loss_sum_quantity", Colname.quantity])

    result_df = df.withColumn(
        adjusted_sum_quantity,
        f.col(Colname.quantity) + f.col("grid_loss_sum_quantity"),
    )

    result = result_df.select(
        Colname.grid_area_code,
        Colname.balance_responsible_party_id,
        Colname.energy_supplier_id,
        Colname.observation_time,
        f.col(adjusted_sum_quantity).alias(Colname.quantity),
        Colname.qualities,
    ).orderBy(
        Colname.grid_area_code,
        Colname.balance_responsible_party_id,
        Colname.energy_supplier_id,
        Colname.observation_time,
    )

    return EnergyResults(result)
