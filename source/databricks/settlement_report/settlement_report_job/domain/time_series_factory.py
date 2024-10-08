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

from pyspark.sql import DataFrame, functions as F, Window, Column

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_naming_convention import (
    METERING_POINT_TYPES,
)
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.system_operator_filter import (
    filter_time_series_on_charge_owner,
)
from settlement_report_job.logger import Logger
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
    TimeSeriesPointCsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.utils import (
    map_from_dict,
)
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
    resolution: DataProductMeteringPointResolution,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
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

    prepared_time_series = _generate_time_series(
        filtered_time_series_points=time_series_points,
        resolution=resolution,
        time_zone=time_zone,
    )
    return prepared_time_series


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._read_from_view"
)
def _read_from_view(
    period_start: datetime,
    period_end: datetime,
    resolution: DataProductMeteringPointResolution,
    repository: WholesaleRepository,
) -> DataFrame:
    return repository.read_metering_point_time_series().where(
        (F.col(DataProductColumnNames.observation_time) >= period_start)
        & (F.col(DataProductColumnNames.observation_time) < period_end)
        & (F.col(DataProductColumnNames.resolution) == resolution.value)
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


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._generate_time_series"
)
def _generate_time_series(
    filtered_time_series_points: DataFrame,
    resolution: DataProductMeteringPointResolution,
    time_zone: str,
) -> DataFrame:
    desired_number_of_quantity_columns = _get_desired_quantity_column_count(resolution)

    filtered_time_series_points = filtered_time_series_points.withColumn(
        EphemeralColumns.start_of_day,
        _get_start_of_day(DataProductColumnNames.observation_time, time_zone),
    )

    win = Window.partitionBy(
        DataProductColumnNames.grid_area_code,
        DataProductColumnNames.energy_supplier_id,
        DataProductColumnNames.metering_point_id,
        DataProductColumnNames.metering_point_type,
        EphemeralColumns.start_of_day,
    ).orderBy(DataProductColumnNames.observation_time)
    filtered_time_series_points = filtered_time_series_points.withColumn(
        "chronological_order", F.row_number().over(win)
    )

    pivoted_df = (
        filtered_time_series_points.groupBy(
            DataProductColumnNames.grid_area_code,
            DataProductColumnNames.energy_supplier_id,
            DataProductColumnNames.metering_point_id,
            DataProductColumnNames.metering_point_type,
            EphemeralColumns.start_of_day,
        )
        .pivot(
            "chronological_order",
            list(range(1, desired_number_of_quantity_columns + 1)),
        )
        .agg(F.first(DataProductColumnNames.quantity))
    )

    quantity_column_names = [
        F.col(str(i)).alias(f"{TimeSeriesPointCsvColumnNames.energy_prefix}{i}")
        for i in range(1, desired_number_of_quantity_columns + 1)
    ]

    return pivoted_df.select(
        F.col(DataProductColumnNames.grid_area_code),
        F.col(DataProductColumnNames.metering_point_id).alias(
            TimeSeriesPointCsvColumnNames.metering_point_id
        ),
        F.col(DataProductColumnNames.energy_supplier_id).alias(
            TimeSeriesPointCsvColumnNames.energy_supplier_id
        ),
        map_from_dict(METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(TimeSeriesPointCsvColumnNames.metering_point_type),
        F.col(EphemeralColumns.start_of_day).alias(
            TimeSeriesPointCsvColumnNames.start_of_day
        ),
        *quantity_column_names,
    )


def _get_start_of_day(col: Column | str, time_zone: str) -> Column:
    col = F.col(col) if isinstance(col, str) else col
    return F.to_utc_timestamp(
        F.date_trunc("DAY", F.from_utc_timestamp(col, time_zone)), time_zone
    )


def _get_desired_quantity_column_count(
    resolution: DataProductMeteringPointResolution,
) -> int:
    if resolution == DataProductMeteringPointResolution.HOUR:
        return 25
    elif resolution == DataProductMeteringPointResolution.QUARTER:
        return 25 * 4
    else:
        raise ValueError(f"Unknown time series resolution: {resolution.value}")
