from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.infrastructure import csv_writer
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.charge_price_points.charge_price_points_factory import (
    create_charge_price_points,
)
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.domain.charge_price_points.order_by_columns import (
    order_by_columns,
)
from settlement_report_job.entry_points.tasks.task_base import (
    TaskBase,
)
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from telemetry_logging import use_span


class ChargePricePointsTask(TaskBase):
    def __init__(
        self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs
    ) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """
        Entry point for the logic of creating charge prices.
        """
        if not self.args.include_basis_data:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        charge_price_points = create_charge_price_points(
            args=self.args, repository=repository
        )

        csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=charge_price_points,
            report_data_type=ReportDataType.ChargePricePoints,
            order_by_columns=order_by_columns(),
        )
