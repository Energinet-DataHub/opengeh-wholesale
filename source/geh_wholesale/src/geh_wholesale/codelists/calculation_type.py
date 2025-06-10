from enum import Enum


class CalculationType(Enum):
    BALANCE_FIXING = "balance_fixing"
    AGGREGATION = "aggregation"
    WHOLESALE_FIXING = "wholesale_fixing"
    FIRST_CORRECTION_SETTLEMENT = "first_correction_settlement"
    SECOND_CORRECTION_SETTLEMENT = "second_correction_settlement"
    THIRD_CORRECTION_SETTLEMENT = "third_correction_settlement"


def is_wholesale_calculation_type(calculation_type: CalculationType) -> bool:
    return (
        calculation_type == CalculationType.WHOLESALE_FIXING
        or calculation_type == CalculationType.FIRST_CORRECTION_SETTLEMENT
        or calculation_type == CalculationType.SECOND_CORRECTION_SETTLEMENT
        or calculation_type == CalculationType.THIRD_CORRECTION_SETTLEMENT
    )
