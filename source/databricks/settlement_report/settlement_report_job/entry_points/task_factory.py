from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.entry_points.task_type import TaskType
from settlement_report_job.entry_points.tasks.energy_resuls_task import (
    EnergyResultsTask,
)
from settlement_report_job.entry_points.tasks.monthly_amounts_task import (
    MonthlyAmountsTask,
)
from settlement_report_job.entry_points.tasks.task_base import (
    TaskBase,
)
from settlement_report_job.entry_points.tasks.metering_point_periods_task import (
    MeteringPointPeriodsTask,
)
from settlement_report_job.entry_points.tasks.time_series_task import TimeSeriesTask
from settlement_report_job.entry_points.tasks.wholesale_results_task import (
    WholesaleResultsTask,
)


def create(
    task_type: TaskType,
    spark: SparkSession,
    dbutils: Any,
    args: SettlementReportArgs,
) -> TaskBase:
    if task_type is TaskType.MeteringPointPeriods:
        return MeteringPointPeriodsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.TimeSeriesQuarterly:
        return TimeSeriesTask(
            spark=spark,
            dbutils=dbutils,
            args=args,
            task_type=TaskType.TimeSeriesQuarterly,
        )
    elif task_type is TaskType.TimeSeriesHourly:
        return TimeSeriesHourlyTask(spark=spark, dbutils=dbutils, args=args)

    elif task_type is TaskType.EnergyResults:
        return EnergyResultsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.WholesaleResults:
        return WholesaleResultsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.MonthlyAmounts:
        return MonthlyAmountsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.MeteringPointPeriods:
        return MeteringPointPeriodsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.EnergyResults:
        return EnergyResultsTask(spark=spark, dbutils=dbutils, args=args)
    elif task_type is TaskType.WholesaleResults:
        return WholesaleResultsTask(spark=spark, dbutils=dbutils, args=args)
