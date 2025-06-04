from enum import Enum


class ChargeQuality(Enum):
    CALCULATED = "calculated"
    ESTIMATED = "estimated"
    MEASURED = "measured"
    MISSING = "missing"
