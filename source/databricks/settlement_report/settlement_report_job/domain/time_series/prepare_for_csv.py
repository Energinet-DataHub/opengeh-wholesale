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

from pyspark.sql import DataFrame, functions as F, Window, Column

from settlement_report_job.domain.report_naming_convention import (
    METERING_POINT_TYPES,
)
from settlement_report_job.logger import Logger
from settlement_report_job.domain.csv_column_names import (
    TimeSeriesPointCsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.utils import (
    map_from_dict,
)
from settlement_report_job.infrastructure import logging_configuration
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

log = Logger(__name__)


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory.prepare_for_csv"
)
def prepare_for_csv(
    filtered_time_series_points: DataFrame,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    time_zone: str,
) -> DataFrame:
    desired_number_of_quantity_columns = _get_desired_quantity_column_count(
        metering_point_resolution
    )

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
    resolution: MeteringPointResolutionDataProductValue,
) -> int:
    if resolution == MeteringPointResolutionDataProductValue.HOUR:
        return 25
    elif resolution == MeteringPointResolutionDataProductValue.QUARTER:
        return 25 * 4
    else:
        raise ValueError(f"Unknown time series resolution: {resolution}")
