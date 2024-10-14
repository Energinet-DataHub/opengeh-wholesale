from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain import csv_writer
from settlement_report_job.domain.DataProductValues.metering_point_resolution import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.report_data_type import ReportDataType
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.energy_results_factory import create_energy_results
from settlement_report_job.domain.time_series.time_series_factory import (
    create_time_series_for_wholesale,
)
from settlement_report_job.domain.task_type import TaskType

from settlement_report_job.utils import create_zip_file
from settlement_report_job.logger import Logger

log = Logger(__name__)


def execute_hourly_time_series(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    _execute_time_series(
        spark,
        dbutils,
        args,
        ReportDataType.TimeSeriesHourly,
        task_key="hourly_time_series_files",
    )


def execute_quarterly_time_series(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    _execute_time_series(
        spark,
        dbutils,
        args,
        ReportDataType.TimeSeriesQuarterly,
        task_key="quarterly_time_series_files",
    )


def _execute_time_series(
    spark: SparkSession,
    dbutils: Any,
    args: SettlementReportArgs,
    report_data_type: ReportDataType,
    task_key: str,
) -> None:
    """
    Entry point for the logic of creating time series.
    """
    if not args.include_basis_data:
        return

    if report_data_type is ReportDataType.TimeSeriesHourly:
        metering_point_resolution = MeteringPointResolutionDataProductValue.HOUR
    elif report_data_type is ReportDataType.TimeSeriesQuarterly:
        metering_point_resolution = MeteringPointResolutionDataProductValue.QUARTER
    else:
        raise ValueError(f"Unsupported report data type: {report_data_type}")

    repository = WholesaleRepository(spark, args.catalog_name)
    time_series_df = create_time_series_for_wholesale(
        period_start=args.period_start,
        period_end=args.period_end,
        calculation_id_by_grid_area=args.calculation_id_by_grid_area,
        time_zone=args.time_zone,
        energy_supplier_ids=args.energy_supplier_ids,
        metering_point_resolution=metering_point_resolution,
        repository=repository,
        requesting_actor_market_role=args.requesting_actor_market_role,
        requesting_actor_id=args.requesting_actor_id,
    )
    time_series_files = csv_writer.write(
        dbutils,
        args,
        time_series_df,
        report_data_type,
    )

    dbutils.jobs.taskValues.set(key=task_key, value=time_series_files)


def execute_energy_results(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    """
    Entry point for the logic of creating energy results.
    """
    repository = WholesaleRepository(spark, args.catalog_name)
    energy_results_df = create_energy_results(args=args, repository=repository)

    energy_result_files = csv_writer.write(
        dbutils,
        args,
        energy_results_df,
        ReportDataType.EnergyResults,
    )

    dbutils.jobs.taskValues.set(key="energy_result_files", value=energy_result_files)


def execute_zip(spark: SparkSession, dbutils: Any, args: SettlementReportArgs) -> None:
    """
    Entry point for the logic of creating the final zip file.
    """
    files_to_zip = []

    task_types_to_zip = {
        TaskType.HOURLY_TIME_SERIES: "hourly_time_series_files",
        TaskType.QUARTERLY_TIME_SERIES: "quarterly_time_series_files",
        TaskType.ENERGY_RESULTS: "energy_result_files",
    }

    for taskKey, key in task_types_to_zip.items():
        files_to_zip.extend(dbutils.jobs.taskValues.get(taskKey=taskKey.value, key=key))

    log.info(f"Files to zip: {files_to_zip}")
    zip_file_path = f"{args.settlement_reports_output_path}/{args.report_id}.zip"
    log.info(f"Creating zip file: '{zip_file_path}'")
    create_zip_file(dbutils, args.report_id, zip_file_path, files_to_zip)
    log.info(f"Finished creating '{zip_file_path}'")
