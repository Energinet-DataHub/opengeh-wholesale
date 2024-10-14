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
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.time_series.prepare_for_csv import (
    prepare_for_csv,
)
from settlement_report_job.domain.time_series.read_and_filter import (
    read_and_filter_for_wholesale,
    read_and_filter_for_balance_fixing,
)
from settlement_report_job.logger import Logger
from settlement_report_job.infrastructure import logging_configuration
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

log = Logger(__name__)

from typing import Optional
from uuid import UUID


class GridAreaSelection:
    def __init__(
        self,
        grid_area_codes: list[str],
        calculation_id_by_grid_area: Optional[dict[str, UUID]] = None,
    ):
        self.grid_area_codes = grid_area_codes
        self.calculation_id_by_grid_area = calculation_id_by_grid_area

    def has_calculation_ids(self) -> bool:
        return self.calculation_id_by_grid_area is not None


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

    return prepared_time_series
