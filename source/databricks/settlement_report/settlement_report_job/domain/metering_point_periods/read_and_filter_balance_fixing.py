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

from pyspark.sql import DataFrame, functions as F

from telemetry_logging import Logger, use_span

from settlement_report_job.domain.utils.factory_filters import (
    read_and_filter_by_latest_calculations,
)
from settlement_report_job.domain.utils.merge_periods import (
    merge_connected_periods,
)
from settlement_report_job.domain.metering_point_periods.clamp_period import (
    clamp_to_selected_period,
)
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.utils.repository_filtering import (
    read_filtered_metering_point_periods_by_grid_area_codes,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def read_and_filter(
    period_start: datetime,
    period_end: datetime,
    grid_area_codes: list[str],
    energy_supplier_ids: list[str] | None,
    select_columns: list[str],
    time_zone: str,
    repository: WholesaleRepository,
) -> DataFrame:

    metering_point_periods = read_filtered_metering_point_periods_by_grid_area_codes(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        grid_area_codes=grid_area_codes,
        energy_supplier_ids=energy_supplier_ids,
    )

    metering_point_periods_daily = _explode_into_daily_period(
        metering_point_periods, time_zone
    )

    metering_point_periods_from_latest_calculations = (
        read_and_filter_by_latest_calculations(
            df=metering_point_periods_daily,
            grid_area_codes=grid_area_codes,
            period_start=period_start,
            period_end=period_end,
            time_zone=time_zone,
            time_column_name=DataProductColumnNames.from_date,
            repository=repository,
        )
    )

    metering_point_periods_from_latest_calculations = (
        metering_point_periods_from_latest_calculations.select(*select_columns)
    )

    metering_point_periods_from_latest_calculations = merge_connected_periods(
        metering_point_periods_from_latest_calculations
    )

    metering_point_periods_from_latest_calculations = clamp_to_selected_period(
        metering_point_periods_from_latest_calculations, period_start, period_end
    )

    return metering_point_periods_from_latest_calculations


def _explode_into_daily_period(df: DataFrame, time_zone: str) -> DataFrame:
    df = df.withColumn(
        "local_daily_from_date",
        F.explode(
            F.sequence(
                F.from_utc_timestamp(DataProductColumnNames.from_date, time_zone),
                F.date_sub(
                    F.from_utc_timestamp(DataProductColumnNames.to_date, time_zone), 1
                ),
                F.expr("interval 1 day"),
            )
        ),
    )
    df = df.withColumn("local_daily_to_date", F.date_add("local_daily_from_date", 1))

    df = df.withColumn(
        DataProductColumnNames.from_date,
        F.to_utc_timestamp("local_daily_from_date", time_zone),
    ).withColumn(
        DataProductColumnNames.to_date,
        F.to_utc_timestamp("local_daily_to_date", time_zone),
    )

    return df
