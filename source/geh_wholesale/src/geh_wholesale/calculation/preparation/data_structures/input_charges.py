from dataclasses import dataclass

from pyspark.sql import DataFrame

from geh_wholesale.calculation.preparation.data_structures.charge_price_information import (
    ChargePriceInformation,
)
from geh_wholesale.calculation.preparation.data_structures.charge_prices import ChargePrices


@dataclass
class InputChargesContainer:
    charge_price_information: ChargePriceInformation
    charge_prices: ChargePrices
    charge_links: DataFrame
