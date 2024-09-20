from pyspark.sql import SparkSession

from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.time_series_factory import create_time_series
from settlement_report_job.infrastructure.database_definitions import (
    get_output_volume_name,
)

from settlement_report_job.utils import create_zip_file, get_dbutils
from settlement_report_job.logger import Logger

log = Logger(__name__)


def execute(spark: SparkSession, args: SettlementReportArgs) -> None:
    """
    Entry point for the logic of creating settlement reports.
    """
    dbutils = get_dbutils(spark)
    report_directory = f"{get_output_volume_name()}/{args.report_id}"
    zip_file_path = f"{report_directory}/final_report.zip"

    hourly_time_series_files = create_time_series(
        spark,
        args,
        report_directory,
        DataProductMeteringPointResolution.HOUR,
    )
    quarterly_time_series_files = create_time_series(
        spark,
        args,
        report_directory,
        DataProductMeteringPointResolution.QUARTER,
    )

    files_to_zip = []
    files_to_zip.extend(hourly_time_series_files)
    files_to_zip.extend(quarterly_time_series_files)
    print(files_to_zip)
    print(hourly_time_series_files)
    print(quarterly_time_series_files)
    log.info(f"Creating zip file: '{zip_file_path}'")
    create_zip_file(dbutils, args.report_id, zip_file_path, files_to_zip)
    log.info(f"Finished creating '{zip_file_path}'")
