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

from pyspark.sql import DataFrame

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.DataProductValues.metering_point_resolution import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.time_series.time_series_prepare_for_csv import (
    prepare_for_csv,
)
from settlement_report_job.domain.time_series.time_series_read_and_filter import (
    read_and_filter_for_wholesale,
)
from settlement_report_job.logger import Logger
from settlement_report_job.infrastructure import logging_configuration

log = Logger(__name__)


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory.create_time_series_for_wholesale"
)
def create_time_series_for_wholesale(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    time_zone: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating time series points")

    time_series_points = read_and_filter_for_wholesale(
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area=calculation_id_by_grid_area,
        energy_supplier_ids=energy_supplier_ids,
        metering_point_resolution=metering_point_resolution,
        requesting_actor_market_role=requesting_actor_market_role,
        requesting_actor_id=requesting_actor_id,
        repository=repository,
    )

    prepared_time_series = prepare_for_csv(
        filtered_time_series_points=time_series_points,
        metering_point_resolution=metering_point_resolution,
        time_zone=time_zone,
    )
    return prepared_time_series
