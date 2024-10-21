from pyspark.sql import DataFrame

from settlement_report_job import logging
from settlement_report_job.domain.csv_column_names import EphemeralColumns
from settlement_report_job.domain.get_start_of_day import get_start_of_day
from settlement_report_job.wholesale.column_names import DataProductColumnNames

log = logging.Logger(__name__)


def _filter_by_latest_calculations(
    df: DataFrame, latest_calculations: DataFrame, time_zone: str
) -> DataFrame:
    df = df.withColumn(
        EphemeralColumns.start_of_day,
        get_start_of_day(DataProductColumnNames.observation_time, time_zone),
    )

    return (
        df.join(
            latest_calculations,
            on=[
                df[DataProductColumnNames.calculation_id]
                == latest_calculations[DataProductColumnNames.calculation_id],
                df[DataProductColumnNames.grid_area_code]
                == latest_calculations[DataProductColumnNames.grid_area_code],
                df[EphemeralColumns.start_of_day]
                == latest_calculations[DataProductColumnNames.start_of_day],
            ],
            how="inner",
        )
        .select(df["*"])
        .drop(EphemeralColumns.start_of_day)
    )
