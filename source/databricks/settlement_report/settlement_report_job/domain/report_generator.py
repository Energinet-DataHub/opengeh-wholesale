from pyspark.sql import SparkSession

from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.time_series_factory import create_time_series

from settlement_report_job.utils import create_zip_file, get_dbutils
from settlement_report_job.constants import get_output_volume_name
from settlement_report_job.infrastructure.logger import Logger

log = Logger(__name__)


def execute(spark: SparkSession, args: SettlementReportArgs) -> None:
    """
    Entry point for the logic of creating settlement reports.
    """
    create_time_series(spark, args)
    dbutils = get_dbutils(spark)
    report_directory = f"{get_output_volume_name()}/{args.report_id}"
    zip_file_path = f"{report_directory}/final_report.zip"

    files_to_zip = []
    time_series_files = create_time_series(spark, args, report_directory)
    files_to_zip.extend(time_series_files)

    log.info(f"Creating zip file at '{report_directory}.zip'")
    create_zip_file(dbutils, args.report_id, zip_file_path, files_to_zip)
    log.info(f"Finished creating '{report_directory}/some-name.zip'")
