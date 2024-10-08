from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain import csv_writer
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.report_data_type import ReportDataType
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.time_series_factory import create_time_series
from settlement_report_job.domain.energy_results_factory import create_energy_results
from settlement_report_job.domain.task_type import TaskType

from settlement_report_job.utils import create_zip_file
from settlement_report_job.logger import Logger

log = Logger(__name__)


def execute_hourly_time_series(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    """
    Entry point for the logic of creating hourly time series.
    """
    if not args.include_basis_data:
        return

    repository = WholesaleRepository(spark, args.catalog_name)
    hourly_time_series_df = create_time_series(
        period_start=args.period_start,
        period_end=args.period_end,
        calculation_id_by_grid_area=args.calculation_id_by_grid_area,
        energy_supplier_ids=args.energy_supplier_ids,
        requesting_actor_id=args.requesting_actor_id,
        requesting_actor_market_role=args.requesting_actor_market_role,
        time_zone=args.time_zone,
        resolution=DataProductMeteringPointResolution.HOUR,
        repository=repository,
    )
    hourly_time_series_files = csv_writer.write(
        dbutils,
        args,
        hourly_time_series_df,
        ReportDataType.TimeSeriesHourly,
    )

    dbutils.jobs.taskValues.set(
        key="hourly_time_series_files", value=hourly_time_series_files
    )


def execute_quarterly_time_series(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    """
    Entry point for the logic of creating quarterly time series.
    """
    if not args.include_basis_data:
        return

    repository = WholesaleRepository(spark, args.catalog_name)
    quarterly_time_series_df = create_time_series(
        period_start=args.period_start,
        period_end=args.period_end,
        calculation_id_by_grid_area=args.calculation_id_by_grid_area,
        energy_supplier_ids=args.energy_supplier_ids,
        requesting_actor_id=args.requesting_actor_id,
        requesting_actor_market_role=args.requesting_actor_market_role,
        time_zone=args.time_zone,
        resolution=DataProductMeteringPointResolution.QUARTER,
        repository=repository,
    )
    quarterly_time_series_files = csv_writer.write(
        dbutils,
        args,
        quarterly_time_series_df,
        ReportDataType.TimeSeriesQuarterly,
    )

    dbutils.jobs.taskValues.set(
        key="quarterly_time_series_files", value=quarterly_time_series_files
    )


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
