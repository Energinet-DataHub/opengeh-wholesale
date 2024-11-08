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

from pyspark.sql import DataFrame

from settlement_report_job import logging
from settlement_report_job.domain.dataframe_utils.merge_periods import (
    merge_connected_periods,
)
from settlement_report_job.domain.metering_point_periods.clamp_period import (
    clamp_to_selected_period,
)
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.repository_filtering import (
    read_filtered_metering_point_periods_by_grid_area_codes,
)

logger = logging.Logger(__name__)


@logging.use_span()
def read_and_filter(
    period_start: datetime,
    period_end: datetime,
    grid_area_codes: list[str],
    energy_supplier_ids: list[str] | None,
    select_columns: list[str],
    repository: WholesaleRepository,
) -> DataFrame:

    metering_point_periods = read_filtered_metering_point_periods_by_grid_area_codes(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        grid_area_codes=grid_area_codes,
        energy_supplier_ids=energy_supplier_ids,
    )

    metering_point_periods = metering_point_periods.select(*select_columns)

    metering_point_periods = merge_connected_periods(metering_point_periods)

    metering_point_periods = clamp_to_selected_period(
        metering_point_periods, period_start, period_end
    )

    return metering_point_periods
