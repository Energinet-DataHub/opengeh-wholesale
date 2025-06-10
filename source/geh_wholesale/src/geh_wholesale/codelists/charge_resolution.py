from enum import Enum


class ChargeResolution(Enum):
    """Time resolution of the charges, which is read from input delta table."""

    MONTH = "P1M"
    """Applies to subscriptions and fees"""
    DAY = "P1D"
    """Applies to tariffs"""
    HOUR = "PT1H"
    """Applies to tariffs"""
