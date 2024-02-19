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
from pyspark.sql.functions import when

from package.calculation.preparation.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from package.constants import Colname


def get_charge_link_metering_point_periods(
    charge_links: DataFrame,
    metering_points: DataFrame,
) -> ChargeLinkMeteringPointPeriods:
    charge_link_metering_point_periods = charge_links.join(
        metering_points,
        [
            charge_links[Colname.metering_point_id]
            == metering_points[Colname.metering_point_id],
        ],
        "inner",
    ).select(
        charge_links[Colname.charge_key],
        charge_links[Colname.metering_point_id],
        when(
            charge_links[Colname.from_date] > metering_points[Colname.from_date],
            charge_links[Colname.from_date],
        )
        .otherwise(metering_points[Colname.from_date])
        .alias(Colname.from_date),
        when(
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
