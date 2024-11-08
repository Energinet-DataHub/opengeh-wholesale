from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain import csv_writer
from settlement_report_job.domain.charge_links.charge_links_factory import (
    create_charge_links,
)
from settlement_report_job.domain.metering_point_periods.metering_point_periods_factory import (
    create_metering_point_periods_for_wholesale,
)
from settlement_report_job.domain.order_by_columns import get_order_by_columns
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.domain.report_data_type import ReportDataType
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
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
from settlement_report_job.domain.task_type import TaskType
from settlement_report_job.domain.wholesale_results.wholesale_results_factory import (
    create_wholesale_results,
)
from settlement_report_job.infrastructure.calculation_type import CalculationType
from settlement_report_job.infrastructure.paths import get_report_output_path

from settlement_report_job.utils import create_zip_file
from telemetry_logging import Logger, use_span
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.domain.market_role import MarketRole


class ReportGenerator:
    def __init__(
        self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs
    ) -> None:
        self.spark = spark
        self.dbutils = dbutils
        self.args = args
        self.log = Logger(__name__)

    @use_span()
    def execute_hourly_time_series(self) -> None:
        self._execute_time_series(
            ReportDataType.TimeSeriesHourly,
            task_key="hourly_time_series_files",
        )

    @use_span()
    def execute_quarterly_time_series(self) -> None:
        self._execute_time_series(
            ReportDataType.TimeSeriesQuarterly,
            task_key="quarterly_time_series_files",
        )

    def _execute_time_series(
        self,
        report_data_type: ReportDataType,
        task_key: str,
    ) -> None:
        """
        Entry point for the logic of creating time series.
        """
        if not self.args.include_basis_data:
            return

        if report_data_type is ReportDataType.TimeSeriesHourly:
            metering_point_resolution = MeteringPointResolutionDataProductValue.HOUR
        elif report_data_type is ReportDataType.TimeSeriesQuarterly:
            metering_point_resolution = MeteringPointResolutionDataProductValue.QUARTER
        else:
            raise ValueError(f"Unsupported report data type: {report_data_type}")

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        if self.args.calculation_type is CalculationType.BALANCE_FIXING:
            time_series_df = create_time_series_for_balance_fixing(
                period_start=self.args.period_start,
                period_end=self.args.period_end,
                grid_area_codes=self.args.grid_area_codes,
                time_zone=self.args.time_zone,
                energy_supplier_ids=self.args.energy_supplier_ids,
                metering_point_resolution=metering_point_resolution,
                requesting_market_role=self.args.requesting_actor_market_role,
                repository=repository,
            )
        else:
            time_series_df = create_time_series_for_wholesale(
                period_start=self.args.period_start,
                period_end=self.args.period_end,
                calculation_id_by_grid_area=self.args.calculation_id_by_grid_area,
                time_zone=self.args.time_zone,
                energy_supplier_ids=self.args.energy_supplier_ids,
                metering_point_resolution=metering_point_resolution,
                repository=repository,
                requesting_actor_market_role=self.args.requesting_actor_market_role,
                requesting_actor_id=self.args.requesting_actor_id,
            )

        time_series_files = csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=time_series_df,
            report_data_type=report_data_type,
            order_by_columns=get_order_by_columns(
                report_data_type, self.args.requesting_actor_market_role
            ),
        )

        self.dbutils.jobs.taskValues.set(key=task_key, value=time_series_files)

    @use_span()
    def execute_charge_links(self) -> None:
        """
        Entry point for the logic of creating charge links.
        """
        if not self.args.include_basis_data:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        charge_links = create_charge_links(args=self.args, repository=repository)

        charge_links_files = csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=charge_links,
            report_data_type=ReportDataType.ChargeLinks,
            order_by_columns=get_order_by_columns(
                ReportDataType.ChargeLinks, self.args.requesting_actor_market_role
            ),
        )

        self.dbutils.jobs.taskValues.set(
            key="charge_links_files", value=charge_links_files
        )

<<<<<<< HEAD
    @use_span()
