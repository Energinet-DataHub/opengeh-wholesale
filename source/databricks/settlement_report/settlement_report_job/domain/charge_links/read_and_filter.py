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
from datetime import datetime
from uuid import UUID

from pyspark.sql import DataFrame, functions as F

from settlement_report_job import logging
from settlement_report_job.domain.dataframe_utils.merge_periods import (
    merge_connected_periods,
)
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.repository_filtering import (
    read_charge_link_periods,
    read_metering_point_periods_by_calculation_ids,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames

logger = logging.Logger(__name__)


@logging.use_span()
def read_and_filter_for_wholesale(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    logger.info("Creating charge links")

    metering_point_periods = read_metering_point_periods_by_calculation_ids(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area=calculation_id_by_grid_area,
        energy_supplier_ids=energy_supplier_ids,
    )
    charge_link_periods = read_charge_link_periods(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        charge_owner_id=requesting_actor_id,
        requesting_actor_market_role=requesting_actor_market_role,
    )

    charge_link_periods = _join_charge_link_and_metering_point_periods(
        charge_link_periods=charge_link_periods,
        metering_point_periods=metering_point_periods,
        requesting_actor_market_role=requesting_actor_market_role,
    )

    return charge_link_periods


def _join_charge_link_and_metering_point_periods(
    charge_link_periods: DataFrame,
    metering_point_periods: DataFrame,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    metering_point_periods = _merge_metering_point_periods(
        metering_point_periods, requesting_actor_market_role
    )

    link_from_date = "link_from_date"
    link_to_date = "link_to_date"
    metering_point_from_date = "metering_point_from_date"
    metering_point_to_date = "metering_point_to_date"

    charge_link_periods = charge_link_periods.withColumnRenamed(
        DataProductColumnNames.from_date, link_from_date
    ).withColumnRenamed(DataProductColumnNames.to_date, link_to_date)
    metering_point_periods = metering_point_periods.withColumnRenamed(
        DataProductColumnNames.from_date, metering_point_from_date
    ).withColumnRenamed(DataProductColumnNames.to_date, metering_point_to_date)

    charge_link_periods = (
        charge_link_periods.join(
            metering_point_periods,
            on=[
                DataProductColumnNames.calculation_id,
                DataProductColumnNames.metering_point_id,
            ],
            how="inner",
        )
        .where(
            (F.col(link_from_date) < F.col(metering_point_to_date))
            & (F.col(link_to_date) > F.col(metering_point_from_date))
        )
        .select(
            DataProductColumnNames.metering_point_id,
            DataProductColumnNames.metering_point_type,
            DataProductColumnNames.charge_type,
            DataProductColumnNames.charge_code,
            DataProductColumnNames.charge_owner_id,
            DataProductColumnNames.quantity,
            F.greatest(F.col(link_from_date), F.col(metering_point_from_date)).alias(
                DataProductColumnNames.from_date
            ),
            F.least(F.col(link_to_date), F.col(metering_point_to_date)).alias(
                DataProductColumnNames.to_date
            ),
            DataProductColumnNames.grid_area_code,
            DataProductColumnNames.energy_supplier_id,
        )
    )

    return charge_link_periods


def _merge_metering_point_periods(
    metering_point_periods: DataFrame,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    """
    Before joining metering point periods on the charge link periods, the metering point periods must be merged if
    for instance a period is split in two for instance due to a change in resolution, balance responsible party or energy
    supplier (grid access providers only)
    """
    metering_point_periods = metering_point_periods.select(
        DataProductColumnNames.calculation_id,
        DataProductColumnNames.metering_point_id,
        DataProductColumnNames.metering_point_type,
        DataProductColumnNames.grid_area_code,
        DataProductColumnNames.from_date,
        DataProductColumnNames.to_date,
        DataProductColumnNames.energy_supplier_id,
    )
    if requesting_actor_market_role is MarketRole.GRID_ACCESS_PROVIDER:
        # grid access provider should not see energy suppliers. We need to remove this columns so that a potential
        # change in energy supplier will not be appearing in the merged periods
        metering_point_periods = metering_point_periods.drop(
            DataProductColumnNames.energy_supplier_id
        )

    metering_point_periods = merge_connected_periods(metering_point_periods)

    if requesting_actor_market_role is MarketRole.GRID_ACCESS_PROVIDER:
        # add the energy_supplier_id column again to have the same columns in all cases
        metering_point_periods = metering_point_periods.withColumn(
            DataProductColumnNames.energy_supplier_id, F.lit(None)
        )

    return metering_point_periods
