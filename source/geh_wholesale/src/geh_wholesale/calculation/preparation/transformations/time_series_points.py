from datetime import datetime

from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from geh_wholesale.constants import Colname
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository


def get_time_series_points(
    calculation_input_reader: MigrationsWholesaleRepository,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    time_series_points_df = (
        calculation_input_reader.read_time_series_points()
        .where(col(Colname.observation_time) >= period_start_datetime)
        .where(col(Colname.observation_time) < period_end_datetime)
    )
    if "observation_year" in time_series_points_df.columns:
        time_series_points_df = time_series_points_df.drop("observation_year")  # Drop year partition column

    if "observation_month" in time_series_points_df.columns:
        time_series_points_df = time_series_points_df.drop("observation_month")  # Drop month partition column

    return time_series_points_df
