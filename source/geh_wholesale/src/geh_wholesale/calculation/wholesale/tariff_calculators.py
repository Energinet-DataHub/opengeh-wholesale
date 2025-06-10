from geh_wholesale.calculation.preparation.data_structures.prepared_tariffs import (
    PreparedTariffs,
)
from geh_wholesale.calculation.wholesale.calculate_total_quantity_and_amount import (
    calculate_total_quantity_and_amount,
)
from geh_wholesale.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
)
from geh_wholesale.codelists import ChargeType


def calculate_tariff_price_per_co_es(
    prepared_tariffs: PreparedTariffs,
) -> WholesaleResults:
    """Calculate tariff amount time series.

    A result is calculated per
    - grid area
    - charge key (charge id, charge type, charge owner)
    - settlement method
    - metering point type (except exchange metering points)
    - energy supplier

    Resolution has already been filtered, so only one resolution is present
    in the tariffs data frame. So responsibility of creating results per
    resolution is managed outside this module.
    """
    df = calculate_total_quantity_and_amount(prepared_tariffs.df, ChargeType.TARIFF)
    return WholesaleResults(df)
