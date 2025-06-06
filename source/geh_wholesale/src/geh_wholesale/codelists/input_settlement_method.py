from enum import Enum


class InputSettlementMethod(Enum):
    """This type should be replaced by `SettlementMethod` when the contract with the migration subsystem has been updated."""

    FLEX = "D01"
    NON_PROFILED = "E02"
