from datetime import datetime

from pyspark.sql import DataFrame, functions as F

from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)


def clamp_to_selected_period(
    periods: DataFrame, selected_period_start: datetime, selected_period_end: datetime
) -> DataFrame:
    periods = periods.withColumn(
        DataProductColumnNames.to_date,
        F.when(
            F.col(DataProductColumnNames.to_date) > selected_period_end,
            selected_period_end,
        ).otherwise(F.col(DataProductColumnNames.to_date)),
    ).withColumn(
        DataProductColumnNames.from_date,
        F.when(
            F.col(DataProductColumnNames.from_date) < selected_period_start,
            selected_period_start,
        ).otherwise(F.col(DataProductColumnNames.from_date)),
    )
    return periods
