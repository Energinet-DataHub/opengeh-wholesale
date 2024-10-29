from enum import Enum


class TaskType(Enum):
    """
    Enum for the different types of tasks that can be executed.
    The value needs to correspond to the name of the task in the SettlementReportJob workflow in the infrastructure.
    """

    HOURLY_TIME_SERIES = "hourly_time_series"
    QUARTERLY_TIME_SERIES = "quarterly_time_series"
    ENERGY_RESULTS = "energy_results"
    MONTHLY_AMOUNTS = "monthly_amounts"
    ZIP = "zip"
