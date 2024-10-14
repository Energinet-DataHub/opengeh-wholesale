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
from settlement_report_job.domain.csv_column_names import EphemeralColumns
from settlement_report_job.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)
from settlement_report_job.domain.get_start_of_day import get_start_of_day
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.system_operator_filter import (
    filter_time_series_on_charge_owner,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

log = logging.Logger(__name__)


@logging.use_span()
def read_and_filter_for_balance_fixing(
    period_start: datetime,
    period_end: datetime,
    grid_area_codes: list[str],
    energy_supplier_ids: list[str] | None,
    resolution: MeteringPointResolutionDataProductValue,
    time_zone: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating time series points")
    time_series_points = _read_from_view(
        period_start,
        period_end,
        resolution,
        energy_supplier_ids,
        repository,
    )

@logging.use_span()
def read_and_filter_for_balance_fixing(
    period_start: datetime,
    period_end: datetime,
    grid_area_codes: list[str],
    energy_supplier_ids: list[str] | None,
    resolution: MeteringPointResolutionDataProductValue,
    time_zone: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating time series points")
    time_series_points = _read_from_view(
        period_start,
        period_end,
        resolution,
        repository,
    )

    latest_balance_fixing_calculations = repository.read_latest_calculations().where(
        (
            F.col(DataProductColumnNames.calculation_type)
            == CalculationTypeDataProductValue.BALANCE_FIXING
        )
        & (F.col(DataProductColumnNames.grid_area_code).isin(grid_area_codes))
        & (F.col(DataProductColumnNames.start_of_day) >= period_start)
        & (F.col(DataProductColumnNames.start_of_day) < period_end)
    )
    time_series_points = _filter_by_latest_calculations(
        time_series_points, latest_balance_fixing_calculations, time_zone=time_zone
    )

    if energy_supplier_ids:
        time_series_points = time_series_points.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)
        )

    return time_series_points


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory.read_and_filter_for_wholesale"
)
    latest_balance_fixing_calculations = repository.read_latest_calculations().where(
        (
            F.col(DataProductColumnNames.calculation_type)
            == CalculationTypeDataProductValue.BALANCE_FIXING
        )
        & (F.col(DataProductColumnNames.grid_area_code).isin(grid_area_codes))
        & (F.col(DataProductColumnNames.start_of_day) >= period_start)
        & (F.col(DataProductColumnNames.start_of_day) < period_end)
    )
    time_series_points = _filter_by_latest_calculations(
        time_series_points, latest_balance_fixing_calculations, time_zone=time_zone
    )

    return time_series_points


@logging.use_span()
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
        energy_supplier_ids=energy_supplier_ids,
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

    return time_series_points


@logging.use_span()
def _read_from_view(
    period_start: datetime,
    period_end: datetime,
    resolution: MeteringPointResolutionDataProductValue,
    energy_supplier_ids: list[str] | None,
    repository: WholesaleRepository,
) -> DataFrame:
    time_series_points = repository.read_metering_point_time_series().where(
        (F.col(DataProductColumnNames.observation_time) >= period_start)
        & (F.col(DataProductColumnNames.observation_time) < period_end)
        & (F.col(DataProductColumnNames.resolution) == resolution)
    )

    if energy_supplier_ids:
        time_series_points = time_series_points.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)
        )

    return time_series_points


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
