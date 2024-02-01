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

from pyspark.sql import DataFrame
from pyspark.sql.functions import (
    col,
    when,
    lit,
)
from package.constants import Colname
from datetime import datetime
from package.calculation_input import TableReader
from package.calculation_input.schemas import metering_point_period_schema
from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from package.common import assert_schema


def get_metering_point_periods_df(
    calculation_input_reader: TableReader,
    period_start: datetime,
    period_end: datetime,
    calculation_grid_areas: list[str],
) -> DataFrame:
    metering_point_periods_df = (
        calculation_input_reader.read_metering_point_periods()
        .where(
            col(Colname.grid_area).isin(calculation_grid_areas)
            | col(Colname.from_grid_area).isin(calculation_grid_areas)
            | col(Colname.to_grid_area).isin(calculation_grid_areas)
        )
        .where(col(Colname.from_date) < period_end)
        .where(col(Colname.to_date).isNull() | (col(Colname.to_date) > period_start))
    )

    metering_point_periods_df = _clamp_at_period_boundaries(
        metering_point_periods_df, period_start, period_end
    )
    metering_point_periods_df = _fix_settlement_method(metering_point_periods_df)
    metering_point_periods_df = _fix_metering_point_type(metering_point_periods_df)

    metering_point_periods_df = metering_point_periods_df.select(
        Colname.metering_point_id,
        Colname.metering_point_type,
        Colname.calculation_type,
        Colname.settlement_method,
        Colname.grid_area,
        Colname.resolution,
        Colname.from_grid_area,
        Colname.to_grid_area,
        Colname.parent_metering_point_id,
        Colname.energy_supplier_id,
        Colname.balance_responsible_id,
        Colname.from_date,
        Colname.to_date,
    )

    return metering_point_periods_df


def _clamp_at_period_boundaries(
    df: DataFrame,
    period_start: datetime,
    period_end: datetime,
) -> DataFrame:
    df = df.withColumn(
        Colname.from_date,
        when(col(Colname.from_date) < period_start, period_start).otherwise(
            col(Colname.from_date)
        ),
    ).withColumn(
        Colname.to_date,
        when(
            col(Colname.to_date).isNull() | (col(Colname.to_date) > period_end),
            period_end,
        ).otherwise(col(Colname.to_date)),
    )

    return df


def _fix_metering_point_type(df: DataFrame) -> DataFrame:
    return df.withColumn(
        Colname.metering_point_type,
        when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.CONSUMPTION.value,
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
            col(Colname.metering_point_type)
            == InputMeteringPointType.VE_PRODUCTION.value,
            lit(MeteringPointType.VE_PRODUCTION.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.NET_PRODUCTION.value,
            lit(MeteringPointType.NET_PRODUCTION.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.SUPPLY_TO_GRID.value,
            lit(MeteringPointType.SUPPLY_TO_GRID.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.CONSUMPTION_FROM_GRID.value,
            lit(MeteringPointType.CONSUMPTION_FROM_GRID.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.WHOLESALE_SERVICES_INFORMATION.value,
            lit(MeteringPointType.WHOLESALE_SERVICES_INFORMATION.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.OWN_PRODUCTION.value,
            lit(MeteringPointType.OWN_PRODUCTION.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.NET_FROM_GRID.value,
            lit(MeteringPointType.NET_FROM_GRID.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.NET_TO_GRID.value,
            lit(MeteringPointType.NET_TO_GRID.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.TOTAL_CONSUMPTION.value,
            lit(MeteringPointType.TOTAL_CONSUMPTION.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.ELECTRICAL_HEATING.value,
            lit(MeteringPointType.ELECTRICAL_HEATING.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.NET_CONSUMPTION.value,
            lit(MeteringPointType.NET_CONSUMPTION.value),
        )
        .when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.EFFECT_SETTLEMENT.value,
            lit(MeteringPointType.EFFECT_SETTLEMENT.value),
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
