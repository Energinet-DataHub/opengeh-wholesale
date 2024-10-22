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

from settlement_report_job import logging
from settlement_report_job.domain.csv_column_names import EphemeralColumns
from settlement_report_job.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)
from settlement_report_job.domain.get_start_of_day import get_start_of_day
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.system_operator_filter import (
    filter_on_charge_owner,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

log = logging.Logger(__name__)


@logging.use_span()
def read_and_filter(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating charge links")

    metering_point_periods = _read_metering_point_periods(
        period_start=period_start,
        period_end=period_end,
        energy_supplier_ids=energy_supplier_ids,
        repository=repository,
    )
    charge_link_periods = _read_charge_link_periods(
        period_start=period_start,
        period_end=period_end,
        requesting_actor_id=requesting_actor_id,
        requesting_actor_market_role=requesting_actor_market_role,
        repository=repository,
    )

    charge_link_periods = charge_link_periods.where(
        _filter_on_calculation_id_by_grid_area(calculation_id_by_grid_area)
    )

    charge_link_periods = charge_link_periods.join(
        metering_point_periods,
        on=[DataProductColumnNames.calculation_id, DataProductColumnNames.metering_point_id],
        how="inner",
    ).where(
        F.col(charge_link_periods[DataProductColumnNames.from_date]
    )






def _read_metering_point_periods(
    period_start: datetime,
    period_end: datetime,
    energy_supplier_ids: list[str] | None,
    repository: WholesaleRepository,
) -> DataFrame:

    metering_point_periods = repository.read_metering_point_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end)
        & (F.col(DataProductColumnNames.to_date) > period_start)
    )
    if energy_supplier_ids is not None:
        metering_point_periods = metering_point_periods.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)
        )

    return metering_point_periods


def _read_charge_link_periods(
    period_start: datetime,
    period_end: datetime,
    requesting_actor_id: str,
    requesting_actor_market_role: MarketRole,
    repository: WholesaleRepository,
) -> DataFrame:
    charge_link_periods = repository.read_charge_link_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end)
        & (F.col(DataProductColumnNames.to_date) > period_start)
    )

    if requesting_actor_market_role is MarketRole.SYSTEM_OPERATOR:

    elif requesting_actor_market_role is MarketRole.GRID_ACCESS_PROVIDER:

    return charge_link_periods


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


def _filter_by_latest_calculations(
    time_series_points: DataFrame, latest_calculations: DataFrame, time_zone: str
) -> DataFrame:
    time_series_points = time_series_points.withColumn(
        EphemeralColumns.start_of_day,
        get_start_of_day(DataProductColumnNames.observation_time, time_zone),
    )

    return (
        time_series_points.join(
            latest_calculations,
            on=[
                time_series_points[DataProductColumnNames.calculation_id]
                == latest_calculations[DataProductColumnNames.calculation_id],
                time_series_points[DataProductColumnNames.grid_area_code]
                == latest_calculations[DataProductColumnNames.grid_area_code],
                time_series_points[EphemeralColumns.start_of_day]
                == latest_calculations[DataProductColumnNames.start_of_day],
            ],
            how="inner",
        )
        .select(time_series_points["*"])
        .drop(EphemeralColumns.start_of_day)
    )
