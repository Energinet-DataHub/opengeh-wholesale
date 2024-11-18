from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.entry_points.tasks.task_base import TaskBase
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.infrastructure.paths import get_report_output_path

from settlement_report_job.infrastructure.create_zip_file import create_zip_file
from telemetry_logging import use_span


class ZipTask(TaskBase):
    def __init__(
        self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs
    ) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """
        Entry point for the logic of creating the final zip file.
        """
        report_output_path = get_report_output_path(self.args)
        files_to_zip = [
            f"{report_output_path}/{file_info.name}"
            for file_info in self.dbutils.fs.ls(report_output_path)
        ]

        self.log.info(f"Files to zip: {files_to_zip}")
        zip_file_path = (
            f"{self.args.settlement_reports_output_path}/{self.args.report_id}.zip"
        )
        self.log.info(f"Creating zip file: '{zip_file_path}'")
        create_zip_file(self.dbutils, self.args.report_id, zip_file_path, files_to_zip)
        self.log.info(f"Finished creating '{zip_file_path}'")
