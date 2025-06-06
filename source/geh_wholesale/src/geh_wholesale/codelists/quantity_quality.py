from enum import Enum


class QuantityQuality(Enum):
    MISSING = "missing"
    ESTIMATED = "estimated"
    MEASURED = "measured"
    CALCULATED = "calculated"
