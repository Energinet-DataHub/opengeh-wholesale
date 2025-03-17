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
from pyspark.sql import Column, DataFrame

from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname


def is_parent_metering_point(col: Column | str) -> bool:
    col = f.col(col) if isinstance(col, str) else col
    return (
        (col == MeteringPointType.CONSUMPTION.value)
        | (col == MeteringPointType.PRODUCTION.value)
        | (col == MeteringPointType.EXCHANGE.value)
    )


def is_child_metering_point(col: Column | str) -> bool:
    col = f.col(col) if isinstance(col, str) else col
    return ~is_parent_metering_point(col)  # type: ignore


def add_parent_data_to_child_metering_point_periods(
    all_metering_point_periods: DataFrame,
) -> DataFrame:
    """All child metering points are updated with the energy supplier id and balance responsible party id of its parent metering point."""
    parent_metering_points_periods = all_metering_point_periods.where(
        is_parent_metering_point(Colname.metering_point_type)
    )
    child_metering_points_with_energy_suppliers = _get_child_metering_points_with_energy_suppliers(
        all_metering_point_periods
    )
    metering_point_periods_union = parent_metering_points_periods.union(child_metering_points_with_energy_suppliers)
    return metering_point_periods_union


def _get_child_metering_points_with_energy_suppliers(
    all_metering_point_periods: DataFrame,
) -> DataFrame:
    """Returns all child metering points.
    The energy supplier of child metering points is added from its parent metering point.
    """
    es = "energy_supplier_id_temp"
    mp = "metering_point_id_temp"
    from_date = "from_date_temp"
    to_date = "to_date_temp"
    potential_parent_metering_points = all_metering_point_periods.where(
        is_parent_metering_point(Colname.metering_point_type)
    ).select(
        f.col(Colname.metering_point_id).alias(mp),
        f.col(Colname.energy_supplier_id).alias(es),
        f.col(Colname.from_date).alias(from_date),
        f.col(Colname.to_date).alias(to_date),
    )

    all_child_metering_points = all_metering_point_periods.where(is_child_metering_point(Colname.metering_point_type))

    child_metering_points_with_energy_suppliers = all_child_metering_points.join(
        potential_parent_metering_points,
        (potential_parent_metering_points[mp] == all_child_metering_points[Colname.parent_metering_point_id])
        & (
            (  # When child from date is within parent period
                (all_child_metering_points[Colname.from_date] >= potential_parent_metering_points[from_date])
                & (all_child_metering_points[Colname.from_date] < potential_parent_metering_points[to_date])
            )
            | (  # When child to date is within parent period
                (all_child_metering_points[Colname.to_date] > potential_parent_metering_points[from_date])
                & (all_child_metering_points[Colname.to_date] <= potential_parent_metering_points[to_date])
            )
            | (  # When child from date is before parent from date and to date is after parent to date
                (all_child_metering_points[Colname.to_date] > potential_parent_metering_points[to_date])
                & (all_child_metering_points[Colname.from_date] < potential_parent_metering_points[from_date])
            )
        ),
        "inner",
    ).select(
        all_child_metering_points[Colname.metering_point_id],
        all_child_metering_points[Colname.metering_point_type],
        all_child_metering_points[Colname.calculation_type],
        all_child_metering_points[Colname.settlement_method],
        all_child_metering_points[Colname.grid_area_code],
        all_child_metering_points[Colname.resolution],
        all_child_metering_points[Colname.from_grid_area_code],
        all_child_metering_points[Colname.to_grid_area_code],
        all_child_metering_points[Colname.parent_metering_point_id],
        potential_parent_metering_points[es].alias(
            Colname.energy_supplier_id
        ),  # energy_supplier_id is always null on child metering points
        all_child_metering_points[Colname.balance_responsible_party_id],
        f.when(
            potential_parent_metering_points[from_date] > all_child_metering_points[Colname.from_date],
            potential_parent_metering_points[from_date],
        )
        .otherwise(all_child_metering_points[Colname.from_date])
        .alias(Colname.from_date),
        f.when(
            potential_parent_metering_points[to_date] < all_child_metering_points[Colname.to_date],
            potential_parent_metering_points[to_date],
        )
        .otherwise(all_child_metering_points[Colname.to_date])
        .alias(Colname.to_date),
    )

    return child_metering_points_with_energy_suppliers
