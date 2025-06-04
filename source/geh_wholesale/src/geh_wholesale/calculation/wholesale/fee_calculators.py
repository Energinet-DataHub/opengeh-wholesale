from geh_wholesale.calculation.preparation.data_structures import PreparedFees
from geh_wholesale.calculation.wholesale.calculate_total_quantity_and_amount import (
    calculate_total_quantity_and_amount,
)
from geh_wholesale.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
)
from geh_wholesale.codelists import ChargeType


def calculate(prepared_fees: PreparedFees) -> WholesaleResults:
    df = calculate_total_quantity_and_amount(prepared_fees.df, ChargeType.FEE)
    return WholesaleResults(df)
