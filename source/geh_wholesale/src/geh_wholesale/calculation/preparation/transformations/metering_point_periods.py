from datetime import datetime

from geh_common.pyspark.clamp import clamp_period_end, clamp_period_start
from pyspark.sql import DataFrame
from pyspark.sql.functions import (
    col,
    lit,
    when,
)

from geh_wholesale.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from geh_wholesale.constants import Colname
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository


def get_metering_point_periods_df(
    calculation_input_reader: MigrationsWholesaleRepository,
    period_start: datetime,
    period_end: datetime,
    calculation_grid_areas: list[str],
) -> DataFrame:
    metering_point_periods_df = (
        calculation_input_reader.read_metering_point_periods()
        .where(
            col(Colname.grid_area_code).isin(calculation_grid_areas)
            | col(Colname.from_grid_area_code).isin(calculation_grid_areas)
            | col(Colname.to_grid_area_code).isin(calculation_grid_areas)
        )
        .where(col(Colname.from_date) < period_end)
        .where(col(Colname.to_date).isNull() | (col(Colname.to_date) > period_start))
    )

    metering_point_periods_df = metering_point_periods_df.withColumn(
        Colname.from_date, clamp_period_start(Colname.from_date, period_start)
    ).withColumn(Colname.to_date, clamp_period_end(Colname.to_date, period_end))
    metering_point_periods_df = _fix_settlement_method(metering_point_periods_df)
    metering_point_periods_df = _fix_metering_point_type(metering_point_periods_df)

    metering_point_periods_df = metering_point_periods_df.select(
        Colname.metering_point_id,
        Colname.metering_point_type,
        Colname.calculation_type,
        Colname.settlement_method,
        Colname.grid_area_code,
        Colname.resolution,
        Colname.from_grid_area_code,
        Colname.to_grid_area_code,
        Colname.parent_metering_point_id,
        Colname.energy_supplier_id,
        Colname.balance_responsible_party_id,
        Colname.from_date,
        Colname.to_date,
    )

    return metering_point_periods_df


def _fix_metering_point_type(df: DataFrame) -> DataFrame:
    return df.withColumn(
        Colname.metering_point_type,
        when(
            col(Colname.metering_point_type) == InputMeteringPointType.CONSUMPTION.value,
            lit(MeteringPointType.CONSUMPTION.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.PRODUCTION.value,
            lit(MeteringPointType.PRODUCTION.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.EXCHANGE.value,
            lit(MeteringPointType.EXCHANGE.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.VE_PRODUCTION.value,
            lit(MeteringPointType.VE_PRODUCTION.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.NET_PRODUCTION.value,
            lit(MeteringPointType.NET_PRODUCTION.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.SUPPLY_TO_GRID.value,
            lit(MeteringPointType.SUPPLY_TO_GRID.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.CONSUMPTION_FROM_GRID.value,
            lit(MeteringPointType.CONSUMPTION_FROM_GRID.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.WHOLESALE_SERVICES_INFORMATION.value,
            lit(MeteringPointType.WHOLESALE_SERVICES_INFORMATION.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.OWN_PRODUCTION.value,
            lit(MeteringPointType.OWN_PRODUCTION.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.NET_FROM_GRID.value,
            lit(MeteringPointType.NET_FROM_GRID.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.NET_TO_GRID.value,
            lit(MeteringPointType.NET_TO_GRID.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.TOTAL_CONSUMPTION.value,
            lit(MeteringPointType.TOTAL_CONSUMPTION.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.ELECTRICAL_HEATING.value,
            lit(MeteringPointType.ELECTRICAL_HEATING.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.NET_CONSUMPTION.value,
            lit(MeteringPointType.NET_CONSUMPTION.value),
        )
        .when(
            col(Colname.metering_point_type) == InputMeteringPointType.CAPACITY_SETTLEMENT.value,
            lit(MeteringPointType.CAPACITY_SETTLEMENT.value),
        )
        .otherwise(lit("Unknown type")),
        # The otherwise is to avoid changing the nullability of the column.
    )


def _fix_settlement_method(df: DataFrame) -> DataFrame:
    return df.withColumn(
        Colname.settlement_method,
        when(
            col(Colname.settlement_method) == InputSettlementMethod.FLEX.value,
            lit(SettlementMethod.FLEX.value),
        ).when(
            col(Colname.settlement_method) == InputSettlementMethod.NON_PROFILED.value,
            lit(SettlementMethod.NON_PROFILED.value),
        ),
    )
