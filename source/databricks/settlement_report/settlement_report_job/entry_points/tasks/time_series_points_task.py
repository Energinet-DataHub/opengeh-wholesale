from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain.time_series_points.order_by_columns import (
    order_by_columns,
)
from settlement_report_job.entry_points.tasks.task_type import TaskType
from settlement_report_job.entry_points.tasks.task_base import TaskBase
from settlement_report_job.infrastructure import csv_writer
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.time_series_points.time_series_points_factory import (
    create_time_series_points_for_wholesale,
    create_time_series_points_for_balance_fixing,
)
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from telemetry_logging import use_span
from settlement_report_job.infrastructure.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)


class TimeSeriesPointsTask(TaskBase):
    def __init__(
        self,
        spark: SparkSession,
        dbutils: Any,
        args: SettlementReportArgs,
        task_type: TaskType,
    ) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)
        self.task_type = task_type

    @use_span()
    def execute(
        self,
    ) -> None:
        """
        Entry point for the logic of creating time series.
        """
        if not self.args.include_basis_data:
            return

        if self.task_type is TaskType.TimeSeriesHourly:
            report_type = ReportDataType.TimeSeriesHourly
            metering_point_resolution = MeteringPointResolutionDataProductValue.HOUR
        elif self.task_type is TaskType.TimeSeriesQuarterly:
            report_type = ReportDataType.TimeSeriesQuarterly
            metering_point_resolution = MeteringPointResolutionDataProductValue.QUARTER
        else:
            raise ValueError(f"Unsupported report data type: {self.task_type}")

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        if self.args.calculation_type is CalculationType.BALANCE_FIXING:
            time_series_points_df = create_time_series_points_for_balance_fixing(
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
            time_series_points_df = create_time_series_points_for_wholesale(
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

        csv_writer.write(
            dbutils=self.dbutils,
            args=self.args,
            df=time_series_points_df,
            report_data_type=report_type,
            order_by_columns=order_by_columns(self.args.requesting_actor_market_role),
        )
