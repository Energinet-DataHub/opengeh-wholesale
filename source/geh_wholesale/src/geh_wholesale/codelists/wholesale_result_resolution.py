from enum import Enum


class WholesaleResultResolution(Enum):
    """Time resolution of a wholesale result, which is stored in the wholesale result delta table."""

    MONTH = "P1M"
    """Applies to all (tariffs, subscriptions and fees) for monthly amount and total amount"""
    DAY = "P1D"
    """Applies to tariffs, subscriptions and fees"""
    HOUR = "PT1H"
    """Applies to tariffs"""
