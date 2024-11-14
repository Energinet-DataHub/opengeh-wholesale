from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.entry_points.tasks.task_base import (
    TaskBase,
)
from settlement_report_job.infrastructure import csv_writer
from settlement_report_job.domain.charge_links.charge_links_factory import (
    create_charge_links,
)
from settlement_report_job.domain.metering_point_periods.metering_point_periods_factory import (
    create_metering_point_periods,
)
from settlement_report_job.infrastructure.order_by_columns import get_order_by_columns
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.monthly_amounts.monthly_amounts_factory import (
    create_monthly_amounts,
)
from settlement_report_job.domain.energy_results.energy_results_factory import (
    create_energy_results,
)
from settlement_report_job.domain.time_series.time_series_factory import (
    create_time_series_for_wholesale,
    create_time_series_for_balance_fixing,
)
from settlement_report_job.domain.wholesale_results.wholesale_results_factory import (
    create_wholesale_results,
)
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from settlement_report_job.infrastructure.paths import get_report_output_path

from settlement_report_job.infrastructure.utils import create_zip_file
from telemetry_logging import Logger, use_span
from settlement_report_job.infrastructure.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.domain.utils.market_role import MarketRole


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
            order_by_columns=get_order_by_columns(
                ReportDataType.ChargeLinks, self.args.requesting_actor_market_role
            ),
        )
