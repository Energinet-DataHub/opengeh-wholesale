from pyspark.sql import Column
from pyspark.sql import functions as F


def get_start_of_day(col: Column | str, time_zone: str) -> Column:
    col = F.col(col) if isinstance(col, str) else col
    return F.to_utc_timestamp(
        F.date_trunc("DAY", F.from_utc_timestamp(col, time_zone)), time_zone
    )
