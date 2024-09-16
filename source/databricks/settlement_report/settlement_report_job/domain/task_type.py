from enum import Enum


class TaskType(Enum):
    HOURLY = "hourly"
    QUARTERLY = "quarterly"
    REST = "rest"
    ZIP = "zip"
