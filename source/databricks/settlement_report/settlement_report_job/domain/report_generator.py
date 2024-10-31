from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.domain import csv_writer
from settlement_report_job.domain.charge_links.charge_links_factory import (
    create_charge_links,
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

from settlement_report_job.utils import create_zip_file
from settlement_report_job.logging import Logger
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.domain.market_role import MarketRole

log = Logger(__name__)


def execute_hourly_time_series(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    _execute_time_series(
        spark,
        dbutils,
        args,
        ReportDataType.TimeSeriesHourly,
        task_key="hourly_time_series_files",
    )


def execute_quarterly_time_series(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    _execute_time_series(
        spark,
        dbutils,
        args,
        ReportDataType.TimeSeriesQuarterly,
        task_key="quarterly_time_series_files",
    )


def _execute_time_series(
    spark: SparkSession,
    dbutils: Any,
    args: SettlementReportArgs,
    report_data_type: ReportDataType,
    task_key: str,
) -> None:
    """
    Entry point for the logic of creating time series.
    """
    if not args.include_basis_data:
        return

    if report_data_type is ReportDataType.TimeSeriesHourly:
        metering_point_resolution = MeteringPointResolutionDataProductValue.HOUR
    elif report_data_type is ReportDataType.TimeSeriesQuarterly:
        metering_point_resolution = MeteringPointResolutionDataProductValue.QUARTER
    else:
        raise ValueError(f"Unsupported report data type: {report_data_type}")

    repository = WholesaleRepository(spark, args.catalog_name)
    if args.calculation_type is CalculationType.BALANCE_FIXING:
        time_series_df = create_time_series_for_balance_fixing(
            period_start=args.period_start,
            period_end=args.period_end,
            grid_area_codes=args.grid_area_codes,
            time_zone=args.time_zone,
            energy_supplier_ids=args.energy_supplier_ids,
            metering_point_resolution=metering_point_resolution,
            requesting_market_role=args.requesting_actor_market_role,
            repository=repository,
        )
    else:
        time_series_df = create_time_series_for_wholesale(
            period_start=args.period_start,
            period_end=args.period_end,
            calculation_id_by_grid_area=args.calculation_id_by_grid_area,
            time_zone=args.time_zone,
            energy_supplier_ids=args.energy_supplier_ids,
            metering_point_resolution=metering_point_resolution,
            repository=repository,
            requesting_actor_market_role=args.requesting_actor_market_role,
            requesting_actor_id=args.requesting_actor_id,
        )

    time_series_files = csv_writer.write(
        dbutils=dbutils,
        args=args,
        df=time_series_df,
        report_data_type=report_data_type,
        order_by_columns=get_order_by_columns(
            report_data_type, args.requesting_actor_market_role
        ),
    )

    dbutils.jobs.taskValues.set(key=task_key, value=time_series_files)


def execute_charge_links(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    """
    Entry point for the logic of creating charge links.
    """
    if not args.include_basis_data:
        return

    repository = WholesaleRepository(spark, args.catalog_name)
    charge_links = create_charge_links(args=args, repository=repository)

    charge_links_files = csv_writer.write(
        dbutils=dbutils,
        args=args,
        df=charge_links,
        report_data_type=ReportDataType.ChargeLinks,
        order_by_columns=get_order_by_columns(
            ReportDataType.ChargeLinks, args.requesting_actor_market_role
        ),
    )

    dbutils.jobs.taskValues.set(key="charge_links_files", value=charge_links_files)


def execute_energy_results(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    """
    Entry point for the logic of creating energy results.
    """
    if args.requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR:
        return

    repository = WholesaleRepository(spark, args.catalog_name)
    energy_results_df = create_energy_results(args=args, repository=repository)

    energy_result_files = csv_writer.write(
        dbutils=dbutils,
        args=args,
        df=energy_results_df,
        report_data_type=ReportDataType.EnergyResults,
        order_by_columns=get_order_by_columns(
            ReportDataType.EnergyResults, args.requesting_actor_market_role
        ),
    )

    dbutils.jobs.taskValues.set(key="energy_result_files", value=energy_result_files)


def execute_wholesale_results(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    """
    Entry point for the logic of creating wholesale results.
    """
    repository = WholesaleRepository(spark, args.catalog_name)
    wholesale_results_df = create_wholesale_results(args=args, repository=repository)

    wholesale_result_files = csv_writer.write(
        dbutils=dbutils,
        args=args,
        df=wholesale_results_df,
        report_data_type=ReportDataType.WholesaleResults,
        order_by_columns=get_order_by_columns(
            ReportDataType.WholesaleResults, args.requesting_actor_market_role
        ),
    )

    dbutils.jobs.taskValues.set(
        key="wholesale_result_files", value=wholesale_result_files
    )


def execute_monthly_amounts(
    spark: SparkSession, dbutils: Any, args: SettlementReportArgs
) -> None:
    """
    Entry point for the logic of monthly amounts.
    """
    repository = WholesaleRepository(spark, args.catalog_name)
    monthly_amounts = create_monthly_amounts(args=args, repository=repository)

    monthly_amounts_files = csv_writer.write(
        dbutils,
        args,
        monthly_amounts,
        ReportDataType.MonthlyAmounts,
        order_by_columns=get_order_by_columns(
            ReportDataType.MonthlyAmounts, args.requesting_actor_market_role
        ),
    )

    dbutils.jobs.taskValues.set(
        key="monthly_amounts_files", value=monthly_amounts_files
    )


def execute_zip(spark: SparkSession, dbutils: Any, args: SettlementReportArgs) -> None:
    """
    Entry point for the logic of creating the final zip file.
    """
    files_to_zip = []

    task_types_to_zip = {
        TaskType.HOURLY_TIME_SERIES: "hourly_time_series_files",
        TaskType.QUARTERLY_TIME_SERIES: "quarterly_time_series_files",
        TaskType.CHARGE_LINKS: "charge_links_files",
        TaskType.ENERGY_RESULTS: "energy_result_files",
        TaskType.MONTHLY_AMOUNTS: "monthly_amounts_files",
    }

    for taskKey, key in task_types_to_zip.items():
        try:
            files_to_zip.extend(
                dbutils.jobs.taskValues.get(taskKey=taskKey.value, key=key)
            )
        except ValueError:
            log.info(
                f"Task Key {taskKey.value} was not found in TaskValues, continuing without it."
            )

    log.info(f"Files to zip: {files_to_zip}")
    zip_file_path = f"{args.settlement_reports_output_path}/{args.report_id}.zip"
    log.info(f"Creating zip file: '{zip_file_path}'")
    create_zip_file(dbutils, args.report_id, zip_file_path, files_to_zip)
    log.info(f"Finished creating '{zip_file_path}'")
