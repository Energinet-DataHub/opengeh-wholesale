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

from package.codelists import MeteringPointType
from package.constants import Colname
from package.infrastructure import logging_configuration


@logging_configuration.use_span("get_metering_points_and_child_metering_points")
def get_child_metering_points_with_energy_suppliers(
    all_metering_point_periods: DataFrame,
) -> DataFrame:
    """
    Returns all metering points.
    The energy supplier of child metering points is added from its parent metering point.
    """
    production_and_consumption_metering_points = all_metering_point_periods.filter(
        (f.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    )

    es = "energy_supplier_id_temp"
    mp = "metering_point_id_temp"
    from_date = "from_date_temp"
    to_date = "to_date_temp"
    potential_parent_metering_points = (
        production_and_consumption_metering_points.select(
            f.col(Colname.metering_point_id).alias(mp),
            f.col(Colname.energy_supplier_id).alias(es),
            f.col(Colname.from_date).alias(from_date),
            f.col(Colname.to_date).alias(to_date),
        )
    )

    all_child_metering_points = _get_all_child_metering_points(
        all_metering_point_periods
    )

    child_metering_points_with_energy_suppliers = all_child_metering_points.join(
        potential_parent_metering_points,
        (
            potential_parent_metering_points[mp]
            == all_child_metering_points[Colname.parent_metering_point_id]
        )
        & (
            (  # When child from date is within parent period
                (
                    all_child_metering_points[Colname.from_date]
                    >= potential_parent_metering_points[from_date]
                )
                & (
                    all_child_metering_points[Colname.from_date]
                    < potential_parent_metering_points[to_date]
                )
            )
            | (  # When child to date is within parent period
                (
                    all_child_metering_points[Colname.to_date]
                    > potential_parent_metering_points[from_date]
                )
                & (
                    all_child_metering_points[Colname.to_date]
                    <= potential_parent_metering_points[to_date]
                )
            )
            | (  # When child from date is before parent from date and to date is after parent to date
                (
                    all_child_metering_points[Colname.to_date]
                    > potential_parent_metering_points[to_date]
                )
                & (
                    all_child_metering_points[Colname.from_date]
                    < potential_parent_metering_points[from_date]
                )
            )
        ),
        "left",
    ).select(
        all_child_metering_points[Colname.metering_point_id],
        all_child_metering_points[Colname.metering_point_type],
        all_child_metering_points[Colname.calculation_type],
        all_child_metering_points[Colname.settlement_method],
        all_child_metering_points[Colname.grid_area],
        all_child_metering_points[Colname.resolution],
        all_child_metering_points[Colname.from_grid_area],
        all_child_metering_points[Colname.to_grid_area],
        all_child_metering_points[Colname.parent_metering_point_id],
        potential_parent_metering_points[es].alias(
            Colname.energy_supplier_id
        ),  # energy_supplier_id is always null on child metering points
        all_child_metering_points[Colname.balance_responsible_id],
        f.when(
            potential_parent_metering_points[from_date]
            > all_child_metering_points[Colname.from_date],
            potential_parent_metering_points[from_date],
        )
        .otherwise(all_child_metering_points[Colname.from_date])
        .alias(Colname.from_date),
        f.when(
            potential_parent_metering_points[to_date]
            < all_child_metering_points[Colname.to_date],
            potential_parent_metering_points[to_date],
        )
        .otherwise(all_child_metering_points[Colname.to_date])
        .alias(Colname.to_date),
    )

    return child_metering_points_with_energy_suppliers


def _get_all_child_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    """
    Returns all child metering points.
    """
    return metering_points_periods_df.filter(
        (f.col(Colname.metering_point_type) == MeteringPointType.VE_PRODUCTION.value)
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
