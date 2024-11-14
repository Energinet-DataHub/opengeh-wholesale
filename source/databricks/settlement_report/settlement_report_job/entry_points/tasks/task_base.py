from abc import abstractmethod
from typing import Any

from pyspark.sql import SparkSession

from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from telemetry_logging import Logger


class TaskBase:
    def __init__(
        self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs
    ) -> None:
        self.spark = spark
        self.dbutils = dbutils
        self.args = args
        self.log = Logger(__name__)

    @abstractmethod
    def execute(self) -> None:
        pass
