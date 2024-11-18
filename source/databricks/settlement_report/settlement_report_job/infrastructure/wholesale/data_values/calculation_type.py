from enum import Enum


class CalculationTypeDataProductValue(Enum):
    AGGREGATION = "aggregation"
    BALANCE_FIXING = "balance_fixing"
    WHOLESALE_FIXING = "wholesale_fixing"
    FIRST_CORRECTION_SETTLEMENT = "first_correction_settlement"
    SECOND_CORRECTION_SETTLEMENT = "second_correction_settlement"
    THIRD_CORRECTION_SETTLEMENT = "third_correction_settlement"
