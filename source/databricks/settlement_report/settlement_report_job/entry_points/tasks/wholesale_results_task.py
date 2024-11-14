from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain.wholesale_results.order_by_columns import (
    order_by_columns,
)
from settlement_report_job.entry_points.tasks.task_base import TaskBase
from settlement_report_job.infrastructure import csv_writer
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.wholesale_results.wholesale_results_factory import (
    create_wholesale_results,
)
from telemetry_logging import use_span


class WholesaleResultsTask(TaskBase):
    def __init__(
        self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs
    ) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """
        Entry point for the logic of creating wholesale results.
        """
        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        wholesale_results_df = create_wholesale_results(
            args=self.args, repository=repository
        )

        csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=wholesale_results_df,
            report_data_type=ReportDataType.WholesaleResults,
            order_by_columns=order_by_columns(),
        )
