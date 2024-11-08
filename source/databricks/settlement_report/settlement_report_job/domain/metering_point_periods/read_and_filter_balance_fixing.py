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

    # get from latest calculations
    latest_balance_fixing_calculations = repository.read_latest_calculations().where(
        (
            F.col(DataProductColumnNames.calculation_type)
            == CalculationTypeDataProductValue.BALANCE_FIXING.value
        )
        & (F.col(DataProductColumnNames.grid_area_code).isin(grid_area_codes))
        & (F.col(DataProductColumnNames.start_of_day) >= period_start)
        & (F.col(DataProductColumnNames.start_of_day) < period_end)
    )

    metering_point_periods = metering_point_periods.select(*select_columns)

    metering_point_periods = merge_connected_periods(metering_point_periods)

    metering_point_periods = clamp_to_selected_period(
        metering_point_periods, period_start, period_end
    )

    return metering_point_periods


def filter_by_latest_calculations(
    df: DataFrame,
    latest_calculations: DataFrame,
    df_time_column: str | Column,
    time_zone: str,
) -> DataFrame:
    df = df.withColumn(
        EphemeralColumns.start_of_day,
        get_start_of_day(df_time_column, time_zone),
    )

    return (
        df.join(
            latest_calculations,
            on=[
                df[DataProductColumnNames.calculation_id]
                == latest_calculations[DataProductColumnNames.calculation_id],
                df[DataProductColumnNames.grid_area_code]
                == latest_calculations[DataProductColumnNames.grid_area_code],
                df[EphemeralColumns.start_of_day]
                == latest_calculations[DataProductColumnNames.start_of_day],
            ],
            how="inner",
        )
        .select(df["*"])
        .drop(EphemeralColumns.start_of_day)
    )
