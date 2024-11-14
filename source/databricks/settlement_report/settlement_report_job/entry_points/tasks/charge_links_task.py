from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain.charge_links.order_by_columns import order_by_columns
from settlement_report_job.entry_points.tasks.task_base import (
    TaskBase,
)
from settlement_report_job.infrastructure import csv_writer
from settlement_report_job.domain.charge_links.charge_links_factory import (
    create_charge_links,
)
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from telemetry_logging import use_span


class ChargeLinksTask(TaskBase):
    def __init__(
        self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs
    ) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """
        Entry point for the logic of creating charge links.
        """
        if not self.args.include_basis_data:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        charge_links = create_charge_links(args=self.args, repository=repository)

        csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=charge_links,
            report_data_type=ReportDataType.ChargeLinks,
            order_by_columns=order_by_columns(self.args.requesting_actor_market_role),
        )
