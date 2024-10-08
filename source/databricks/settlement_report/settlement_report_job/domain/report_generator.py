from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain import time_series_writer
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.report_data_type import ReportDataType
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.time_series_factory import (
    create_time_series_for_wholesale,
)
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
    hourly_time_series_df = create_time_series_for_wholesale(
        period_start=args.period_start,
        period_end=args.period_end,
        calculation_id_by_grid_area=args.calculation_id_by_grid_area,
        time_zone=args.time_zone,
        energy_supplier_ids=args.energy_supplier_ids,
        resolution=DataProductMeteringPointResolution.HOUR,
        repository=repository,
    )
    hourly_time_series_files = time_series_writer.write(
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
    quarterly_time_series_df = create_time_series_for_wholesale(
        period_start=args.period_start,
        period_end=args.period_end,
        calculation_id_by_grid_area=args.calculation_id_by_grid_area,
        energy_supplier_ids=args.energy_supplier_ids,
        time_zone=args.time_zone,
        resolution=DataProductMeteringPointResolution.QUARTER,
        repository=repository,
    )
    quarterly_time_series_files = time_series_writer.write(
        dbutils,
        args,
        quarterly_time_series_df,
        ReportDataType.TimeSeriesQuarterly,
    )

    dbutils.jobs.taskValues.set(
        key="quarterly_time_series_files", value=quarterly_time_series_files
    )


def execute_zip(spark: SparkSession, dbutils: Any, args: SettlementReportArgs) -> None:
    """
    Entry point for the logic of creating the final zip file.
    """
    files_to_zip = []
    files_to_zip.extend(
        dbutils.jobs.taskValues.get(
            taskKey=TaskType.HOURLY_TIME_SERIES.value,
            key="hourly_time_series_files",
        )
    )
    files_to_zip.extend(
        dbutils.jobs.taskValues.get(
            taskKey=TaskType.QUARTERLY_TIME_SERIES.value,
            key="quarterly_time_series_files",
        )
    )
    log.info(f"Files to zip: {files_to_zip}")
    zip_file_path = f"{args.settlement_reports_output_path}/{args.report_id}.zip"
    log.info(f"Creating zip file: '{zip_file_path}'")
    create_zip_file(dbutils, args.report_id, zip_file_path, files_to_zip)
    log.info(f"Finished creating '{zip_file_path}'")
