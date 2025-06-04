from enum import Enum


class ChargeUnit(Enum):
    KWH = "kWh"
    """Applies to tariffs"""
    PIECES = "pcs"
    """Applies to fees and subscriptions"""
