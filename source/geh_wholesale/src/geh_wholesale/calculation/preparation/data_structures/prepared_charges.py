from dataclasses import dataclass

from geh_wholesale.calculation.preparation.data_structures.prepared_fees import PreparedFees
from geh_wholesale.calculation.preparation.data_structures.prepared_subscriptions import PreparedSubscriptions
from geh_wholesale.calculation.preparation.data_structures.prepared_tariffs import PreparedTariffs


@dataclass
class PreparedChargesContainer:
    hourly_tariffs: PreparedTariffs | None = None
    daily_tariffs: PreparedTariffs | None = None
    subscriptions: PreparedSubscriptions | None = None
    fees: PreparedFees | None = None
