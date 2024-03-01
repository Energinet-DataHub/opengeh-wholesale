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
import pyspark.sql.functions as f

from package.calculation.preparation.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from package.codelists import MeteringPointType
from package.constants import Colname


def get_charge_link_metering_point_periods(
    charge_links: DataFrame,
    metering_points: DataFrame,
) -> ChargeLinkMeteringPointPeriods:
    metering_points = _add_energy_supplier_id_to_child_metering_points(metering_points)
    charge_link_metering_point_periods = charge_links.join(
        metering_points,
        [
            charge_links[Colname.metering_point_id]
            == metering_points[Colname.metering_point_id],
        ],
        "inner",
    ).select(
        charge_links[Colname.charge_key],
        charge_links[Colname.charge_type],
        charge_links[Colname.metering_point_id],
        charge_links[Colname.charge_quantity],
        f.when(
            charge_links[Colname.from_date] > metering_points[Colname.from_date],
            charge_links[Colname.from_date],
        )
        .otherwise(metering_points[Colname.from_date])
        .alias(Colname.from_date),
        f.when(
            charge_links[Colname.to_date] < metering_points[Colname.to_date],
            charge_links[Colname.to_date],
        )
        .otherwise(metering_points[Colname.to_date])
        .alias(Colname.to_date),
        metering_points[Colname.metering_point_type],
        metering_points[Colname.settlement_method],
        metering_points[Colname.grid_area],
        metering_points[Colname.energy_supplier_id],
    )
    return ChargeLinkMeteringPointPeriods(charge_link_metering_point_periods)


def _get_production_and_consumption_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (f.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    ).select(Colname.metering_point_id, Colname.energy_supplier_id)


def _get_all_child_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (f.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.VE_PRODUCTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.NET_PRODUCTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.SUPPLY_TO_GRID.value)
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.CONSUMPTION_FROM_GRID.value
        )
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.WHOLESALE_SERVICES_INFORMATION.value
        )
        | (f.col(Colname.metering_point_type) == MeteringPointType.OWN_PRODUCTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.NET_FROM_GRID.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.NET_TO_GRID.value)
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.TOTAL_CONSUMPTION.value
        )
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.ELECTRICAL_HEATING.value
        )
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.NET_CONSUMPTION.value
        )
        | (
            f.col(Colname.metering_point_type)
            == MeteringPointType.EFFECT_SETTLEMENT.value
        )
    )


def _add_energy_supplier_id_to_child_metering_points(
    metering_point_periods_df: DataFrame,
) -> DataFrame:
    metering_point_periods_df.show()
    production_and_consumption_metering_points = (
        _get_production_and_consumption_metering_points(metering_point_periods_df)
    )
    es = "energy_supplier_id_temp"
    mp = "metering_point_id_temp"
    production_and_consumption_metering_points = (
        production_and_consumption_metering_points.withColumnRenamed(
            Colname.energy_supplier_id, es
        ).withColumnRenamed(Colname.metering_point_id, mp)
    )
    production_and_consumption_metering_points.show()
    all_metering_points = _get_all_child_metering_points(metering_point_periods_df)
    all_metering_points.show()
    metering_points_periods_for_wholesale_calculation = all_metering_points.join(
        production_and_consumption_metering_points,
        production_and_consumption_metering_points[mp]
        == all_metering_points[
            Colname.parent_metering_point_id
        ],  # parent_metering_point_id is always null on child metering points
        "left",
    )
    metering_points_periods_for_wholesale_calculation.show()
    metering_points_periods_for_wholesale_calculation.select(
        all_metering_points[Colname.metering_point_id],
        all_metering_points[Colname.metering_point_type],
        all_metering_points[Colname.calculation_type],
        all_metering_points[Colname.settlement_method],
        all_metering_points[Colname.grid_area],
        all_metering_points[Colname.resolution],
        all_metering_points[Colname.from_grid_area],
        all_metering_points[Colname.to_grid_area],
        all_metering_points[Colname.parent_metering_point_id],
        production_and_consumption_metering_points[es].alias(
            Colname.energy_supplier_id
        ),  # energy_supplier_id is always null on child metering points
        all_metering_points[Colname.balance_responsible_id],
        all_metering_points[Colname.from_date],
        all_metering_points[Colname.to_date],
    )
    metering_points_periods_for_wholesale_calculation.show()
    return metering_points_periods_for_wholesale_calculation
