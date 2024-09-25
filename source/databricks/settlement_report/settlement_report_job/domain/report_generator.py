from pyspark.sql import SparkSession

from settlement_report_job.domain import time_series_writer
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.time_series_factory import create_time_series
from settlement_report_job.infrastructure.database_definitions import (
    get_output_volume_name,
)
from settlement_report_job.domain.task_type import TaskType

from settlement_report_job.utils import create_zip_file, get_dbutils
from settlement_report_job.logger import Logger

log = Logger(__name__)


def execute(spark: SparkSession, args: SettlementReportArgs) -> None:
    """
    Entry point for the logic of creating settlement reports.
    """
    dbutils = get_dbutils(spark)
    report_directory = f"{get_output_volume_name()}/{args.report_id}"

    hourly_time_series_df = create_time_series(
        spark,
        args,
        DataProductMeteringPointResolution.HOUR,
    )
    hourly_time_series_files = time_series_writer.write(
        dbutils,
        args,
        report_directory,
        hourly_time_series_df,
        DataProductMeteringPointResolution.HOUR,
    )

    quarterly_time_series_df = create_time_series(
        spark,
        args,
        DataProductMeteringPointResolution.QUARTER,
    )
    quarterly_time_series_files = time_series_writer.write(
        dbutils,
        args,
        report_directory,
        quarterly_time_series_df,
        DataProductMeteringPointResolution.QUARTER,
    )

    files_to_zip = []
    files_to_zip.extend(hourly_time_series_files)
    files_to_zip.extend(quarterly_time_series_files)
    log.info(f"Files to zip: {files_to_zip}")
    zip_file_path = f"{get_output_volume_name()}/{args.report_id}.zip"
    log.info(f"Creating zip file: '{zip_file_path}'")
    create_zip_file(dbutils, args.report_id, zip_file_path, files_to_zip)
    log.info(f"Finished creating '{zip_file_path}'")


def execute_hourly_time_series(spark: SparkSession, args: SettlementReportArgs) -> None:
    """
    Entry point for the logic of creating hourly time series.
    """
    dbutils = get_dbutils(spark)
    report_directory = f"{get_output_volume_name()}/{args.report_id}"

    hourly_time_series_df = create_time_series(
        spark,
        args,
        DataProductMeteringPointResolution.HOUR,
    )
    hourly_time_series_files = time_series_writer.write(
        dbutils,
        args,
        report_directory,
        hourly_time_series_df,
        DataProductMeteringPointResolution.HOUR,
    )

    dbutils.jobs.taskValues.set(
        key="hourly_time_series_files", value=hourly_time_series_files
    )


def execute_quarterly_time_series(
    spark: SparkSession, args: SettlementReportArgs
) -> None:
    """
    Entry point for the logic of creating quarterly time series.
    """
    dbutils = get_dbutils(spark)
    report_directory = f"{get_output_volume_name()}/{args.report_id}"

    quarterly_time_series_df = create_time_series(
        spark,
        args,
        DataProductMeteringPointResolution.QUARTER,
    )
    quarterly_time_series_files = time_series_writer.write(
        dbutils,
        args,
        report_directory,
        quarterly_time_series_df,
        DataProductMeteringPointResolution.QUARTER,
    )

    dbutils.jobs.taskValues.set(
        key="quarterly_time_series_files", value=quarterly_time_series_files
    )


def execute_zip(spark: SparkSession, args: SettlementReportArgs) -> None:
    """
    Entry point for the logic of creating the final zip file.
    """
    dbutils = get_dbutils(spark)
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
    zip_file_path = f"{get_output_volume_name()}/{args.report_id}.zip"
    log.info(f"Creating zip file: '{zip_file_path}'")
    create_zip_file(dbutils, args.report_id, zip_file_path, files_to_zip)
    log.info(f"Finished creating '{zip_file_path}'")
