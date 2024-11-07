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
from settlement_report_job.domain.dataframe_utils.factory_filters import (
    filter_by_charge_owner_and_tax_depending_on_market_role,
)
from settlement_report_job.domain.dataframe_utils.join_metering_points_periods_and_charge_links_periods import (
    join_metering_points_periods_and_charge_links_periods,
)
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.repository_filtering import (
    read_metering_point_periods_by_calculation_ids,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames

logger = logging.Logger(__name__)


@logging.use_span()
def read_and_filter(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    logger.info("Creating charge prices")

    charge_prices = (
        repository.read_charge_prices()
        .where((F.col(DataProductColumnNames.charge_time) >= period_start))
        .where(F.col(DataProductColumnNames.charge_time) < period_end)
    )

    charge_prices = _join_with_charge_link_and_metering_point_periods(
        charge_prices,
        period_start,
        period_end,
        calculation_id_by_grid_area,
        energy_supplier_ids,
        repository,
    )

    charge_price_information_periods = (
        repository.read_charge_price_information_periods()
    )

    charge_prices = charge_prices.join(
        charge_price_information_periods,
        on=[
            DataProductColumnNames.calculation_id,
            DataProductColumnNames.charge_key,
        ],
        how="inner",
    ).select(
        charge_prices["*"],
        charge_price_information_periods[DataProductColumnNames.is_tax],
        charge_price_information_periods[DataProductColumnNames.resolution],
    )

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.GRID_ACCESS_PROVIDER,
    ]:
        charge_prices = filter_by_charge_owner_and_tax_depending_on_market_role(
            charge_prices,
            requesting_actor_market_role,
            requesting_actor_id,
        )

    return charge_prices


def _join_with_charge_link_and_metering_point_periods(
    charge_prices: DataFrame,
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    repository: WholesaleRepository,
) -> DataFrame:
    charge_link_periods = repository.read_charge_link_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end)
        & (F.col(DataProductColumnNames.to_date) > period_start)
    )

    metering_point_periods = read_metering_point_periods_by_calculation_ids(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area=calculation_id_by_grid_area,
        energy_supplier_ids=energy_supplier_ids,
    )

    df = join_metering_points_periods_and_charge_links_periods(
        charge_link_periods, metering_point_periods
    )

    charge_prices = (
        charge_prices.join(
            df,
            on=[
                DataProductColumnNames.calculation_id,
                DataProductColumnNames.charge_key,
            ],
            how="inner",
        )
        .where((F.col(DataProductColumnNames.charge_time) >= period_start))
        .where(F.col(DataProductColumnNames.charge_time) < period_end)
        .select(
            charge_prices["*"],
            df[DataProductColumnNames.grid_area_code],
        )
    )

    return charge_prices
