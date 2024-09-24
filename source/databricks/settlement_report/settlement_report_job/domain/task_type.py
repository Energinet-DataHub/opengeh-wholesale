from enum import Enum


class TaskType(Enum):
    HOURLY_TIME_SERIES = "hourly_time_series"
    QUARTERLY_TIME_SERIES = "quarterly_time_series"
    REST = "rest"
    ZIP = "zip"
