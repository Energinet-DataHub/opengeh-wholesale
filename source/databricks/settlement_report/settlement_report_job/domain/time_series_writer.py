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
from typing import Any

from pyspark.sql import DataFrame
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.logger import Logger
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
    TimeSeriesPointCsvColumnNames,
)
from settlement_report_job.utils import (
    write_files,
    get_new_files,
    merge_files,
)
from settlement_report_job.infrastructure import logging_configuration

log = Logger(__name__)


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory.create_time_series"
)
def write(
    dbutils: Any,
    args: SettlementReportArgs,
    report_directory: str,
    prepared_time_series: DataFrame,
    resolution: DataProductMeteringPointResolution,
) -> list[str]:
    result_path = f"{report_directory}/time_series_{resolution.value}"
    headers = write_files(
        df=prepared_time_series,
        path=result_path,
        partition_by_chunk_index=args.prevent_large_text_files,
        partition_by_grid_area=True,  # always true for time series
        order_by=[
            DataProductColumnNames.grid_area_code,
            TimeSeriesPointCsvColumnNames.metering_point_type,
            TimeSeriesPointCsvColumnNames.metering_point_id,
            TimeSeriesPointCsvColumnNames.start_of_day,
        ],
    )
    resolution_name = (
        "TSSD60" if resolution == DataProductMeteringPointResolution.HOUR else "TSSD15"
    )
    new_files = get_new_files(
        result_path,
        file_name_template="_".join(
            [
                resolution_name,
                "{grid_area}",
                args.period_start.strftime("%d-%m-%Y"),
                args.period_end.strftime("%d-%m-%Y"),
                "{chunk_index}",
            ]
        ),
        partition_by_chunk_index=args.prevent_large_text_files,
        partition_by_grid_area=True,  # always true for time series
    )
    files = merge_files(
        dbutils=dbutils,
        new_files=new_files,
        headers=headers,
    )
    return files
