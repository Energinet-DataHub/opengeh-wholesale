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

from package.calculation.preparation.data_structures.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from package.constants import Colname


def get_charge_link_metering_point_periods(
    charge_links: DataFrame,
    metering_points: DataFrame,
) -> ChargeLinkMeteringPointPeriods:

    # Alias the DataFrames
    charge_links_alias = charge_links.alias("cl")
    metering_points_alias = metering_points.alias("mp")

    # Perform the join operation with explicit aliases
    charge_link_metering_point_periods = (
        charge_links_alias.join(
            metering_points_alias,
            f.col("cl." + Colname.metering_point_id)
            == f.col("mp." + Colname.metering_point_id),
            "inner",
        )
        # Filter to get the overlap between the metering point period and the charge link period
        .where(
            (f.col("cl." + Colname.from_date) < f.col("mp." + Colname.to_date))
            & (f.col("cl." + Colname.to_date) > f.col("mp." + Colname.from_date))
        )
        # Select the required columns with explicit aliases
        .select(
            f.col("cl." + Colname.charge_key),
            f.col("cl." + Colname.charge_type),
            f.col("cl." + Colname.metering_point_id),
            f.col("cl." + Colname.quantity),
            f.when(
                f.col("cl." + Colname.from_date) > f.col("mp." + Colname.from_date),
                f.col("cl." + Colname.from_date),
            )
            .otherwise(f.col("mp." + Colname.from_date))
            .alias(Colname.from_date),
            f.when(
                f.col("cl." + Colname.to_date) < f.col("mp." + Colname.to_date),
                f.col("cl." + Colname.to_date),
            )
            .otherwise(f.col("mp." + Colname.to_date))
            .alias(Colname.to_date),
            f.col("mp." + Colname.metering_point_type),
            f.col("mp." + Colname.settlement_method),
            f.col("mp." + Colname.grid_area_code),
            f.col("mp." + Colname.energy_supplier_id),
        )
    )

    # charge_link_metering_point_periods = (
    #     charge_links.join(
    #         metering_points,
    #         [
    #             charge_links[Colname.metering_point_id]
    #             == metering_points[Colname.metering_point_id],
    #         ],
    #         "inner",
    #     )
    #     # We only want the overlap between the metering point period and the charge link period.
    #     .where(
    #         (charge_links[Colname.from_date] < metering_points[Colname.to_date])
    #         & (charge_links[Colname.to_date] > metering_points[Colname.from_date])
    #     ).select(
    #         charge_links[Colname.charge_key],
    #         charge_links[Colname.charge_type],
    #         charge_links[Colname.metering_point_id],
    #         charge_links[Colname.quantity],
    #         f.when(
    #             charge_links[Colname.from_date] > metering_points[Colname.from_date],
    #             charge_links[Colname.from_date],
    #         )
    #         .otherwise(metering_points[Colname.from_date])
    #         .alias(Colname.from_date),
    #         f.when(
    #             charge_links[Colname.to_date] < metering_points[Colname.to_date],
    #             charge_links[Colname.to_date],
    #         )
    #         .otherwise(metering_points[Colname.to_date])
    #         .alias(Colname.to_date),
    #         metering_points[Colname.metering_point_type],
    #         metering_points[Colname.settlement_method],
    #         metering_points[Colname.grid_area_code],
    #         metering_points[Colname.energy_supplier_id],
    #     )

    return ChargeLinkMeteringPointPeriods(charge_link_metering_point_periods)
