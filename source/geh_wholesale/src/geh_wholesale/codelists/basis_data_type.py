from enum import Enum


class BasisDataType(Enum):
    TIME_SERIES_QUARTER = "time_series_quarter"
    TIME_SERIES_HOUR = "time_series_hour"
    MASTER_BASIS_DATA = "master_basis_data"
