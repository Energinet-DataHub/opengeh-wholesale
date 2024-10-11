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

from pyspark.sql import DataFrame, functions as F, Column

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.DataProductValues.metering_point_resolution import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.system_operator_filter import (
    filter_time_series_on_charge_owner,
)
from settlement_report_job.logger import Logger
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.infrastructure import logging_configuration

log = Logger(__name__)


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory.read_and_filter"
)
def read_and_filter_for_wholesale(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating time series points")

    time_series_points = _read_from_view(
        period_start=period_start,
        period_end=period_end,
        resolution=metering_point_resolution,
        repository=repository,
    )

    time_series_points = time_series_points.where(
        _filter_on_calculation_id_by_grid_area(calculation_id_by_grid_area)
    )

    if requesting_actor_market_role is MarketRole.SYSTEM_OPERATOR:
        time_series_points = filter_time_series_on_charge_owner(
            time_series=time_series_points,
            system_operator_id=requesting_actor_id,
            charge_link_periods=repository.read_charge_link_periods(),
            charge_price_information_periods=repository.read_charge_price_information_periods(),
        )

    if energy_supplier_ids:
        time_series_points = time_series_points.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)
        )

    return time_series_points


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._read_from_view"
)
def _read_from_view(
    period_start: datetime,
    period_end: datetime,
    resolution: MeteringPointResolutionDataProductValue,
    repository: WholesaleRepository,
) -> DataFrame:
    return repository.read_metering_point_time_series().where(
        (F.col(DataProductColumnNames.observation_time) >= period_start)
        & (F.col(DataProductColumnNames.observation_time) < period_end)
        & (F.col(DataProductColumnNames.resolution) == resolution)
    )


def _filter_on_calculation_id_by_grid_area(
    calculation_id_by_grid_area: dict[str, UUID],
) -> Column:
    calculation_id_by_grid_area_structs = [
        F.struct(F.lit(grid_area_code), F.lit(str(calculation_id)))
        for grid_area_code, calculation_id in calculation_id_by_grid_area.items()
    ]

    return F.struct(
        F.col(DataProductColumnNames.grid_area_code),
        F.col(DataProductColumnNames.calculation_id),
    ).isin(calculation_id_by_grid_area_structs)