=======
    @logging.use_span()
    def execute_metering_point_periods(self) -> None:
        """
        Entry point for the logic of creating metering point periods.
        """
        if not self.args.include_basis_data:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        charge_links = create_metering_point_periods_for_wholesale(
            args=self.args, repository=repository
        )

        metering_point_periods_files = csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=charge_links,
            report_data_type=ReportDataType.MeteringPointPeriods,
            order_by_columns=get_order_by_columns(
                ReportDataType.MeteringPointPeriods,
                self.args.requesting_actor_market_role,
            ),
        )

        self.dbutils.jobs.taskValues.set(
            key="metering_point_periods_files", value=metering_point_periods_files
        )

    @logging.use_span()
>>>>>>> bfc1610bc251498da3661872c50fa84c0111488e
    def execute_energy_results(self) -> None:
        """
        Entry point for the logic of creating energy results.
        """
        if self.args.requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        energy_results_df = create_energy_results(args=self.args, repository=repository)

        energy_result_files = csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=energy_results_df,
            report_data_type=ReportDataType.EnergyResults,
            order_by_columns=get_order_by_columns(
                ReportDataType.EnergyResults, self.args.requesting_actor_market_role
            ),
        )

        self.dbutils.jobs.taskValues.set(
            key="energy_result_files", value=energy_result_files
        )

    @use_span()
    def execute_wholesale_results(self) -> None:
        """
        Entry point for the logic of creating wholesale results.
        """
        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        wholesale_results_df = create_wholesale_results(
            args=self.args, repository=repository
        )

        wholesale_result_files = csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=wholesale_results_df,
            report_data_type=ReportDataType.WholesaleResults,
            order_by_columns=get_order_by_columns(
                ReportDataType.WholesaleResults, self.args.requesting_actor_market_role
            ),
        )

        self.dbutils.jobs.taskValues.set(
            key="wholesale_result_files", value=wholesale_result_files
        )

    @use_span()
    def execute_monthly_amounts(self) -> None:
        """
        Entry point for the logic of creating wholesale results.
        """
        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        wholesale_results_df = create_monthly_amounts(
            args=self.args, repository=repository
        )

        monthly_amounts_files = csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=wholesale_results_df,
            report_data_type=ReportDataType.MonthlyAmounts,
            order_by_columns=get_order_by_columns(
                ReportDataType.MonthlyAmounts, self.args.requesting_actor_market_role
            ),
        )

        self.dbutils.jobs.taskValues.set(
            key="monthly_amounts_files", value=monthly_amounts_files
        )

    @use_span()
    def execute_zip(self) -> None:
        """
        Entry point for the logic of creating the final zip file.
        """
        files_to_zip = []

        task_types_to_zip = {
            TaskType.HOURLY_TIME_SERIES: "hourly_time_series_files",
            TaskType.QUARTERLY_TIME_SERIES: "quarterly_time_series_files",
            TaskType.METERING_POINT_PERIODS: "metering_point_periods_files",
            TaskType.CHARGE_LINKS: "charge_links_files",
            TaskType.MONTHLY_AMOUNTS: "monthly_amounts_files",
            TaskType.ENERGY_RESULTS: "energy_result_files",
            TaskType.WHOLESALE_RESULTS: "wholesale_result_files",
        }

        for task_key, key in task_types_to_zip.items():
            try:
                file_names = self.dbutils.jobs.taskValues.get(
                    taskKey=task_key.value, key=key
                )
                files_to_zip.extend(
                    [
                        f"{get_report_output_path(self.args)}/{file_name}"
                        for file_name in file_names
                    ]
                )
            except ValueError:
                self.log.info(
                    f"Task Key {task_key.value} was not found in TaskValues, continuing without it."
                )

        self.log.info(f"Files to zip: {files_to_zip}")
        zip_file_path = (
            f"{self.args.settlement_reports_output_path}/{self.args.report_id}.zip"
        )
        self.log.info(f"Creating zip file: '{zip_file_path}'")
        create_zip_file(self.dbutils, self.args.report_id, zip_file_path, files_to_zip)
        self.log.info(f"Finished creating '{zip_file_path}'")
