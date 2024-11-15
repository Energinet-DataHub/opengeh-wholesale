from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain.energy_results.order_by_columns import (
    order_by_columns,
)
from settlement_report_job.entry_points.tasks.task_base import TaskBase
from settlement_report_job.infrastructure import csv_writer
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.energy_results.energy_results_factory import (
    create_energy_results,
)
from telemetry_logging import use_span
from settlement_report_job.domain.utils.market_role import MarketRole


class EnergyResultsTask(TaskBase):
    def __init__(
        self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs
    ) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """
        Entry point for the logic of creating energy results.
        """
        if self.args.requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        energy_results_df = create_energy_results(args=self.args, repository=repository)

        csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=energy_results_df,
            report_data_type=ReportDataType.EnergyResults,
            order_by_columns=order_by_columns(self.args.requesting_actor_market_role),
        )
