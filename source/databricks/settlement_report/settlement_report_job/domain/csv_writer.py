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

from settlement_report_job import logging
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_data_type import ReportDataType
from settlement_report_job.domain.report_name_factory import FileNameFactory
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.utils import (
    write_files,
    get_new_files,
    merge_files,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames

log = logging.Logger(__name__)


@logging.use_span()
def write(
    dbutils: Any,
    args: SettlementReportArgs,
    df: DataFrame,
    report_data_type: ReportDataType,
    rows_per_file: int = 1_000_000,
) -> list[str]:

    report_output_path = f"{args.settlement_reports_output_path}/{args.report_id}"
    spark_output_path = f"{report_output_path}/{_get_folder_name(report_data_type)}"

    df_ready_for_writing = _apply_report_type_df_changes(df, args, report_data_type)

    partition_columns = _get_partition_columns_for_report_type(report_data_type, args)

    order_by_columns = _get_order_by_columns_for_report_type(report_data_type, args)

    headers = write_files(
        df=df_ready_for_writing,
        path=spark_output_path,
        partition_columns=partition_columns,
        order_by=order_by_columns,
        rows_per_file=rows_per_file,
        locale=args.locale,
    )

    file_name_factory = FileNameFactory(report_data_type, args)
    new_files = get_new_files(
        spark_output_path,
        report_output_path,
        file_name_factory,
        partition_columns=partition_columns,
    )
    files = merge_files(
        dbutils=dbutils,
        new_files=new_files,
        headers=headers,
        locale=args.locale,
    )
    return files


def _check_if_only_one_grid_area_is_selected(args: SettlementReportArgs) -> bool:
    return (
        args.calculation_id_by_grid_area is not None
        and len(args.calculation_id_by_grid_area) == 1
    ) or (args.grid_area_codes is not None and len(args.grid_area_codes) == 1)


def _get_partition_columns_for_report_type(
    report_type: ReportDataType, args: SettlementReportArgs
) -> list[str]:
    partition_columns = []
    if report_type in [
        ReportDataType.TimeSeriesHourly,
        ReportDataType.TimeSeriesQuarterly,
    ]:
        partition_columns = [CsvColumnNames.grid_area_code]
        if _is_partitioning_by_energy_supplier_id_needed(args):
            partition_columns.append(CsvColumnNames.energy_supplier_id)

    if report_type in [ReportDataType.EnergyResults]:
        if args.split_report_by_grid_area or _check_if_only_one_grid_area_is_selected(
            args
        ):
            partition_columns = [EphemeralColumns.grid_area_code]

        if _is_partitioning_by_energy_supplier_id_needed(args):
            partition_columns.append(CsvColumnNames.energy_supplier_id)

    if args.prevent_large_text_files:
        partition_columns.append(EphemeralColumns.chunk_index)

    return partition_columns


def _get_order_by_columns_for_report_type(
    report_type: ReportDataType, args: SettlementReportArgs
) -> list[str]:
    if report_type in [
        ReportDataType.TimeSeriesHourly,
        ReportDataType.TimeSeriesQuarterly,
    ]:
        return [
            CsvColumnNames.grid_area_code,
            CsvColumnNames.type_of_mp,
            CsvColumnNames.metering_point_id,
            CsvColumnNames.start_date_time,
        ]

    if report_type in [ReportDataType.EnergyResults]:
        order_by_columns = [
            CsvColumnNames.grid_area_code,
            CsvColumnNames.type_of_mp,
            CsvColumnNames.settlement_method,
            CsvColumnNames.start_date_time,
        ]

        if args.requesting_actor_market_role == MarketRole.DATAHUB_ADMINISTRATOR:
            order_by_columns.insert(1, CsvColumnNames.energy_supplier_id)

        return order_by_columns

    return []


def _apply_report_type_df_changes(
    df: DataFrame,
    args: SettlementReportArgs,
    report_type: ReportDataType,
) -> DataFrame:
    if (
        report_type
        in [ReportDataType.TimeSeriesHourly, ReportDataType.TimeSeriesQuarterly]
        and args.requesting_actor_market_role is MarketRole.GRID_ACCESS_PROVIDER
    ):
        df = df.drop(DataProductColumnNames.energy_supplier_id)

    return df


def _get_folder_name(report_data_type: ReportDataType) -> str:
    if report_data_type == ReportDataType.TimeSeriesHourly:
        return "time_series_hourly"
    elif report_data_type == ReportDataType.TimeSeriesQuarterly:
        return "time_series_quarterly"
    elif report_data_type == ReportDataType.EnergyResults:
        return "energy_results"
    else:
        raise ValueError(f"Unsupported report data type: {report_data_type}")


def _is_partitioning_by_energy_supplier_id_needed(args: SettlementReportArgs) -> bool:
    """
    When this partitioning by energy_supplier_id, the energy_supplier_id will be included in the file name

    """
    if args.requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        return (
            args.energy_supplier_ids is not None and len(args.energy_supplier_ids) == 1
        )
    elif args.requesting_actor_market_role is MarketRole.ENERGY_SUPPLIER:
        return True
    elif args.requesting_actor_market_role is MarketRole.GRID_ACCESS_PROVIDER:
        return False
    else:
        raise ValueError(
            f"Unsupported requesting actor market role: {args.requesting_actor_market_role}"
        )
