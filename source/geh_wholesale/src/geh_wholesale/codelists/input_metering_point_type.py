from enum import Enum


class InputMeteringPointType(Enum):
    """This type should be replaced by `MeteringPointType` when the contract with the migration subsystem has been updated."""

    PRODUCTION = "E18"
    CONSUMPTION = "E17"
    EXCHANGE = "E20"
    VE_PRODUCTION = "D01"
    NET_PRODUCTION = "D05"
    SUPPLY_TO_GRID = "D06"
    CONSUMPTION_FROM_GRID = "D07"
    WHOLESALE_SERVICES_INFORMATION = "D08"
    OWN_PRODUCTION = "D09"
    NET_FROM_GRID = "D10"
    NET_TO_GRID = "D11"
    TOTAL_CONSUMPTION = "D12"
    ELECTRICAL_HEATING = "D14"
    NET_CONSUMPTION = "D15"
    CAPACITY_SETTLEMENT = "D19"
