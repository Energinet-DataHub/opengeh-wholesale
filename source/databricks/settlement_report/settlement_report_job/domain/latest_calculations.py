from pyspark.sql import DataFrame, functions as F, Window
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
    EphemeralColumns,
)


def get_latest_calculations(
    repository: WholesaleRepository, time_zone: str
) -> DataFrame:

    calculations = repository.read_calculations()

    day_sequence = F.sequence(
        # Create a sequence of the start of each day in the period. The times are local time
        F.from_utc_timestamp(
            DataProductColumnNames.calculation_period_start, time_zone
        ),
        F.from_utc_timestamp(DataProductColumnNames.calculation_period_end, time_zone)
        - F.expr("interval 1 day"),
        F.expr("interval 1 day"),
    )

    calculations = calculations.withColumn(
        EphemeralColumns.start_of_day,
        F.explode(day_sequence),
    ).withColumn(
        # Convert local day start times back to UTC
        EphemeralColumns.start_of_day,
        F.to_utc_timestamp(EphemeralColumns.start_of_day, time_zone),
    )

    window_spec = Window.partitionBy(
        DataProductColumnNames.calculation_type,
        DataProductColumnNames.grid_area_code,
        EphemeralColumns.start_of_day,
    ).orderBy(F.desc(DataProductColumnNames.calculation_version))

    calculations = calculations.withColumn("rank", F.rank().over(window_spec))
    latest_calculations = calculations.where(F.col("rank") == 1).drop("rank")

    return latest_calculations
